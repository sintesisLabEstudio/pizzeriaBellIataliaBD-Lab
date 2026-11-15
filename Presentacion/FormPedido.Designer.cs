namespace PizzeriaBellaItalia.Presentacion
{
    // Clase parcial generada/definida para la construcción visual del formulario.
    // Aquí se crean, configuran y posicionan los controles (UI). La lógica de eventos
    // y el flujo de negocio/CRUD se implementan en FormPedido.cs.
    partial class FormPedido
    {
        // Contenedor de componentes para liberar recursos administrados cuando corresponda.
        private System.ComponentModel.IContainer components = null;

        // Libera recursos del formulario y de sus componentes.
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        // Construye la interfaz gráfica:
        // 1) Instancia controles.
        // 2) Configura propiedades (texto, tamaño, posición, eventos).
        // 3) Compone la jerarquía visual (controles dentro de grupos/formulario).
        private void InitializeComponent()
        {
            // --- Instanciación de controles ---
            // Grupo de configuración de una pizza (tamaño, masa, ingredientes, cantidad).
            grpConfigPizza = new GroupBox();
            lblPizza = new Label();
            lblTamano = new Label();
            cboTamano = new ComboBox();
            lblMasa = new Label();
            cboMasa = new ComboBox();
            lblIngredientes = new Label();
            chkQuesoExtra = new CheckBox();
            chkPepperoni = new CheckBox();
            chkChampinones = new CheckBox();
            chkAceitunas = new CheckBox();
            chkTocino = new CheckBox();
            lblCant = new Label();
            numCantidad = new NumericUpDown();
            btnAgregar = new Button();

            // Grupo de datos del cliente/pedido y totales generales.
            grpDatosCliente = new GroupBox();
            lblCliente = new Label();
            txtCliente = new TextBox();
            lblTelefono = new Label();
            txtTelefono = new TextBox();
            lblFecha = new Label();
            dtpFecha = new DateTimePicker();
            lblHora = new Label();
            dtpHora = new DateTimePicker();
            chkDelivery = new CheckBox();
            chkDescuento = new CheckBox();

            lblST = new Label();
            lblSubtotalValor = new Label();
            lblIVA = new Label();
            lblIvaValor = new Label();
            lblTot = new Label();
            lblTotalValor = new Label();

            // Sección de detalle del pedido en edición y listado de pedidos guardados.
            lblDetalleTitulo = new Label();
            dgvDetalleItem = new DataGridView();
            btnQuitar = new Button();
            lblPedidosTitulo = new Label();
            dgvPedidos = new DataGridView();

            // Botones de acciones CRUD / navegación.
            btnGuardar = new Button();
            btnEliminar = new Button();
            btnLimpiar = new Button();
            btnSalir = new Button();

            // Suspende layout para aplicar cambios de forma eficiente antes del render final.
            grpConfigPizza.SuspendLayout();
            grpDatosCliente.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvDetalleItem).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvPedidos).BeginInit();
            SuspendLayout();

            // --- Grupo: configuración de pizza ---
            // Contiene controles para definir la línea que se agregará al detalle.
            grpConfigPizza.Controls.Add(lblPizza);
            grpConfigPizza.Controls.Add(lblTamano);
            grpConfigPizza.Controls.Add(cboTamano);
            grpConfigPizza.Controls.Add(lblMasa);
            grpConfigPizza.Controls.Add(cboMasa);
            grpConfigPizza.Controls.Add(lblIngredientes);
            grpConfigPizza.Controls.Add(chkQuesoExtra);
            grpConfigPizza.Controls.Add(chkPepperoni);
            grpConfigPizza.Controls.Add(chkChampinones);
            grpConfigPizza.Controls.Add(chkAceitunas);
            grpConfigPizza.Controls.Add(chkTocino);
            grpConfigPizza.Controls.Add(lblCant);
            grpConfigPizza.Controls.Add(numCantidad);
            grpConfigPizza.Controls.Add(btnAgregar);
            grpConfigPizza.Location = new Point(12, 12);
            grpConfigPizza.Name = "grpConfigPizza";
            grpConfigPizza.Size = new Size(310, 205);
            grpConfigPizza.TabIndex = 0;
            grpConfigPizza.TabStop = false;
            grpConfigPizza.Text = "Configurar pizza";

            lblPizza.AutoSize = true; lblPizza.Location = new Point(15, 25); lblPizza.Text = "Pizza:";
            lblTamano.AutoSize = true; lblTamano.Location = new Point(15, 52); lblTamano.Text = "Tamaño:";
            cboTamano.DropDownStyle = ComboBoxStyle.DropDownList;
            cboTamano.Location = new Point(70, 48); cboTamano.Size = new Size(110, 23);

            lblMasa.AutoSize = true; lblMasa.Location = new Point(185, 52); lblMasa.Text = "Masa:";
            cboMasa.DropDownStyle = ComboBoxStyle.DropDownList;
            cboMasa.Location = new Point(225, 48); cboMasa.Size = new Size(75, 23);

            lblIngredientes.AutoSize = true; lblIngredientes.Location = new Point(15, 82); lblIngredientes.Text = "Ingredientes (+$800 c/u):";
            chkQuesoExtra.AutoSize = true; chkQuesoExtra.Location = new Point(15, 105); chkQuesoExtra.Text = "Queso Extra";
            chkPepperoni.AutoSize = true; chkPepperoni.Location = new Point(110, 105); chkPepperoni.Text = "Pepperoni";
            chkChampinones.AutoSize = true; chkChampinones.Location = new Point(15, 128); chkChampinones.Text = "Champiñones";
            chkAceitunas.AutoSize = true; chkAceitunas.Location = new Point(110, 128); chkAceitunas.Text = "Aceitunas";
            chkTocino.AutoSize = true; chkTocino.Location = new Point(200, 105); chkTocino.Text = "Tocino";

            lblCant.AutoSize = true; lblCant.Location = new Point(200, 130); lblCant.Text = "Cant:";
            numCantidad.Location = new Point(240, 126); numCantidad.Size = new Size(60, 23);
            numCantidad.Minimum = 1; numCantidad.Maximum = 50; numCantidad.Value = 1;

            // Evento: agrega el ítem configurado al pedido en memoria (lógica en FormPedido.cs).
            btnAgregar.Location = new Point(15, 162); btnAgregar.Size = new Size(285, 30);
            btnAgregar.Text = "Agregar al pedido";
            btnAgregar.Click += btnAgregar_Click;

            // --- Grupo: datos del cliente y totales del pedido ---
            // Captura cliente/fecha/opciones generales y muestra subtotal, IVA y total.
            grpDatosCliente.Controls.Add(lblCliente);
            grpDatosCliente.Controls.Add(txtCliente);
            grpDatosCliente.Controls.Add(lblTelefono);
            grpDatosCliente.Controls.Add(txtTelefono);
            grpDatosCliente.Controls.Add(lblFecha);
            grpDatosCliente.Controls.Add(dtpFecha);
            grpDatosCliente.Controls.Add(lblHora);
            grpDatosCliente.Controls.Add(dtpHora);
            grpDatosCliente.Controls.Add(chkDelivery);
            grpDatosCliente.Controls.Add(chkDescuento);
            grpDatosCliente.Controls.Add(lblST);
            grpDatosCliente.Controls.Add(lblSubtotalValor);
            grpDatosCliente.Controls.Add(lblIVA);
            grpDatosCliente.Controls.Add(lblIvaValor);
            grpDatosCliente.Controls.Add(lblTot);
            grpDatosCliente.Controls.Add(lblTotalValor);
            grpDatosCliente.Location = new Point(334, 12);
            grpDatosCliente.Name = "grpDatosCliente";
            grpDatosCliente.Size = new Size(310, 205);
            grpDatosCliente.TabIndex = 1;
            grpDatosCliente.TabStop = false;
            grpDatosCliente.Text = "Pedido & Cliente";

            lblCliente.AutoSize = true; lblCliente.Location = new Point(15, 22); lblCliente.Text = "Cliente:";
            txtCliente.Location = new Point(70, 18); txtCliente.Size = new Size(225, 23);

            lblTelefono.AutoSize = true; lblTelefono.Location = new Point(15, 49); lblTelefono.Text = "Teléfono:";
            txtTelefono.Location = new Point(70, 45); txtTelefono.Size = new Size(225, 23);

            lblFecha.AutoSize = true; lblFecha.Location = new Point(15, 76); lblFecha.Text = "Fecha:";
            dtpFecha.Format = DateTimePickerFormat.Short; dtpFecha.Location = new Point(60, 72); dtpFecha.Size = new Size(100, 23);

            lblHora.AutoSize = true; lblHora.Location = new Point(170, 76); lblHora.Text = "Hora:";
            dtpHora.Format = DateTimePickerFormat.Time; dtpHora.ShowUpDown = true;
            dtpHora.Location = new Point(210, 72); dtpHora.Size = new Size(85, 23);

            chkDelivery.AutoSize = true; chkDelivery.Location = new Point(15, 102); chkDelivery.Text = "Delivery (+$2.000)";
            chkDelivery.CheckedChanged += chkOpcion_CheckedChanged;
            chkDescuento.AutoSize = true; chkDescuento.Location = new Point(155, 102); chkDescuento.Text = "Descuento (10%)";
            chkDescuento.CheckedChanged += chkOpcion_CheckedChanged;

            // Etiquetas de totales calculados por el modelo/flujo de Recalcular().
            lblST.AutoSize = true; lblST.Location = new Point(15, 128); lblST.Text = "Subtotal";
            lblSubtotalValor.BorderStyle = BorderStyle.Fixed3D; lblSubtotalValor.Location = new Point(15, 146); lblSubtotalValor.Size = new Size(85, 23); lblSubtotalValor.Text = "$ 0";

            lblIVA.AutoSize = true; lblIVA.Location = new Point(112, 128); lblIVA.Text = "IVA";
            lblIvaValor.BorderStyle = BorderStyle.Fixed3D; lblIvaValor.Location = new Point(112, 146); lblIvaValor.Size = new Size(85, 23); lblIvaValor.Text = "$ 0";

            lblTot.AutoSize = true; lblTot.Location = new Point(210, 128); lblTot.Text = "Total";
            lblTotalValor.BorderStyle = BorderStyle.Fixed3D; lblTotalValor.Location = new Point(210, 146); lblTotalValor.Size = new Size(85, 23); lblTotalValor.Text = "$ 0";

            // --- Detalle del pedido actual (en memoria) ---
            lblDetalleTitulo.AutoSize = true;
            lblDetalleTitulo.Location = new Point(12, 224);
            lblDetalleTitulo.Text = "Detalle del pedido actual (clic en una línea para verla en el formulario):";

            dgvDetalleItem.Location = new Point(12, 244);
            dgvDetalleItem.Size = new Size(632, 140);
            dgvDetalleItem.Name = "dgvDetalleItem";
            dgvDetalleItem.CellClick += dgvDetalleItem_CellClick;

            btnQuitar.Location = new Point(12, 390); btnQuitar.Size = new Size(200, 26);
            btnQuitar.Text = "Quitar ítem seleccionado";
            btnQuitar.Click += btnQuitar_Click;

            // --- Pedidos guardados en base de datos ---
            lblPedidosTitulo.AutoSize = true;
            lblPedidosTitulo.Location = new Point(12, 424);
            lblPedidosTitulo.Text = "Pedidos guardados:";

            dgvPedidos.Location = new Point(12, 444);
            dgvPedidos.Size = new Size(510, 120);
            dgvPedidos.Name = "dgvPedidos";
            dgvPedidos.CellClick += dgvPedidos_CellClick;

            // --- Botones de acciones principales ---
            btnGuardar.Location = new Point(530, 444); btnGuardar.Size = new Size(114, 28); btnGuardar.Text = "Guardar pedido";
            btnGuardar.Click += btnGuardar_Click;

            btnEliminar.Location = new Point(530, 478); btnEliminar.Size = new Size(114, 28); btnEliminar.Text = "Eliminar pedido";
            btnEliminar.Click += btnEliminar_Click;

            btnLimpiar.Location = new Point(530, 512); btnLimpiar.Size = new Size(55, 28); btnLimpiar.Text = "Nuevo";
            btnLimpiar.Click += btnLimpiar_Click;

            btnSalir.Location = new Point(589, 512); btnSalir.Size = new Size(55, 28); btnSalir.Text = "Salir";
            btnSalir.Click += btnSalir_Click;

            // --- Configuración del formulario principal ---
            // Agrega todos los controles y suscribe el evento Load que inicia el flujo de la app.
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(656, 578);
            Controls.Add(grpConfigPizza);
            Controls.Add(grpDatosCliente);
            Controls.Add(lblDetalleTitulo);
            Controls.Add(dgvDetalleItem);
            Controls.Add(btnQuitar);
            Controls.Add(lblPedidosTitulo);
            Controls.Add(dgvPedidos);
            Controls.Add(btnGuardar);
            Controls.Add(btnEliminar);
            Controls.Add(btnLimpiar);
            Controls.Add(btnSalir);
            Name = "FormPedido";
            Text = "Pizzería Bella Italia — Gestión de Pedidos";
            Load += FormPedido_Load;

            // Reanuda layout y aplica ajustes finales.
            grpConfigPizza.ResumeLayout(false);
            grpConfigPizza.PerformLayout();
            grpDatosCliente.ResumeLayout(false);
            grpDatosCliente.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCantidad).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvDetalleItem).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvPedidos).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        // --- Declaración de campos de controles (referencias usadas por la lógica del formulario) ---
        private GroupBox grpConfigPizza;
        private Label lblPizza;
        private Label lblTamano;
        private ComboBox cboTamano;
        private Label lblMasa;
        private ComboBox cboMasa;
        private Label lblIngredientes;
        private CheckBox chkQuesoExtra;
        private CheckBox chkPepperoni;
        private CheckBox chkChampinones;
        private CheckBox chkAceitunas;
        private CheckBox chkTocino;
        private Label lblCant;
        private NumericUpDown numCantidad;
        private Button btnAgregar;

        private GroupBox grpDatosCliente;
        private Label lblCliente;
        private TextBox txtCliente;
        private Label lblTelefono;
        private TextBox txtTelefono;
        private Label lblFecha;
        private DateTimePicker dtpFecha;
        private Label lblHora;
        private DateTimePicker dtpHora;
        private CheckBox chkDelivery;
        private CheckBox chkDescuento;

        private Label lblST;
        private Label lblSubtotalValor;
        private Label lblIVA;
        private Label lblIvaValor;
        private Label lblTot;
        private Label lblTotalValor;

        private Label lblDetalleTitulo;
        private DataGridView dgvDetalleItem;
        private Button btnQuitar;
        private Label lblPedidosTitulo;
        private DataGridView dgvPedidos;

        private Button btnGuardar;
        private Button btnEliminar;
        private Button btnLimpiar;
        private Button btnSalir;
    }
}
