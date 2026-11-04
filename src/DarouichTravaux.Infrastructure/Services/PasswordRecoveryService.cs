using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using DarouichTravaux.Application;
using DarouichTravaux.Infrastructure.Database;

namespace DarouichTravaux.Infrastructure.Services;

/// <summary>Récupération du mot de passe par un code temporaire envoyé à l'adresse du gérant.</summary>
public sealed class PasswordRecoveryService(SqliteDatabase database) : IPasswordRecoveryService
{
    private static readonly TimeSpan Validity = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(1);
    private const int MaxAttempts = 5;
    private const int HashIterations = 200_000;
    private const string GenericMessage = "Si cette adresse correspond au compte gérant, un code a été envoyé.";

    public async Task RequestCodeAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Indiquez votre adresse e-mail.");
        await using var connection = database.OpenConnection();
        await using var lookup = connection.CreateCommand();
        lookup.CommandText = "SELECT Id FROM ManagerAccounts WHERE Email=$email COLLATE NOCASE LIMIT 1";
        lookup.Parameters.AddWithValue("$email", email.Trim());
        var id = (string?)await lookup.ExecuteScalarAsync(cancellationToken);
        if (id is null) return; // Ne révèle pas l'existence d'un compte.
        var now = DateTime.UtcNow;
        await using var recent = connection.CreateCommand();
        recent.CommandText = "SELECT LastSentUtc FROM PasswordResetCodes WHERE AccountId=$id";
        recent.Parameters.AddWithValue("$id", id);
        var last = (string?)await recent.ExecuteScalarAsync(cancellationToken);
        if (last is not null && now - DateTime.Parse(last).ToUniversalTime() < Cooldown)
            return;

        // Le code ne sera jamais enregistré en clair.
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var salt = RandomNumberGenerator.GetBytes(32);
        var hash = Rfc2898DeriveBytes.Pbkdf2(code, salt, HashIterations, HashAlgorithmName.SHA256, 32);

        // Configuration privée : variables d'environnement, pas de secrets dans le dépôt.
        var host = Environment.GetEnvironmentVariable("DAROUICH_SMTP_HOST");
        var user = Environment.GetEnvironmentVariable("DAROUICH_SMTP_USER");
        var secret = Environment.GetEnvironmentVariable("DAROUICH_SMTP_PASSWORD");
        var from = Environment.GetEnvironmentVariable("DAROUICH_SMTP_FROM") ?? user;
        var portString = Environment.GetEnvironmentVariable("DAROUICH_SMTP_PORT") ?? "587";
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(from) ||
            !int.TryParse(portString, out var port) || port < 1 || port > 65535)
            throw new InvalidOperationException("Service e-mail non configuré. Renseignez les variables DAROUICH_SMTP_*.");

        // Enregistrer avant l'envoi pour que le code reçu soit immédiatement vérifiable.
        await using var save = connection.CreateCommand();
        save.CommandText = """
            INSERT INTO PasswordResetCodes(AccountId,CodeHash,CodeSalt,ExpiresUtc,LastSentUtc,Attempts)
            VALUES($id,$hash,$salt,$expires,$sent,0)
            ON CONFLICT(AccountId) DO UPDATE SET CodeHash=$hash,CodeSalt=$salt,ExpiresUtc=$expires,
              LastSentUtc=$sent,Attempts=0
            """;
        save.Parameters.AddWithValue("$id", id);
        save.Parameters.AddWithValue("$hash", Convert.ToBase64String(hash));
        save.Parameters.AddWithValue("$salt", Convert.ToBase64String(salt));
        save.Parameters.AddWithValue("$expires", now.Add(Validity).ToString("O"));
        save.Parameters.AddWithValue("$sent", now.ToString("O"));
        await save.ExecuteNonQueryAsync(cancellationToken);

        using var mail = new MailMessage(from, email.Trim())
        {
            Subject = "Darouich Travaux - code de vérification",
            Body = $"Votre code de vérification est : {code}\nIl expire dans 10 minutes. Ne le communiquez à personne."
        };
        using var client = new SmtpClient(host, port)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(user, secret),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15000
        };
        try
        {
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch
        {
            // Invalider le code non transmis ; ne pas exposer les détails SMTP ou secrets.
            await using var clear = connection.CreateCommand();
            clear.CommandText = "DELETE FROM PasswordResetCodes WHERE AccountId=$id AND CodeHash=$hash";
            clear.Parameters.AddWithValue("$id", id);
            clear.Parameters.AddWithValue("$hash", Convert.ToBase64String(hash));
            await clear.ExecuteNonQueryAsync(cancellationToken);
            throw new InvalidOperationException("Envoi impossible. Vérifiez la connexion Internet et la configuration du service e-mail.");
        }
    }

    public async Task ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default)
    {
        var policy = PasswordPolicy.Validate(newPassword ?? "");
        if (policy is not null) throw new ArgumentException(policy);
        if (string.IsNullOrWhiteSpace(email) || code is null || code.Length != 6 || !code.All(char.IsAsciiDigit))
            throw new ArgumentException("Code de vérification invalide.");
        await using var connection = database.OpenConnection();
        await using var transaction = (Microsoft.Data.Sqlite.SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT p.AccountId,p.CodeHash,p.CodeSalt,p.ExpiresUtc,p.Attempts
            FROM PasswordResetCodes p JOIN ManagerAccounts m ON m.Id=p.AccountId
            WHERE m.Email=$email COLLATE NOCASE
            """;
        command.Parameters.AddWithValue("$email", email.Trim());
        string id, hash, salt, expiry;
        int attempts;
        await using (var reader = await command.ExecuteReaderAsync(cancellationToken))
        {
            if (!await reader.ReadAsync(cancellationToken))
                throw new ArgumentException("Code invalide ou expiré.");
            id = reader.GetString(0); hash = reader.GetString(1);
            salt = reader.GetString(2); expiry = reader.GetString(3); attempts = reader.GetInt32(4);
        }
        if (DateTime.Parse(expiry).ToUniversalTime() <= DateTime.UtcNow || attempts >= MaxAttempts)
            throw new ArgumentException("Code expiré ou nombre d'essais dépassé. Demandez un nouveau code.");

        var calculated = Rfc2898DeriveBytes.Pbkdf2(code, Convert.FromBase64String(salt),
            HashIterations, HashAlgorithmName.SHA256, 32);
        if (!CryptographicOperations.FixedTimeEquals(calculated, Convert.FromBase64String(hash)))
        {
            await using var fail = connection.CreateCommand();
            fail.Transaction = transaction;
            fail.CommandText = "UPDATE PasswordResetCodes SET Attempts=Attempts+1 WHERE AccountId=$id";
            fail.Parameters.AddWithValue("$id", id);
            await fail.ExecuteNonQueryAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new ArgumentException("Code incorrect.");
        }
        var newHash = PasswordHasher.Hash(newPassword);
        await using var update = connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = """
            UPDATE ManagerAccounts SET PasswordHash=$hash,PasswordSalt=$salt,
            PasswordIterations=$iterations,MustChangePassword=0,UpdatedAtUtc=$now WHERE Id=$id
            """;
        update.Parameters.AddWithValue("$hash", newHash.Hash);
        update.Parameters.AddWithValue("$salt", newHash.Salt);
        update.Parameters.AddWithValue("$iterations", PasswordHasher.Iterations);
        update.Parameters.AddWithValue("$now", DateTime.UtcNow.ToString("O"));
        update.Parameters.AddWithValue("$id", id);
        await update.ExecuteNonQueryAsync(cancellationToken);
        await using var delete = connection.CreateCommand();
        delete.Transaction = transaction;
        delete.CommandText = "DELETE FROM PasswordResetCodes WHERE AccountId=$id";
        delete.Parameters.AddWithValue("$id", id);
        await delete.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
