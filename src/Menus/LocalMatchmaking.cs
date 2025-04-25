using Godot;
using System;
using System.Diagnostics;
using System.Net.Quic;
using System.Runtime.InteropServices;
using BattleshipWithWords;
using BattleshipWithWords.Networkutils;
using Godot.Collections;

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
    
    public Action OnBackButtonPressed;
    public Action OnPlayButtonPressed;
    
    private double _sendInterval = 0.5;
    private double _sendAccumulator = 0;
    private PacketPeerUdp _discoveryPeer;
    private  ENetMultiplayerPeer _multiplayerPeer;
    
    private bool _peerFound;
    private bool _connected;
    private bool _listening;
    private bool _failedToConnect;
    private bool _success;
    
    private int _udpPort = 55531;
    private int _tcpPort = 55532;
    
    // two ports below only used when testing locally on same system and two separate dst/src ports are required.
    private int _dstUdpPort;
    private int _dstTcpPort;
    
    private string _peerIp;
    private string _ip;
    
    //platform specific
    private GodotObject _androidPlugin;
    private GodotObject _helloPlugin;
    private bool _isAndroid;

    public override void _ExitTree()
    {
        _discoveryPeer.Close();
    }

    private void _onBackButtonPressed()
    {
       _discoveryPeer?.Close();
       _multiplayerPeer?.Close();
       OnBackButtonPressed?.Invoke();
    }

    private void _onPlayButtonPressed()
    {
        OnPlayButtonPressed?.Invoke();
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
            _updateLabel.Text = $"Found peer at {ip}:{port}";
            GD.Print($"Discovered peer at {ip}:{port}");
        }
    }

    public override void _Ready()
    {
        _backButton.Pressed += _onBackButtonPressed;
        _playButton.Pressed += _onPlayButtonPressed;
        _playButton.Hide();

        if (OS.GetName() == "Android")
            _isAndroid = true;
        
        if (Engine.HasSingleton("GodotAndroidPluginTemplate"))
        {
            _helloPlugin = Engine.GetSingleton("GodotAndroidPluginTemplate");
            _helloPlugin.Call("helloWorld");
        }
        else
        {
            GD.Print("No HelloPlugin found");
        }

        GD.Print(Engine.GetSingletonList());
        GD.Print(Engine.HasSingleton("GodotAndroidMdns"));
        if (_isAndroid && Engine.HasSingleton("GodotAndroidMdns"))
        {
            _androidPlugin = Engine.GetSingleton("GodotAndroidMdns");

            // Connect the discovery_result signal
            _androidPlugin.Connect("discovery_result", new Callable(this, nameof(_onDiscoveryResult)));

            // Start discovery
            var result = (Godot.Collections.Dictionary)_androidPlugin.Call("start_discovery");
            GD.Print("Discovery started: ", result); 
            _statusLabel.Text = $"Discovery started: {result}";
        }
        else if(_isAndroid)
        {
            _statusLabel.Text = "Plugin not found";
        }
        else
        {
            // in prod single udp port is used, testing/local execution requires two 
            // different ports to communicate with each other with same IP address
            _dstUdpPort = _udpPort;
            int srcUdpPort = _udpPort;
        
            var config = GetNode("/root/Config") as Config;
            if (config!.UseLocalNetworking())
            {
                _dstUdpPort = config.LocalDstUdpPort;
                srcUdpPort = config.LocalSrcUdpPort;
                _tcpPort = config.LocalTcpPort;
            }

            _ip = NetworkUtils.GetLocalIp();
            _discoveryPeer = new PacketPeerUdp();
            _discoveryPeer.SetBroadcastEnabled(true);
            _discoveryPeer.SetDestAddress("255.255.255.255", _dstUdpPort);
            _statusLabel.Text = "Searching for local game";
        
            var err = _discoveryPeer.Bind(srcUdpPort);
            if (err != Error.Ok)
            {
                _statusLabel.Text = "There was an issue searching for local game:" + err;
                _failedToConnect = true;
            }
            // GD.Print($"Listening on {srcUdpPort} --- sending on {_dstUdpPort}");
        }
    }
    

    public override void _Process(double delta)
    {
        if (_isAndroid)
            return;
        if (_failedToConnect || _success) return;
        
        _sendAccumulator += delta;
        if (_sendAccumulator >= _sendInterval)
        {
            if (_updateLabel.Text.Length < 3)
                _updateLabel.Text += ".";
            else
                _updateLabel.Text = "";
            
            if (!_peerFound)
                SendPing();
            else
                SendPong();
            _sendAccumulator = 0; 
        }
        else
        {
            if (!_connected && !_listening)
                CheckForPingPong();
        }
    }


    private void SendPing()
    {
        var message = new Godot.Collections.Dictionary {
            { "type", "ping" },
            { "ip", _ip },
        };
        _discoveryPeer.PutPacket(Json.Stringify(message).ToUtf8Buffer());
    }

    private void SendPong()
    {
        var message = new Godot.Collections.Dictionary {
            { "type", "pong" },
            { "ip", _ip },
        };
        _discoveryPeer.PutPacket(Json.Stringify(message).ToUtf8Buffer());
    }

    private void CheckForPingPong()
    {
        // GD.Print("checking for traffic");
        if (_discoveryPeer.GetAvailablePacketCount() > 0)
        {
            var bytes = _discoveryPeer.GetPacket();
            var jsonStr = bytes.GetStringFromUtf8();
            var d = (Godot.Collections.Dictionary)Json.ParseString(jsonStr);
            var type = d["type"].AsString();
            var ip = d["ip"].AsString();
            GD.Print($"received {type} from {ip}");
            _peerIp = ip;
            
            if (type == "ping")
            {
                _statusLabel.Text = "Found other player\nWaiting for them to connect";
                _peerFound = true;
                _discoveryPeer.SetDestAddress(_peerIp, _dstUdpPort);
                _discoveryPeer.SetBroadcastEnabled(false);
                ListenAndServe();
            } else if (type == "pong")
            {
                _statusLabel.Text = "Connecting to game";
                _connected = true;
                ClientConnect();
            }
        }
    }

    private void ClientConnect()
    {
        _multiplayerPeer = new ENetMultiplayerPeer();
        var err = _multiplayerPeer.CreateClient(_peerIp, _tcpPort);
        if (err != Error.Ok)
        {
            _statusLabel.Text = "There was an issue connecting to the game";
            _failedToConnect = true;
            _multiplayerPeer.Close();
            return;
        }
        Multiplayer.MultiplayerPeer = _multiplayerPeer;
        _statusLabel.Text = "Connected to other player!";
        _playButton.Show();
        _success = true;
        _updateLabel.Text = "=)";
    }

    private void ListenAndServe()
    {
        _multiplayerPeer = new ENetMultiplayerPeer();
        GD.Print($"creating server on {_tcpPort}"); 
        var err =_multiplayerPeer.CreateServer(_tcpPort, 2, 24, Int32.MaxValue, Int32.MaxValue);
        if (err != Error.Ok)
        {
            _statusLabel.Text = "There was an issue creating game";
            _failedToConnect = true;
            return;
        }
        Multiplayer.MultiplayerPeer = _multiplayerPeer;
        _multiplayerPeer.PeerConnected += id =>
        {
            _statusLabel.Text = "Connected to other player!";
            _playButton.Show();
            _connected = true;
            _success = true;
            _updateLabel.Text = "=)";
        };
        _listening = true;
    }
}
