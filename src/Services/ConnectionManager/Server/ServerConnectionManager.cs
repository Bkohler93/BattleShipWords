using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace BattleshipWithWords.Services.ConnectionManager.Server;

public interface IServerConnectionListener
{
   public void Connecting();
   public void Connected();
   public void UnableToConnect();
   public void Disconnected();
   public void Reconnecting();
   public void Reconnected();
   public void Receive(BaseServerReceiveMessage message);
   public void Disconnecting();
}

public partial class ServerConnectionManager:Node
{
   private WebSocketPeer _websocketPeer;
   private WebSocketPeer.State _websocketState;
   private ServerConnectionStateMachine _stateMachine;
   private Timer _reconnectTimer;
   private int _reconnectCounter;
   private int _reconnectWaitPeriod = 2;
   private int _reconnectMaxTries = 3;
   
   private string _url;
   private TlsOptions _tlsOptions; 
   
   public IServerConnectionListener Listener;

   private static readonly JsonSerializerOptions JsonSerializerOptions = CreateSerializerOptions();

   private static JsonSerializerOptions CreateSerializerOptions()
   {
      var options = new JsonSerializerOptions
      {
         WriteIndented = true, 
         PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
         Converters = { new JsonStringEnumConverter() }
      };
      return options; 
   }

   public ServerConnectionManager()
   {
      _websocketPeer = new WebSocketPeer();
      _stateMachine = new ServerConnectionStateMachine(this);
   }

   public void SetListener(IServerConnectionListener serverConnectionListener)
   {
      Listener = serverConnectionListener;
   }

   public override void _Process(double delta)
   {
      PollAndProcessConnection(delta);
   }

   public override void _Notification(int what)
   {
      switch (what)
      {
         case (int)NotificationApplicationPaused:
            GD.Print($"ServerConnectionManager: Application Paused");
            break;
      }
   }

   public override void _Ready()
   {
      _reconnectTimer = new Timer();
      _reconnectTimer.SetWaitTime(_reconnectWaitPeriod);
      _reconnectTimer.Timeout += _reconnect;
      AddChild(_reconnectTimer);
   }

   private void _reconnect()
   {
      _reconnectCounter++;
      var err = _websocketPeer.ConnectToUrl(_url, _tlsOptions);
      GD.Print($"ServerConnectionManager: error trying to reconnect -- {err}");
   }

   private void PollAndProcessConnection(double delta)
   {
      _websocketPeer.Poll();

      var newState = _websocketPeer.GetReadyState();
      
      if (newState != _websocketState)
      {
         _stateMachine.HandleNewWebsocketState(newState);
         _websocketState = newState;
      }

      if (_websocketState != WebSocketPeer.State.Open) return;
      while (_websocketPeer.GetAvailablePacketCount() > 0)
      {
         var bytes = _websocketPeer.GetPacket();
         GD.Print($"received --- {bytes.GetStringFromUtf8()}");
            
         var msg = JsonSerializer.Deserialize<BaseServerReceiveMessage>(bytes, JsonSerializerOptions);
         Listener.Receive(msg);
      }
   }

   public WebSocketPeer.State State => _websocketState;
   public bool RetriedMaxTimes => _reconnectCounter > _reconnectMaxTries;

   public Error Connect(string url, TlsOptions tlsOpts)
   {
      _url = url;
      _tlsOptions = tlsOpts;
      var err = _websocketPeer.ConnectToUrl(url, tlsOpts);
      return err;
   }

   public void Close()
   {
      _websocketPeer?.Close();
   }

   public Error Send(BaseServerSendMessage req)
   {
      var options = new JsonSerializerOptions
         {
             WriteIndented = false,
             // Optional: Serialize enum as string
             Converters = { new JsonStringEnumConverter<ServerMessageType>() }
         };
      var text = JsonSerializer.Serialize(req, options);
      return _websocketPeer.SendText(text);
   }

   public void StartReconnecting()
   {
      Listener?.Reconnecting();
      _reconnectTimer.Start();
   }

   public void RetryReconnecting()
   {
      _reconnectCounter++;
      _reconnectTimer.Start(_reconnectWaitPeriod * _reconnectCounter);
   }
}

public enum ServerMessageType
{
   Matchmaking,
   Gameplay,
   // ... other types
}

public class BaseServerReceiveMessage
{
   public ServerMessageType Type { get; set; }
   public IServerReceivable Payload { get; set; }
}

[JsonDerivedType(typeof(MatchmakingResponse), typeDiscriminator: "MatchmakingResponse")]
[JsonDerivedType(typeof(GameplayReceiveMessage), typeDiscriminator: "GameplayMessage")] // Example for another type
public interface IServerReceivable
{
}

public class MatchmakingResponse : IServerReceivable
{
   [JsonPropertyName("user_one_name")]
   public string UserOneName { get; set; }

   [JsonPropertyName("user_two_name")]
   public string UserTwoName { get; set; }
   
   [JsonPropertyName("user_one_id")]
   public Guid UserOneId { get; set; }
   
   [JsonPropertyName("user_two_id")]
   public Guid UserTwoId { get; set; }
}

public class GameplayReceiveMessage : IServerReceivable {}

public class BaseServerSendMessage
{
   public ServerMessageType Type { get; set; }
   public IServerSendable Payload { get; set; }
}

[JsonDerivedType(typeof(MatchmakingRequest), typeDiscriminator: "MatchmakingRequest")]
[JsonDerivedType(typeof(GameplaySendMessage), typeDiscriminator: "GameplayMessage")] // Example for another type
public interface IServerSendable
{
}

public class MatchmakingRequest : IServerSendable
{
    public string Name { get; set; }
    
    public static BaseServerSendMessage New(string name)
    {
       var req = new MatchmakingRequest
       {
          Name = name,
       };
       return new BaseServerSendMessage
       {
          Type = ServerMessageType.Matchmaking,
          Payload = req
       };
    }
}

public class GameplaySendMessage : IServerSendable
{
    public int PlayerId { get; set; }
    public string Action { get; set; }
}
