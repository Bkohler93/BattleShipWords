using System.Collections.Generic;
using System.Text.Json.Serialization;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

public class SetupPlacementAttempt : IServerSendable
{
    [JsonPropertyName("starting_col")]public int StartingCol { get; set; }
    [JsonPropertyName("starting_row")]public int StartingRow { get; set; }
    [JsonPropertyName("placement_direction")]public PlacementDirection PlacementDirection { get; set; }
    [JsonPropertyName("word")]public string Word { get; set; }
}


public class SetupFinalize :IServerSendable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("selected_words")]
    public Dictionary<int,string> SelectedWords { get; set; }

    [JsonPropertyName("word_placements")]
    public Dictionary<int, List<Coordinate>> WordPlacements { get; set; }
}

public class SetupOpponentDone : IServerReceivable
{
    
}

public class StartOrderDecider : IServerReceivable
{
   
}