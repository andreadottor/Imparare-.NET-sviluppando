namespace Ticketing.Application.Tickets;

public sealed class DuplicateTicketException : Exception
{
    public DuplicateTicketException()
    {
    }

    public DuplicateTicketException(string message)
        : base(message)
    {
    }

    public DuplicateTicketException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}