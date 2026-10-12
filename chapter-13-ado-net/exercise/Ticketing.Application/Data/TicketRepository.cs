using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Ticketing.Application.Tickets;

namespace Ticketing.Application.Data;

public sealed class TicketRepository
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

    public TicketRepository(string connectionString)
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

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(ct);

        using SqlCommand command = new(sql, connection);
        AddTicketParameters(command, ticket);

        try
        {
            await command.ExecuteNonQueryAsync(ct);
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

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(ct);

        using SqlCommand command = new(sql, connection);
        command.Parameters.Add(
            "@Code",
            SqlDbType.NVarChar,
            30).Value = code;

        await using SqlDataReader reader = await command.ExecuteReaderAsync(ct);

        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return ReadTicket(reader);
    }

    public async Task<IReadOnlyCollection<Ticket>> GetAllAsync(CancellationToken ct)
    {
        string sql = $"""
            {SelectColumns}
            ORDER BY CreatedAt DESC;
            """;

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(ct);

        using SqlCommand command = new(sql, connection);
        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(ct);

        List<Ticket> tickets = new();

        while (await reader.ReadAsync(ct))
        {
            tickets.Add(ReadTicket(reader));
        }

        return tickets;
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

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(ct);

        using SqlCommand command = new(sql, connection);
        AddTicketParameters(command, ticket);

        int affectedRows = await command.ExecuteNonQueryAsync(ct);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(string code, CancellationToken ct)
    {
        const string sql = """
            DELETE FROM dbo.Tickets
            WHERE Code = @Code;
            """;

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(ct);

        using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Code", SqlDbType.NVarChar, 30).Value = code;

        int affectedRows =
            await command.ExecuteNonQueryAsync(ct);

        return affectedRows > 0;
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM dbo.Tickets;
            """;

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(ct);

        using SqlCommand command = new(sql, connection);
        object? result = await command.ExecuteScalarAsync(ct);

        return Convert.ToInt32(result);
    }

    public async Task<int> CountByPriorityAsync(int priority, CancellationToken ct)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM dbo.Tickets
            WHERE Priority = @Priority;
            """;

        await using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync(ct);

        using SqlCommand command = new(sql, connection);
        command.Parameters.Add("@Priority", SqlDbType.Int).Value = priority;

        object? result = await command.ExecuteScalarAsync(ct);

        return Convert.ToInt32(result);
    }

    private static void AddTicketParameters(SqlCommand command, Ticket ticket)
    {
        if (ticket is not IncidentTicket incident)
        {
            throw new NotSupportedException($"Il tipo {ticket.GetType().Name} non è supportato.");
        }

        command.Parameters.Add(
            "@Code",
            SqlDbType.NVarChar,
            30).Value = ticket.Code;

        command.Parameters.Add(
            "@TicketType",
            SqlDbType.NVarChar,
            30).Value = ticket.TypeName;

        command.Parameters.Add(
            "@Title",
            SqlDbType.NVarChar,
            200).Value = ticket.Title;

        command.Parameters.Add(
            "@Description",
            SqlDbType.NVarChar,
            -1).Value = ticket.Description;

        command.Parameters.Add(
            "@Priority",
            SqlDbType.Int).Value = ticket.Priority;

        command.Parameters.Add(
            "@CreatedAt",
            SqlDbType.DateTimeOffset).Value = ticket.CreatedAt;

        command.Parameters.Add(
            "@AffectedService",
            SqlDbType.NVarChar,
            100).Value = incident.AffectedService;
    }

    private static Ticket ReadTicket(SqlDataReader reader)
    {
        string code        = reader.GetString(reader.GetOrdinal("Code"));
        string ticketType  = reader.GetString(reader.GetOrdinal("TicketType"));
        string title       = reader.GetString(reader.GetOrdinal("Title"));
        string description = reader.GetString(reader.GetOrdinal("Description"));
        int priority       = reader.GetInt32(reader.GetOrdinal("Priority"));
        DateTimeOffset createdAt = reader.GetDateTimeOffset(reader.GetOrdinal("CreatedAt"));
        string affectedService   = reader.GetString(reader.GetOrdinal("AffectedService"));

        return ticketType switch
        {
            "Incident" => new IncidentTicket(
                code,
                title,
                description,
                priority,
                createdAt,
                affectedService),
            _ => throw new InvalidOperationException(
                $"Tipo di ticket non supportato: {ticketType}.")
        };
    }
}
