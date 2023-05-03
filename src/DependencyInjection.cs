using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryCore;
public static class DependencyInjection
{
    /// <summary>
    /// Add MemoryCore to the service collection.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="keysComparison">The string comparison type to use when comparing keys.</param>
    /// <returns>Returns <paramref name="services"/>.</returns>
    public static IServiceCollection AddMemoryCore(this IServiceCollection services,
        StringComparison keysComparison = StringComparison.Ordinal)
    {
        services.AddSingleton<IMemoryCore>(new MemoryCoreManager(keysComparison));
        services.AddSingleton<IMemoryCache>(s => s.GetService<IMemoryCore>()!);
        return services;
    }
}
