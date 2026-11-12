using System;
using System.Data.SqlClient;

namespace PizzeriaBellaItalia.Dominio;

/// <summary>
/// Clase para inicializar la base de datos pizzeriaBD
/// Crea la base de datos y la tabla Pedidos si no existen
/// </summary>
public static class InitDatabase
{
    /// <summary>
    /// Inicializa la base de datos creando base de datos y tabla si no existen
    /// </summary>
    public static void Inicializar()
    {
        try
        {
            // Primero: Crear la base de datos si no existe
            CrearBaseDatos();

            // Segundo: Crear la tabla Pedidos si no existe
            CrearTablaPedidos();

            Console.WriteLine("✅ Base de datos inicializada correctamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error al inicializar la base de datos: {ex.Message}");
        }
    }

    /// <summary>
    /// Crea la base de datos PizzeriaDB si no existe
    /// </summary>
    private static void CrearBaseDatos()
    {
        string cadenaConexionMaster = @"Server=(LocalDB)\MSSQLLocalDB;Database=master;Integrated Security=True;Connect Timeout=30;Encrypt=False";

        try
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexionMaster))
            {
                conexion.Open();

                string sqlCrearBD = @"
                    IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'PizzeriaDB')
                    BEGIN
                        CREATE DATABASE PizzeriaDB;
                        PRINT 'Base de datos PizzeriaDB creada exitosamente';
                    END";

                using (SqlCommand cmd = new SqlCommand(sqlCrearBD, conexion))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✅ Base de datos verificada/creada");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al crear la base de datos: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Crea la tabla Pedidos si no existe
    /// </summary>
    private static void CrearTablaPedidos()
    {
        string cadenaConexion = @"Server=(LocalDB)\MSSQLLocalDB;Database=PizzeriaDB;Integrated Security=True;Connect Timeout=30;Encrypt=False";

        try
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                string sqlCrearTabla = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Pedidos')
                    BEGIN
                        CREATE TABLE Pedidos
                        (
                            IdPedido INT IDENTITY(1,1) PRIMARY KEY,
                            Cliente NVARCHAR(100) NOT NULL,
                            Telefono NVARCHAR(30) NOT NULL,
                            TamanoNombre NVARCHAR(50) NOT NULL,
                            PrecioBaseTamano INT NOT NULL,
                            MasaNombre NVARCHAR(50) NOT NULL,
                            RecargoMasa INT NOT NULL,
                            QuesoExtra BIT NOT NULL DEFAULT 0,
                            Pepperoni BIT NOT NULL DEFAULT 0,
                            Champinones BIT NOT NULL DEFAULT 0,
                            Aceitunas BIT NOT NULL DEFAULT 0,
                            Tocino BIT NOT NULL DEFAULT 0,
                            EsDelivery BIT NOT NULL DEFAULT 0,
                            AplicaConvenioEstudiante BIT NOT NULL DEFAULT 0,
                            CostoIngredientes INT NOT NULL DEFAULT 0,
                            SubtotalPizza INT NOT NULL DEFAULT 0,
                            MontoDescuento INT DEFAULT 0,
                            CostoDelivery INT DEFAULT 0,
                            NetoAfecto INT NOT NULL DEFAULT 0,
                            MontoIva INT NOT NULL DEFAULT 0,
                            TotalPagar INT NOT NULL DEFAULT 0,
                            FechaCreacion DATETIME DEFAULT GETDATE()
                        );
                        PRINT 'Tabla Pedidos creada exitosamente';
                    END";

                using (SqlCommand cmd = new SqlCommand(sqlCrearTabla, conexion))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✅ Tabla Pedidos verificada/creada");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error al crear la tabla Pedidos: {ex.Message}", ex);
        }
    }
}
