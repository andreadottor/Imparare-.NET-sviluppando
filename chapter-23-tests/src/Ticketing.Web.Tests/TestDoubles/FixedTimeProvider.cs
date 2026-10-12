namespace Ticketing.Web.Tests.TestDoubles;

public sealed class FixedTimeProvider(DateTimeOffset value)
    : TimeProvider
{
    public override DateTimeOffset GetUtcNow()
    {
        return value;
    }
}