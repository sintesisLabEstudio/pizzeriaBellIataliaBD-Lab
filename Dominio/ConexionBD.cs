using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace PizzeriaBellaItalia.Dominio;

// Clase utilitaria responsable de construir y entregar conexiones a SQL Server.
// Centraliza la configuración para que el resto del código (DAO, inicialización, etc.) no dependa
// de una cadena de conexión hardcodeada.
public static class ConexionBD
{
    // Cadena por defecto (fallback) usada solo si no hay configuración externa válida.
    // Apunta a LocalDB y a la base "pizzeriaBD" para facilitar ejecución en entornos de desarrollo.
    private const string DefaultConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=pizzeriaBD;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;";

    // Resuelve la cadena de conexión con la siguiente prioridad:
    // 1) Variable de entorno PIZZERIA_CONNECTION_STRING
    // 2) appsettings.json (ConnectionStrings:PizzeriaBD) junto al ejecutable
    // 3) Valor por defecto
    public static string ObtenerCadenaConexion()
    {
        // 1) Permite configurar el entorno sin modificar archivos (útil en despliegues/CI).
        var fromEnv = Environment.GetEnvironmentVariable("PIZZERIA_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        try
        {
            // 2) Intenta leer configuración desde appsettings.json copiado al directorio de salida.
            var baseDir = AppContext.BaseDirectory;
            var path = Path.Combine(baseDir, "appsettings.json");
            if (File.Exists(path))
            {
                using var stream = File.OpenRead(path);
                using var doc = JsonDocument.Parse(stream);

                // Navega el JSON: { "ConnectionStrings": { "PizzeriaBD": "..." } }
                if (doc.RootElement.TryGetProperty("ConnectionStrings", out var cs) &&
                    cs.TryGetProperty("PizzeriaBD", out var value) &&
                    value.ValueKind == JsonValueKind.String)
                {
                    var s = value.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                        return s;
                }
            }
        }
        catch
        {
            // Si falla la lectura/parsing del archivo, se continúa con el valor por defecto
            // para no bloquear la app por un error de configuración no crítico.
        }

        // 3) Fallback seguro para desarrollo local.
        return DefaultConnectionString;
    }

    // Crea el objeto SqlConnection (aún sin abrir). Útil cuando el llamador controla el ciclo de vida.
    public static SqlConnection CrearConexion() => new(ObtenerCadenaConexion());

    // Crea y abre una conexión lista para usar.
    // Importante: el llamador debe disponerla (using) para liberar recursos y cerrar la conexión.
    public static SqlConnection ObtenerConexion()
    {
        var conexion = CrearConexion();
        conexion.Open();
        return conexion;
    }
}
