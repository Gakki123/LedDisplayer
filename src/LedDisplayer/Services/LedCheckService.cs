using System.Text.Json;
using LedDisplayer.Models;
using StackExchange.Redis;

// ReSharper disable InvertIf

namespace LedDisplayer.Services;

public sealed class LedCheckService : BackgroundService
{
    private readonly LedManager _ledManager;
    private readonly IDatabase _database;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LedDisplayService> _logger;

    private bool _isInitialized;

    public LedCheckService(LedManager ledManager, IConfiguration configuration, IConnectionMultiplexer connectionMultiplexer, ILogger<LedDisplayService> logger)
    {
        _ledManager = ledManager;
        _database = connectionMultiplexer.GetDatabase();
        _configuration = configuration;
        _logger = logger;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        IList<Led>? list = GetLedsFromRedis();

        if (list != null)
        {
            _ledManager.Initialize(list);
        }

        _isInitialized = true;

        return base.StartAsync(cancellationToken);
    }

    private IList<Led>? GetLedsFromRedis()
    {
        string ledListKeyFromRedis = _configuration.GetValue<string>("Led:LedListKeyFromRedis", "LedList");

        RedisValue redisValue = _database.StringGet(ledListKeyFromRedis);

        if (redisValue.HasValue)
        {
            IList<Led>? list = JsonSerializer.Deserialize<IEnumerable<Led>>(redisValue.ToString())?.ToList();
            return list;
        }

        return null;
    }

    /// <inheritdoc />
    // ReSharper disable once CognitiveComplexity
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested && _isInitialized)
        {
            bool checkUpdate = _configuration.GetValue<bool>("Led:CheckUpdate", true);

            if (checkUpdate)
            {
                IList<Led>? list = GetLedsFromRedis();

                if (list != null)
                {
                    foreach (Led led in list)
                    {
                        Led? oldLed = _ledManager.Get(led.Id);

                        if (oldLed is null)
                        {
                            _ledManager.Add(led);
                        }
                        else if (oldLed.Name != led.Name || oldLed.Ip != led.Ip)
                        {
                            _ledManager.Update(led);
                        }
                    }

                    IEnumerable<KeyValuePair<string, Led>> canRemoveLeds = _ledManager.Leds.Where(c => list.All(l => l.Id != c.Key));

                    foreach (KeyValuePair<string, Led> led in canRemoveLeds)
                    {
                        _ledManager.Remove(led.Key);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _ledManager.Clear();

        return base.StopAsync(cancellationToken);
    }
}
