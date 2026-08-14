using DarouichTravaux.Domain;
namespace DarouichTravaux.Application;

public record InvoiceItemInput(string Designation, string Unit, decimal Quantity, decimal UnitPrice, decimal TaxRate);
public interface IInvoiceService { Task<Invoice> CreateAsync(Guid clientId, Guid? projectId, DateOnly date, DateOnly dueDate, IReadOnlyCollection<InvoiceItemInput> items, CancellationToken ct = default); Task AddPaymentAsync(Guid invoiceId, decimal amount, string method, CancellationToken ct = default); }
public interface IClientService { Task<IReadOnlyList<Client>> SearchAsync(string? query, CancellationToken ct = default); Task<Client> SaveAsync(Client client, CancellationToken ct = default); }
public interface IProjectService { Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct = default); }
public interface IDashboardService { Task<DashboardDto> GetAsync(CancellationToken ct = default); }
public interface IPdfService { byte[] CreateInvoice(Invoice invoice, CompanySettings company); string BuildInvoiceFileName(Invoice invoice); }
public record DashboardDto(decimal Invoiced, decimal Collected, decimal Outstanding, decimal Expenses, decimal Margin, int ActiveProjects, int OverdueInvoices);

public static class InvoiceRules
{
    public static (decimal Net, decimal Tax, decimal Gross) Calculate(IEnumerable<InvoiceItemInput> items)
    {
        var rows = items.ToArray();
        if (rows.Length == 0) throw new ArgumentException("Une facture doit contenir au moins une ligne.");
        if (rows.Any(x => x.Quantity <= 0 || x.UnitPrice < 0 || x.TaxRate < 0)) throw new ArgumentException("Quantité, prix ou TVA invalide.");
        var net = rows.Sum(x => decimal.Round(x.Quantity * x.UnitPrice, 2));
        var tax = rows.Sum(x => decimal.Round(x.Quantity * x.UnitPrice * x.TaxRate / 100, 2));
        return (net, tax, net + tax);
    }
}
