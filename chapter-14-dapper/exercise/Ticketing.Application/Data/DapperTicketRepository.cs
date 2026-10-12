using Dapper;
using Microsoft.Data.SqlClient;
using Ticketing.Application.Tickets;

namespace Ticketing.Application.Data;

public sealed class DapperTicketRepository : ITicketRepository
{
    private const string SelectColumns = """
        SELECT
            Code,
            TicketType,
            Title,
            Description,
            Priority,
            CreatedAt,
            AffectedService
        FROM dbo.Tickets
        """;

    private readonly string _connectionString;

    public DapperTicketRepository(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "La connection string non può essere vuota.",
                nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    public async Task InsertAsync(Ticket ticket, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO dbo.Tickets
            (
                Code,
                TicketType,
                Title,
                Description,
                Priority,
                CreatedAt,
                AffectedService
            )
            VALUES
            (
                @Code,
                @TicketType,
                @Title,
                @Description,
                @Priority,
                @CreatedAt,
                @AffectedService
            );
            """;

        await using SqlConnection connection = await OpenConnectionAsync(ct);

        CommandDefinition command = new(sql, CreateParameters(ticket), cancellationToken: ct);

        try
        {
            await connection.ExecuteAsync(command);
        }
        catch (SqlException exception)
            when (exception.Number is 2601 or 2627)
        {
            throw new DuplicateTicketException(
                $"Il ticket {ticket.Code} è già presente.",
                exception);
        }
    }

    public async Task<Ticket?> FindAsync(string code, CancellationToken ct)
    {
        string sql = $"""
            {SelectColumns}
            WHERE Code = @Code;
            """;

        await using SqlConnection connection = await OpenConnectionAsync(ct);

        CommandDefinition command = new(sql, new { Code = code }, cancellationToken: ct);

        TicketRow? row = await connection.QuerySingleOrDefaultAsync<TicketRow>(command);

        return row is null ? null : MapTicket(row);
    }

    public async Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct)
    {
        string sql = $"""
            {SelectColumns}
            ORDER BY CreatedAt DESC;
            """;

        await using SqlConnection connection = await OpenConnectionAsync(ct);

        CommandDefinition command = new(sql, cancellationToken: ct);

        IEnumerable<TicketRow> rows = await connection.QueryAsync<TicketRow>(command);

        return rows.Select(MapTicket).ToList();
    }

    public async Task<IReadOnlyCollection<Ticket>> GetByPriorityAsync(int priority, CancellationToken ct)
    {
        string sql = $"""
            {SelectColumns}
            WHERE Priority = @Priority
            ORDER BY CreatedAt DESC;
            """;

        await using SqlConnection connection = await OpenConnectionAsync(ct);

        CommandDefinition command = new(sql, new { Priority = priority }, cancellationToken: ct);

        IEnumerable<TicketRow> rows = await connection.QueryAsync<TicketRow>(command);

        return rows.Select(MapTicket).ToList();
    }

    public async Task<bool> UpdateAsync(Ticket ticket, CancellationToken ct)
    {
        const string sql = """
            UPDATE dbo.Tickets
            SET
                Title           = @Title,
                Description     = @Description,
                Priority        = @Priority,
                AffectedService = @AffectedService
            WHERE Code = @Code;
            """;

        await using SqlConnection connection = await OpenConnectionAsync(ct);

        CommandDefinition command = new(sql, CreateParameters(ticket), cancellationToken: ct);

        int affectedRows = await connection.ExecuteAsync(command);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(string code, CancellationToken ct)
    {
        const string sql = """
            DELETE FROM dbo.Tickets
            WHERE Code = @Code;
            """;

        await using SqlConnection connection = await OpenConnectionAsync(ct);

        CommandDefinition command = new(sql, new { Code = code }, cancellationToken: ct);

        int affectedRows = await connection.ExecuteAsync(command);
        return affectedRows > 0;
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        const string sql = "SELECT COUNT(*) FROM dbo.Tickets;";

        await using SqlConnection connection =
            await OpenConnectionAsync(ct);

        CommandDefinition command = new(sql, cancellationToken: ct);

        return await connection.ExecuteScalarAsync<int>(command);
    }

    private async Task<SqlConnection> OpenConnectionAsync(CancellationToken ct)
    {
        SqlConnection connection = new(_connectionString);

        try
        {
            await connection.OpenAsync(ct);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    private static object CreateParameters(Ticket ticket)
    {
        if (ticket is not IncidentTicket incident)
        {
            throw new NotSupportedException(
                $"Il tipo {ticket.GetType().Name} non è supportato.");
        }

        return new
        {
            ticket.Code,
            TicketType = ticket.TypeName,
            ticket.Title,
            ticket.Description,
            ticket.Priority,
            ticket.CreatedAt,
            incident.AffectedService
        };
    }

    private static Ticket MapTicket(TicketRow row)
    {
        return row.TicketType switch
        {
            "Incident" => new IncidentTicket(
                row.Code,
                row.Title,
                row.Description,
                row.Priority,
                row.CreatedAt,
                row.AffectedService),
            _ => throw new InvalidOperationException(
                $"Tipo di ticket non supportato: {row.TicketType}.")
        };
    }
}