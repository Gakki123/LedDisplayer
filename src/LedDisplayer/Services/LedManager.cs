using System.Collections.Concurrent;
using LedDisplayer.Models;

namespace LedDisplayer.Services;

public class LedManager
{
    private readonly ILogger<LedManager> _logger;
    public ConcurrentDictionary<string, Led> Leds { get; } = new ConcurrentDictionary<string, Led>();

    public LedManager(ILogger<LedManager> logger)
    {
        _logger = logger;
    }

    public void Initialize(IEnumerable<Led> ledList)
    {
        Add(ledList);
    }

    public Led? Get(string key)
    {
        Leds.TryGetValue(key, out Led? led);

        return led;
    }

    private void Add(IEnumerable<Led> ledList)
    {
        foreach (Led led in ledList)
        {
            Add(led);
        }
    }

    public bool Add(Led led)
    {
        bool result = Leds.TryAdd(led.Id, led);

        if (result)
        {
            _logger.LogInformation("Added led id: '{LedId}', Current total led count: {LedCount}", led.Id, Leds.Count);
        }

        return result;
    }

    public bool Remove(string id)
    {
        Led? led = Get(id);

        if (led is null)
        {
            return false;
        }

        bool result = Leds.TryRemove(led.Id, out led);

        if (result)
        {
            _logger.LogInformation("Removed led id: '{LedId}', Current total led count: {LedCount}", led!.Id, Leds.Count);
        }

        return result;
    }

    public bool Update(Led led)
    {
        Led? oldLed = Get(led.Id);

        if (oldLed is null)
        {
            return false;
        }

        if (oldLed.Id == led.Id && oldLed.Name == led.Name && oldLed.Ip == led.Ip)
        {
            return false;
        }

        bool result = Leds.TryUpdate(led.Id, led, oldLed);

        if (result)
        {
            _logger.LogInformation("Updated led id: '{LedId}', Current total led count: {LedCount}", led.Id, Leds.Count);
        }

        return result;
    }

    public void Clear()
    {
        Leds.Clear();
    }
}
