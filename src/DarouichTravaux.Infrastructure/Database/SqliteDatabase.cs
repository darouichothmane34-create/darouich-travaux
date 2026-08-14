using DarouichTravaux.Application;
using Microsoft.Data.Sqlite;

namespace DarouichTravaux.Infrastructure.Database;

public sealed class SqliteDatabase(string databasePath) : IDatabaseInitializer
{
    public string DatabasePath { get; } = databasePath;
    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection($"Data Source={DatabasePath};Mode=ReadWriteCreate;Cache=Shared");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys=ON; PRAGMA busy_timeout=5000;";
        command.ExecuteNonQuery();
        return connection;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath)!);
        await using var connection = OpenConnection();
        await using var command = connection.CreateCommand();
        command.CommandText = Schema;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private const string Schema = """
        PRAGMA journal_mode=WAL;
        CREATE TABLE IF NOT EXISTS ManagerAccounts(Id TEXT PRIMARY KEY, Email TEXT NOT NULL UNIQUE COLLATE NOCASE, PasswordHash TEXT NOT NULL, PasswordSalt TEXT NOT NULL, PasswordIterations INTEGER NOT NULL, MustChangePassword INTEGER NOT NULL, CreatedAtUtc TEXT NOT NULL, UpdatedAtUtc TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS Clients(Id TEXT PRIMARY KEY, Name TEXT NOT NULL, CompanyName TEXT, Phone TEXT, Email TEXT, Address TEXT, City TEXT, Ice TEXT, TaxId TEXT, Rc TEXT, Cnie TEXT, Notes TEXT, CreatedAtUtc TEXT NOT NULL, UpdatedAtUtc TEXT NOT NULL);
        CREATE INDEX IF NOT EXISTS IX_Clients_Name ON Clients(Name COLLATE NOCASE);
        CREATE TABLE IF NOT EXISTS Projects(Id TEXT PRIMARY KEY, Code TEXT NOT NULL UNIQUE, Name TEXT NOT NULL, ClientId TEXT NOT NULL REFERENCES Clients(Id) ON DELETE RESTRICT, Address TEXT, City TEXT, Description TEXT, StartDate TEXT NOT NULL, ExpectedEndDate TEXT, ActualEndDate TEXT, ContractAmount REAL NOT NULL, Budget REAL NOT NULL, Progress REAL NOT NULL CHECK(Progress BETWEEN 0 AND 100), Manager TEXT, Status INTEGER NOT NULL, CreatedAtUtc TEXT NOT NULL, UpdatedAtUtc TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS Invoices(Id TEXT PRIMARY KEY, Number TEXT NOT NULL UNIQUE, ClientId TEXT NOT NULL REFERENCES Clients(Id) ON DELETE RESTRICT, ClientName TEXT NOT NULL, ProjectId TEXT REFERENCES Projects(Id) ON DELETE SET NULL, Date TEXT NOT NULL, DueDate TEXT NOT NULL, Status INTEGER NOT NULL, TotalExcludingTax REAL NOT NULL, TaxTotal REAL NOT NULL, TotalIncludingTax REAL NOT NULL, PaidAmount REAL NOT NULL CHECK(PaidAmount >= 0), CreatedAtUtc TEXT NOT NULL, UpdatedAtUtc TEXT NOT NULL);
        CREATE TABLE IF NOT EXISTS Expenses(Id TEXT PRIMARY KEY, ProjectId TEXT REFERENCES Projects(Id) ON DELETE SET NULL, Date TEXT NOT NULL, Description TEXT NOT NULL, Category TEXT NOT NULL, AmountIncludingTax REAL NOT NULL CHECK(AmountIncludingTax >= 0), CreatedAtUtc TEXT NOT NULL, UpdatedAtUtc TEXT NOT NULL);
        """;
}
