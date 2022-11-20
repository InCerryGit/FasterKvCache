using System;

namespace FasterKv.Cache.Core;

public interface ISystemClock
{
    DateTimeOffset Now();
}

public sealed class DefaultSystemClock : ISystemClock
{
    public DateTimeOffset Now()
    {
        return DateTimeOffset.Now;
    }
}