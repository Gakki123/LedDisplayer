using System.Text.Json;
using LedDisplayer.Models;
using StackExchange.Redis;

namespace LedDisplayer;

public class LedDisplayService
{
    private readonly LedManager _ledManager;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LedDisplayService> _logger;

    public LedDisplayService(LedManager ledManager, IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration, ILogger<LedDisplayService> logger)
    {
        _ledManager = ledManager;
        _connectionMultiplexer = connectionMultiplexer;
        _configuration = configuration;
        _logger = logger;
    }

    public void DoWork(object? state)
    {
    }

    private IList<LedMessage>? GetLedListMsgFromRedis()
    {
        string? ledListMsgKeyFromRedis = _configuration.GetValue<string>("Led:LedListMsgKeyFromRedis", "LedListMsg");

        if (string.IsNullOrWhiteSpace(ledListMsgKeyFromRedis))
        {
            return null;
        }

        try
        {
            RedisValue redisValue = _connectionMultiplexer.GetDatabase().StringGet(ledListMsgKeyFromRedis);

            if (!redisValue.HasValue)
            {
                return null;
            }

            IList<LedMessage>? list = JsonSerializer.Deserialize<IEnumerable<LedMessage>>(redisValue.ToString())?.ToList();
            return list;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get redis => '{LedListMsgKeyFromRedis}', error message: {ErrorMessage}", ledListMsgKeyFromRedis, e.Message);
            return null;
        }
    }
}
