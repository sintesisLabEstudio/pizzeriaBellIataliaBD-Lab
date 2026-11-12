using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace PizzeriaBellaItalia.Dominio;

/// <summary>
/// Capa de Acceso a Datos (DAO) para la tabla Pedidos
/// Implementa operaciones CRUD con queries SQL parametrizadas
/// </summary>
public class PedidoDAO
{
    // ========================================================================== CREATE
    /// <summary>
    /// Inserta un nuevo pedido en la base de datos
    /// </summary>
    public bool InsertarPedido(PedidoPizza pedido)
    {
        try
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                string query = @"INSERT INTO Pedidos 
                    (Cliente, Telefono, TamanoNombre, PrecioBaseTamano, MasaNombre, RecargoMasa,
                     QuesoExtra, Pepperoni, Champinones, Aceitunas, Tocino, EsDelivery, AplicaConvenioEstudiante,
                     CostoIngredientes, SubtotalPizza, MontoDescuento, CostoDelivery, NetoAfecto, MontoIva, TotalPagar)
                    VALUES 
                    (@Cliente, @Telefono, @TamanoNombre, @PrecioBaseTamano, @MasaNombre, @RecargoMasa,
                     @QuesoExtra, @Pepperoni, @Champinones, @Aceitunas, @Tocino, @EsDelivery, @AplicaConvenioEstudiante,
                     @CostoIngredientes, @SubtotalPizza, @MontoDescuento, @CostoDelivery, @NetoAfecto, @MontoIva, @TotalPagar)";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    // Parámetros de entrada
                    cmd.Parameters.AddWithValue("@Cliente", pedido.Cliente);
                    cmd.Parameters.AddWithValue("@Telefono", pedido.Telefono);
                    cmd.Parameters.AddWithValue("@TamanoNombre", pedido.TamanoNombre);
                    cmd.Parameters.AddWithValue("@PrecioBaseTamano", pedido.PrecioBaseTamano);
                    cmd.Parameters.AddWithValue("@MasaNombre", pedido.MasaNombre);
                    cmd.Parameters.AddWithValue("@RecargoMasa", pedido.RecargoMasa);

                    // Booleanos como BIT
                    cmd.Parameters.AddWithValue("@QuesoExtra", pedido.QuesoExtra ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Pepperoni", pedido.Pepperoni ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Champinones", pedido.Champinones ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Aceitunas", pedido.Aceitunas ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Tocino", pedido.Tocino ? 1 : 0);
                    cmd.Parameters.AddWithValue("@EsDelivery", pedido.EsDelivery ? 1 : 0);
                    cmd.Parameters.AddWithValue("@AplicaConvenioEstudiante", pedido.AplicaConvenioEstudiante ? 1 : 0);

                    // Cálculos
                    cmd.Parameters.AddWithValue("@CostoIngredientes", pedido.CalcularCostoIngredientes());
                    cmd.Parameters.AddWithValue("@SubtotalPizza", pedido.CalcularSubtotalPizza());
                    cmd.Parameters.AddWithValue("@MontoDescuento", pedido.CalcularMontoDescuento());
                    cmd.Parameters.AddWithValue("@CostoDelivery", pedido.CalcularCostoDelivery());
                    cmd.Parameters.AddWithValue("@NetoAfecto", pedido.CalcularNetoAfecto());
                    cmd.Parameters.AddWithValue("@MontoIva", pedido.CalcularMontoIva());
                    cmd.Parameters.AddWithValue("@TotalPagar", pedido.CalcularTotalPagar());

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error SQL al insertar pedido: {ex.Message}");
            return false;
        }
    }

    // ========================================================================== READ - OBTENER TODOS
    /// <summary>
    /// Obtiene todos los pedidos como DataTable para vincular a DataGrid
    /// </summary>
    public DataTable ObtenerPedidosDataTable()
    {
        DataTable dt = new DataTable();
        try
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                string query = @"SELECT IdPedido, Cliente, Telefono, TamanoNombre, TotalPagar, FechaCreacion
                                 FROM Pedidos
                                 ORDER BY IdPedido DESC";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
                {
                    adapter.Fill(dt);
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error SQL al obtener pedidos: {ex.Message}");
        }
        return dt;
    }

    /// <summary>
    /// Obtiene todos los pedidos como lista de objetos PedidoPizza
    /// </summary>
    public List<PedidoPizza> ObtenerPedidos()
    {
        List<PedidoPizza> pedidos = new List<PedidoPizza>();
        try
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                string query = @"SELECT IdPedido, Cliente, Telefono, TamanoNombre, PrecioBaseTamano, MasaNombre, RecargoMasa,
                                        QuesoExtra, Pepperoni, Champinones, Aceitunas, Tocino, EsDelivery, AplicaConvenioEstudiante,
                                        CostoIngredientes, SubtotalPizza, MontoDescuento, CostoDelivery, NetoAfecto, MontoIva, TotalPagar, FechaCreacion
                                 FROM Pedidos
                                 ORDER BY IdPedido DESC";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            pedidos.Add(MapearPedido(reader));
                        }
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error SQL al obtener lista de pedidos: {ex.Message}");
        }
        return pedidos;
    }

    // ========================================================================== READ - FILTRAR POR CLIENTE
    /// <summary>
    /// Busca pedidos por nombre de cliente (búsqueda parcial con LIKE)
    /// </summary>
    public DataTable BuscarPedidosPorCliente(string filtro)
    {
        DataTable dt = new DataTable();
        try
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                string query = @"SELECT IdPedido, Cliente, Telefono, TamanoNombre, TotalPagar, FechaCreacion
                                 FROM Pedidos
                                 WHERE Cliente LIKE @Filtro
                                 ORDER BY IdPedido DESC";

                using (SqlDataAdapter adapter = new SqlDataAdapter(query, conexion))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@Filtro", $"%{filtro}%");
                    adapter.Fill(dt);
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error SQL al buscar pedidos: {ex.Message}");
        }
        return dt;
    }

    /// <summary>
    /// Obtiene un pedido específico por ID
    /// </summary>
    public PedidoPizza ObtenerPedidoPorId(int idPedido)
    {
        PedidoPizza pedido = null;
        try
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                string query = @"SELECT IdPedido, Cliente, Telefono, TamanoNombre, PrecioBaseTamano, MasaNombre, RecargoMasa,
                                        QuesoExtra, Pepperoni, Champinones, Aceitunas, Tocino, EsDelivery, AplicaConvenioEstudiante,
                                        CostoIngredientes, SubtotalPizza, MontoDescuento, CostoDelivery, NetoAfecto, MontoIva, TotalPagar, FechaCreacion
                                 FROM Pedidos
                                 WHERE IdPedido = @IdPedido";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pedido = MapearPedido(reader);
                        }
                    }
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error SQL al obtener pedido por ID: {ex.Message}");
        }
        return pedido;
    }

    // ========================================================================== UPDATE
    /// <summary>
    /// Actualiza un pedido existente en la base de datos
    /// </summary>
    public bool ActualizarPedido(PedidoPizza pedido)
    {
        try
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                string query = @"UPDATE Pedidos SET
                    Cliente = @Cliente,
                    Telefono = @Telefono,
                    TamanoNombre = @TamanoNombre,
                    PrecioBaseTamano = @PrecioBaseTamano,
                    MasaNombre = @MasaNombre,
                    RecargoMasa = @RecargoMasa,
                    QuesoExtra = @QuesoExtra,
                    Pepperoni = @Pepperoni,
                    Champinones = @Champinones,
                    Aceitunas = @Aceitunas,
                    Tocino = @Tocino,
                    EsDelivery = @EsDelivery,
                    AplicaConvenioEstudiante = @AplicaConvenioEstudiante,
                    CostoIngredientes = @CostoIngredientes,
                    SubtotalPizza = @SubtotalPizza,
                    MontoDescuento = @MontoDescuento,
                    CostoDelivery = @CostoDelivery,
                    NetoAfecto = @NetoAfecto,
                    MontoIva = @MontoIva,
                    TotalPagar = @TotalPagar
                    WHERE IdPedido = @IdPedido";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdPedido", pedido.IdPedido);
                    cmd.Parameters.AddWithValue("@Cliente", pedido.Cliente);
                    cmd.Parameters.AddWithValue("@Telefono", pedido.Telefono);
                    cmd.Parameters.AddWithValue("@TamanoNombre", pedido.TamanoNombre);
                    cmd.Parameters.AddWithValue("@PrecioBaseTamano", pedido.PrecioBaseTamano);
                    cmd.Parameters.AddWithValue("@MasaNombre", pedido.MasaNombre);
                    cmd.Parameters.AddWithValue("@RecargoMasa", pedido.RecargoMasa);
                    cmd.Parameters.AddWithValue("@QuesoExtra", pedido.QuesoExtra ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Pepperoni", pedido.Pepperoni ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Champinones", pedido.Champinones ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Aceitunas", pedido.Aceitunas ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Tocino", pedido.Tocino ? 1 : 0);
                    cmd.Parameters.AddWithValue("@EsDelivery", pedido.EsDelivery ? 1 : 0);
                    cmd.Parameters.AddWithValue("@AplicaConvenioEstudiante", pedido.AplicaConvenioEstudiante ? 1 : 0);
                    cmd.Parameters.AddWithValue("@CostoIngredientes", pedido.CalcularCostoIngredientes());
                    cmd.Parameters.AddWithValue("@SubtotalPizza", pedido.CalcularSubtotalPizza());
                    cmd.Parameters.AddWithValue("@MontoDescuento", pedido.CalcularMontoDescuento());
                    cmd.Parameters.AddWithValue("@CostoDelivery", pedido.CalcularCostoDelivery());
                    cmd.Parameters.AddWithValue("@NetoAfecto", pedido.CalcularNetoAfecto());
                    cmd.Parameters.AddWithValue("@MontoIva", pedido.CalcularMontoIva());
                    cmd.Parameters.AddWithValue("@TotalPagar", pedido.CalcularTotalPagar());

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error SQL al actualizar pedido: {ex.Message}");
            return false;
        }
    }

    // ========================================================================== DELETE
    /// <summary>
    /// Elimina un pedido de la base de datos por su ID
    /// </summary>
    public bool EliminarPedido(int idPedido)
    {
        try
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                string query = @"DELETE FROM Pedidos WHERE IdPedido = @IdPedido";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@IdPedido", idPedido);
                    int filasAfectadas = cmd.ExecuteNonQuery();
                    return filasAfectadas > 0;
                }
            }
        }
        catch (SqlException ex)
        {
            Console.WriteLine($"Error SQL al eliminar pedido: {ex.Message}");
            return false;
        }
    }

    // ========================================================================== HELPERS
    /// <summary>
    /// Mapea una fila de SqlDataReader a un objeto PedidoPizza
    /// </summary>
    private PedidoPizza MapearPedido(SqlDataReader reader)
    {
        return new PedidoPizza
        {
            IdPedido = (int)reader["IdPedido"],
            Cliente = reader["Cliente"].ToString(),
            Telefono = reader["Telefono"].ToString(),
            TamanoNombre = reader["TamanoNombre"].ToString(),
            PrecioBaseTamano = (int)reader["PrecioBaseTamano"],
            MasaNombre = reader["MasaNombre"].ToString(),
            RecargoMasa = (int)reader["RecargoMasa"],
            QuesoExtra = (bool)reader["QuesoExtra"],
            Pepperoni = (bool)reader["Pepperoni"],
            Champinones = (bool)reader["Champinones"],
            Aceitunas = (bool)reader["Aceitunas"],
            Tocino = (bool)reader["Tocino"],
            EsDelivery = (bool)reader["EsDelivery"],
            AplicaConvenioEstudiante = (bool)reader["AplicaConvenioEstudiante"],
            FechaCreacion = (DateTime)reader["FechaCreacion"]
        };
    }
}
