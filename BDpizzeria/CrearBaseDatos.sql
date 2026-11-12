-- Crear Base de Datos Pizzería
-- Nota: Los archivos .mdf y .ldf se crean en la ruta estándar de SQL Server
-- Ubicación: C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\
IF EXISTS (SELECT * FROM sys.databases WHERE name = 'pizzeriaBD')
BEGIN
    ALTER DATABASE pizzeriaBD SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE pizzeriaBD;
END
GO

CREATE DATABASE pizzeriaBD;
GO

USE pizzeriaBD;

-- Tabla 1: Clientes
CREATE TABLE Cliente (
	IdCliente INT IDENTITY(1,1) PRIMARY KEY,
	Nombre VARCHAR(100) NOT NULL,
	Telefono VARCHAR(20) NOT NULL
);
GO

-- Tabla 2: Pedidos (Cabecera)
CREATE TABLE Pedido (
	IdPedido INT IDENTITY(1,1) PRIMARY KEY,
	IdCliente INT NOT NULL,
	FechaHora DATETIME DEFAULT GETDATE(),
	EsDelivery BIT NOT NULL,
	AplicaConvenioEstudiante BIT NOT NULL,
	Subtotal DECIMAL(18,2) NOT NULL,
	Descuento DECIMAL(18,2) NOT NULL,
	DeliveryCosto DECIMAL(18,2) NOT NULL,
	Iva DECIMAL(18,2) NOT NULL,
	Total DECIMAL(18,2) NOT NULL,
	CONSTRAINT FK_Pedido_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente(IdCliente)
);
GO

-- Tabla 3: Detalle de Pedido (Ítems / Pizzas asociadas al pedido)
CREATE TABLE DetallePedido (
	IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
	IdPedido INT NOT NULL,
	Tamano VARCHAR(50) NOT NULL,
	TipoMasa VARCHAR(50) NOT NULL,
	QuesoExtra BIT NOT NULL,
	Pepperoni BIT NOT NULL,
	Champinones BIT NOT NULL,
	Aceitunas BIT NOT NULL,
	Tocino BIT NOT NULL,
	SubtotalItem DECIMAL(18,2) NOT NULL,
	CONSTRAINT FK_Detalle_Pedido FOREIGN KEY (IdPedido) REFERENCES Pedido(IdPedido) ON DELETE CASCADE
);
GO

-- Índices para mejor rendimiento
CREATE INDEX IX_Pedido_IdCliente ON Pedido(IdCliente);
CREATE INDEX IX_DetallePedido_IdPedido ON DetallePedido(IdPedido);
GO

PRINT 'Base de datos PizzeriaBellaItaliaDB creada exitosamente';
GO
