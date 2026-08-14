using DarouichTravaux.Domain;

namespace DarouichTravaux.Application;

public interface IDatabaseInitializer { Task InitializeAsync(CancellationToken cancellationToken = default); }
public interface IManagerSetupService { Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default); Task CreateManagerAsync(string email, string password, CancellationToken cancellationToken = default); }
public interface IAuthenticationService
{
    bool IsAuthenticated { get; }
    Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    void Logout();
}
public record LoginResult(bool Succeeded, bool MustChangePassword, string? Error = null);
public interface IBackupService
{
    string BackupDirectory { get; }
    Task<string> CreateBackupAsync(CancellationToken cancellationToken = default);
    Task RestoreBackupAsync(string sourcePath, CancellationToken cancellationToken = default);
}
public interface IClientService
{
    Task<IReadOnlyList<Client>> SearchAsync(string? query, CancellationToken cancellationToken = default);
    Task SaveAsync(Client client, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
public interface IDashboardService { Task<DashboardDto> GetAsync(CancellationToken cancellationToken = default); }
public record DashboardDto(decimal Invoiced, decimal Collected, decimal Outstanding, decimal Expenses, decimal Margin, int ActiveProjects, int OverdueInvoices);
public interface IInvoiceFileNameService { string BuildInvoiceFileName(Invoice invoice); }

public static class PasswordPolicy
{
    public static string? Validate(string value)
    {
        if (value.Length < 10) return "Le mot de passe doit contenir au moins 10 caractères.";
        if (!value.Any(char.IsUpper) || !value.Any(char.IsLower) || !value.Any(char.IsDigit) || !value.Any(c => !char.IsLetterOrDigit(c)))
            return "Ajoutez une majuscule, une minuscule, un chiffre et un caractère spécial.";
        return null;
    }
}
