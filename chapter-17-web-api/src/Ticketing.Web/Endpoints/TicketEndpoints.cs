using System.ComponentModel.DataAnnotations;
using Ticketing.Application.Services;
using Ticketing.Application.Tickets;

namespace Ticketing.Web.Endpoints;

public static class TicketEndpoints
{
    public static RouteGroupBuilder MapTicketEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder tickets = endpoints.MapGroup("/api/tickets");

        tickets.MapGet("", GetTicketsAsync);

        tickets.MapGet("/{code}", GetTicketAsync)
               .WithName("GetTicketByCode");

        tickets.MapPost("", CreateTicketAsync);
        tickets.MapDelete("/{code}", DeleteTicketAsync);

        return tickets;
    }

    private static async Task<IResult> GetTicketsAsync(TicketService service, CancellationToken ct)
    {
        IReadOnlyCollection<Ticket> tickets =
            await service.GetAllAsync(ct);

        TicketResponse[] response = tickets.Select(ToResponse).ToArray();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetTicketAsync(
        string code,
        TicketService service,
        CancellationToken ct)
    {
        Ticket? ticket = await service.FindAsync(code, ct);

        if (ticket is null)
        {
            return TicketNotFound(code);
        }

        return Results.Ok(ToResponse(ticket));
    }

    private static async Task<IResult> CreateTicketAsync(
        CreateIncidentTicketRequest request,
        TicketService service,
        CancellationToken ct)
    {
        try
        {
            var ticket = new IncidentTicket(
                request.Code,
                request.Title,
                request.Description,
                request.Priority,
                DateTimeOffset.UtcNow,
                request.AffectedService);

            await service.RegisterAsync(ticket, ct);

            return Results.CreatedAtRoute(
                "GetTicketByCode",
                new { code = ticket.Code },
                ToResponse(ticket));
        }
        catch (DuplicateTicketException exception)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ticket already exists",
                detail: exception.Message,
                type: "https://api.example.com/problems/ticket-already-exists",
                instance: "/api/tickets");
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["request"] = [exception.Message]
                });
        }
    }

    private static async Task<IResult> DeleteTicketAsync(
        string code,
        TicketService service,
        CancellationToken ct)
    {
        bool deleted = await service.DeleteAsync(code, ct);

        return deleted
            ? Results.NoContent()
            : TicketNotFound(code);
    }

    private static IResult TicketNotFound(string code)
    {
        return Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Ticket not found",
            detail: $"The ticket {code} does not exist.",
            type: "https://api.example.com/problems/ticket-not-found",
            instance: $"/api/tickets/{code}");
    }

    private static TicketResponse ToResponse(Ticket ticket)
    {
        return ticket switch
        {
            IncidentTicket incident => new TicketResponse(
                incident.Code,
                incident.TypeName,
                incident.Title,
                incident.Description,
                incident.Priority,
                incident.CreatedAt,
                incident.AffectedService),
            _ => throw new NotSupportedException(
                $"Il tipo {ticket.GetType().Name} non è supportato.")
        };
    }
}

public sealed record CreateIncidentTicketRequest(
    [Required]
    [StringLength(30)]
    [RegularExpression(@"\A(?!\.{1,2}\z)[A-Za-z0-9.-]+\z")]
    string Code,

    [Required]
    [StringLength(200)]
    string Title,

    [Required]
    string Description,

    [Range(1, 3)]
    int Priority,

    [Required]
    [StringLength(100)]
    string AffectedService);

public sealed record TicketResponse(
    string Code,
    string Type,
    string Title,
    string Description,
    int Priority,
    DateTimeOffset CreatedAt,
    string AffectedService);