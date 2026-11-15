using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Windows.Forms;
using PizzeriaBellaItalia.Dominio;

namespace PizzeriaBellaItalia.Presentacion
{
    // Formulario principal de la aplicación (capa de presentación).
    // Responsabilidades principales:
    // - Capturar datos del cliente y configuración de pizzas.
    // - Mantener el detalle del pedido en memoria (BindingList<ItemPedido>).
    // - Calcular y mostrar totales.
    // - Invocar al DAO para persistir/consultar pedidos en la base de datos.
    public partial class FormPedido : Form
    {
        // Id del pedido actualmente cargado para edición; null indica "nuevo pedido".
        private int? idPedidoSeleccionado = null;

        // Objeto de acceso a datos (CRUD) para la entidad Pedido y sus detalles.
        private readonly PedidoDAO pedidoDao = new PedidoDAO();

        // Lista enlazable de ítems del pedido actual. Al modificarla y refrescar bindings,
        // la grilla de detalle se actualiza automáticamente.
        private readonly BindingList<ItemPedido> items = new BindingList<ItemPedido>();

        public FormPedido()
        {
            // Inicializa los controles definidos en FormPedido.Designer.cs.
            InitializeComponent();
        }

        // Evento Load: se ejecuta al abrir el formulario.
        // Flujo: configurar cultura/controles -> configurar grillas -> inicializar BD -> cargar pedidos guardados.
        private void FormPedido_Load(object? sender, EventArgs e)
        {
            // Define formato regional para moneda/números mostrados en la UI.
            CultureInfo.CurrentCulture = new CultureInfo("es-CL");

            // Carga opciones de tamaño con precios base visibles para el usuario.
            cboTamano.Items.Clear();
            cboTamano.Items.Add("Personal ($5.000)");
            cboTamano.Items.Add("Mediana ($8.500)");
            cboTamano.Items.Add("Familiar ($12.000)");
            cboTamano.SelectedIndex = 1;

            // Carga opciones de masa (incluye recargo en borde de queso).
            cboMasa.Items.Clear();
            cboMasa.Items.Add("Tradicional");
            cboMasa.Items.Add("Delgada");
            cboMasa.Items.Add("Borde de Queso (+$1.500)");
            cboMasa.SelectedIndex = 0;

            // Inicializa fecha y hora del pedido con el momento actual.
            dtpFecha.Value = DateTime.Now;
            dtpHora.Value = DateTime.Now;

            // Configura comportamiento visual/funcional de grillas y enlaza el detalle a la lista en memoria.
            ConfigurarGrilla(dgvDetalleItem);
            ConfigurarGrilla(dgvPedidos);
            dgvDetalleItem.DataSource = items;
            FormatearGrillaDetalle();

            // Calcula totales iniciales (pedido vacío).
            Recalcular();

            // Intenta preparar la base de datos y cargar pedidos existentes.
            // Si falla, la app sigue abierta pero informa el problema al usuario.
            try
            {
                InitDatabase.Inicializar();
                CargarGrillaPedidos();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo inicializar o conectar a la base de datos.\n\n" +
                    ex.Message + "\n\n" +
                    "Revise appsettings.json o la variable de entorno PIZZERIA_CONNECTION_STRING.",
                    "Base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        // ---------- Configuración de grillas ----------
        // Aplica una configuración estándar de solo lectura/selección a un DataGridView.
        private static void ConfigurarGrilla(DataGridView dgv)
        {
            dgv.ReadOnly = true;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.MultiSelect = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
        }

        // Ajusta encabezados, pesos y formatos de columnas de la grilla de detalle
        // según las propiedades expuestas por ItemPedido.
        private void FormatearGrillaDetalle()
        {
            if (dgvDetalleItem.Columns.Count == 0) return;

            // Helper local para configurar una columna si existe.
            void Col(string nombre, string? titulo, float peso, string? formato = null)
            {
                if (!dgvDetalleItem.Columns.Contains(nombre)) return;
                var c = dgvDetalleItem.Columns[nombre];
                if (titulo != null) c.HeaderText = titulo;
                c.FillWeight = peso;
                if (formato != null) c.DefaultCellStyle.Format = formato;
            }

            Col("Cantidad", "Cant.", 8);
            Col("Resumen", null, 38);
            Col("PrecioUnitario", "Precio unit.", 11, "N0");
            Col("Subtotal", "Subtotal", 11, "N0");
            Col("Descuento", "Descuento", 11, "N0");
            Col("Iva", "I.V.A.", 10, "N0");
            Col("Delivery", "Delivery", 9);
            Col("TotalLinea", "Total línea", 12, "N0");
        }

        // Consulta los pedidos persistidos y los muestra en la grilla inferior.
        private void CargarGrillaPedidos()
        {
            try
            {
                dgvPedidos.DataSource = pedidoDao.ObtenerPedidosDataTable();
                if (dgvPedidos.Columns.Contains("Total"))
                    dgvPedidos.Columns["Total"].DefaultCellStyle.Format = "N0";
                if (dgvPedidos.Columns.Contains("Fecha"))
                    dgvPedidos.Columns["Fecha"].DefaultCellStyle.Format = "g";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la grilla de pedidos: " + ex.Message, "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ---------- Armado del pedido ----------
        // Construye un ItemPedido a partir de los controles de configuración de pizza.
        // Traduce textos de combos a valores de dominio usados en cálculos/persistencia.
        private ItemPedido ConstruirItemDesdeFormulario()
        {
            string tamanoTexto = cboTamano.SelectedItem?.ToString() ?? "Mediana";
            string tamano = tamanoTexto.Contains("Personal") ? "Personal"
                          : tamanoTexto.Contains("Familiar") ? "Familiar" : "Mediana";

            string masaTexto = cboMasa.SelectedItem?.ToString() ?? "Tradicional";
            string masa = masaTexto.Contains("Delgada") ? "Delgada"
                        : masaTexto.Contains("Borde") ? "Borde de Queso" : "Tradicional";

            return new ItemPedido
            {
                Tamano = tamano,
                TipoMasa = masa,
                QuesoExtra = chkQuesoExtra.Checked,
                Pepperoni = chkPepperoni.Checked,
                Champinones = chkChampinones.Checked,
                Aceitunas = chkAceitunas.Checked,
                Tocino = chkTocino.Checked,
                Cantidad = (int)numCantidad.Value,
                // Flags a nivel pedido que también se reflejan por línea para cálculos/visualización.
                AplicaDescuento = chkDescuento.Checked,
                EsDelivery = chkDelivery.Checked
            };
        }

        // Construye el objeto PedidoPizza completo (cabecera + copia de ítems actuales).
        // Este objeto es el que se envía al DAO para insertar/actualizar.
        private PedidoPizza ConstruirPedidoDesdeFormulario()
        {
            var pedido = new PedidoPizza
            {
                IdPedido = idPedidoSeleccionado ?? 0,
                FechaCreacion = dtpFecha.Value.Date + dtpHora.Value.TimeOfDay,
                Cliente = txtCliente.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                EsDelivery = chkDelivery.Checked,
                AplicaConvenioEstudiante = chkDescuento.Checked
            };
            pedido.Items.AddRange(items);
            return pedido;
        }

        // Recalcula totales del pedido actual y refresca la grilla de detalle.
        // Se invoca ante cambios de opciones (delivery/descuento) o modificaciones de ítems.
        private void Recalcular()
        {
            // Sincroniza flags globales del pedido hacia cada línea para que el detalle muestre
            // descuento/IVA/delivery coherentes con la selección actual.
            foreach (var it in items)
            {
                it.AplicaDescuento = chkDescuento.Checked;
                it.EsDelivery = chkDelivery.Checked;
            }
            items.ResetBindings();
            FormatearGrillaDetalle();

            PedidoPizza p = ConstruirPedidoDesdeFormulario();
            lblSubtotalValor.Text = $"$ {p.CalcularSubtotalPizza():N0}";
            lblIvaValor.Text = $"$ {p.CalcularMontoIva():N0}";
            lblTotalValor.Text = $"$ {p.CalcularTotalPagar():N0}";
        }

        // Evento común para cambios en opciones que afectan totales (delivery/descuento).
        private void chkOpcion_CheckedChanged(object? sender, EventArgs e)
        {
            Recalcular();
        }

        // ---------- Agregar / quitar líneas ----------
        // Agrega una nueva línea al detalle o acumula cantidad si ya existe una configuración igual.
        private void btnAgregar_Click(object? sender, EventArgs e)
        {
            ItemPedido nuevo = ConstruirItemDesdeFormulario();

            // Busca si ya hay una línea con la misma configuración (tamaño, masa, ingredientes).
            ItemPedido? existente = null;
            foreach (var it in items)
                if (it.MismaConfiguracion(nuevo)) { existente = it; break; }

            if (existente != null)
                existente.Cantidad += nuevo.Cantidad;   // Misma pizza: se incrementa cantidad.
            else
                items.Add(nuevo);

            // Restablece selección de ingredientes/cantidad para facilitar la carga del siguiente ítem.
            numCantidad.Value = 1;
            chkQuesoExtra.Checked = false;
            chkPepperoni.Checked = false;
            chkChampinones.Checked = false;
            chkAceitunas.Checked = false;
            chkTocino.Checked = false;

            Recalcular();
        }

        // ---------- Ver una línea del detalle en el formulario ----------
        // Al hacer clic en una fila del detalle, carga sus valores en los controles para revisión.
        private void dgvDetalleItem_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvDetalleItem.Rows[e.RowIndex].DataBoundItem is not ItemPedido it) return;

            // Mapea valores de dominio a índices/estados de controles de la UI.
            cboTamano.SelectedIndex = it.Tamano switch
            {
                "Personal" => 0,
                "Familiar" => 2,
                _ => 1
            };
            cboMasa.SelectedIndex = it.TipoMasa switch
            {
                "Delgada" => 1,
                "Borde de Queso" => 2,
                _ => 0
            };

            chkQuesoExtra.Checked = it.QuesoExtra;
            chkPepperoni.Checked = it.Pepperoni;
            chkChampinones.Checked = it.Champinones;
            chkAceitunas.Checked = it.Aceitunas;
            chkTocino.Checked = it.Tocino;
            numCantidad.Value = Math.Clamp(it.Cantidad, (int)numCantidad.Minimum, (int)numCantidad.Maximum);
        }

        // Quita la línea seleccionada del detalle en memoria y recalcula totales.
        private void btnQuitar_Click(object? sender, EventArgs e)
        {
            if (dgvDetalleItem.CurrentRow?.DataBoundItem is ItemPedido it)
            {
                items.Remove(it);
                Recalcular();
            }
            else
            {
                MessageBox.Show("Seleccione un ítem del detalle para quitarlo.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        // ---------- Nuevo / limpiar ----------
        // Botón "Nuevo": reinicia el formulario a estado inicial.
        private void btnLimpiar_Click(object? sender, EventArgs e)
        {
            LimpiarCamposFormulario();
        }

        // Limpia datos de cliente, opciones, detalle y estado de edición.
        private void LimpiarCamposFormulario()
        {
            txtCliente.Clear();
            txtTelefono.Clear();
            dtpFecha.Value = DateTime.Now;
            dtpHora.Value = DateTime.Now;
            cboTamano.SelectedIndex = 1;
            cboMasa.SelectedIndex = 0;
            numCantidad.Value = 1;
            chkQuesoExtra.Checked = false;
            chkPepperoni.Checked = false;
            chkChampinones.Checked = false;
            chkAceitunas.Checked = false;
            chkTocino.Checked = false;
            chkDelivery.Checked = false;
            chkDescuento.Checked = false;

            items.Clear();
            idPedidoSeleccionado = null;
            btnGuardar.Text = "Guardar pedido";
            Recalcular();
            txtCliente.Focus();
        }

        // ---------- Guardar (crear o actualizar) ----------
        // Valida datos mínimos, decide si insertar o actualizar según idPedidoSeleccionado,
        // persiste mediante DAO y refresca la UI.
        private void btnGuardar_Click(object? sender, EventArgs e)
        {
            // Validaciones de entrada en la UI antes de intentar persistir.
            if (string.IsNullOrWhiteSpace(txtCliente.Text))
            {
                MessageBox.Show("Ingrese el nombre del cliente.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCliente.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTelefono.Text))
            {
                MessageBox.Show("Ingrese el teléfono del cliente.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTelefono.Focus();
                return;
            }

            if (items.Count == 0)
            {
                MessageBox.Show("Agregue al menos una pizza al pedido con el botón \"Agregar al pedido\".",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Construye el modelo de dominio a partir del estado actual del formulario.
                PedidoPizza pedido = ConstruirPedidoDesdeFormulario();
                bool resultado;
                string mensaje;

                // Flujo de persistencia: CREATE si es nuevo, UPDATE si se está editando uno existente.
                if (idPedidoSeleccionado == null)
                {
                    resultado = pedidoDao.InsertarPedido(pedido);
                    mensaje = $"Pedido registrado exitosamente (N° {pedido.IdPedido}).";
                }
                else
                {
                    resultado = pedidoDao.ActualizarPedido(pedido);
                    mensaje = "Pedido actualizado correctamente.";
                }

                if (resultado)
                {
                    MessageBox.Show(mensaje, "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarCamposFormulario();
                    CargarGrillaPedidos();
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el pedido.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el pedido: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- Eliminar ----------
        // Elimina el pedido seleccionado en la grilla inferior previa confirmación del usuario.
        private void btnEliminar_Click(object? sender, EventArgs e)
        {
            if (idPedidoSeleccionado == null)
            {
                MessageBox.Show("Seleccione un pedido de la grilla inferior para eliminar.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("¿Está seguro de eliminar este pedido?", "Confirmar",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (pedidoDao.EliminarPedido(idPedidoSeleccionado.Value))
                {
                    MessageBox.Show("Pedido eliminado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LimpiarCamposFormulario();
                    CargarGrillaPedidos();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el pedido.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al eliminar: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ---------- Seleccionar un pedido guardado ----------
        // Carga un pedido desde la base de datos al formulario para visualizarlo o editarlo.
        private void dgvPedidos_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                int id = Convert.ToInt32(dgvPedidos.Rows[e.RowIndex].Cells["IdPedido"].Value);
                PedidoPizza? p = pedidoDao.ObtenerPedidoPorId(id);
                if (p == null) return;

                // Marca el pedido como seleccionado para habilitar modo edición/eliminación.
                idPedidoSeleccionado = id;

                // Carga datos de cabecera en controles.
                txtCliente.Text = p.Cliente;
                txtTelefono.Text = p.Telefono == "Sin Teléfono" ? string.Empty : p.Telefono;
                dtpFecha.Value = p.FechaCreacion;
                dtpHora.Value = p.FechaCreacion;

                // Carga detalle del pedido en la lista enlazada a la grilla.
                items.Clear();
                foreach (var it in p.Items) items.Add(it);

                // Actualiza opciones generales; los handlers disparan Recalcular() para refrescar totales.
                chkDelivery.Checked = p.EsDelivery;
                chkDescuento.Checked = p.AplicaConvenioEstudiante;

                btnGuardar.Text = "Actualizar pedido";
                Recalcular();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar el pedido: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Cierra la aplicación.
        private void btnSalir_Click(object? sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
