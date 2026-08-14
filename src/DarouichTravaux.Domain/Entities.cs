namespace DarouichTravaux.Domain;

public abstract class Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}

public sealed class ManagerAccount : Entity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string PasswordSalt { get; set; }
    public int PasswordIterations { get; set; }
    public bool MustChangePassword { get; set; } = true;
}

public sealed class Client : Entity
{
    public required string Name { get; set; }
    public string? CompanyName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Ice { get; set; }
    public string? TaxId { get; set; }
    public string? Rc { get; set; }
    public string? Cnie { get; set; }
    public string? Notes { get; set; }
}

public enum ProjectStatus { Draft, Planned, Active, Paused, Completed, Cancelled }
public sealed class Project : Entity
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public Guid ClientId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? ExpectedEndDate { get; set; }
    public DateOnly? ActualEndDate { get; set; }
    public decimal ContractAmount { get; set; }
    public decimal Budget { get; set; }
    public decimal Progress { get; set; }
    public string? Manager { get; set; }
    public ProjectStatus Status { get; set; }
}

public enum InvoiceStatus { Draft, Sent, PartiallyPaid, Paid, Overdue, Cancelled }
public sealed class Invoice : Entity
{
    public required string Number { get; set; }
    public Guid ClientId { get; set; }
    public string ClientName { get; set; } = "";
    public Guid? ProjectId { get; set; }
    public DateOnly Date { get; set; }
    public DateOnly DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public decimal TotalExcludingTax { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal TotalIncludingTax { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal Balance => TotalIncludingTax - PaidAmount;
}

public sealed class Expense : Entity
{
    public Guid? ProjectId { get; set; }
    public DateOnly Date { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public decimal AmountIncludingTax { get; set; }
}
