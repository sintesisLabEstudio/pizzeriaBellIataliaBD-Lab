using System;
using System.Windows.Forms;
using PizzeriaBellaItalia.Presentacion;

namespace PizzeriaBellaItalia;

// Punto de entrada de la aplicación WinForms.
// Flujo general: configurar manejo global de errores -> inicializar WinForms -> abrir formulario principal.
static class Program
{
    // STAThread es requerido por componentes de Windows Forms (COM/portapapeles, controles, etc.).
    [STAThread]
    static void Main()
    {
        // Configura el modo para capturar excepciones del hilo de UI y poder mostrarlas de forma controlada.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // Captura excepciones no controladas que ocurren en el hilo de la interfaz (eventos, clicks, etc.).
        Application.ThreadException += (_, args) => MostrarError(args.Exception);

        // Captura excepciones no controladas en otros hilos/dominio de la aplicación.
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex) MostrarError(ex);
        };

        try
        {
            // Inicializa configuración moderna de la app (estilos, DPI, fuentes, etc. según .NET).
            ApplicationConfiguration.Initialize();

            // Crea y muestra el formulario principal. La app permanece activa hasta que se cierre este formulario.
            Application.Run(new FormPedido());
        }
        catch (Exception ex)
        {
            // Última red de seguridad ante errores durante el arranque o ejecución.
            MostrarError(ex);
        }
    }

    // Muestra un mensaje de error amigable al usuario sin exponer detalles técnicos sensibles (stack trace).
    private static void MostrarError(Exception ex)
    {
        MessageBox.Show(
            "Ocurrió un error inesperado en la aplicación.\n\n" + ex.Message,
            "Error",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
