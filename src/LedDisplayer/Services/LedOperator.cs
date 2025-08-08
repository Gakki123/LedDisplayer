using System.Collections.Concurrent;
using LedDisplayer.Models;

namespace LedDisplayer;

public sealed class LedOperator
{
    public ConcurrentDictionary<int, Led> Leds => LedManager.TotalLeds;

    private LedManager LedManager { get; }

    public LedOperator(LedManager ledManager)
    {
        LedManager = ledManager;
    }
}
