using Microsoft.Extensions.DependencyInjection;

namespace FasterKv.Cache.Core.Abstractions;

/// <summary>
/// FasterKvCache options extension.
/// </summary>
public interface IFasterKvCacheExtensionOptions
{
    /// <summary>
    /// Adds the services.
    /// </summary>
    /// <param name="services">Services.</param>
    void AddServices(IServiceCollection services, string name);
}