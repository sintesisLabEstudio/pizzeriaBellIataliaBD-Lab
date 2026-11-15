using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace PizzeriaBellaItalia.Dominio
{
    // Representa una línea/detalle del pedido (una configuración de pizza y su cantidad).
    // Contiene tanto datos de configuración como cálculos de precios por línea.
    public class ItemPedido
    {
        // Constantes de negocio usadas para calcular precios por ítem.
        private const double PRECIO_INGREDIENTE_EXTRA = 800.0;
        private const double TASA_DESCUENTO = 0.10;
        private const double TASA_IVA = 0.19;

        // ---- Propiedades visibles en la grilla (BindingList/DataGridView) ----
        // Cantidad de pizzas iguales en esta línea.
        public int Cantidad { get; set; } = 1;

        // Texto descriptivo de la línea (se calcula dinámicamente para mostrar en la UI).
        public string Resumen => ConstruirResumen();

        // Precio de una unidad: precio base por tamaño + recargo por masa + extras seleccionados.
        public double PrecioUnitario => PrecioBase(Tamano) + Recargo(TipoMasa) + ContarIngredientes() * PRECIO_INGREDIENTE_EXTRA;

        // Subtotal de la línea sin descuento/IVA (precio unitario * cantidad).
        public double Subtotal => PrecioUnitario * Cantidad;

        // Descuento aplicado a la línea (si corresponde al pedido/convenio).
        public double Descuento => AplicaDescuento ? Subtotal * TASA_DESCUENTO : 0.0;

        // IVA calculado sobre (subtotal - descuento) de la línea.
        public double Iva => (Subtotal - Descuento) * TASA_IVA;

        // Indicador textual de delivery para mostrar en la grilla.
        public string Delivery => EsDelivery ? "Sí" : "No";

        // Total final de la línea.
        public double TotalLinea => Subtotal - Descuento + Iva;

        // ---- Datos internos de configuración (se ocultan en la grilla con [Browsable(false)]) ----
        // Flags de contexto del pedido que afectan el cálculo mostrado por línea.
        [Browsable(false)] public bool AplicaDescuento { get; set; }
        [Browsable(false)] public bool EsDelivery { get; set; }

        // Configuración de la pizza.
        [Browsable(false)] public string Tamano { get; set; } = "Mediana";
        [Browsable(false)] public string TipoMasa { get; set; } = "Tradicional";
        [Browsable(false)] public bool QuesoExtra { get; set; }
        [Browsable(false)] public bool Pepperoni { get; set; }
        [Browsable(false)] public bool Champinones { get; set; }
        [Browsable(false)] public bool Aceitunas { get; set; }
        [Browsable(false)] public bool Tocino { get; set; }

        // Devuelve el precio base según tamaño de pizza.
        public static double PrecioBase(string tamano) => tamano switch
        {
            "Personal" => 5000.0,
            "Familiar" => 12000.0,
            _ => 8500.0 // Mediana u otro valor no reconocido
        };

        // Aplica recargo si la masa corresponde a borde de queso.
        public static double Recargo(string masa) =>
            masa.Contains("Borde", StringComparison.OrdinalIgnoreCase) ? 1500.0 : 0.0;

        // Cuenta cuántos ingredientes extra fueron seleccionados.
        public int ContarIngredientes() =>
            (QuesoExtra ? 1 : 0) + (Pepperoni ? 1 : 0) + (Champinones ? 1 : 0) +
            (Aceitunas ? 1 : 0) + (Tocino ? 1 : 0);

        // Construye un resumen legible para la grilla/detalle del pedido.
        private string ConstruirResumen()
        {
            var extras = new List<string>();
            if (QuesoExtra) extras.Add("Queso Extra");
            if (Pepperoni) extras.Add("Pepperoni");
            if (Champinones) extras.Add("Champiñones");
            if (Aceitunas) extras.Add("Aceitunas");
            if (Tocino) extras.Add("Tocino");

            string ingredientes = extras.Count > 0
                ? "con " + string.Join(", ", extras)
                : "sin ingredientes extra";

            return $"{Cantidad} x Pizza {Tamano}, masa {TipoMasa}, {ingredientes}";
        }

        /// <summary>
        /// Determina si otro ítem tiene la misma configuración de pizza.
        /// Se usa en la UI para sumar cantidades en una misma línea en lugar de duplicarla.
        /// </summary>
        public bool MismaConfiguracion(ItemPedido o) =>
            Tamano == o.Tamano && TipoMasa == o.TipoMasa && QuesoExtra == o.QuesoExtra &&
            Pepperoni == o.Pepperoni && Champinones == o.Champinones &&
            Aceitunas == o.Aceitunas && Tocino == o.Tocino;
    }

    /// <summary>
    /// Representa el pedido completo (cabecera): datos del cliente, opciones generales y lista de ítems.
    /// También centraliza los cálculos de totales a nivel de pedido.
    /// </summary>
    public class PedidoPizza
    {
        // Constantes de negocio a nivel de pedido.
        private const double RECARGO_DELIVERY = 2000.0;
        private const double TASA_DESCUENTO = 0.10;
        private const double TASA_IVA = 0.19;

        // Identificador en base de datos (0 o null-equivalente lógico antes de insertar).
        public int IdPedido { get; set; }

        // Fecha y hora del pedido.
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Datos del cliente asociados al pedido.
        public string Cliente { get; set; } = string.Empty;
        public string Telefono { get; set; } = "Sin Teléfono";

        // Opciones generales del pedido.
        public bool EsDelivery { get; set; }
        public bool AplicaConvenioEstudiante { get; set; }

        // Detalle: colección de líneas/pizzas del pedido.
        public List<ItemPedido> Items { get; set; } = new();

        // Suma de subtotales de todas las líneas.
        public double CalcularSubtotalPizza() => Items.Sum(i => i.Subtotal);

        // Descuento global del pedido (si aplica convenio).
        public double CalcularMontoDescuento() =>
            AplicaConvenioEstudiante ? CalcularSubtotalPizza() * TASA_DESCUENTO : 0.0;

        // Costo fijo de delivery si el pedido es con despacho.
        public double CalcularCostoDelivery() => EsDelivery ? RECARGO_DELIVERY : 0.0;

        // Base imponible/neta antes de IVA (según regla definida por el modelo).
        public double CalcularNetoAfecto() =>
            CalcularSubtotalPizza() - CalcularMontoDescuento() + CalcularCostoDelivery();

        // IVA del pedido.
        public double CalcularMontoIva() => CalcularNetoAfecto() * TASA_IVA;

        // Total final a pagar.
        public double CalcularTotalPagar() => CalcularNetoAfecto() + CalcularMontoIva();
    }
}
