using System.Net.NetworkInformation;
using System.Text.Json;
using KellermanSoftware.CompareNetObjects;
using LedDisplayer.Models;
using StackExchange.Redis;

// ReSharper disable InvertIf

namespace LedDisplayer;

public sealed class LedCheckService
{
    private readonly LedManager _ledManager;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly IConfiguration _configuration;
    private readonly ICompareLogic _compareLogic;
    private readonly ILogger<LedCheckService> _logger;

    private static readonly List<bool> Changed = new List<bool>();

    public LedCheckService(LedManager ledManager, IConnectionMultiplexer connectionMultiplexer, IConfiguration configuration, ICompareLogic compareLogic, ILogger<LedCheckService> logger)
    {
        _ledManager = ledManager;
        _connectionMultiplexer = connectionMultiplexer;
        _configuration = configuration;
        _compareLogic = compareLogic;
        _logger = logger;
    }


    public void DoWork(object? state)
    {
        IList<Led>? list = GetLedListFromRedis();

        Changed.Clear();

        if (list != null)
        {
            Check(list);

            PingTest();

            if (Changed.Any(_ => true))
            {
                if (_ledManager.FaultyLeds.IsEmpty)
                {
                    _logger.LogInformation("Current total led count: {LedTotalCount}, Faulty led count: {FaultyCount}", _ledManager.TotalLeds.Count, _ledManager.FaultyLeds.Count);
                }
                else
                {
                    _logger.LogInformation("Current total led count: {LedTotalCount}, Faulty led count: {FaultyCount}, Faulty leds: {FaultyLeds}",
                        _ledManager.TotalLeds.Count,
                        _ledManager.FaultyLeds.Count,
                        _ledManager.FaultyLeds.Select(c => c.Key));
                }
            }
        }
    }

    private void Check(IList<Led> list)
    {
        foreach (Led led in list)
        {
            _ledManager.TotalLeds.TryGetValue(led.OptionId, out Led? oldLed);

            if (oldLed is null)
            {
                bool result = _ledManager.TotalLeds.TryAdd(led.OptionId, led);

                if (result)
                {
                    _logger.LogInformation("Added led id: '{LedId}'", led.OptionId);
                }

                Changed.Add(result);
            }
            //else if (oldLed.OptionName != led.OptionName || oldLed.OptionIp != led.OptionIp)
            else if (!_compareLogic.Compare(oldLed, led).AreEqual)
            {
                bool result = _ledManager.TotalLeds.TryUpdate(led.OptionId, led, oldLed);

                if (result)
                {
                    _logger.LogInformation("Updated led id: '{LedId}'", led.OptionId);
                }

                Changed.Add(result);
            }
        }

        IEnumerable<KeyValuePair<int, Led>> canRemoveLeds = _ledManager.TotalLeds.Where(c => list.All(l => l.OptionId != c.Key)).ToList();

        foreach (KeyValuePair<int, Led> pair in canRemoveLeds)
        {
            bool result = _ledManager.TotalLeds.TryRemove(pair.Value.OptionId, out _);

            if (result)
            {
                _logger.LogInformation("Removed led id: '{LedId}'", pair.Value.OptionId);
            }

            Changed.Add(result);
        }
    }

    private void PingTest()
    {
        using (Ping pingSender = new Ping())
        {
            foreach (KeyValuePair<int, Led> led in _ledManager.TotalLeds)
            {
                try
                {
                    PingReply reply = pingSender.Send(led.Value.OptionIp);

                    if (reply.Status == IPStatus.Success)
                    {
                        _ledManager.NormalLeds.TryAdd(led.Key, led.Value);
                        _ledManager.FaultyLeds.TryRemove(led.Key, out Led _);
                    }
                    else
                    {
                        _ledManager.NormalLeds.TryRemove(led.Key, out Led _);
                        _ledManager.FaultyLeds.TryAdd(led.Key, led.Value);
                    }
                }
                catch
                {
                    _ledManager.NormalLeds.TryRemove(led.Key, out Led _);
                    _ledManager.FaultyLeds.TryAdd(led.Key, led.Value);
                }
            }
        }
    }

    private IList<Led>? GetLedListFromRedis()
    {
        string? ledListKeyFromRedis = _configuration.GetValue<string>("Led:LedListKeyFromRedis", "LedList");

        if (string.IsNullOrWhiteSpace(ledListKeyFromRedis))
        {
            return null;
        }

        try
        {
            RedisValue redisValue = _connectionMultiplexer.GetDatabase(0).StringGet(ledListKeyFromRedis);

            if (!redisValue.HasValue)
            {
                return null;
            }

            IList<Led>? list = JsonSerializer.Deserialize<IEnumerable<Led>>(redisValue.ToString())?.ToList();
            return list;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to get redis => '{LedListKeyFromRedis}', error message: {ErrorMessage}", ledListKeyFromRedis, e.Message);
            return null;
        }
    }
}
