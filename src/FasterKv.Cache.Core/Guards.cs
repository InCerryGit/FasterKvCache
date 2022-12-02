using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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
        [NotNull]
        this T? obj,
        [CallerArgumentExpression(nameof(obj))]
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
        [NotNull]
        this string? obj,
        [CallerArgumentExpression(nameof(obj))]
        string? name = null)
        => obj.IsNullOrEmpty() ? throw new ArgumentNullException(name) : obj!;

    /// <summary>
    /// IsNullOrEmpty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullOrEmpty(
        [NotNullWhen(false)]
        this string? str)
        => string.IsNullOrEmpty(str);

    /// <summary>
    /// NotNullOrEmpty
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool NotNullOrEmpty(
        [NotNullWhen(true)]
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
    public static TimeSpan ArgumentNotNegativeOrZero(this TimeSpan timeSpan, [CallerArgumentExpression(nameof(timeSpan))]string? name = null) =>
            timeSpan > TimeSpan.Zero ? timeSpan : throw new ArgumentOutOfRangeException(name);
}