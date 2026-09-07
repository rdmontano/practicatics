namespace SysAqua.Payments.Domain;

public sealed record Payment(
    Guid Id,
    Guid InvoiceId,
    decimal Amount,
    string Method,
    string Reference,
    DateTimeOffset RegisteredAt);

public sealed class PaymentDomainException : Exception
{
    public PaymentDomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}

