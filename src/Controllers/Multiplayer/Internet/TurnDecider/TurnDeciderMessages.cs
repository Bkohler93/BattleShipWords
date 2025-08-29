using System.Text.Json.Serialization;
using BattleshipWithWords.Services.ConnectionManager.Server;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.TurnDecider;

public class UIEventMessage : IServerSendable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("ui_event")]
    public UIEvent UIEvent { get; set; }
}

public class UIUpdateMessage : IServerReceivable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("ui_update")]
    public UIUpdate UIUpdate { get; set; }
}