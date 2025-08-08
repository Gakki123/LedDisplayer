using System.Text.Json.Serialization;

namespace LedDisplayer.Models;

[Serializable]
public sealed class Led
{
    [JsonPropertyName("OptionID")]
    public int OptionId { get; set; }

    public string OptionName { get; set; }

    [JsonPropertyName("OptionIP")]
    public string OptionIp { get; set; }

    public int RuleCodeWithoutSQ { get; set; }

    public int InterViewFromDB { get; set; }

    public int FontSize { get; set; }

    public string FontName { get; set; }

    public int SpaceCol { get; set; }

    public int SpaceRow { get; set; }

    public bool ShowTitle { get; set; }

    public bool SpaceWithTitle { get; set; }

    public int Ptype { get; set; }

    public int ShowDirection { get; set; }

    public int TxtStayTime { get; set; }

    [JsonPropertyName("LEDColumns")]
    public LedColumns[] LedColumns { get; set; }
}

[Serializable]
public sealed class LedColumns
{
    public string ColsName { get; set; }

    public string ColsTitle { get; set; }

    public int Width { get; set; }

    public int Align { get; set; }

    public int DisplayModeWhileEof { get; set; }
}
