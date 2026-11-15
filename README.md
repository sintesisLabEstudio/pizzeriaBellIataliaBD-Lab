# Pizzería Bella Italia (WinForms + ADO.NET)

Aplicación de escritorio para gestión de pedidos de una pizzería, con persistencia en SQL Server mediante ADO.NET (`Microsoft.Data.SqlClient`).

## Requisitos
- Windows
- .NET SDK 10.0 (o compatible con `net10.0-windows`)
- SQL Server Express, LocalDB u otra instancia de SQL Server accesible

## Configuración de base de datos
La cadena de conexión se resuelve en este orden:
1. Variable de entorno `PIZZERIA_CONNECTION_STRING`
2. `appsettings.json` → `ConnectionStrings:PizzeriaBD`
3. Valor por defecto: LocalDB (`(localdb)\MSSQLLocalDB`, base `pizzeriaBD`)

Ejemplo (`appsettings.json`):
```json
{
  "ConnectionStrings": {
    "PizzeriaBD": "Server=.\\SQLEXPRESS;Database=pizzeriaBD;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;"
  }
}
```

### Crear/actualizar esquema
- Script canónico: `BDpizzeria/CrearBaseDatos.sql`
- Al iniciar la aplicación se intenta ejecutar `InitDatabase.Inicializar()` para crear la base y tablas si no existen (según la cadena configurada).

Tablas: `dbo.Clientes`, `dbo.Pedidos`, `dbo.DetallePedidos`.

## Ejecución
Desde la carpeta del proyecto:
```powershell
dotnet restore
dotnet build
dotnet run
```

También puede abrir `PizzeriaBellaItalia.slnx` en Visual Studio.

## Funcionalidades
- Alta, edición, eliminación y consulta de pedidos
- Detalle por ítems (tamaño, masa, ingredientes, cantidad)
- Cálculo de subtotal, descuento, delivery, IVA y total
- Persistencia transaccional de pedido y detalle

## Estructura
- `Presentacion/`: formularios WinForms
- `Dominio/`: modelo de negocio, DAO y acceso a datos
- `BDpizzeria/`: scripts SQL
- `appsettings.json`: configuración de conexión (copiado al output en build)
