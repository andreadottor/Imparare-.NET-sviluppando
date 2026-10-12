using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
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
               .RequireAuthorization("ManageTickets")
               .WithSummary("Creates an incident ticket")
               .WithDescription("Returns 409 when the ticket code already exists.")
               .ProducesValidationProblem()
               .ProducesProblem(StatusCodes.Status409Conflict);

        tickets.MapPut("/{code}", UpdateTicketAsync)
               .RequireAuthorization("ManageTickets")
               .WithSummary("Updates the editable fields of an incident ticket")
               .WithDescription("Preserves code, creation date and lifecycle. Returns 204 on success, 400 for invalid input, or 404 when missing.")
               .ProducesValidationProblem()
               .ProducesProblem(StatusCodes.Status404NotFound)
               .Produces(StatusCodes.Status401Unauthorized)
               .Produces(StatusCodes.Status403Forbidden);

        tickets.MapPost("/{code}/close", CloseTicketAsync)
               .RequireAuthorization("ManageTickets")
               .WithSummary("Closes a ticket")
               .WithDescription("Returns 204 for a closed ticket, including repeated requests which preserve the first closure date; returns 404 when missing.")
               .ProducesProblem(StatusCodes.Status404NotFound)
               .Produces(StatusCodes.Status401Unauthorized)
               .Produces(StatusCodes.Status403Forbidden);

        tickets.MapDelete("/{code}", DeleteTicketAsync)
               .RequireAuthorization("ManageTickets")
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

    private static async Task<Results<Ok<TicketResponse>, ProblemHttpResult>> GetTicketAsync(
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
            IncidentTicket ticket = await service.CreateIncidentAsync(
                request.Code,
                request.Title,
                request.Description,
                request.Priority,
                request.AffectedService,
                ct);

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

    private static async Task<Results<NoContent, ProblemHttpResult, ValidationProblem>> UpdateTicketAsync(
            string code,
            UpdateIncidentTicketRequest request,
            TicketService service,
            CancellationToken ct)
    {
        try
        {
            bool updated = await service.UpdateIncidentAsync(code, request.Title,
                request.Description, request.Priority, request.AffectedService, ct);

            return updated ? TypedResults.NoContent() : TicketNotFound(code);
        }
        catch (ArgumentException exception)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["request"] = [exception.Message]
            });
        }
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> CloseTicketAsync(
            string code,
            TicketService service,
            CancellationToken ct)
    {
        bool closed = await service.CloseAsync(code, ct);
        return closed ? TypedResults.NoContent() : TicketNotFound(code);
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
                incident.AffectedService,
                incident.Status,
                incident.ClosedAt),
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

public sealed record UpdateIncidentTicketRequest(
    [property: Required, MaxLength(200)] string Title,
    [property: Required(AllowEmptyStrings = true)] string Description,
    [property: Range(1, 3)] int Priority,
    [property: Required, MaxLength(100)] string AffectedService);

public sealed record TicketResponse(
    string Code,
    string Type,
    string Title,
    string Description,
    int Priority,
    DateTimeOffset CreatedAt,
    string AffectedService,
    [property: JsonConverter(typeof(JsonStringEnumConverter<TicketStatus>))] TicketStatus Status,
    DateTimeOffset? ClosedAt);
