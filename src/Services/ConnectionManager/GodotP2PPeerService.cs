using System;
using BattleshipWithWords.Utilities;
using Godot;
using Godot.Collections;

namespace androidplugintest.ConnectionManager;

public partial class ENetP2PPeerService : Node, IP2PPeerService
{
    private ENetMultiplayerPeer _peer;
    private bool _isHost;
    private const int GameChannel = 0;
    private const int ConnChannel = 1;
    private long _peerId;
    private int _serverPort;
     
    public event Action ConnectedToServer;
    public event Action ConnectionFailed;
    public event Action<long> PeerConnected;
    public event Action<long> PeerDisconnected;
    public event Action ServerDisconnected;
    public event Action<Dictionary> GameMessageReceived;
    public event Action<string> AppMessageReceived;
    public event Action ApplicationResumed;
    public event Action PeerAppPaused;
    
    public bool IsHost() => _isHost;

    public override void _ExitTree()
    {
    }

    public void Shutdown()
    {
        GD.Print("ENetP2PPeerService::Shutdown()");
        _disconnectMultiplayerPeer();
        _disconnectSignals();
    } 

    private void _disconnectSignals()
    {
        ConnectedToServer = null;
        ConnectionFailed = null;
        PeerConnected = null;
        PeerDisconnected = null;
        PeerAppPaused = null;
        ServerDisconnected = null;
        GameMessageReceived = null;
        AppMessageReceived = null;
    }

    public override void _Ready()
    {
        Multiplayer.ConnectedToServer += () =>
        {
            ConnectedToServer?.Invoke();
        };
        Multiplayer.ConnectionFailed += () =>
        {
            ConnectionFailed?.Invoke(); 
        };
        Multiplayer.PeerConnected += (id) =>
        {
            if (_isHost && _peerId != 0)
            {
                Multiplayer.MultiplayerPeer?.DisconnectPeer((int)_peerId, false);
            }
            else if (!_isHost && id != 1) return; // client should ignore id's other than 1 (server's id is always 1)
            
            _peerId = id; 
           PeerConnected?.Invoke(id);
        };
        Multiplayer.PeerDisconnected += (id) =>
        {
            GD.Print($"ENetP2PPeerService: PeerDisconnected: {id}");
            if (id == _peerId)
            {
                PeerDisconnected?.Invoke(id); 
                _peerId = 0; 
            }
        };
        Multiplayer.ServerDisconnected += () =>
        {
            GD.Print($"ENetP2PPeerService: ServerDisconnected");
            _peerId = 0;
            ServerDisconnected?.Invoke();
        };
    }
    
    public override void _Notification(int what)
    {
        if (_peer == null || _peer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Disconnected) return;
        
        switch ((long)what)
        {
            case NotificationApplicationFocusIn:
                break;
            case NotificationApplicationFocusOut:
                break;
            case NotificationApplicationPaused:
                SendAppChan("pause");
                break;
            case NotificationApplicationResumed:
                ApplicationResumed?.Invoke();
                break;
            case NotificationPredelete:
                GD.Print("P2P Service pre-delete");
                break;
        }
    }
    
    public void DisconnectPeer()
    {
        if (IsConnected())
        {
            GD.Print("IsConnected and disconnecting peer"); 
            _peer.DisconnectPeer((int)_peerId, true);
        }
    }

    public void Disconnect()
    {
        _disconnectMultiplayerPeer();
    }
    
    public void DeferredDisconnect()
    {
        CallDeferred(nameof(_disconnectMultiplayerPeer));
    }

    private void _disconnectMultiplayerPeer()
    {
        GD.Print("disconnecting MultiplayerPeer");
        Multiplayer.MultiplayerPeer?.Close();
        _peer.Close();
        _peer = null;
        Multiplayer.MultiplayerPeer = null;
    }

    public bool IsConnected()
    {
        var status =Multiplayer.MultiplayerPeer.GetConnectionStatus();
        GD.Print($"IsConnected status = {status}");
        return status == MultiplayerPeer.ConnectionStatus.Connected;
    }

    public Result ConnectAsClient(string ip, int port)
    {
        if (_isHost)
        {
            Disconnect(); 
        }
        _isHost = false;
        _peer = new ENetMultiplayerPeer();
        var err = _peer.CreateClient(ip, port);
        if (err != Error.Ok)
            return Result.Fail(err.ToString());

        Multiplayer.MultiplayerPeer = _peer;
        return Result.Ok();
    }

    public Result<int> CreateServer()
    {
        _isHost = true;
        _peer = new ENetMultiplayerPeer();
        if (_serverPort == 0)
        {
            for (var i = 50000; i < 65535; i++)
            {
                var err = _peer.CreateServer(i);
                if (err == Error.Ok)
                {
                    _serverPort = i;
                    break;
                }
                else
                {
                    GD.PrintErr($"could not create server on port {i} - {err.ToString()}");
                }
            }
        }
        else
        {
            var err = _peer.CreateServer(_serverPort);
            if (err != Error.Ok) return Result<int>.Fail(err.ToString());
        }
            
        Multiplayer.MultiplayerPeer = _peer;
        return Result<int>.Ok(_serverPort);
    }

    public void SendGameChan(Dictionary msg)
    {
        RpcId(_peerId, MethodName.Rpc_receiveGameCh, msg);
    }

    public void SendAppChan(string message)
    {
        RpcId(_peerId, MethodName.Rpc_receiveAppCh, message);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = GameChannel)]
    private void Rpc_receiveGameCh(Dictionary msg)
    {
        GameMessageReceived?.Invoke(msg);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable,
        TransferChannel = ConnChannel)]
    private void Rpc_receiveAppCh(string msg)
    {
        AppMessageReceived?.Invoke(msg);
    }
}