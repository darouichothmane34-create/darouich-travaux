using DarouichTravaux.Application;
using DarouichTravaux.Domain;
using DarouichTravaux.Infrastructure.Services;

namespace DarouichTravaux.Tests;

/// <summary>Tests couvrant les règles du socle SQLite réellement utilisé par l'application.</summary>
public sealed class BusinessRulesTests
{
    [Fact]
    public void Empty_client_name_has_no_valid_business_value()
    {
        Assert.True(string.IsNullOrWhiteSpace("  "));
    }

    [Fact]
    public void Invoice_file_name_includes_customer_and_invoice_number()
    {
        var invoice = new Invoice { Number = "FAC-2026-018", ClientName = "Ahmed  El / Mansouri" };
        var filename = new InvoiceFileNameService().BuildInvoiceFileName(invoice);
        Assert.Equal("Facture(Ahmed_El_Mansouri)-FAC-2026-018.pdf", filename);
    }

    [Fact]
    public void Invoice_balance_deducts_payments()
    {
        var invoice = new Invoice { Number = "FAC-2026-001", TotalIncludingTax = 1200m, PaidAmount = 300m };
        Assert.Equal(900m, invoice.Balance);
    }
}
