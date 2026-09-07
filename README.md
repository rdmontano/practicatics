# SYSAQUA - Módulo de gestión de pagos y recaudación

Avance de implementación de la Unidad 3. El módulo permite consultar facturas
pendientes, registrar un pago válido, actualizar la factura a **Pagada** y
generar los datos básicos del comprobante.

## Trazabilidad con el SRS

- **RF-08:** consultar las facturas pendientes.
- **RF-09:** registrar un pago asociado a una factura pendiente.
- **RF-10:** actualizar la factura a Pagada y emitir el comprobante.

## Stack seleccionado

- C# y ASP.NET Core (.NET 8).
- SQL Server para persistencia transaccional.
- xUnit para pruebas automatizadas.
- GitHub Actions para integración continua.

La API usa repositorios en memoria para que el prototipo y las pruebas sean
repetibles. El archivo `database/01_schema_and_payment_procedure.sql` contiene
la estructura y el procedimiento transaccional previstos para SQL Server.

## Ejecutar el módulo

Requisitos: SDK de .NET 8.

```bash
dotnet restore SysAqua.Payments.sln
dotnet run --project src/SysAqua.Payments/SysAqua.Payments.csproj
```

Factura de demostración: `11111111-1111-1111-1111-111111111111`, saldo
`25.50`.

Consultar pendientes:

```text
GET /api/invoices/pending
```

Registrar pago:

```json
POST /api/payments
{
  "invoiceId": "11111111-1111-1111-1111-111111111111",
  "amount": 25.50,
  "method": "Transferencia",
  "reference": "TRX-001"
}
```

## Ejecutar pruebas

```bash
dotnet test SysAqua.Payments.sln --configuration Release
```

## Flujo de ramas

Se aplica GitHub Flow:

1. `main` conserva una versión estable.
2. Cada cambio nace desde `main` en una rama `feature/<descripcion>`.
3. Se realizan commits pequeños con mensajes claros.
4. Se abre un pull request hacia `main`.
5. El pull request solo se fusiona si el pipeline CI termina en verde.

Ejemplo de rama: `feature/registrar-pago`.

## Integración continua

El workflow `.github/workflows/ci.yml` se ejecuta con cada push y pull request.
Restaura dependencias, compila en modo Release y ejecuta todas las pruebas.

## Fuentes técnicas

- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/apis
- https://learn.microsoft.com/en-us/sql/connect/ado-net/local-transactions
- https://docs.github.com/actions/guides/building-and-testing-net
- https://github.com/actions/checkout
- https://github.com/actions/setup-dotnet
