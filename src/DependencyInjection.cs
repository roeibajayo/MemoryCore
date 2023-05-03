using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace MemoryCore;
public static class DependencyInjection
{
    public static IServiceCollection AddMemoryCore(this IServiceCollection services,
        StringComparison keysComparison = StringComparison.Ordinal)
    {
        services.AddSingleton<IMemoryCore>(new MemoryCoreManager(keysComparison));
        services.AddSingleton<IMemoryCache>(s => s.GetService<IMemoryCore>()!);
        services.AddHostedService<MemoryCoreManagerCleaner>();
        return services;
    }
}
