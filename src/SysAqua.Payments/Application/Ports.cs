using SysAqua.Payments.Domain;

namespace SysAqua.Payments.Application;

public interface IInvoiceRepository
{
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Invoice>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task UpdateAsync(Invoice invoice, CancellationToken cancellationToken = default);
}

public interface IPaymentRepository
{
    Task<Payment?> FindByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task AddAsync(Payment payment, CancellationToken cancellationToken = default);
}

