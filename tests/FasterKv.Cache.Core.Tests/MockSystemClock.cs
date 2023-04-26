using FasterKv.Cache.Core.Abstractions;

namespace FasterKv.Cache.Core.Tests;

public class MockSystemClock : ISystemClock
{
    private DateTimeOffset _now;

    public MockSystemClock(DateTimeOffset now)
    {
        _now = now;
    }

    public DateTimeOffset Now()
    {
        return _now;
    }

    public long NowUnixTimestamp()
    {
        return _now.ToUnixTimeMilliseconds();
    }
    
    public void AddSeconds(int seconds)
    {
        _now = _now.AddSeconds(seconds);
    }
}