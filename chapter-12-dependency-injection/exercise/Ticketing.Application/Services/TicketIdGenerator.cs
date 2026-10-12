namespace Ticketing.Application.Services;

public sealed class TicketIdGenerator
{
    public string Create()
    {
        return Guid.NewGuid().ToString("N");
    }
}
