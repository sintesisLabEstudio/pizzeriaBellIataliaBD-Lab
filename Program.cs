using System;
using System.Windows.Forms;
using PizzeriaBellaItalia.Presentacion;

namespace PizzeriaBellaItalia;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new FormPedido());
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error crítico en la aplicación:\n\n{ex.Message}\n\n{ex.StackTrace}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}