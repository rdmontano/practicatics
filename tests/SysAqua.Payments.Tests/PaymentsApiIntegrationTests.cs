using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SysAqua.Payments.Domain;
using SysAqua.Payments.Infrastructure;

namespace SysAqua.Payments.Tests;

public sealed class PaymentsApiIntegrationTests
{
    [Fact]
    public async Task PostPayment_ValidRequest_ReturnsCreatedAndRemovesInvoiceFromPendingList()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/payments", new
        {
            invoiceId = InMemoryInvoiceRepository.SampleInvoiceId,
            amount = 25.50m,
            method = "Transferencia",
            reference = "IT-TRX-001"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var pending = await client.GetFromJsonAsync<List<Invoice>>("/api/invoices/pending");
        Assert.NotNull(pending);
        Assert.DoesNotContain(pending, invoice => invoice.Id == InMemoryInvoiceRepository.SampleInvoiceId);
    }

    [Fact]
    public async Task PostPayment_UnknownInvoice_ReturnsBadRequestAndKeepsSeedInvoicePending()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/payments", new
        {
            invoiceId = Guid.NewGuid(),
            amount = 25.50m,
            method = "Efectivo",
            reference = "IT-TRX-002"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var pending = await client.GetFromJsonAsync<List<Invoice>>("/api/invoices/pending");
        Assert.NotNull(pending);
        Assert.Contains(pending, invoice => invoice.Id == InMemoryInvoiceRepository.SampleInvoiceId);
    }
}

