namespace Ticketing.Application.Data;

internal sealed class TicketRow
{
    public string Code { get; set; } = string.Empty;
    public string TicketType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string AffectedService { get; set; } = string.Empty;
}