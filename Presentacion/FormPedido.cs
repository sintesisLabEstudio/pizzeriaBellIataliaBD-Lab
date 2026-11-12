// NOTA: Se cambió de Microsoft.Data.SqlClient a System.Data.SqlClient
// porque System.Data.SqlClient es el estándar en .NET 10 para Windows Forms
// Si prefieres Microsoft.Data.SqlClient, instala el paquete NuGet:
// Install-Package Microsoft.Data.SqlClient
using System.Data.SqlClient;  // ✅ CORRECTO PARA .NET 10
using PizzeriaBellaItalia.Dominio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace PizzeriaBellaItalia.Presentacion;

public partial class FormPedido : Form
{
    private readonly GestionPedidos gesPed = new();

    public FormPedido()
    {
        InitializeComponent();
    }

    private void FormPedido_Load(object sender, EventArgs e)
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // Inicializar la base de datos
        InitDatabase.Inicializar();

        PoblarComboBoxesConDiccionarios();
        LimpiarFormulario();

        // Cargar DataGrid con pedidos existentes
        CargarDataGridPedidos();
    }

    // -------------------------------------------------------------------- BOTÓNES
    private void BtnCalcular_Click(object sender, EventArgs e)
    {
        if (!ValidaInputs()) return;

        PedidoPizza pedido = new()
        {
            Cliente = txtCliente.Text.Trim(),
            Telefono = txtTelefono.Text.Trim(),
            TamanoNombre = cboTamano.Text,
            PrecioBaseTamano = Convert.ToInt32(cboTamano.SelectedValue),
            MasaNombre = cboMasa.Text,
            RecargoMasa = Convert.ToInt32(cboMasa.SelectedValue),
            QuesoExtra = chkQuesoExtra.Checked,
            Pepperoni = chkPepperoni.Checked,
            Champinones = chkChampinones.Checked,
            Aceitunas = chkAceitunas.Checked,
            Tocino = chkTocino.Checked,
            EsDelivery = rdbDelivery.Checked,
            AplicaConvenioEstudiante = chkConvenioEstudiante.Checked
        };

        lblSubtotalValor.Text = $"$ {pedido.CalcularSubtotalPizza():N0} CLP";
        lblDescuentoValor.Text = $"- $ {pedido.CalcularMontoDescuento():N0} CLP";
        lblDeliveryValor.Text = $"+ $ {pedido.CalcularCostoDelivery():N0} CLP";
        lblIvaValor.Text = $"+ $ {pedido.CalcularMontoIva():N0} CLP";
        lblTotalValor.Text = $"$ {pedido.CalcularTotalPagar():N0} CLP";

        gesPed.AgregarPedido(pedido);

        grpResumen.Enabled = true;

        //CONSULTA GRABAR EN BD 
        // Se usa Button1 (primer botón = Yes), Button2 (No), Button3 (Cancel)
        DialogResult resultado = MessageBox.Show(
            $"¿Grabar pedido?\n\nTotal: ${pedido.CalcularTotalPagar():N0} CLP",
            "Confirmar",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button1  // ✅ Button1 = Yes (primer botón)
        );

        if (resultado == DialogResult.Yes)
        {
            // Captura el resultado de la grabación y muestra el mensaje correspondiente
            bool grabado = GrabarRegistroPedido(pedido);
            // si graba ok
            if (grabado)
            {
                // ✅ ÉXITO - Se grabó correctamente
                MessageBox.Show(
                    "✅ Pedido guardado exitosamente en la base de datos",
                    "Grabación Exitosa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // Opcional: Limpiar el formulario después de grabar
                LimpiarFormulario();
            }
            // Si no grabó, el error ya se muestra en la función GrabarRegistroPedido()
        }
        else if (resultado == DialogResult.No)
        {
            MessageBox.Show("Pedido no guardado", "Cancelado", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else if (resultado == DialogResult.Cancel)
        {
            LimpiarFormulario();
        }
    }

    private void BtnLimpiar_Click(object sender, EventArgs e)
    {
        LimpiarFormulario();
    }

    private void BtnSalir_Click(object sender, EventArgs e)
    {
        DialogResult respuesta = MessageBox.Show(
            "¿Está seguro de que desea salir del sistema?",
            "Confirmar Salida",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );

        if (respuesta == DialogResult.Yes)
        {
            this.Close();
        }
    }

    // -------------------------------------------------------------------- MÉTODOS AUXILIARES
    private void PoblarComboBoxesConDiccionarios()
    {
        //crea diccionario opcionesTamano
        Dictionary<string, int> opcionesTamano = new()
        {
            { "Seleccione uno", -1 },
            { "Personal (1 Persona)", 5000 },
            { "Mediana (2 a 3 Personas)", 8500 },
            { "Familiar (4 a 5 Personas)", 12000 }
        };
        //asigna al combo un binding enlace al diccionario
        cboTamano.DataSource = new BindingSource { DataSource = opcionesTamano };
        cboTamano.DisplayMember = "Key";
        cboTamano.ValueMember = "Value";
        cboTamano.DropDownStyle = ComboBoxStyle.DropDownList;
        //crea diccionario opcionesMasa
        Dictionary<string, int> opcionesMasa = new()
        {
            { "Seleccione uno", -1 },
            { "Masa Tradicional (Sin Recargo)", 0 },
            { "Masa Delgada (Sin Recargo)", 0 },
            { "Borde de Queso (+$1.500)", 1500 }
        };
        //asigna al combo un binding enlace al diccionario
        cboMasa.DataSource = new BindingSource { DataSource = opcionesMasa };
        cboMasa.DisplayMember = "Key";
        cboMasa.ValueMember = "Value";
        cboMasa.DropDownStyle = ComboBoxStyle.DropDownList;
    }

    private bool ValidaInputs()
    {
        if (string.IsNullOrWhiteSpace(txtCliente.Text))
        {
            MessageBox.Show(
                "Por favor, ingrese el nombre del cliente antes de calcular.",
                "Validación de Datos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            txtCliente.Focus();
            return false;
        }

        if (cboTamano.SelectedValue == null || Convert.ToInt32(cboTamano.SelectedValue) == -1 ||
            cboMasa.SelectedValue == null || Convert.ToInt32(cboMasa.SelectedValue) == -1)
        {
            MessageBox.Show(
                "Por favor, seleccione un tamaño y un tipo de masa válidos.",
                "Validación de Datos",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return false;
        }
        return true;
    }

    private void LimpiarFormulario()
    {
        txtCliente.Clear();
        txtTelefono.Clear();

        if (cboTamano.Items.Count > 0) cboTamano.SelectedIndex = 0;
        if (cboMasa.Items.Count > 0) cboMasa.SelectedIndex = 0;

        chkQuesoExtra.Checked = false;
        chkPepperoni.Checked = false;
        chkChampinones.Checked = false;
        chkAceitunas.Checked = false;
        chkTocino.Checked = false;

        rdbRetiro.Checked = true;
        chkConvenioEstudiante.Checked = false;

        lblSubtotalValor.Text = "$ 0 CLP";
        lblDescuentoValor.Text = "$ 0 CLP";
        lblDeliveryValor.Text = "$ 0 CLP";
        lblIvaValor.Text = "$ 0 CLP";
        lblTotalValor.Text = "$ 0 CLP";

        grpResumen.Enabled = false;

        txtCliente.Focus();
    }

    //graba en la BD
    public bool GrabarRegistroPedido(PedidoPizza pedido)
    {
        try
        {
            // Conexión a BD local - Usa MSSQLLocalDB (servidor de desarrollo local)
            // Server=(localdb)\MSSQLLocalDB es la instancia local de SQL Express
            using (SqlConnection conexion = new("Server=(localdb)\\MSSQLLocalDB;Database=PizzeriaDB;Integrated Security=True;"))
            {
                conexion.Open();

                using (SqlCommand comando = new(
                    "INSERT INTO Pedidos (Cliente, Telefono, TamanoNombre, PrecioBaseTamano, MasaNombre, RecargoMasa, " +
                    "QuesoExtra, Pepperoni, Champinones, Aceitunas, Tocino, EsDelivery, AplicaConvenioEstudiante, " +
                    "CostoIngredientes, SubtotalPizza, MontoDescuento, CostoDelivery, NetoAfecto, MontoIva, TotalPagar) " +
                    "VALUES (@Cliente, @Telefono, @TamanoNombre, @PrecioBaseTamano, @MasaNombre, @RecargoMasa, " +
                    "@QuesoExtra, @Pepperoni, @Champinones, @Aceitunas, @Tocino, @EsDelivery, @AplicaConvenioEstudiante, " +
                    "@CostoIngredientes, @SubtotalPizza, @MontoDescuento, @CostoDelivery, @NetoAfecto, @MontoIva, @TotalPagar)",
                    conexion))
                {
                    comando.Parameters.AddWithValue("@Cliente", pedido.Cliente);
                    comando.Parameters.AddWithValue("@Telefono", pedido.Telefono);
                    comando.Parameters.AddWithValue("@TamanoNombre", pedido.TamanoNombre);
                    comando.Parameters.AddWithValue("@PrecioBaseTamano", pedido.PrecioBaseTamano);
                    comando.Parameters.AddWithValue("@MasaNombre", pedido.MasaNombre);
                    comando.Parameters.AddWithValue("@RecargoMasa", pedido.RecargoMasa);
                    comando.Parameters.AddWithValue("@QuesoExtra", pedido.QuesoExtra);
                    comando.Parameters.AddWithValue("@Pepperoni", pedido.Pepperoni);
                    comando.Parameters.AddWithValue("@Champinones", pedido.Champinones);
                    comando.Parameters.AddWithValue("@Aceitunas", pedido.Aceitunas);
                    comando.Parameters.AddWithValue("@Tocino", pedido.Tocino);
                    comando.Parameters.AddWithValue("@EsDelivery", pedido.EsDelivery);
                    comando.Parameters.AddWithValue("@AplicaConvenioEstudiante", pedido.AplicaConvenioEstudiante);
                    comando.Parameters.AddWithValue("@CostoIngredientes", pedido.CalcularCostoIngredientes());
                    comando.Parameters.AddWithValue("@SubtotalPizza", pedido.CalcularSubtotalPizza());
                    comando.Parameters.AddWithValue("@MontoDescuento", pedido.CalcularMontoDescuento());
                    comando.Parameters.AddWithValue("@CostoDelivery", pedido.CalcularCostoDelivery());
                    comando.Parameters.AddWithValue("@NetoAfecto", pedido.CalcularNetoAfecto());
                    comando.Parameters.AddWithValue("@MontoIva", pedido.CalcularMontoIva());
                    comando.Parameters.AddWithValue("@TotalPagar", pedido.CalcularTotalPagar());

                    int filasAfectadas = comando.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
            return false;
        }
    }

    // ===========================# 1. Verificar qué remotos tienes configurados
git remote -v

# 2. Si no ves nada, configurar el remoto
git remote add origin https://github.com/sintesisLabEstudio/pizzeriaBellIataliaBD-Lab.git

# 3. Verificar que se agregó correctamente
git remote -v

# 4. Ahora hacer push a master
git push -u origin master# 1. Verificar qué remotos tienes configurados
git remote -v

# 2. Si no ves nada, configurar el remoto
git remote add origin https://github.com/sintesisLabEstudio/pizzeriaBellIataliaBD-Lab.git

# 3. Verificar que se agregó correctamente
git remote -v

# 4. Ahora hacer push a master
git push -u origin master=============================================== MÉTODOS CRUD CON DATAGRID
    private void CargarDataGridPedidos()
    {
        try
        {
            PedidoDAO dao = new PedidoDAO();
            System.Data.DataTable dt = dao.ObtenerPedidosDataTable();
            dgvPedidos.DataSource = dt;

            // Ajustar ancho de columnas
            if (dgvPedidos.Columns.Count > 0)
            {
                dgvPedidos.Columns["IdPedido"].Width = 60;
                dgvPedidos.Columns["Cliente"].Width = 120;
                dgvPedidos.Columns["TotalPagar"].Width = 90;
                dgvPedidos.Columns["FechaCreacion"].Width = 140;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar pedidos: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// Evento al seleccionar una fila del DataGrid
    private void DgvPedidos_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;

        try
        {
            int idPedido = Convert.ToInt32(dgvPedidos.Rows[e.RowIndex].Cells["IdPedido"].Value);

            // Cargar el pedido en el formulario para edición
            PedidoDAO dao = new PedidoDAO();
            PedidoPizza pedido = dao.ObtenerPedidoPorId(idPedido);

            if (pedido != null)
            {
                CargarPedidoEnFormulario(pedido);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al seleccionar pedido: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// Carga los datos de un pedido en los controles del formulario
    private void CargarPedidoEnFormulario(PedidoPizza pedido)
    {
        txtCliente.Text = pedido.Cliente;
        txtTelefono.Text = pedido.Telefono;

        // Seleccionar en los combos
        foreach (var item in cboTamano.Items)
        {
            var kvp = (KeyValuePair<string, int>)item;
            if (kvp.Key == pedido.TamanoNombre)
            {
                cboTamano.SelectedItem = item;
                break;
            }
        }

        foreach (var item in cboMasa.Items)
        {
            var kvp = (KeyValuePair<string, int>)item;
            if (kvp.Key == pedido.MasaNombre)
            {
                cboMasa.SelectedItem = item;
                break;
            }
        }

        // Cargar checkboxes
        chkQuesoExtra.Checked = pedido.QuesoExtra;
        chkPepperoni.Checked = pedido.Pepperoni;
        chkChampinones.Checked = pedido.Champinones;
        chkAceitunas.Checked = pedido.Aceitunas;
        chkTocino.Checked = pedido.Tocino;

        // Cargar opciones de entrega
        if (pedido.EsDelivery)
            rdbDelivery.Checked = true;
        else
            rdbRetiro.Checked = true;

        chkConvenioEstudiante.Checked = pedido.AplicaConvenioEstudiante;

        // Guardar el ID en una propiedad temporal (agregar al final de la clase)
        formularioPedidoActualId = pedido.IdPedido;

        // Mostrar los totales
        lblSubtotalValor.Text = $"$ {pedido.CalcularSubtotalPizza():N0} CLP";
        lblDescuentoValor.Text = $"- $ {pedido.CalcularMontoDescuento():N0} CLP";
        lblDeliveryValor.Text = $"+ $ {pedido.CalcularCostoDelivery():N0} CLP";
        lblIvaValor.Text = $"+ $ {pedido.CalcularMontoIva():N0} CLP";
        lblTotalValor.Text = $"$ {pedido.CalcularTotalPagar():N0} CLP";

        grpResumen.Enabled = true;
    }

    /// Evento del botón Editar
    private void BtnEditar_Click(object sender, EventArgs e)
    {
        if (dgvPedidos.SelectedRows.Count == 0)
        {
            MessageBox.Show("Seleccione un pedido de la tabla para editar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show("Puede editar los datos del pedido seleccionado y luego hacer clic en Grabar para guardar los cambios.", "Modo Edición", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    /// <summary>
    /// Evento del botón Eliminar
    /// </summary>
    private void BtnEliminar_Click(object sender, EventArgs e)
    {
        if (dgvPedidos.SelectedRows.Count == 0)
        {
            MessageBox.Show("Seleccione un pedido de la tabla para eliminar", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            int idPedido = Convert.ToInt32(dgvPedidos.SelectedRows[0].Cells["IdPedido"].Value);
            string cliente = dgvPedidos.SelectedRows[0].Cells["Cliente"].Value.ToString();

            DialogResult confirmacion = MessageBox.Show(
                $"¿Está seguro de que desea eliminar el pedido de {cliente}?",
                "Confirmar Eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmacion == DialogResult.Yes)
            {
                PedidoDAO dao = new PedidoDAO();
                if (dao.EliminarPedido(idPedido))
                {
                    MessageBox.Show("Pedido eliminado exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarDataGridPedidos();
                    LimpiarFormulario();
                    formularioPedidoActualId = 0;
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el pedido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Evento del botón Grabar (guardar cambios o nuevo)
    /// </summary>
    private void BtnGrabar_Click(object sender, EventArgs e)
    {
        if (!ValidaInputs()) return;

        try
        {
            // Crear objeto pedido con datos del formulario
            PedidoPizza pedido = new()
            {
                IdPedido = formularioPedidoActualId,
                Cliente = txtCliente.Text.Trim(),
                Telefono = txtTelefono.Text.Trim(),
                TamanoNombre = cboTamano.Text,
                PrecioBaseTamano = Convert.ToInt32(cboTamano.SelectedValue),
                MasaNombre = cboMasa.Text,
                RecargoMasa = Convert.ToInt32(cboMasa.SelectedValue),
                QuesoExtra = chkQuesoExtra.Checked,
                Pepperoni = chkPepperoni.Checked,
                Champinones = chkChampinones.Checked,
                Aceitunas = chkAceitunas.Checked,
                Tocino = chkTocino.Checked,
                EsDelivery = rdbDelivery.Checked,
                AplicaConvenioEstudiante = chkConvenioEstudiante.Checked
            };

            PedidoDAO dao = new PedidoDAO();

            if (formularioPedidoActualId == 0)
            {
                // Nuevo pedido
                if (dao.InsertarPedido(pedido))
                {
                    MessageBox.Show("Pedido guardado exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarDataGridPedidos();
                    LimpiarFormulario();
                    formularioPedidoActualId = 0;
                }
                else
                {
                    MessageBox.Show("No se pudo guardar el pedido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Actualizar pedido existente
                if (dao.ActualizarPedido(pedido))
                {
                    MessageBox.Show("Pedido actualizado exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    CargarDataGridPedidos();
                    LimpiarFormulario();
                    formularioPedidoActualId = 0;
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el pedido", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // Variable para almacenar el ID del pedido actual (0 = nuevo, >0 = edición)
    private int formularioPedidoActualId = 0;
}