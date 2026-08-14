using DarouichTravaux.Application;
using DarouichTravaux.Infrastructure.Database;
using Microsoft.Data.Sqlite;

namespace DarouichTravaux.Infrastructure.Services;

public sealed class AuthenticationService(SqliteDatabase database) : IAuthenticationService, IManagerSetupService
{
    private Guid? _authenticatedAccountId;
    public async Task<bool> IsConfiguredAsync(CancellationToken cancellationToken = default) { await using var connection=database.OpenConnection(); await using var command=connection.CreateCommand(); command.CommandText="SELECT EXISTS(SELECT 1 FROM ManagerAccounts)"; return Convert.ToBoolean(await command.ExecuteScalarAsync(cancellationToken)); }
    public async Task CreateManagerAsync(string email,string password,CancellationToken cancellationToken=default) { if(await IsConfiguredAsync(cancellationToken))throw new InvalidOperationException("Le compte gérant existe déjà."); if(!new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(email))throw new ArgumentException("Email invalide."); var error=PasswordPolicy.Validate(password);if(error is not null)throw new ArgumentException(error);var value=PasswordHasher.Hash(password);await using var connection=database.OpenConnection();await using var command=connection.CreateCommand();command.CommandText="INSERT INTO ManagerAccounts VALUES($id,$email,$hash,$salt,$iterations,0,$now,$now)";command.Parameters.AddWithValue("$id",Guid.NewGuid().ToString());command.Parameters.AddWithValue("$email",email.Trim());command.Parameters.AddWithValue("$hash",value.Hash);command.Parameters.AddWithValue("$salt",value.Salt);command.Parameters.AddWithValue("$iterations",PasswordHasher.Iterations);command.Parameters.AddWithValue("$now",DateTime.UtcNow.ToString("O"));await command.ExecuteNonQueryAsync(cancellationToken); }

    public bool IsAuthenticated => _authenticatedAccountId.HasValue;

    public async Task<LoginResult> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        await using var connection = database.OpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, PasswordHash, PasswordSalt, PasswordIterations, MustChangePassword FROM ManagerAccounts WHERE Email=$email COLLATE NOCASE LIMIT 1";
        command.Parameters.AddWithValue("$email", email.Trim());
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) || !PasswordHasher.Verify(password, reader.GetString(1), reader.GetString(2), reader.GetInt32(3)))
            return new(false, false, "Email ou mot de passe incorrect.");
        _authenticatedAccountId = Guid.Parse(reader.GetString(0));
        return new(true, reader.GetBoolean(4));
    }

    public async Task ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        if (_authenticatedAccountId is null) throw new UnauthorizedAccessException();
        var policyError = PasswordPolicy.Validate(newPassword);
        if (policyError is not null) throw new ArgumentException(policyError);
        await using var connection = database.OpenConnection();
        await using var select = connection.CreateCommand();
        select.CommandText = "SELECT PasswordHash,PasswordSalt,PasswordIterations FROM ManagerAccounts WHERE Id=$id";
        select.Parameters.AddWithValue("$id", _authenticatedAccountId.Value.ToString());
        await using var reader = await select.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken) || !PasswordHasher.Verify(currentPassword, reader.GetString(0), reader.GetString(1), reader.GetInt32(2))) throw new ArgumentException("Le mot de passe actuel est incorrect.");
        await reader.DisposeAsync();
        var value = PasswordHasher.Hash(newPassword);
        await using var update = connection.CreateCommand();
        update.CommandText = "UPDATE ManagerAccounts SET PasswordHash=$hash,PasswordSalt=$salt,PasswordIterations=$iterations,MustChangePassword=0,UpdatedAtUtc=$now WHERE Id=$id";
        update.Parameters.AddWithValue("$hash", value.Hash); update.Parameters.AddWithValue("$salt", value.Salt); update.Parameters.AddWithValue("$iterations", PasswordHasher.Iterations); update.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O")); update.Parameters.AddWithValue("$id", _authenticatedAccountId.Value.ToString());
        await update.ExecuteNonQueryAsync(cancellationToken);
    }
    public void Logout() => _authenticatedAccountId = null;
}
