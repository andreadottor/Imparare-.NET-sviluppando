using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Application.Data;
using Ticketing.Application.Tickets;
using Xunit;

namespace Ticketing.Application.Tests.Data;

public sealed class SqlServerTicketRepositoryTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    [SqlServerTheory]
    [InlineData("EfCore")]
    [InlineData("AdoNet")]
    [InlineData("Dapper")]
    public async Task Repository_RoundTripsLifecycleAndPreservesDatesDuringStaleUpdates(string implementation)
    {
        await using var database = new TestDatabase();
        await database.Context.Database.MigrateAsync();
        ITicketRepository repository = database.CreateRepository(implementation);
        var ticket = new IncidentTicket("INC-SQL", "Title", "Description", 1, CreatedAt, "Portal");
        await repository.InsertAsync(ticket, default);
        Ticket? open = await repository.FindAsync(ticket.Code, default);
        Assert.NotNull(open);
        Assert.Equal(TicketStatus.Open, open.Status);
        Assert.Null(open.ClosedAt);

        DateTimeOffset closedAt = CreatedAt.AddHours(1);
        Assert.True(await repository.CloseAsync(ticket.Code, closedAt, default));
        Assert.True(await repository.CloseAsync(ticket.Code, closedAt.AddHours(1), default));
        Assert.False(await repository.CloseAsync("missing", closedAt, default));

        var staleUpdate = new IncidentTicket(ticket.Code, "Updated", "New description", 2,
            CreatedAt.AddYears(1), "API");
        Assert.True(await repository.UpdateAsync(staleUpdate, default));
        var closed = Assert.IsType<IncidentTicket>(await repository.FindAsync(ticket.Code, default));
        Assert.Equal(TicketStatus.Closed, closed.Status);
        Assert.Equal(closedAt, closed.ClosedAt);
        Assert.Equal(CreatedAt, closed.CreatedAt);
        Assert.Equal("Updated", closed.Title);
        Assert.Equal("New description", closed.Description);
        Assert.Equal(2, closed.Priority);
        Assert.Equal("API", closed.AffectedService);
        Assert.Equal(closedAt, Assert.Single(await repository.GetAllAsync(default)).ClosedAt);
        Assert.False(await repository.UpdateAsync(new IncidentTicket("missing", "Title", "Description", 1,
            CreatedAt, "Portal"), default));

        var restored = new IncidentTicket("INC-RESTORED", "Title", "Description", 1,
            CreatedAt, "Portal", closedAt);
        await repository.InsertAsync(restored, default);
        Assert.Equal(closedAt, (await repository.FindAsync(restored.Code, default))!.ClosedAt);

        await repository.InsertAsync(new IncidentTicket("INC-RACE", "Title", "Description", 1,
            CreatedAt, "Portal"), default);
        bool[] results = await Task.WhenAll(
            repository.CloseAsync("INC-RACE", closedAt, default),
            repository.CloseAsync("INC-RACE", closedAt.AddMinutes(1), default));
        Assert.All(results, Assert.True);
        Ticket? race = await repository.FindAsync("INC-RACE", default);
        Assert.NotNull(race);
        Assert.Contains(race.ClosedAt, new DateTimeOffset?[] { closedAt, closedAt.AddMinutes(1) });
        DateTimeOffset? first = race.ClosedAt;
        await repository.CloseAsync("INC-RACE", closedAt.AddDays(1), default);
        Assert.Equal(first, (await repository.FindAsync("INC-RACE", default))!.ClosedAt);
    }

    [SqlServerTheory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public async Task Migration_PreservesExistingRowsAndRejectsInvalidState(int status, bool hasDate)
    {
        await using var database = new TestDatabase();
        IMigrator migrator = database.Context.GetService<IMigrator>();
        await migrator.MigrateAsync("20261002095315_InitialCreate");
        await database.Context.Database.ExecuteSqlAsync($"INSERT INTO dbo.Tickets (Code, TicketType, Title, Description, Priority, CreatedAt, AffectedService) VALUES ({"INC-LEGACY"}, {"Incident"}, {"Legacy title"}, {"Description"}, {1}, {CreatedAt}, {"Portal"})");
        await migrator.MigrateAsync();

        Ticket? legacy = await database.CreateRepository("EfCore").FindAsync("INC-LEGACY", default);
        Assert.NotNull(legacy);
        Assert.Equal("Legacy title", legacy.Title);
        Assert.Equal(CreatedAt, legacy.CreatedAt);
        Assert.Equal(TicketStatus.Open, legacy.Status);
        Assert.Null(legacy.ClosedAt);
        Assert.False(database.Context.Database.HasPendingModelChanges());

        DateTimeOffset? closedAt = hasDate ? CreatedAt.AddHours(1) : null;
        SqlException exception = await Assert.ThrowsAsync<SqlException>(() =>
            database.Context.Database.ExecuteSqlAsync($"UPDATE dbo.Tickets SET Status = {status}, ClosedAt = {closedAt} WHERE Code = {"INC-LEGACY"}"));
        Assert.Equal(547, exception.Number);
    }

    private sealed class TestDatabase : IAsyncDisposable, IDbContextFactory<TicketingDbContext>
    {
        private readonly string _connectionString;
        private readonly DbContextOptions<TicketingDbContext> _options;
        public TicketingDbContext Context { get; }

        public TestDatabase()
        {
            // Always use a uniquely named disposable database, never the configured application's database.
            var connection = new SqlConnectionStringBuilder(
                Environment.GetEnvironmentVariable("TICKETING_TEST_SQLSERVER") ??
                throw new InvalidOperationException("Configure TICKETING_TEST_SQLSERVER for SQL Server integration tests."))
            {
                InitialCatalog = "TicketingLifecycleTests_" + Guid.NewGuid().ToString("N")
            };
            _connectionString = connection.ConnectionString;
            _options = new DbContextOptionsBuilder<TicketingDbContext>().UseSqlServer(_connectionString).Options;
            Context = CreateDbContext();
        }

        public TicketingDbContext CreateDbContext() => new(_options);

        public ITicketRepository CreateRepository(string implementation) => implementation switch
        {
            "EfCore" => new EfCoreTicketRepository(this),
            "AdoNet" => new AdoNetTicketRepository(_connectionString),
            "Dapper" => new DapperTicketRepository(_connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(implementation))
        };

        public async ValueTask DisposeAsync()
        {
            try { await Context.Database.EnsureDeletedAsync(); }
            finally { await Context.DisposeAsync(); }
        }
    }
}

public sealed class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(
            Environment.GetEnvironmentVariable("TICKETING_TEST_SQLSERVER")))
        {
            Skip = "Set TICKETING_TEST_SQLSERVER to run SQL Server integration tests on an explicit test server.";
        }
    }
}
