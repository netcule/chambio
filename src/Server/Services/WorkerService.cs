using Chambio.Server.Abstractions;
using Chambio.Server.Options;
using Microsoft.Extensions.Options;

namespace Chambio.Server.Services;

public class WorkerService : IHostedService, IDisposable
{
    Timer? _timer;

    readonly WorkerOptions _options;

    readonly IServiceProvider _provider;

    public WorkerService(IOptions<WorkerOptions> options,
        IServiceProvider serviceProvider)
    {
        _options = options.Value;
        _provider = serviceProvider;
    }

    public async Task DoWorkAsync(CancellationToken cancellationToken)
    {
        IServiceScope scope = _provider.CreateScope();

        IEnumerable<Worker> workers = scope.ServiceProvider
            .GetServices<Worker>();

        foreach (Worker worker in workers)
            await worker.RunAsync(cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.Period is not null)
            _timer = new(_ => DoWorkAsync(cancellationToken).Wait(), null,
                TimeSpan.Zero, TimeSpan.FromMinutes((double)_options.Period));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();
}
