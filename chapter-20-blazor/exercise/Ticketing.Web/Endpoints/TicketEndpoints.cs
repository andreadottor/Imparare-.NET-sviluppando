using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Ticketing.Application.Services;
using Ticketing.Application.Tickets;

namespace Ticketing.Web.Endpoints;

public static class TicketEndpoints
{
    public static RouteGroupBuilder MapTicketEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder tickets = endpoints
            .MapGroup("/api/tickets")
            .WithTags("Tickets");

        tickets.MapGet("", GetTicketsAsync)
               .WithSummary("Returns all tickets")
               .WithDescription("Returns an empty array when no tickets exist.");

        tickets.MapGet("/{code}", GetTicketAsync)
               .WithName("GetTicketByCode")
               .WithSummary("Returns a ticket by code")
               .WithDescription("Returns 404 when the ticket does not exist.")
               .ProducesProblem(StatusCodes.Status404NotFound);

        tickets.MapPost("", CreateTicketAsync)
               .WithSummary("Creates an incident ticket")
               .WithDescription("Returns 409 when the ticket code already exists.")
               .ProducesValidationProblem()
               .ProducesProblem(StatusCodes.Status409Conflict);

        tickets.MapDelete("/{code}", DeleteTicketAsync)
               .WithSummary("Deletes a ticket")
               .WithDescription("Returns 404 when the ticket does not exist.")
               .ProducesProblem(StatusCodes.Status404NotFound);

        return tickets;
    }

    private static async Task<Ok<TicketResponse[]>> GetTicketsAsync(TicketService service, CancellationToken ct)
    {
        IReadOnlyCollection<Ticket> tickets = await service.GetAllAsync(ct);

        TicketResponse[] response = tickets.Select(ToResponse).ToArray();

        return TypedResults.Ok(response);
    }

    private static async
        Task<Results<Ok<TicketResponse>, ProblemHttpResult>> GetTicketAsync(
            string code,
            TicketService service,
            CancellationToken ct)
    {
        Ticket? ticket = await service.FindAsync(code, ct);

        if (ticket is null)
        {
            return TicketNotFound(code);
        }

        return TypedResults.Ok(ToResponse(ticket));
    }

    private static async Task<Results<CreatedAtRoute<TicketResponse>, ProblemHttpResult, ValidationProblem>> CreateTicketAsync(
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

            return TypedResults.CreatedAtRoute(
                value: ToResponse(ticket),
                routeName: "GetTicketByCode",
                routeValues: new { code = ticket.Code });
        }
        catch (DuplicateTicketException exception)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Ticket already exists",
                detail: exception.Message,
                type: "https://api.example.com/problems/ticket-already-exists",
                instance: "/api/tickets");
        }
        catch (ArgumentException exception)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["request"] = [exception.Message]
                });
        }
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteTicketAsync(
            string code,
            TicketService service,
            CancellationToken ct)
    {
        bool deleted = await service.DeleteAsync(code, ct);

        return deleted
            ? TypedResults.NoContent()
            : TicketNotFound(code);
    }

    private static ProblemHttpResult TicketNotFound(string code)
    {
        return TypedResults.Problem(
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