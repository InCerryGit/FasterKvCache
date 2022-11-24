using System;

namespace FasterKv.Cache.Core.Abstractions;

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