namespace Infrastructure.Services;

public sealed class CacheCircuitState
{
    private long _unavailableUntilUtcTicks;

    public bool IsUnavailable(DateTimeOffset now)
    {
        return Volatile.Read(ref _unavailableUntilUtcTicks) > now.UtcDateTime.Ticks;
    }

    public DateTimeOffset MarkUnavailable(
        DateTimeOffset now,
        TimeSpan duration)
    {
        var unavailableUntil = now.Add(duration);
        Interlocked.Exchange(
            ref _unavailableUntilUtcTicks,
            unavailableUntil.UtcDateTime.Ticks);

        return unavailableUntil;
    }

    public void MarkAvailable()
    {
        Interlocked.Exchange(ref _unavailableUntilUtcTicks, 0);
    }
}
