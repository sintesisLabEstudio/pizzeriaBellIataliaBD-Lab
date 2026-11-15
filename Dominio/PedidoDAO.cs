using System.Data;
using Microsoft.Data.SqlClient;

namespace PizzeriaBellaItalia.Dominio;

// DAO (Data Access Object): encapsula el acceso a datos y las operaciones CRUD contra SQL Server.
// Flujo general:
// - La UI construye objetos de dominio (PedidoPizza/ItemPedido).
// - El DAO traduce esos objetos a comandos SQL parametrizados.
// - Las operaciones de escritura usan transacciones para mantener consistencia entre tablas.
public class PedidoDAO
{
    // Convierte valores monetarios de double (modelo en memoria/UI) a decimal redondeado
    // para persistirlos de forma consistente en columnas DECIMAL de SQL Server.
    private static decimal Money(double value) => Math.Round((decimal)value, 0, MidpointRounding.AwayFromZero);

    // Busca un cliente por (Nombre, Telefono). Si no existe, lo crea y devuelve su Id.
    // Se ejecuta dentro de la transacción del pedido para que cliente+pedido queden consistentes.
    private static int ObtenerOCrearCliente(SqlConnection con, SqlTransaction tx, string nombre, string telefono)
    {
        // Normalización básica de datos de entrada.
        nombre = (nombre ?? string.Empty).Trim();
        telefono = string.IsNullOrWhiteSpace(telefono) ? "Sin Teléfono" : telefono.Trim();

        // 1) Intento de búsqueda de cliente existente con la misma identidad básica.
        using (var buscar = new SqlCommand(
            @"SELECT TOP 1 IdCliente FROM dbo.Clientes
              WHERE Nombre = @N AND Telefono = @T
              ORDER BY IdCliente DESC", con, tx))
        {
            buscar.Parameters.Add(new SqlParameter("@N", SqlDbType.NVarChar, 100) { Value = nombre });
            buscar.Parameters.Add(new SqlParameter("@T", SqlDbType.NVarChar, 30) { Value = telefono });
            var existente = buscar.ExecuteScalar();
            if (existente is not null && existente != DBNull.Value)
                return Convert.ToInt32(existente);
        }

        // 2) Si no existe, se inserta y se recupera el Id generado (IDENTITY).
        using var crear = new SqlCommand(
            @"INSERT INTO dbo.Clientes (Nombre, Telefono)
              VALUES (@N, @T);
              SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx);
        crear.Parameters.Add(new SqlParameter("@N", SqlDbType.NVarChar, 100) { Value = nombre });
        crear.Parameters.Add(new SqlParameter("@T", SqlDbType.NVarChar, 30) { Value = telefono });
        return Convert.ToInt32(crear.ExecuteScalar());
    }

    // Carga los parámetros comunes de la cabecera del pedido (tabla Pedidos) en un SqlCommand.
    // Evita duplicar código entre INSERT y UPDATE.
    private static void CargarParametrosPedido(SqlCommand cmd, PedidoPizza p, int idCliente)
    {
        cmd.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int) { Value = idCliente });
        cmd.Parameters.Add(new SqlParameter("@FechaHora", SqlDbType.DateTime2) { Value = p.FechaCreacion });
        cmd.Parameters.Add(new SqlParameter("@EsDelivery", SqlDbType.Bit) { Value = p.EsDelivery });
        cmd.Parameters.Add(new SqlParameter("@Convenio", SqlDbType.Bit) { Value = p.AplicaConvenioEstudiante });
        cmd.Parameters.Add(new SqlParameter("@Subtotal", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = Money(p.CalcularSubtotalPizza()) });
        cmd.Parameters.Add(new SqlParameter("@Descuento", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = Money(p.CalcularMontoDescuento()) });
        cmd.Parameters.Add(new SqlParameter("@DeliveryCosto", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = Money(p.CalcularCostoDelivery()) });
        cmd.Parameters.Add(new SqlParameter("@Iva", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = Money(p.CalcularMontoIva()) });
        cmd.Parameters.Add(new SqlParameter("@Total", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = Money(p.CalcularTotalPagar()) });
    }

    // Inserta las líneas del pedido en dbo.DetallePedidos.
    // Se llama dentro de una transacción junto con la cabecera para mantener atomicidad.
    private static void InsertarDetalles(SqlConnection con, SqlTransaction tx, int idPedido, IEnumerable<ItemPedido> items)
    {
        const string sql = @"
INSERT INTO dbo.DetallePedidos
    (IdPedido, Tamano, TipoMasa, QuesoExtra, Pepperoni, Champinones, Aceitunas, Tocino, Cantidad, PrecioUnitario, SubtotalItem, Resumen)
VALUES
    (@IdPedido, @Tamano, @TipoMasa, @QuesoExtra, @Pepperoni, @Champinones, @Aceitunas, @Tocino, @Cantidad, @PrecioUnitario, @SubtotalItem, @Resumen)";

        foreach (var it in items)
        {
            using var cmd = new SqlCommand(sql, con, tx);
            cmd.Parameters.Add(new SqlParameter("@IdPedido", SqlDbType.Int) { Value = idPedido });
            cmd.Parameters.Add(new SqlParameter("@Tamano", SqlDbType.NVarChar, 50) { Value = it.Tamano });
            cmd.Parameters.Add(new SqlParameter("@TipoMasa", SqlDbType.NVarChar, 50) { Value = it.TipoMasa });
            cmd.Parameters.Add(new SqlParameter("@QuesoExtra", SqlDbType.Bit) { Value = it.QuesoExtra });
            cmd.Parameters.Add(new SqlParameter("@Pepperoni", SqlDbType.Bit) { Value = it.Pepperoni });
            cmd.Parameters.Add(new SqlParameter("@Champinones", SqlDbType.Bit) { Value = it.Champinones });
            cmd.Parameters.Add(new SqlParameter("@Aceitunas", SqlDbType.Bit) { Value = it.Aceitunas });
            cmd.Parameters.Add(new SqlParameter("@Tocino", SqlDbType.Bit) { Value = it.Tocino });
            cmd.Parameters.Add(new SqlParameter("@Cantidad", SqlDbType.Int) { Value = Math.Max(1, it.Cantidad) });
            cmd.Parameters.Add(new SqlParameter("@PrecioUnitario", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = Money(it.PrecioUnitario) });
            cmd.Parameters.Add(new SqlParameter("@SubtotalItem", SqlDbType.Decimal) { Precision = 18, Scale = 2, Value = Money(it.Subtotal) });
            cmd.Parameters.Add(new SqlParameter("@Resumen", SqlDbType.NVarChar, 300) { Value = it.Resumen ?? string.Empty });
            cmd.ExecuteNonQuery();
        }
    }

    // CREATE: inserta un pedido completo (cliente + cabecera + detalles) en una transacción.
    public bool InsertarPedido(PedidoPizza pedido)
    {
        // Validaciones defensivas antes de tocar la base de datos.
        ArgumentNullException.ThrowIfNull(pedido);
        if (pedido.Items.Count == 0) throw new InvalidOperationException("El pedido debe contener al menos un ítem.");

        using var con = ConexionBD.ObtenerConexion();
        using var tx = con.BeginTransaction();
        try
        {
            // 1) Resolver/crear cliente y obtener su Id.
            int idCliente = ObtenerOCrearCliente(con, tx, pedido.Cliente, pedido.Telefono);

            // 2) Insertar cabecera del pedido y recuperar IdPedido generado.
            int idPedido;
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Pedidos (IdCliente, FechaHora, EsDelivery, AplicaConvenioEstudiante, Subtotal, Descuento, DeliveryCosto, Iva, Total)
VALUES (@IdCliente, @FechaHora, @EsDelivery, @Convenio, @Subtotal, @Descuento, @DeliveryCosto, @Iva, @Total);
SELECT CAST(SCOPE_IDENTITY() AS INT);", con, tx))
            {
                CargarParametrosPedido(cmd, pedido, idCliente);
                idPedido = Convert.ToInt32(cmd.ExecuteScalar());
            }

            // 3) Insertar líneas/detalle asociadas al pedido.
            InsertarDetalles(con, tx, idPedido, pedido.Items);
            pedido.IdPedido = idPedido;

            // 4) Confirmar transacción si todo fue exitoso.
            tx.Commit();
            return true;
        }
        catch
        {
            // Ante cualquier error, deshacer cambios parciales para evitar inconsistencia.
            tx.Rollback();
            throw;
        }
    }

    // UPDATE: actualiza cabecera y reemplaza completamente los detalles del pedido.
    // Estrategia: actualizar pedido -> borrar detalles anteriores -> insertar detalles actuales.
    public bool ActualizarPedido(PedidoPizza pedido)
    {
        ArgumentNullException.ThrowIfNull(pedido);
        if (pedido.IdPedido <= 0) throw new InvalidOperationException("Id de pedido inválido.");
        if (pedido.Items.Count == 0) throw new InvalidOperationException("El pedido debe contener al menos un ítem.");

        using var con = ConexionBD.ObtenerConexion();
        using var tx = con.BeginTransaction();
        try
        {
            // 1) Resolver/crear cliente (puede cambiar respecto al original).
            int idCliente = ObtenerOCrearCliente(con, tx, pedido.Cliente, pedido.Telefono);

            // 2) Actualizar cabecera del pedido.
            int filas;
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Pedidos
SET IdCliente = @IdCliente,
    FechaHora = @FechaHora,
    EsDelivery = @EsDelivery,
    AplicaConvenioEstudiante = @Convenio,
    Subtotal = @Subtotal,
    Descuento = @Descuento,
    DeliveryCosto = @DeliveryCosto,
    Iva = @Iva,
    Total = @Total
WHERE IdPedido = @IdPedido", con, tx))
            {
                CargarParametrosPedido(cmd, pedido, idCliente);
                cmd.Parameters.Add(new SqlParameter("@IdPedido", SqlDbType.Int) { Value = pedido.IdPedido });
                filas = cmd.ExecuteNonQuery();
            }

            // 3) Reemplazar detalle: borrar líneas anteriores e insertar las actuales.
            using (var cmd = new SqlCommand("DELETE FROM dbo.DetallePedidos WHERE IdPedido = @Id", con, tx))
            {
                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = pedido.IdPedido });
                cmd.ExecuteNonQuery();
            }

            InsertarDetalles(con, tx, pedido.IdPedido, pedido.Items);

            tx.Commit();
            return filas > 0;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // DELETE: elimina un pedido y sus detalles en una transacción.
    // (También existe ON DELETE CASCADE en FK, pero se eliminan detalles explícitamente por claridad/control.)
    public bool EliminarPedido(int idPedido)
    {
        if (idPedido <= 0) return false;

        using var con = ConexionBD.ObtenerConexion();
        using var tx = con.BeginTransaction();
        try
        {
            // 1) Eliminar detalles asociados.
            using (var cmd = new SqlCommand("DELETE FROM dbo.DetallePedidos WHERE IdPedido = @Id", con, tx))
            {
                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = idPedido });
                cmd.ExecuteNonQuery();
            }

            // 2) Eliminar cabecera del pedido.
            int filas;
            using (var cmd = new SqlCommand("DELETE FROM dbo.Pedidos WHERE IdPedido = @Id", con, tx))
            {
                cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = idPedido });
                filas = cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return filas > 0;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    // READ (lista): obtiene los pedidos para mostrar en la grilla inferior de la UI.
    // Incluye datos de cliente y una agregación de cantidad total de pizzas por pedido.
    public DataTable ObtenerPedidosDataTable()
    {
        const string query = @"
SELECT p.IdPedido,
       c.Nombre AS Cliente,
       c.Telefono,
       ISNULL(SUM(d.Cantidad), 0) AS Pizzas,
       p.EsDelivery AS Delivery,
       p.Total,
       p.FechaHora AS Fecha
FROM dbo.Pedidos p
INNER JOIN dbo.Clientes c ON c.IdCliente = p.IdCliente
LEFT JOIN dbo.DetallePedidos d ON d.IdPedido = p.IdPedido
GROUP BY p.IdPedido, c.Nombre, c.Telefono, p.EsDelivery, p.Total, p.FechaHora
ORDER BY p.IdPedido DESC";

        using var con = ConexionBD.ObtenerConexion();
        using var da = new SqlDataAdapter(query, con);
        var dt = new DataTable();
        da.Fill(dt);
        return dt;
    }

    // READ (detalle): carga un pedido completo por Id (cabecera + líneas) para edición en el formulario.
    public PedidoPizza? ObtenerPedidoPorId(int id)
    {
        if (id <= 0) return null;

        using var con = ConexionBD.ObtenerConexion();
        PedidoPizza? pedido = null;

        // 1) Cargar cabecera del pedido y datos del cliente.
        using (var cmd = new SqlCommand(@"
SELECT p.IdPedido, p.FechaHora, p.EsDelivery, p.AplicaConvenioEstudiante, c.Nombre, c.Telefono
FROM dbo.Pedidos p
INNER JOIN dbo.Clientes c ON c.IdCliente = p.IdCliente
WHERE p.IdPedido = @Id", con))
        {
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            pedido = new PedidoPizza
            {
                IdPedido = Convert.ToInt32(r["IdPedido"]),
                FechaCreacion = r["FechaHora"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(r["FechaHora"]),
                Cliente = r["Nombre"]?.ToString() ?? string.Empty,
                Telefono = r["Telefono"]?.ToString() ?? "Sin Teléfono",
                EsDelivery = r["EsDelivery"] != DBNull.Value && Convert.ToBoolean(r["EsDelivery"]),
                AplicaConvenioEstudiante = r["AplicaConvenioEstudiante"] != DBNull.Value && Convert.ToBoolean(r["AplicaConvenioEstudiante"])
            };
        }

        // 2) Cargar líneas/detalle del pedido en el mismo objeto de dominio.
        using (var cmd = new SqlCommand(@"
SELECT Tamano, TipoMasa, QuesoExtra, Pepperoni, Champinones, Aceitunas, Tocino, Cantidad
FROM dbo.DetallePedidos
WHERE IdPedido = @Id
ORDER BY IdDetalle", con))
        {
            cmd.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                pedido.Items.Add(new ItemPedido
                {
                    Tamano = r["Tamano"]?.ToString() ?? "Mediana",
                    TipoMasa = r["TipoMasa"]?.ToString() ?? "Tradicional",
                    QuesoExtra = r["QuesoExtra"] != DBNull.Value && Convert.ToBoolean(r["QuesoExtra"]),
                    Pepperoni = r["Pepperoni"] != DBNull.Value && Convert.ToBoolean(r["Pepperoni"]),
                    Champinones = r["Champinones"] != DBNull.Value && Convert.ToBoolean(r["Champinones"]),
                    Aceitunas = r["Aceitunas"] != DBNull.Value && Convert.ToBoolean(r["Aceitunas"]),
                    Tocino = r["Tocino"] != DBNull.Value && Convert.ToBoolean(r["Tocino"]),
                    Cantidad = r["Cantidad"] == DBNull.Value ? 1 : Convert.ToInt32(r["Cantidad"])
                });
            }
        }

        return pedido;
    }
}
