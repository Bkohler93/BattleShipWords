using System.Text.Json.Serialization;
using BattleshipWithWords.Controllers.Multiplayer.Internet;
using BattleshipWithWords.Controllers.Multiplayer.Internet.TurnDecider;

namespace BattleshipWithWords.Services.ConnectionManager.Server;

public enum ServerMessageType
{
   ConnectingMessage,
   Matchmaking,
   Gameplay,
   // ... other types
}

[JsonDerivedType(typeof(AuthenticatedMessage), typeDiscriminator: "AuthenticatedMessage")]
[JsonDerivedType(typeof(PlayerLeftRoom), typeDiscriminator: "PlayerLeftRoom")]
[JsonDerivedType(typeof(PlayerJoinedRoom), typeDiscriminator: "PlayerJoinedRoom")]
[JsonDerivedType(typeof(RoomChanged), typeDiscriminator: "RoomChanged")]
[JsonDerivedType(typeof(RoomFull), typeDiscriminator: "RoomFull")]
[JsonDerivedType(typeof(StartSetup), typeDiscriminator: "StartSetup")]
[JsonDerivedType(typeof(StartOrderDecider), typeDiscriminator: "StartOrderDecider")]
[JsonDerivedType(typeof(SetupOpponentDone), typeDiscriminator: "SetupOpponentDone")]
[JsonDerivedType(typeof(UIUpdateMessage), typeDiscriminator: "UIUpdateMessage")]
public interface IServerReceivable
{
}


public class StartSetup : IServerReceivable
{
   [JsonPropertyName("three_letter_words")] public string[] ThreeLetterWords { get; set; }
   [JsonPropertyName("four_letter_words")] public string[] FourLetterWords { get; set; }
   [JsonPropertyName("five_letter_words")] public string[] FiveLetterWords { get; set; }
}

public class PlayerLeftRoom : IServerReceivable
{
   [JsonPropertyName("user_left_id")] public string UserLeftId { get; set; }
}

public class PlayerJoinedRoom : IServerReceivable
{
   [JsonPropertyName("user_joined_id")] public string UserJoinedId { get; set; }
}

public class RoomChanged : IServerReceivable
{

   [JsonPropertyName("new_room_id")] public string NewRoomId { get; set; }
   [JsonPropertyName("player_count")] public int PlayerCount { get; set; }
   [JsonPropertyName("avg_skill")] public int AverageSkill { get; set; }
}

public class RoomFull : IServerReceivable
{
   [JsonPropertyName("room_id")] public string RoomId { get; set; }
   [JsonPropertyName("player_count")] public int PlayerCount { get; set; }
}

public class AuthenticatedMessage : IServerReceivable
{
   [JsonPropertyName("user_id")] public string UserId { get; set; }
}



[JsonDerivedType(typeof(ConnectingMessage), typeDiscriminator: "ConnectingMessage")]
[JsonDerivedType(typeof(RequestMatchmaking), typeDiscriminator: "RequestMatchmaking")]
[JsonDerivedType(typeof(ExitMatchmaking), typeDiscriminator: "ExitMatchmaking")]
[JsonDerivedType(typeof(ConfirmMatch), typeDiscriminator: "ConfirmMatch")]
[JsonDerivedType(typeof(SetupPlacementAttempt), typeDiscriminator: "SetupPlacementAttempt")]
[JsonDerivedType(typeof(SetupFinalize), typeDiscriminator: "SetupFinalize")]
[JsonDerivedType(typeof(UIEventMessage), typeDiscriminator: "UIEventMessage")]
// [JsonDerivedType(typeof(SetupUndo), typeDiscriminator: "SetupUndo")]
// [JsonDerivedType(typeof(SetupFinalize), typeDiscriminator: "SetupFinalize")]
public interface IServerSendable
{
   
}

public class ConfirmMatch : IServerSendable
{
   [JsonPropertyName("user_id")] public string UserId { get; set; }
   [JsonPropertyName("room_id")] public string RoomId { get; set; }
}

public class ExitMatchmaking : IServerSendable
{
   [JsonPropertyName("user_id")] public string UserId { get; set; }
   [JsonPropertyName("user_skill")] public int UserSkill { get; set; }
}

public class ConnectingMessage : IServerSendable
{
   [JsonPropertyName("jwt_string")] public string JwtString { get; set; }
}

public class RequestMatchmaking : IServerSendable
{
   [JsonPropertyName("user_id")] public string UserId { get; set; }
   [JsonPropertyName("name")] public string Name { get; set; }
   [JsonPropertyName("time_created")] public long TimeCreated { get; set; }
   [JsonPropertyName("skill")] public int Skill { get; set; }
   [JsonPropertyName("region")] public string Region { get; set; }
}

public class GameplaySendMessage : IServerSendable
{
    public int PlayerId { get; set; }
    public string Action { get; set; }
}
