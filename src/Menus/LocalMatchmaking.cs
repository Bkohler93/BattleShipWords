using Godot;
using System;
using System.Diagnostics;
using System.Net.Quic;
using System.Runtime.InteropServices;
using BattleshipWithWords;

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
    
    // two ports below only used when testing on same system and two separate dst/src ports are required.
    private int _dstUdpPort;
    private int _dstTcpPort;
    private string _peerIp;
    private string _ip;

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

    public override void _Ready()
    {
        _backButton.Pressed += _onBackButtonPressed;
        _playButton.Pressed += _onPlayButtonPressed;
        _playButton.Hide();
        
        var config = GetNode("/root/Config") as Config;
        _dstUdpPort = _udpPort;
        int srcUdpPort = _udpPort;
        
        if (config!.AreLocalUdpPortsSet())
        {
            _dstUdpPort = config.LocalDstUdpPort;
            srcUdpPort = config.LocalSrcUdpPort;
            _tcpPort = config.LocalTcpPort;
        }

        _ip = GetLocalIp();
        _discoveryPeer = new PacketPeerUdp();
        _discoveryPeer.SetBroadcastEnabled(true);
        _discoveryPeer.SetDestAddress("255.255.255.255", _dstUdpPort);
        _statusLabel.Text = "Searching for local game";
        
        var err = _discoveryPeer.Bind(srcUdpPort);
        if (err != Error.Ok)
        {
            _statusLabel.Text = "There was an issue searching for local game";
            _failedToConnect = true;
        }

        // GD.Print($"Listening on {srcUdpPort} --- sending on {_dstUdpPort}");
    }
    
    private string GetLocalIp()
    {
        foreach (string ip in IP.GetLocalAddresses())
        {
            if (System.Net.IPAddress.TryParse(ip, out var parsedIp))
            {
                // Check it's IPv4 and not a loopback address
                if (parsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                    !ip.StartsWith("127."))
                {
                    return ip;
                }
            }
        }

        return "Unable to determine local IP";
    }

    public override void _Process(double delta)
    {
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
