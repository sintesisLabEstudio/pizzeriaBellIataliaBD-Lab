using Microsoft.Data.SqlClient;

namespace PizzeriaBellaItalia.Dominio;

/// <summary>
/// Inicializa la base de datos y las tablas requeridas por la aplicación.
/// Flujo:
/// 1) Lee la cadena de conexión configurada.
/// 2) Se conecta a "master" para crear la base si no existe.
/// 3) Se conecta a la base de la app para crear tablas/índices faltantes.
/// Esto permite que la app pueda levantar un esquema mínimo en entornos nuevos.
/// </summary>
public static class InitDatabase
{
    public static void Inicializar()
    {
        // Obtiene la configuración actual y extrae el nombre de la base a inicializar.
        var builder = new SqlConnectionStringBuilder(ConexionBD.ObtenerCadenaConexion());
        var dbName = string.IsNullOrWhiteSpace(builder.InitialCatalog) ? "pizzeriaBD" : builder.InitialCatalog;

        // Para crear la base (si no existe) hay que conectarse a una base del sistema, típicamente "master".
        var masterBuilder = new SqlConnectionStringBuilder(builder.ConnectionString)
        {
            InitialCatalog = "master"
        };

        // Paso 1: crear la base de datos si aún no existe.
        using (var master = new SqlConnection(masterBuilder.ConnectionString))
        {
            master.Open();
            using var cmd = master.CreateCommand();

            // Se sanitiza mínimamente el nombre embebido y se usa QUOTENAME para reducir riesgo de inyección
            // en el identificador de base de datos dentro de SQL dinámico.
            cmd.CommandText = $@"
IF DB_ID(N'{dbName.Replace("'", "''")}') IS NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(N'{dbName.Replace("]", "]]")}');
    EXEC(@sql);
END";
            cmd.ExecuteNonQuery();
        }

        // Paso 2: crear tablas e índices requeridos por el DAO si no están presentes.
        // La condición principal verifica la tabla de detalle para aplicar el esquema esperado.
        using var con = ConexionBD.ObtenerConexion();
        using var create = con.CreateCommand();
        create.CommandText = @"
IF OBJECT_ID(N'dbo.DetallePedidos', N'U') IS NULL
BEGIN
    -- Tabla de clientes: información básica asociada a los pedidos.
    IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Clientes (
            IdCliente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
            Nombre NVARCHAR(100) NOT NULL,
            Telefono NVARCHAR(30) NOT NULL
        );
    END

    -- Tabla de cabecera de pedidos: totales, fecha y flags del pedido.
    IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NULL
    BEGIN
        CREATE TABLE dbo.Pedidos (
            IdPedido INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pedidos PRIMARY KEY,
            IdCliente INT NOT NULL,
            FechaHora DATETIME2(0) NOT NULL CONSTRAINT DF_Pedidos_FechaHora DEFAULT (SYSDATETIME()),
            EsDelivery BIT NOT NULL CONSTRAINT DF_Pedidos_EsDelivery DEFAULT (0),
            AplicaConvenioEstudiante BIT NOT NULL CONSTRAINT DF_Pedidos_Convenio DEFAULT (0),
            Subtotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Subtotal DEFAULT (0),
            Descuento DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Descuento DEFAULT (0),
            DeliveryCosto DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_DeliveryCosto DEFAULT (0),
            Iva DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Iva DEFAULT (0),
            Total DECIMAL(18,2) NOT NULL CONSTRAINT DF_Pedidos_Total DEFAULT (0),
            CONSTRAINT FK_Pedidos_Clientes FOREIGN KEY (IdCliente) REFERENCES dbo.Clientes(IdCliente)
        );
    END

    -- Tabla de detalle: cada línea/pizza del pedido (configuración, cantidad y montos).
    CREATE TABLE dbo.DetallePedidos (
        IdDetalle INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DetallePedidos PRIMARY KEY,
        IdPedido INT NOT NULL,
        Tamano NVARCHAR(50) NOT NULL,
        TipoMasa NVARCHAR(50) NOT NULL,
        QuesoExtra BIT NOT NULL CONSTRAINT DF_Detalle_QuesoExtra DEFAULT (0),
        Pepperoni BIT NOT NULL CONSTRAINT DF_Detalle_Pepperoni DEFAULT (0),
        Champinones BIT NOT NULL CONSTRAINT DF_Detalle_Champinones DEFAULT (0),
        Aceitunas BIT NOT NULL CONSTRAINT DF_Detalle_Aceitunas DEFAULT (0),
        Tocino BIT NOT NULL CONSTRAINT DF_Detalle_Tocino DEFAULT (0),
        Cantidad INT NOT NULL CONSTRAINT DF_Detalle_Cantidad DEFAULT (1),
        PrecioUnitario DECIMAL(18,2) NOT NULL CONSTRAINT DF_Detalle_PrecioUnitario DEFAULT (0),
        SubtotalItem DECIMAL(18,2) NOT NULL CONSTRAINT DF_Detalle_SubtotalItem DEFAULT (0),
        Resumen NVARCHAR(300) NOT NULL CONSTRAINT DF_Detalle_Resumen DEFAULT (N''),
        CONSTRAINT FK_DetallePedidos_Pedidos FOREIGN KEY (IdPedido) REFERENCES dbo.Pedidos(IdPedido) ON DELETE CASCADE
    );

    -- Índices para mejorar búsquedas por claves foráneas / consultas frecuentes.
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Pedidos_IdCliente' AND object_id = OBJECT_ID('dbo.Pedidos'))
        CREATE INDEX IX_Pedidos_IdCliente ON dbo.Pedidos(IdCliente);
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DetallePedidos_IdPedido' AND object_id = OBJECT_ID('dbo.DetallePedidos'))
        CREATE INDEX IX_DetallePedidos_IdPedido ON dbo.DetallePedidos(IdPedido);
END";
        create.ExecuteNonQuery();
    }
}
