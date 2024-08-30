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

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // TODO: this.hostApplicationLifetime.StopApplication();

        return Task.CompletedTask;
    }
}
