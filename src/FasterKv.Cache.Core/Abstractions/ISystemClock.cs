using System;

namespace FasterKv.Cache.Core.Abstractions;

public interface ISystemClock
{
    DateTimeOffset Now();

    long NowUnixTimestamp();
}

public sealed class DefaultSystemClock : ISystemClock
{
    public DateTimeOffset Now()
    {
        return DateTimeOffset.Now;
    }

    public long NowUnixTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}