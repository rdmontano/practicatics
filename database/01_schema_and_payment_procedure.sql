CREATE TABLE dbo.Facturas
(
    FacturaId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Facturas PRIMARY KEY,
    CodigoCliente NVARCHAR(30) NOT NULL,
    Total DECIMAL(12,2) NOT NULL CONSTRAINT CK_Facturas_Total CHECK (Total > 0),
    Estado NVARCHAR(20) NOT NULL CONSTRAINT DF_Facturas_Estado DEFAULT N'Pendiente',
    FechaPago DATETIME2 NULL,
    PagoId UNIQUEIDENTIFIER NULL,
    RowVersion ROWVERSION NOT NULL,
    CONSTRAINT CK_Facturas_Estado CHECK (Estado IN (N'Pendiente', N'Pagada'))
);
GO

CREATE TABLE dbo.Pagos
(
    PagoId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Pagos PRIMARY KEY,
    FacturaId UNIQUEIDENTIFIER NOT NULL,
    Valor DECIMAL(12,2) NOT NULL CONSTRAINT CK_Pagos_Valor CHECK (Valor > 0),
    Metodo NVARCHAR(30) NOT NULL,
    Referencia NVARCHAR(80) NOT NULL,
    FechaRegistro DATETIME2 NOT NULL CONSTRAINT DF_Pagos_Fecha DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Pagos_Referencia UNIQUE (Referencia),
    CONSTRAINT FK_Pagos_Facturas FOREIGN KEY (FacturaId) REFERENCES dbo.Facturas(FacturaId)
);
GO

CREATE OR ALTER PROCEDURE dbo.usp_RegistrarPago
    @FacturaId UNIQUEIDENTIFIER,
    @Valor DECIMAL(12,2),
    @Metodo NVARCHAR(30),
    @Referencia NVARCHAR(80)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @Total DECIMAL(12,2), @Estado NVARCHAR(20), @PagoId UNIQUEIDENTIFIER = NEWID();

    SELECT @Total = Total, @Estado = Estado
    FROM dbo.Facturas WITH (UPDLOCK, HOLDLOCK)
    WHERE FacturaId = @FacturaId;

    IF @Total IS NULL
        THROW 51001, 'La factura indicada no existe.', 1;
    IF @Estado = N'Pagada'
        THROW 51002, 'La factura ya se encuentra pagada.', 1;
    IF ROUND(@Valor, 2) <> @Total
        THROW 51003, 'El valor recibido no coincide con el saldo.', 1;
    IF EXISTS (SELECT 1 FROM dbo.Pagos WHERE Referencia = @Referencia)
        THROW 51004, 'La referencia del pago ya fue registrada.', 1;

    INSERT INTO dbo.Pagos (PagoId, FacturaId, Valor, Metodo, Referencia)
    VALUES (@PagoId, @FacturaId, @Valor, @Metodo, UPPER(LTRIM(RTRIM(@Referencia))));

    UPDATE dbo.Facturas
       SET Estado = N'Pagada', FechaPago = SYSUTCDATETIME(), PagoId = @PagoId
     WHERE FacturaId = @FacturaId;

    COMMIT TRANSACTION;

    SELECT @PagoId AS PagoId,
           CONCAT(N'REC-', CONVERT(char(8), GETUTCDATE(), 112), N'-', LEFT(REPLACE(CONVERT(nvarchar(36), @PagoId), N'-', N''), 8)) AS Comprobante;
END;
GO

