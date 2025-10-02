using System.Text.Json.Serialization;

namespace BattleshipWithWords.Utilities;

public class Coordinate
{
    [JsonPropertyName("row")]
    public int Row { get; set; }
    [JsonPropertyName("col")]
    public int Col { get; set; }
}