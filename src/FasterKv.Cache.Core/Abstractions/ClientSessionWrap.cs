using System;
using System.Collections.Concurrent;
using FASTER.core;
using FasterKv.Cache.Core.Abstractions;

namespace FasterKv.Cache.Core;

public class ClientSessionWrap<TValue> : IDisposable
{
    public ClientSession<string, ValueWrapper<TValue>, ValueWrapper<TValue>, ValueWrapper<TValue>,
        StoreContext<ValueWrapper<TValue>>, StoreFunctions<string, ValueWrapper<TValue>>> Session { get; }

    private readonly ConcurrentQueue<ClientSession<string, ValueWrapper<TValue>, ValueWrapper<TValue>,
            ValueWrapper<TValue>, StoreContext<ValueWrapper<TValue>>, StoreFunctions<string, ValueWrapper<TValue>>>>
        _innerPool;

    public ClientSessionWrap(
        ClientSession<string, ValueWrapper<TValue>, ValueWrapper<TValue>, ValueWrapper<TValue>,
            StoreContext<ValueWrapper<TValue>>, StoreFunctions<string, ValueWrapper<TValue>>> clientSession,
        ConcurrentQueue<ClientSession<string, ValueWrapper<TValue>, ValueWrapper<TValue>, ValueWrapper<TValue>,
            StoreContext<ValueWrapper<TValue>>, StoreFunctions<string, ValueWrapper<TValue>>>> innerPool)
    {
        Session = clientSession;
        _innerPool = innerPool;
    }

    public void Dispose()
    {
        Session.CompletePending(true);
        _innerPool.Enqueue(Session);
    }
}

public class ClientSessionWrap : IDisposable
{
    public ClientSession<string, ValueWrapper, ValueWrapper, ValueWrapper,
        StoreContext<ValueWrapper>, StoreFunctions<string, ValueWrapper>> Session { get; }

    private readonly ConcurrentQueue<ClientSession<string, ValueWrapper, ValueWrapper,
            ValueWrapper, StoreContext<ValueWrapper>, StoreFunctions<string, ValueWrapper>>>
        _innerPool;

    public ClientSessionWrap(
        ClientSession<string, ValueWrapper, ValueWrapper, ValueWrapper,
            StoreContext<ValueWrapper>, StoreFunctions<string, ValueWrapper>> clientSession,
        ConcurrentQueue<ClientSession<string, ValueWrapper, ValueWrapper, ValueWrapper,
            StoreContext<ValueWrapper>, StoreFunctions<string, ValueWrapper>>> innerPool)
    {
        Session = clientSession;
        _innerPool = innerPool;
    }

    public void Dispose()
    {
        Session.CompletePending(true);
        _innerPool.Enqueue(Session);
    }
}