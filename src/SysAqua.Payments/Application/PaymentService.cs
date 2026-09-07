using SysAqua.Payments.Domain;

namespace SysAqua.Payments.Application;

public sealed record RegisterPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    string Method,
    string Reference);

public sealed record PaymentResult(
    Guid PaymentId,
    Guid InvoiceId,
    decimal Amount,
    string Status,
    string ReceiptNumber,
    DateTimeOffset RegisteredAt);

public sealed class PaymentService
{
    private readonly IInvoiceRepository _invoices;
    private readonly IPaymentRepository _payments;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public PaymentService(IInvoiceRepository invoices, IPaymentRepository payments)
    {
        _invoices = invoices;
        _payments = payments;
    }

    public async Task<PaymentResult> RegisterAsync(
        RegisterPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ValidateCommand(command);
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var invoice = await _invoices.GetByIdAsync(command.InvoiceId, cancellationToken)
                ?? throw new PaymentDomainException("INVOICE_NOT_FOUND", "La factura indicada no existe.");

            if (invoice.Status == InvoiceStatus.Paid)
                throw new PaymentDomainException("INVOICE_ALREADY_PAID", "La factura ya se encuentra pagada.");

            var normalizedReference = command.Reference.Trim().ToUpperInvariant();
            if (await _payments.FindByReferenceAsync(normalizedReference, cancellationToken) is not null)
                throw new PaymentDomainException("DUPLICATE_REFERENCE", "La referencia del pago ya fue registrada.");

            var normalizedAmount = decimal.Round(command.Amount, 2, MidpointRounding.AwayFromZero);
            if (normalizedAmount != invoice.Total)
                throw new PaymentDomainException(
                    "AMOUNT_MISMATCH",
                    $"El valor recibido ({normalizedAmount:0.00}) no coincide con el saldo ({invoice.Total:0.00}).");

            var now = DateTimeOffset.UtcNow;
            var payment = new Payment(
                Guid.NewGuid(),
                invoice.Id,
                normalizedAmount,
                command.Method.Trim(),
                normalizedReference,
                now);

            await _payments.AddAsync(payment, cancellationToken);
            invoice.MarkPaid(payment.Id, now);
            await _invoices.UpdateAsync(invoice, cancellationToken);

            return new PaymentResult(
                payment.Id,
                invoice.Id,
                payment.Amount,
                invoice.Status.ToString(),
                $"REC-{now:yyyyMMdd}-{payment.Id.ToString("N")[..8].ToUpperInvariant()}",
                now);
        }
        finally
        {
            _gate.Release();
        }
    }

    private static void ValidateCommand(RegisterPaymentCommand command)
    {
        if (command.InvoiceId == Guid.Empty)
            throw new PaymentDomainException("INVALID_INVOICE", "Debe indicar una factura válida.");
        if (command.Amount <= 0)
            throw new PaymentDomainException("INVALID_AMOUNT", "El valor del pago debe ser mayor que cero.");
        if (string.IsNullOrWhiteSpace(command.Method))
            throw new PaymentDomainException("INVALID_METHOD", "Debe indicar el método de pago.");
        if (string.IsNullOrWhiteSpace(command.Reference))
            throw new PaymentDomainException("INVALID_REFERENCE", "Debe indicar una referencia de pago.");
    }
}

