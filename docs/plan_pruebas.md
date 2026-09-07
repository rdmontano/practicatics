# Plan de pruebas - Gestión de pagos y recaudación

## Alcance

Se verificará que SYSAQUA consulte facturas pendientes, registre únicamente
pagos válidos, impida pagos duplicados, actualice la factura a Pagada y devuelva
un número de comprobante verificable.

## Criterio de aprobación

El módulo se considera aprobado cuando el 100 % de los casos críticos y de
aceptación pasan, no existen defectos bloqueantes, el build termina sin errores
y el pipeline CI ejecuta las pruebas automáticamente en cada push y pull request.

## Casos de prueba

| ID | Nivel | Entrada y pasos | Resultado esperado | Trazabilidad | Prioridad |
|---|---|---|---|---|---|
| UT-01 | Unitaria | Crear factura pendiente por $42,75; registrar pago por $42,75 con referencia única. | Se crea un pago, la factura queda Pagada y se genera un comprobante `REC-*`. | RF-09, RF-10 | **Crítica** |
| UT-02 | Unitaria | Crear factura por $30,00; intentar pagar $29,00. | Se rechaza con `AMOUNT_MISMATCH`; no se guarda pago y la factura continúa Pendiente. | RF-09 | Alta |
| UT-03 | Unitaria | Pagar una factura y repetir el intento. | El segundo intento se rechaza con `INVOICE_ALREADY_PAID`; permanece un solo pago. | RF-09 | Alta |
| IT-01 | Integración | Enviar `POST /api/payments` con la factura semilla y luego consultar `GET /api/invoices/pending`. | La API responde 201 y la factura ya no aparece entre las pendientes. | RF-08, RF-09, RF-10 | **Crítica** |
| IT-02 | Integración | Enviar `POST /api/payments` con un identificador inexistente; consultar pendientes. | La API responde 400 y no altera las facturas existentes. | RF-09 | Media |
| AT-01 | Aceptación | El recaudador selecciona una factura pendiente, ingresa método y referencia y confirma el valor total. | Ve confirmación de pago, número de comprobante y estado Pagada; la factura desaparece de pendientes. | RF-08, RF-09, RF-10 | **Crítica** |

## Automatización

Se automatizaron las tres pruebas unitarias y las dos pruebas de integración con
xUnit. El caso prioritario es **UT-01**, porque un fallo puede registrar dinero
sin cerrar la factura o cerrar una factura sin conservar el pago.

