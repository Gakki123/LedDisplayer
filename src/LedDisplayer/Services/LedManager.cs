using System.Collections.Concurrent;
using LedDisplayer.Models;

namespace LedDisplayer;

public class LedManager
{
    private readonly ILogger<LedManager> _logger;

    /// <summary>
    ///     all leds
    /// </summary>
    public ConcurrentDictionary<int, Led> TotalLeds { get; } = new ConcurrentDictionary<int, Led>();

    /// <summary>
    ///     fault leds
    /// </summary>

    public ConcurrentDictionary<int, Led> FaultyLeds { get; } = new ConcurrentDictionary<int, Led>();

    /// <summary>
    ///     normal leds
    /// </summary>
    public ConcurrentDictionary<int, Led> NormalLeds { get; } = new ConcurrentDictionary<int, Led>();

    public LedManager(ILogger<LedManager> logger)
    {
        _logger = logger;
    }
}
