using System;
using BattleshipWithWords.Utilities;
using Godot;
using Godot.Collections;
using Timer = System.Timers.Timer;

namespace androidplugintest.ConnectionManager;

internal enum Role
{
    Server,
    Client
}

internal enum Status
{
    Initializing,
    Active,
    Reconnecting,
    Resuming,
    Disconnected,
}

public class Result {
    public bool Success { get; }
    public string Error { get; }

    protected Result(bool success, string error = null) {
        Success = success;
        Error = error;
    }

    public static Result Ok() => new Result(true);
    public static Result Fail(string error) => new Result(false, error);
}

public class Result<T> : Result {
    public T Value { get; }

    private Result(T value) : base(true) {
        Value = value;
    }

    private Result(string error) : base(false, error) {}

    public static Result<T> Ok(T value) => new Result<T>(value);
    public new static Result<T> Fail(string error) => new Result<T>(error);
}

public interface IP2PPeerService
{
    public void Disconnect();
    public void DeferredDisconnect();
    public bool IsConnected();
    public bool IsHost();
    public Result ConnectAsClient(string ip, int port);
    public Result<int> CreateServer();
    public void DisconnectPeer();
    public void SendGameChan(Dictionary message);
    public void SendAppChan(string message);
    event Action ConnectedToServer;
    event Action ConnectionFailed;
    event Action<long> PeerConnected;
    event Action<long> PeerDisconnected;
    event Action ServerDisconnected;
    event Action<Dictionary> GameMessageReceived;
    event Action<string> AppMessageReceived;
    event Action ApplicationResumed;
    public void Shutdown();
}

public interface IP2PConnectionListener
{
    public void Connected();
    public void UnableToConnect();
    public void Reconnected();
    public void ReconnectFailed();
    public void Disconnected();
    public void Receive(Dictionary message);
}

public class P2PConnectionManager
{
    public Action OnConnectionResumed;
    private IP2PConnectionListener _listener;
    private IP2PPeerService _peerService;
    private Role _role;
    private Status _status = Status.Disconnected;
    private string _ip;
    private int _port;
    // private Timer _pongTimer;
    // private Timer _keepAliveTimer;
    private Timer _reconnectTimer;
    private bool _isPeerAppPaused = false;

    private double _reconnectWaitTime = 5.0 * 1000; //ms
    // private double _pongWaitTime = 2.0; // seconds //TODO adjust based on RTT
    // private double _keepAliveWaitTime = 2.0; // seconds

    public P2PConnectionManager(IP2PPeerService peer)
    {
        _peerService = peer;
        _setupTimers();
        _peerService.ApplicationResumed += () =>
        {
           _transitionTo(Status.Resuming); 
        };
        
        _peerService.ConnectedToServer += () =>
        {
            // Starting -> do nothing
            // Active -> do nothing
            // Reconnecting -> do nothing
            // Disconnected -> do nothing
        };
        _peerService.ConnectionFailed += () =>
        {
            switch (_status)
            {
                case Status.Initializing:
                    _peerService.Disconnect();
                    _listener.UnableToConnect();
                    _transitionTo(Status.Disconnected);
                    break;
                case Status.Active:
                    throw _exception("Connection Failed and status is Active");
                case Status.Reconnecting:
                    if (_role == Role.Client)
                    {
                        _peerService.Disconnect();
                        _transitionTo(Status.Reconnecting);
                    }
                    else
                    {
                        throw _exception("Connection Failed sent to Host");
                    }
                    break;
                case Status.Disconnected:
                    throw _exception("Connection failed while status is Disconnected");
                case Status.Resuming:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
        _peerService.PeerConnected += (id) =>
        {
            switch (_status)
            {
                case Status.Initializing:
                    GD.Print($"[ConnectionManager] Peer connected with id {id}");
                    _transitionTo(Status.Active);
                    _listener.Connected();
                    break;     
                case Status.Active:
                    throw _exception($"ConnectionManager should not be active when Peer {id} connects");
                case Status.Reconnecting:
                    _listener.Reconnected();
                    _peerService.SendAppChan("resume");
                    // _transitionTo(Status.Active);
                    break;
                case Status.Disconnected:
                    throw _exception("ConnectionManager should not be disconnected when Peer disconnects");
                case Status.Resuming:
                    _peerService.SendAppChan("resume");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        };
        _peerService.PeerDisconnected += id =>
        {
            switch (_status)
            {
                case Status.Initializing:
                    throw _exception($"[ConnectionManager] ConnectionManager should not be starting when Peer {id} disconnects");
                case Status.Active:
                    _listener.Disconnected();
                    _transitionTo(Status.Reconnecting);
                    break;
                case Status.Reconnecting:
                    break;
                case Status.Disconnected:
                    throw _exception($"ConnectionManager should not be disconnected when Peer {id} disconnects");
                case Status.Resuming:
                    _transitionTo(Status.Reconnecting);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };
        _peerService.ServerDisconnected += () =>
        {
            if (_peerService.IsHost())
            {
                _peerService.CreateServer();
            }
            _transitionTo(Status.Reconnecting);
            GD.Print("[ConnectionManager] Server disconnected");
        };
        _peerService.GameMessageReceived += (msg) =>
        {
            _listener.Receive(msg);
        };
        _peerService.AppMessageReceived += (msg) =>
        {
            switch (msg)
            {
                case "pause":
                    _isPeerAppPaused = true;
                    break;
                case "resume":
                    _peerService.SendAppChan("resume_ack");
                    _isPeerAppPaused = false;
                    break;
                case "resume_ack":
                    _transitionTo(Status.Active);
                    break;
            }
        };
        
        // _peerService.ApplicationPaused += () =>
        // {
        //     Disconnect();
        // };
        // _peerService.PeerAppPaused += () =>
        // {
        //     Disconnect();
        //     Reconnect();
        // };
    }

    public void SetListener(IP2PConnectionListener listener)
    {
        _listener = listener;
    }

    public bool IsHost()
    {
        return _role == Role.Server;
    }

    public bool IsConnected()
    {
        return _peerService.IsConnected();
    }

    public void Disable()
    {
        if (_peerService.IsConnected())
        {
            GD.Print("disconnecting PeerService... it is connected");
            _peerService.Disconnect();
        }
        _transitionTo(Status.Disconnected);
    }
    
    public void DeferredDisable()
    {
        if (_peerService.IsConnected())
            _peerService.DeferredDisconnect();
        _transitionTo(Status.Disconnected);
    }

    
    public void Disconnect()
    {
        if (_peerService.IsConnected())
            _peerService.Disconnect(); 
        _transitionTo(Status.Disconnected);
    }

    private void _setupTimers()
    {
        // _pongTimer = new Timer();
        // _pongTimer.OneShot = true;
        // _pongTimer.Autostart = false;
        // _pongTimer.Timeout += _handlePongTimeout;
        // AddChild(_pongTimer);
        //
        // _keepAliveTimer = new Timer();
        // _keepAliveTimer.OneShot = true;
        // _keepAliveTimer.Autostart = false; 
        // _keepAliveTimer.Timeout += _handleKeepAliveTimeout;
        // AddChild(_keepAliveTimer);
        
        _reconnectTimer = new Timer();
        _reconnectTimer.Interval = _reconnectWaitTime;
        _reconnectTimer.AutoReset = false;
        _reconnectTimer.Elapsed += (sender, args) =>
        {
            if (_isPeerAppPaused)
            {
                _reconnectTimer.Interval = _reconnectWaitTime;
                _reconnectTimer.Enabled = true;
                _reconnectTimer.Start();
            }
            else
            {
                _peerService.DeferredDisconnect(); 
                _transitionTo(Status.Disconnected);
                _listener.ReconnectFailed();
            }
        };
    }

    private Exception _exception(string msg)
    {
        return new Exception($"[ENetConnectionManager] {msg}");
    }

    private void _transitionTo(Status newStatus)
    {
        //logic to perform before transition
        switch (_status)
        {
            case Status.Reconnecting:
                _reconnectTimer.Stop();
                break;
            case Status.Resuming:
                if (newStatus == Status.Active)
                    OnConnectionResumed?.Invoke(); 
                break;
            case Status.Initializing:
            case Status.Active:
            case Status.Disconnected:
            default:
                break;
        }
        
        //logic to perform initially in new status
        _status = newStatus;
        switch (newStatus)
        {
            case Status.Initializing:
                break;
            case Status.Active:
                _reconnectTimer.Stop();
                break;
            case Status.Reconnecting:
                _reconnectTimer.Start();
                if (_role == Role.Client)
                {
                    var result = _peerService.ConnectAsClient(_ip, _port);
                    if (!result.Success)
                        throw _exception($"Could not create client peer - {result.Error}");
                }
                // else
                // {
                //     var result = _peerService.CreateServer(_port);
                //     if (!result.Success)
                //         throw _exception($"Could not create client peer - {result.Error}");
                // }
                break;
            case Status.Disconnected:
                break;
            case Status.Resuming:
                _peerService.SendAppChan("resume");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, null);
        }
    }

    public void Send(IGodotSerializable message)
    {
        var msg = message.ToDictionary();
        switch (_status)
        {
            case Status.Active:
                _peerService.SendGameChan(msg); 
                break;
            case Status.Reconnecting or Status.Disconnected:
                //TODO queue up message to send once reconnected
                break;
            case Status.Resuming:
                //TODO maybe queue up messages to be sent?
                break;
            default:
                throw _exception($"Trying to send in invalid state {_status}");
        }
    }

    public Result ConnectAsClient(string ip, int port)
    {
        _role = Role.Client;
        _ip = ip;
        _port = port;
        var res = _peerService.ConnectAsClient(ip, port);
        if (!res.Success)
            return res;

        _status = Status.Initializing;
        return res;
    }

    public Result<int> CreateServer()
    {
        var res = _peerService.CreateServer();
        if (!res.Success)
            return res;

        _port = res.Value;
        _role = Role.Server;
        _status = Status.Initializing;
        return res;
    }

    public void Reconnect()
    {
       _transitionTo(Status.Reconnecting); 
    }

    public void ApplicationResumed()
    {
        _peerService.SendAppChan("resume");
        _transitionTo(Status.Resuming);
    }

    public void ApplicationPaused()
    {
        _peerService.SendAppChan("pause");
    }

    public void DisconnectSignals()
    {
        OnConnectionResumed = null;
        // _listener = null;
    }

    public void Shutdown()
    {
        DisconnectSignals();
        _listener = null;
        _peerService.Shutdown();
    }
}