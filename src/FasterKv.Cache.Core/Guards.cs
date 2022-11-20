using System;
using System.Runtime.CompilerServices;
#if (NET6_0 || NET7_0)
using System.Diagnostics.CodeAnalysis;
#endif

namespace FasterKv.Cache.Core;

public static class Guards
{
    /// <summary>
    /// ArgumentNotNull
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T ArgumentNotNull<T>(
#if (NET6_0 || NET7)
        [NotNull]
#endif
        this T? obj,
#if (NET6_0 || NET7)
        [CallerArgumentExpression(nameof(obj))]
#endif
        string? name = null)
        => obj ?? throw new ArgumentNullException(name);    
    
    /// <summary>
    /// ArgumentNotNull
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ArgumentNotNullOrEmpty(
#if (NET6_0 || NET7)
        [NotNull]
#endif
        this string? obj,
#if (NET6_0 || NET7)
        [CallerArgumentExpression(nameof(obj))]
#endif
        string? name = null)
        => obj.IsNullOrEmpty() ? throw new ArgumentNullException(name) : obj!;

    /// <summary>
    /// IsNullOrEmpty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(
#if (NET6_0 || NET7_0)
        [NotNullWhen(false)]
#endif
        this string? str)
        => string.IsNullOrEmpty(str);

    /// <summary>
    /// NotNullOrEmpty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotNullOrEmpty(
#if (NET6_0 || NET7_0)
        [NotNullWhen(true)]
#endif
        this string? str)
        => !string.IsNullOrEmpty(str);

    /// <summary>
    /// Argument Not Negative Or Zero
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static TimeSpan ArgumentNotNegativeOrZero(this TimeSpan timeSpan, string? name) =>
            timeSpan > TimeSpan.Zero ? timeSpan : throw new ArgumentOutOfRangeException(name);
}