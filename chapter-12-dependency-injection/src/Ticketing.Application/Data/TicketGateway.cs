using Ticketing.Application.Tickets;

namespace Ticketing.Application.Data;

public sealed class TicketGateway
{
    public async Task SaveAsync(Ticket ticket, CancellationToken ct)
    {
        await Task.Delay(300, ct);

        if (ticket.Code.Equals("INC-FAIL", StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException("Il sistema esterno non è disponibile.");
        }
    }

    public async Task WaitForResponseAsync(CancellationToken ct)
    {
        await Task.Delay(200, ct);
    }
}