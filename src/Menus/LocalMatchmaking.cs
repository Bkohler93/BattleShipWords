using Godot;
using System;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Services;
using Godot.Collections;

internal enum LocalMatchmakingState
{
    FindingPeer,
    AwaitingConfirmation,
    PeerReady,
    AwaitingPeer,
    StartGame,
    Disconnecting
}

public partial class LocalMatchmaking : Control
{
    [Export]
    private Button _backButton;
    
    [Export]
    private Button _playButton;

    [Export]    
    private Label _statusLabel;

    [Export] 
    private Label _updateLabel;

    public Action BackToMainMenu;
    public Action StartGame;
    
    private double _sendInterval = 0.5;
    private double _sendAccumulator = 0;
    private PacketPeerUdp _discoveryPeer;
    private  ENetMultiplayerPeer _hostMultiplayerPeer;
    private  ENetMultiplayerPeer _clientMultiplayerPeer;
    private int _hostPort;
    private int _peerPort;
    private string _peerIp;
    private string _serviceName;
    private int _servicePort;
    private string _serviceType;
    private long _peerId;

    private bool _peerReadyToPlay;
    
    private LocalMatchmakingState _state;
    
    //platform specific
    // private Platform _platform;
    private ILocalPeerFinder _peerFinder;
    // private GodotObject _androidPlugin;
    // private JankyDns _jankyDns;

    private float _updateCounter;

    public LocalMatchmaking()
    {
        var peerFinderFactory = new LocalPeerFinderFactory();
        _peerFinder = peerFinderFactory.Create(PlatformUtils.GetPlatform());
        _peerFinder.ConnectSignals(this);
    }

    public override void _Ready()
    {
        _hostPort = _peerFinder.StartService();
        _startServer();
        _backButton.Pressed += _onBackButtonPressed;
        _playButton.Pressed += _onPlayButtonPressed;
        _playButton.Hide();
        _peerFinder.StartListening();
        _state = LocalMatchmakingState.FindingPeer;
    }

    public override void _Process(double delta)
    {
        switch (_state)
        {
            case LocalMatchmakingState.FindingPeer:
                _statusLabel.Text = "Finding peers on your network.";
                _displayFindingPeer(delta);
                break;
            case LocalMatchmakingState.AwaitingConfirmation:
                _statusLabel.Text = "Peer found! Press 'Play' to continue.";
                _updateLabel.Text = "=)";
                break;
            case LocalMatchmakingState.PeerReady:
                _statusLabel.Text = "Peer ready! Press 'Play' to continue.";
                _updateLabel.Text = "=)";
                break;
            case LocalMatchmakingState.AwaitingPeer:
                _displayFindingPeer(delta);
                _statusLabel.Text = "Waiting for peer to press 'Play'";
                break;
            case LocalMatchmakingState.StartGame:
                StartGame.Invoke();
                break;
            case LocalMatchmakingState.Disconnecting:
                _statusLabel.Text = "Disconnecting from search";
                break;
        }
    }

    private void _displayFindingPeer(double delta)
    {
        _updateCounter += (float)delta;
        if (_updateCounter >= 1.0)
        {
            _updateCounter = 0;
            _updateLabel.Text += ".";
            if (_updateLabel.Text.Length > 3)
            {
                _updateLabel.Text = "";
            }
        }
    }

    public override void _ExitTree()
    {
        _discoveryPeer.Close();
    }
    
    // this is the last method that will be called when exiting this scene. Unregistering 
    // mDNS on Android takes a second, so we need to wait for it to complete before continuing.
    public void OnUnregisteredService()
    {
        _peerFinder.Cleanup();
        if (_state == LocalMatchmakingState.Disconnecting)
            BackToMainMenu?.Invoke();
        if (_state == LocalMatchmakingState.StartGame)
            StartGame?.Invoke();
    }

    private void _onBackButtonPressed()
    {
        _state = LocalMatchmakingState.Disconnecting;
        _playButton.Hide();
       _discoveryPeer?.Close();
       _hostMultiplayerPeer?.Close();
       _clientMultiplayerPeer?.Close();
       if (Multiplayer.HasMultiplayerPeer())
       {
           Multiplayer.MultiplayerPeer.Close();
           Multiplayer.MultiplayerPeer = null; 
       }
       _peerFinder?.StopService();   
    }

    private void _onPlayButtonPressed()
    {
        _playButton.Hide();
        RpcId(_peerId, MethodName.Rpc_SendPlayReady);

        if (_state == LocalMatchmakingState.AwaitingConfirmation)
            _state = LocalMatchmakingState.AwaitingPeer;
        else if (_state == LocalMatchmakingState.PeerReady)
        {
            _state = LocalMatchmakingState.StartGame;
            StartGame?.Invoke();
        }
        else 
            GD.PrintErr($"_onPlayButtonPressed: Invalid state reached: {_state}"); 
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Rpc_SendPlayReady()
    {
        if (_state == LocalMatchmakingState.AwaitingConfirmation)
            _state = LocalMatchmakingState.PeerReady;
        else if (_state == LocalMatchmakingState.AwaitingPeer)
            _state = LocalMatchmakingState.StartGame;
        else
            GD.PrintErr($"Rpc_SendPlayReady: invalid state reached: {_state}");
    }

    private void _onDiscoveryResult(Dictionary result)
    {
        if (result.ContainsKey("error"))
        {
            GD.PrintErr("Discovery error: ", result["error"]);
        }
        else
        {
            var ip = result["ip"].ToString();
            var port = (int)result["port"];
            GD.Print($"Discovered peer at {ip}:{port}");
        }
    }

    public void OnRegisterServiceFailed()
    {
        GD.PrintErr("Register service failed");
    }

    public void OnRegisteredService()
    {
    }

    public void _startServer()
    {
        _hostMultiplayerPeer = new ENetMultiplayerPeer();
        if (_hostPort == -1)
        {
            GD.PrintErr("Could not find available port");
            return;
        }

        var err = _hostMultiplayerPeer.CreateServer(_hostPort);
        if (err != Error.Ok)
        {
            GD.PrintErr("Could not create server: ", err);
            return;
        }
        Multiplayer.MultiplayerPeer = _hostMultiplayerPeer;

        _hostMultiplayerPeer.PeerConnected += _onClientConnected;
        GD.Print($"Listening on port {_hostPort}");
    }

    private void _onClientConnected(long id)
    {
        GD.Print($"Peer connected: {id}");
        // do something... automatically move into setup screen?
        //                 make a button appear for user to initiate game?
        _state = LocalMatchmakingState.AwaitingConfirmation;
        _peerId = id;
        _playButton.Show();
    }

    

    public void OnServerFound(string ip, int port)
    {
        _peerIp = ip;
        _peerPort = port;
        if (Multiplayer.MultiplayerPeer != null)
        {
            Multiplayer.MultiplayerPeer.Close(); 
            Multiplayer.MultiplayerPeer = null; 
        }
        _clientMultiplayerPeer = new ENetMultiplayerPeer();
        var err = _clientMultiplayerPeer.CreateClient(ip, port);
        if (err != Error.Ok)
        {
            GD.PrintErr("Could not create client: ", err);
            _clientMultiplayerPeer.Close();
            return;
        }
        Multiplayer.MultiplayerPeer = _clientMultiplayerPeer;
        _peerId = 1;
        _playButton.Show();
        _state = LocalMatchmakingState.AwaitingConfirmation;
        GD.Print("Connected to server");
    }
}
