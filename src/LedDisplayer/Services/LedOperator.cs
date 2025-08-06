using System.Collections.Concurrent;
using LedDisplayer.Models;

namespace LedDisplayer.Services;

public sealed class LedOperator
{
    public ConcurrentDictionary<string, Led> Leds => LedManager.Leds;

    private LedManager LedManager { get; }

    public LedOperator(LedManager ledManager)
    {
        LedManager = ledManager;
    }
}
