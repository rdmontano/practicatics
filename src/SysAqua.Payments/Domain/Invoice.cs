namespace SysAqua.Payments.Domain;

public enum InvoiceStatus
{
    Pending = 1,
    Paid = 2
}

public sealed class Invoice
{
    public Invoice(Guid id, string customerCode, decimal total)
    {
        if (id == Guid.Empty) throw new ArgumentException("La factura requiere un identificador.", nameof(id));
        if (string.IsNullOrWhiteSpace(customerCode)) throw new ArgumentException("El cliente es obligatorio.", nameof(customerCode));
        if (total <= 0) throw new ArgumentOutOfRangeException(nameof(total), "El total debe ser mayor que cero.");

        Id = id;
        CustomerCode = customerCode.Trim();
        Total = decimal.Round(total, 2, MidpointRounding.AwayFromZero);
        Status = InvoiceStatus.Pending;
    }

    public Guid Id { get; }
    public string CustomerCode { get; }
    public decimal Total { get; }
    public InvoiceStatus Status { get; private set; }
    public Guid? PaymentId { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    public void MarkPaid(Guid paymentId, DateTimeOffset paidAt)
    {
        if (Status == InvoiceStatus.Paid)
            throw new PaymentDomainException("INVOICE_ALREADY_PAID", "La factura ya se encuentra pagada.");
        if (paymentId == Guid.Empty)
            throw new ArgumentException("El pago requiere un identificador.", nameof(paymentId));

        PaymentId = paymentId;
        PaidAt = paidAt;
        Status = InvoiceStatus.Paid;
    }
}

