using System;
using System.Collections.Generic;
using System.IO;
using FASTER.core;
using FasterKv.Cache.Core.Abstractions;

namespace FasterKv.Cache.Core.Configurations;

/// <summary>
/// FasterKvCacheOptions
/// for details, see https://microsoft.github.io/FASTER/docs/fasterkv-basics/#fasterkvsettings
/// </summary>
public class FasterKvCacheOptions
{
    /// <summary>
    /// FasterKv index count, Must be power of 2
    /// </summary>
    /// <para>For example: 1024(2^10) 2048(2^11) 65536(2^16) 131072(2^17)</para>
    /// <para>Each index is 64 bits. So this define 131072 keys. Used 1024Kb memory</para>
    public long IndexCount { get; set; } = 131072;

    /// <summary>
    /// FasterKv used memory size
    /// </summary>
    /// <para>Default: 16MB</para>
    public int MemorySizeBit { get; set; } = 24;

    /// <summary>
    /// FasterKv page size
    /// </summary>
    /// <para>Default: 1MB</para>
    public int PageSizeBit { get; set; } = 20;

    /// <summary>
    /// FasterKv read cache used memory size
    /// </summary>
    /// <para>Default: 16MB</para>
    public int ReadCacheMemorySizeBit { get; set; } = 24;

    /// <summary>
    /// FasterKv read cache page size
    /// </summary>
    /// <para>Default: 1MB</para>
    public int ReadCachePageSizeBit { get; set; } = 20;

    /// <summary>
    /// FasterKv commit logs path
    /// </summary>
    /// <para>Default: {CurrentDirectory}/FasterKvCache/{Environment.ProcessId}-HLog </para>
    public string LogPath { get; set; } =
#if NET6_0_OR_GREATER
            Path.Combine(Environment.CurrentDirectory, $"FasterKvCache/{Environment.ProcessId}-HLog");
#else
        Path.Combine(Environment.CurrentDirectory,
            $"FasterKvCache/{System.Diagnostics.Process.GetCurrentProcess().Id}-HLog");
#endif


    /// <summary>
    /// Serializer Name
    /// </summary>
    public string? SerializerName { get; set; }

    /// <summary>
    /// Expiry key scan thread interval
    /// </summary>
    /// <para>Timed deletion of expired keys</para>
    /// <para>Zero or negative numbers are not scanned</para>
    /// <para>Default: 5min</para>
    public TimeSpan ExpiryKeyScanInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Set Custom Store
    /// </summary>
    public FasterBase? CustomStore { get; set; }

    /// <summary>
    /// Gets the extensions.
    /// </summary>
    /// <value>The extensions.</value>
    internal IList<IFasterKvCacheExtensionOptions> Extensions { get; } = new List<IFasterKvCacheExtensionOptions>();

    /// <summary>
    /// Registers the extension.
    /// </summary>
    /// <param name="extension">Extension.</param>
    public void RegisterExtension(IFasterKvCacheExtensionOptions extension)
    {
        extension.ArgumentNotNull(nameof(extension));

        Extensions.Add(extension);
    }

    internal LogSettings GetLogSettings(string? name)
    {
        name ??= "";
        return new LogSettings
        {
            LogDevice = Devices.CreateLogDevice(Path.Combine(LogPath, name) + ".log",
                preallocateFile: true,
                deleteOnClose: true),
            ObjectLogDevice = Devices.CreateLogDevice(Path.Combine(LogPath, name) + ".obj.log",
                preallocateFile: true,
                deleteOnClose: true),
            PageSizeBits = PageSizeBit,
            MemorySizeBits = MemorySizeBit,
            ReadCacheSettings = new ReadCacheSettings
            {
                MemorySizeBits = ReadCacheMemorySizeBit,
                PageSizeBits = ReadCachePageSizeBit,
            }
        };
    }
}