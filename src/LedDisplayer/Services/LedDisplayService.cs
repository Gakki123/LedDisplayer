using StackExchange.Redis;

namespace LedDisplayer.Services;

public class LedDisplayService : BackgroundService
{
    private readonly LedManager _ledManager;
    private readonly IDatabase _database;
    private readonly ILogger<LedDisplayService> _logger;

    public LedDisplayService(LedManager ledManager, IConnectionMultiplexer connectionMultiplexer, ILogger<LedDisplayService> logger)
    {
        _ledManager = ledManager;
        _database = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && !_ledManager.Leds.IsEmpty)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}
