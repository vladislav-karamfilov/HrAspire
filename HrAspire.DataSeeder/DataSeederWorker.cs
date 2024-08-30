namespace HrAspire.DataSeeder;

using System.Threading;
using System.Threading.Tasks;

public class DataSeederWorker : BackgroundService
{
    private readonly IHostApplicationLifetime hostApplicationLifetime;

    public DataSeederWorker(IHostApplicationLifetime hostApplicationLifetime)
    {
        this.hostApplicationLifetime = hostApplicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: migrate and seed data
        await Task.CompletedTask;

        this.hostApplicationLifetime.StopApplication();
    }
}
