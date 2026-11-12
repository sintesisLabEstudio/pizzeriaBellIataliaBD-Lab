using System;
using System.Data.SqlClient;

namespace PizzeriaBellaItalia.Dominio;

/// <summary>
/// Clase estática para gestionar la conexión con la base de datos pizzeriaBD.mdf
/// Patrón Singleton: garantiza una única instancia de conexión
/// </summary>
public static class ConexionBD
{
    // Cadena de conexión a SQL Server LocalDB (sin archivo MDF adjunto)
    // La base de datos PizzeriaDB se creará automáticamente si no existe
    private static readonly string _cadenaConexion =
        @"Server=(LocalDB)\MSSQLLocalDB;Database=PizzeriaDB;Integrated Security=True;Connect Timeout=30;Encrypt=False";

    /// <summary>
    /// Obtiene una nueva conexión abierta a SQL Server
    /// </summary>
    /// <returns>SqlConnection abierta y lista para usar</returns>
    public static SqlConnection ObtenerConexion()
    {
        try
        {
            SqlConnection conexion = new SqlConnection(_cadenaConexion);
            conexion.Open();
            return conexion;
        }
        catch (SqlException ex)
        {
            throw new Exception($"Error al conectar con la base de datos: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Verifica si la conexión con la base de datos está disponible
    /// </summary>
    /// <returns>true si la conexión es exitosa, false en caso contrario</returns>
    public static bool VerificarConexion()
    {
        try
        {
            using (SqlConnection conexion = ObtenerConexion())
            {
                return conexion.State == System.Data.ConnectionState.Open;
            }
        }
        catch
        {
            return false;
        }
    }
}
