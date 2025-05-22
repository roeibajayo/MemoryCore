using MemoryCore.Persistent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        var typeofIMemoryCore = typeof(IMemoryCore);
        if (services.Any(s => s.ServiceType == typeofIMemoryCore))
            return services;

        var typeofIPersistedStore = typeof(IPersistedStore);
        if (!services.Any(s => s.ServiceType == typeofIPersistedStore))
            services.AddSingleton<IPersistedStore, JsonPersistedStore>();

        services.RemoveAll<IMemoryCore>();
        services.AddSingleton<IMemoryCore>(s =>
        {
            var store = s.GetService<IPersistedStore>();
            return new MemoryCoreManager(MemoryCoreManager.DEFAULT_NAME, keysComparison, store);
        });

        // replace the IMemoryCache service
        services.RemoveAll<IMemoryCache>();
        services.AddSingleton<IMemoryCache>(s => s.GetService<IMemoryCore>()!);

        return services;
    }
}
