using SysAqua.Payments.Application;
using SysAqua.Payments.Domain;
using SysAqua.Payments.Infrastructure;

namespace SysAqua.Payments.Tests;

public sealed class PaymentServiceTests
{
    [Fact]
    public async Task RegisterAsync_WithExactAmount_MarksInvoicePaidAndCreatesReceipt()
    {
        var invoice = new Invoice(Guid.NewGuid(), "CLI-0100", 42.75m);
        var invoices = new InMemoryInvoiceRepository([invoice]);
        var payments = new InMemoryPaymentRepository();
        var service = new PaymentService(invoices, payments);

        var result = await service.RegisterAsync(
            new RegisterPaymentCommand(invoice.Id, 42.75m, "Transferencia", "TRX-CRIT-001"));

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(result.PaymentId, invoice.PaymentId);
        Assert.Equal(42.75m, result.Amount);
        Assert.StartsWith("REC-", result.ReceiptNumber);
        Assert.Single(payments.Items);
    }

    [Fact]
    public async Task RegisterAsync_WithDifferentAmount_RejectsPaymentWithoutChangingInvoice()
    {
        var invoice = new Invoice(Guid.NewGuid(), "CLI-0101", 30.00m);
        var invoices = new InMemoryInvoiceRepository([invoice]);
        var payments = new InMemoryPaymentRepository();
        var service = new PaymentService(invoices, payments);

        var exception = await Assert.ThrowsAsync<PaymentDomainException>(() =>
            service.RegisterAsync(
                new RegisterPaymentCommand(invoice.Id, 29.00m, "Efectivo", "TRX-002")));

        Assert.Equal("AMOUNT_MISMATCH", exception.Code);
        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
        Assert.Empty(payments.Items);
    }

    [Fact]
    public async Task RegisterAsync_WhenInvoiceIsAlreadyPaid_RejectsSecondPayment()
    {
        var invoice = new Invoice(Guid.NewGuid(), "CLI-0102", 18.25m);
        var invoices = new InMemoryInvoiceRepository([invoice]);
        var payments = new InMemoryPaymentRepository();
        var service = new PaymentService(invoices, payments);

        await service.RegisterAsync(
            new RegisterPaymentCommand(invoice.Id, 18.25m, "Tarjeta", "TRX-003"));

        var exception = await Assert.ThrowsAsync<PaymentDomainException>(() =>
            service.RegisterAsync(
                new RegisterPaymentCommand(invoice.Id, 18.25m, "Tarjeta", "TRX-004")));

        Assert.Equal("INVOICE_ALREADY_PAID", exception.Code);
        Assert.Single(payments.Items);
    }
}

