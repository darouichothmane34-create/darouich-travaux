using DarouichTravaux.Application;
using DarouichTravaux.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarouichTravaux.Infrastructure;
public sealed class ApplicationUser : IdentityUser<Guid> { public bool MustChangePassword { get; set; } = true; }
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Client> Clients => Set<Client>(); public DbSet<Project> Projects => Set<Project>(); public DbSet<Quote> Quotes => Set<Quote>(); public DbSet<Invoice> Invoices => Set<Invoice>(); public DbSet<Payment> Payments => Set<Payment>(); public DbSet<Expense> Expenses => Set<Expense>(); public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();
    protected override void OnModelCreating(ModelBuilder b) { base.OnModelCreating(b); b.Entity<Client>().HasIndex(x => x.Name); b.Entity<Project>().HasIndex(x => x.Code).IsUnique(); b.Entity<Invoice>().HasIndex(x => x.Number).IsUnique(); b.Entity<Quote>().HasIndex(x => x.Number).IsUnique(); b.Entity<Project>().HasOne(x => x.Client).WithMany(x => x.Projects).HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict); b.Entity<Invoice>().HasMany(x => x.Items).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade); b.Entity<Invoice>().HasMany(x => x.Payments).WithOne(x => x.Invoice).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict); }
}

public sealed class InvoiceService(AppDbContext db) : IInvoiceService
{
    public async Task<Invoice> CreateAsync(Guid clientId, Guid? projectId, DateOnly date, DateOnly dueDate, IReadOnlyCollection<InvoiceItemInput> items, CancellationToken ct = default) { if (dueDate < date) throw new ArgumentException("L'échéance doit suivre la date de facture."); if (!await db.Clients.AnyAsync(x => x.Id == clientId, ct)) throw new ArgumentException("Client obligatoire."); var totals = InvoiceRules.Calculate(items); var count = await db.Invoices.CountAsync(x => x.Date.Year == date.Year, ct) + 1; var invoice = new Invoice { Number = $"FAC-{date.Year}-{count:000}", ClientId = clientId, ProjectId = projectId, Date = date, DueDate = dueDate, Status = DocumentStatus.Draft, TotalExcludingTax = totals.Net, TaxTotal = totals.Tax, TotalIncludingTax = totals.Gross }; foreach (var row in items) invoice.Items.Add(new InvoiceItem { Designation = row.Designation, Unit = row.Unit, Quantity = row.Quantity, UnitPrice = row.UnitPrice, TaxRate = row.TaxRate }); db.Add(invoice); await db.SaveChangesAsync(ct); return invoice; }
    public async Task AddPaymentAsync(Guid invoiceId, decimal amount, string method, CancellationToken ct = default) { var invoice = await db.Invoices.Include(x => x.Payments).SingleAsync(x => x.Id == invoiceId, ct); if (amount <= 0 || amount > invoice.Balance) throw new ArgumentException("Le paiement doit être positif et inférieur ou égal au solde."); invoice.Payments.Add(new Payment { Amount = amount, Method = method, Date = DateOnly.FromDateTime(DateTime.UtcNow) }); invoice.PaidAmount += amount; invoice.Status = invoice.Balance == 0 ? DocumentStatus.Paid : DocumentStatus.PartiallyPaid; await db.SaveChangesAsync(ct); }
}
