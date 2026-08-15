using DarouichTravaux.Application;
using DarouichTravaux.Infrastructure.Database;

namespace DarouichTravaux.Infrastructure.Services;

public sealed class BackupService(SqliteDatabase database, string backupDirectory) : IBackupService
{
    public string BackupDirectory { get; } = backupDirectory;
    public async Task<string> CreateBackupAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(BackupDirectory);
        var path = Path.Combine(BackupDirectory, $"darouich-travaux-backup-{DateTime.Now:yyyy-MM-dd-HHmm}.db");
        await using var source = database.OpenConnection();
        await using (var checkpoint = source.CreateCommand()) { checkpoint.CommandText = "PRAGMA wal_checkpoint(FULL)"; await checkpoint.ExecuteNonQueryAsync(cancellationToken); }
        await using var destination = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={path}");
        await destination.OpenAsync(cancellationToken);
        source.BackupDatabase(destination);
        return path;
    }
    public async Task RestoreBackupAsync(string sourcePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(sourcePath)) throw new FileNotFoundException("Sauvegarde introuvable.", sourcePath);
        await using var candidate = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={sourcePath};Mode=ReadOnly");
        await candidate.OpenAsync(cancellationToken);
        await using var check = candidate.CreateCommand(); check.CommandText = "PRAGMA integrity_check";
        if (!string.Equals((string?)await check.ExecuteScalarAsync(cancellationToken), "ok", StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("La sauvegarde SQLite est endommagée.");
        var safetyCopy = database.DatabasePath + ".before-restore";
        File.Copy(database.DatabasePath, safetyCopy, true);
        File.Copy(sourcePath, database.DatabasePath, true);
    }
}
