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
   public void Receive(IServerReceivable message);
   public void Disconnecting();
   void HttpResponse(string response);
}

public partial class ServerConnectionManager:Node
{
   private HttpRequest _httpRequest;
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
      
      _httpRequest = new HttpRequest();
      AddChild(_httpRequest);
      _httpRequest.RequestCompleted += _httpRequestCompleted;
   }

   private void _httpRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
   {
     if (responseCode == 200)
     {
        Listener.HttpResponse(body.GetStringFromUtf8());
     }
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
         var msg = JsonSerializer.Deserialize<IServerReceivable>(bytes, JsonSerializerOptions);
         Listener.Receive(msg);
      }
   }
   
   // private void RouteMessage()

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

   public Error Send(IServerSendable req)
   {
      var text = JsonSerializer.Serialize(req);
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

   public Error HttpGet(string url)
   {
      return _httpRequest.Request(url);
   }
}

