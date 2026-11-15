-- Esquema canónico para Pizzería Bella Italia
-- Compatible con Dominio/PedidoDAO.cs

IF DB_ID(N'pizzeriaBD') IS NULL
BEGIN
    CREATE DATABASE pizzeriaBD;
END
GO

USE pizzeriaBD;
GO

IF OBJECT_ID(N'dbo.DetallePedidos', N'U') IS NOT NULL DROP TABLE dbo.DetallePedidos;
IF OBJECT_ID(N'dbo.Pedidos', N'U') IS NOT NULL DROP TABLE dbo.Pedidos;
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NOT NULL DROP TABLE dbo.Clientes;
GO

CREATE TABLE dbo.Clientes
(
    IdCliente INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Telefono NVARCHAR(30) NOT NULL
);
GO

CREATE TABLE dbo.Pedidos
(
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
GO

CREATE TABLE dbo.DetallePedidos
(
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
GO

CREATE INDEX IX_Pedidos_IdCliente ON dbo.Pedidos(IdCliente);
CREATE INDEX IX_DetallePedidos_IdPedido ON dbo.DetallePedidos(IdPedido);
GO

PRINT 'Base de datos pizzeriaBD lista.';
GO
