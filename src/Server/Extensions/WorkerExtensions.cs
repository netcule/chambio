using Chambio.Server.Abstractions;
using Chambio.Server.Options;
using Chambio.Server.Services;
using Chambio.Server.Workers;
using Microsoft.Net.Http.Headers;

namespace Chambio.Server.Extensions;

public static class WorkerExtensions
{
    public static IServiceCollection AddWorkers(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<WikiOptions>(configuration
            .GetSection(nameof(WikiOptions)));

        services.AddHttpClient<WikiService>();

        services.AddHttpClient<Worker, USWorker>(nameof(USWorker), c =>
        {
            c.BaseAddress = new("https://theunitedstates.io/");
        });

        services.AddHttpClient<Worker, ILWorker>(nameof(ILWorker), c =>
        {
            c.BaseAddress = new("https://knesset.gov.il/Odata/ParliamentInfo.svc/");
            c.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        });

        services.Configure<WorkerOptions>(configuration
            .GetSection(nameof(WorkerOptions)));

        services.AddHostedService<WorkerService>();

        return services;
    }
}
