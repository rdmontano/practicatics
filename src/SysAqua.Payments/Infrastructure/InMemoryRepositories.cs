using System.Collections.Concurrent;
using SysAqua.Payments.Application;
using SysAqua.Payments.Domain;

namespace SysAqua.Payments.Infrastructure;

public sealed class InMemoryInvoiceRepository : IInvoiceRepository
{
    public static readonly Guid SampleInvoiceId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly ConcurrentDictionary<Guid, Invoice> _items = new();

    public InMemoryInvoiceRepository(IEnumerable<Invoice>? seed = null)
    {
        var invoices = seed?.ToArray() ?? [];
        if (invoices.Length == 0)
            invoices = [new Invoice(SampleInvoiceId, "CLI-0001", 25.50m)];

        foreach (var invoice in invoices)
            _items[invoice.Id] = invoice;
    }

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _items.TryGetValue(id, out var invoice);
        return Task.FromResult(invoice);
    }

    public Task<IReadOnlyCollection<Invoice>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Invoice> result = _items.Values
            .Where(invoice => invoice.Status == InvoiceStatus.Pending)
            .OrderBy(invoice => invoice.CustomerCode)
            .ToArray();
        return Task.FromResult(result);
    }

    public Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default)
    {
        _items[invoice.Id] = invoice;
        return Task.CompletedTask;
    }
}

public sealed class InMemoryPaymentRepository : IPaymentRepository
{
    private readonly ConcurrentDictionary<string, Payment> _items = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<Payment> Items => _items.Values.ToArray();

    public Task<Payment?> FindByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        _items.TryGetValue(reference, out var payment);
        return Task.FromResult(payment);
    }

    public Task AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (!_items.TryAdd(payment.Reference, payment))
            throw new PaymentDomainException("DUPLICATE_REFERENCE", "La referencia del pago ya fue registrada.");
        return Task.CompletedTask;
    }
}
