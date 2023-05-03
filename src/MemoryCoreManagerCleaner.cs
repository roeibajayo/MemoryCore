using Microsoft.Extensions.Hosting;

namespace MemoryCore;

internal sealed class MemoryCoreManagerCleaner : IHostedService
{
    internal readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(60));
    internal readonly CancellationTokenSource _cts = new();
    internal readonly CancellationToken _token;
    private readonly MemoryCoreManager memoryCore;

    public MemoryCoreManagerCleaner(IMemoryCore memoryCore)
    {
        _token = _cts.Token;

        StartCleanJob();
        this.memoryCore = memoryCore as MemoryCoreManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        StartCleanJob();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Dispose();
        return Task.CompletedTask;
    }

    internal async void StartCleanJob()
    {
        while (true)
        {
            await _timer.WaitForNextTickAsync(_token);

            if (!_token.IsCancellationRequested)
                return;

            memoryCore.ClearExpired();
        }
    }
    private void Dispose()
    {
        _timer.Dispose();
        _cts.Dispose();
        memoryCore.Clear();
    }

}
