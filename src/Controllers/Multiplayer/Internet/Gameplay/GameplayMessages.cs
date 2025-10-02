using System.Collections.Generic;
using System.Text.Json.Serialization;
using BattleshipWithWords.Services.ConnectionManager.Server;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet.Gameplay;

public class PlayerQuit : IServerReceivable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
}

public class TurnDeciderUIEventMessage : IServerSendable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("ui_event")]
    public IUIEvent IuiEvent { get; set; }
}

public class TurnDeciderUIUpdateMessage : IServerReceivable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("ui_update")]
    public UIUpdate UIUpdate { get; set; }
    
    [JsonPropertyName("turn_decider_results")]
    public Dictionary<string, TurnDeciderResult> TurnDeciderResults { get; set; }
}

public enum TurnDeciderResult
{
    Won,
    Lost,
    Tie,
    WaitingForOtherPlayer,
    OtherPlayerWent,
}

public class GameplayUIEventMessage : IServerSendable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("ui_event")]
    public IUIEvent IuiEvent { get; set; }
}

public class GameplayUIUpdateMessage : IServerReceivable
{
    [JsonPropertyName("user_id")]
    public string UserId { get; set; }
    
    [JsonPropertyName("ui_update")]
    public UIUpdate UIUpdate { get; set; }
    
    [JsonPropertyName("turn_result")]
    public TurnResult TurnResult{ get; set; }
}

public enum TurnResult
{
    GoAgain,
    EndTurn,
    Win,
    Loss,
    NoChange,
}