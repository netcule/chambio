namespace Chambio.Server.Abstractions;

public abstract class Worker
{
    public abstract Task RunAsync(CancellationToken cancellationToken);
}
