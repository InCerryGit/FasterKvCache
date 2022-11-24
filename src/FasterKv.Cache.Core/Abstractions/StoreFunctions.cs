using FASTER.core;

namespace FasterKv.Cache.Core.Abstractions;

internal sealed class StoreContext<TOutput>
{
    private Status _status;
    private TOutput? _output;

    internal void Populate(ref Status status, ref TOutput output)
    {
        _status = status;
        _output = output;
    }

    internal void FinalizeRead(out Status status, out TOutput output)
    {
        status = _status;
        output = _output!;
    }
}

internal sealed class StoreFunctions<TKey, TOutput> : SimpleFunctions<TKey, TOutput, StoreContext<TOutput>>
{
    public override void ReadCompletionCallback(ref TKey key,
        ref TOutput input,
        ref TOutput output,
        StoreContext<TOutput>? ctx,
        Status status,
        RecordMetadata recordMetadata)
    {
        ctx?.Populate(ref status, ref output);
    }
}