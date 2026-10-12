using Ticketing.Application.Data;
using Ticketing.Application.Tickets;

namespace Ticketing.Application.Services;

public sealed class TicketService
{
    private readonly TicketRegistry<Ticket> _registry;
    private readonly TicketGateway _gateway;

    public TicketService(TicketRegistry<Ticket> registry, TicketGateway gateway)
    {
        _registry = registry;
        _gateway = gateway;
    }

    /// <summary>
    /// Registra e salva un nuovo ticket.
    /// </summary>
    /// <param name="ticket">Il ticket da registrare.</param>
    /// <param name="ct">
    /// Il token usato per richiedere l'annullamento.
    /// </param>
    /// <returns>Il task che rappresenta l'operazione.</returns>
    /// <exception cref="DuplicateTicketException">
    /// Viene generata quando il codice è già presente.
    /// </exception>
    /// <exception cref="IOException">
    /// Viene generata quando il salvataggio non riesce.
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// Viene generata quando l'operazione viene annullata.
    /// </exception>
    public async Task RegisterAsync(Ticket ticket, CancellationToken ct = default)
    {
        _registry.Add(ticket);

        try
        {
            await _gateway.SaveAsync(ticket, ct);
        }
        catch
        {
            _registry.Remove(ticket.Code);
            throw;
        }
    }

    /// <summary>
    /// Cerca un ticket tramite il codice.
    /// </summary>
    /// <param name="code">Il codice da cercare.</param>
    /// <param name="ct">
    /// Il token usato per richiedere l'annullamento.
    /// </param>
    /// <returns>
    /// Il ticket trovato oppure <see langword="null"/>.
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// Viene generata quando l'operazione viene annullata.
    /// </exception>
    public async Task<Ticket?> FindAsync(string code, CancellationToken ct = default)
    {
        await _gateway.WaitForResponseAsync(ct);

        _registry.TryGet(code, out Ticket? ticket);
        return ticket;
    }
}
