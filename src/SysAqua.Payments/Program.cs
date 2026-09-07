using SysAqua.Payments.Application;
using SysAqua.Payments.Domain;
using SysAqua.Payments.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IInvoiceRepository, InMemoryInvoiceRepository>();
builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddSingleton<PaymentService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/invoices/pending", async (
    IInvoiceRepository repository,
    CancellationToken cancellationToken) =>
{
    var invoices = await repository.GetPendingAsync(cancellationToken);
    return Results.Ok(invoices);
});

app.MapPost("/api/payments", async (
    RegisterPaymentRequest request,
    PaymentService service,
    CancellationToken cancellationToken) =>
{
    try
    {
        var result = await service.RegisterAsync(
            new RegisterPaymentCommand(
                request.InvoiceId,
                request.Amount,
                request.Method,
                request.Reference),
            cancellationToken);

        return Results.Created($"/api/payments/{result.PaymentId}", result);
    }
    catch (PaymentDomainException exception)
    {
        return Results.BadRequest(new
        {
            error = exception.Code,
            message = exception.Message
        });
    }
});

app.Run();

public sealed record RegisterPaymentRequest(
    Guid InvoiceId,
    decimal Amount,
    string Method,
    string Reference);

public partial class Program
{
}
