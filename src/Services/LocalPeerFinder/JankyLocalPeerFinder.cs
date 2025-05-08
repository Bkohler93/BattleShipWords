using System;
using System.Collections.Generic;
using Godot;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BattleshipWithWords.Nodes.Menus;

namespace BattleshipWithWords.Services;


public partial class JankyLocalPeerFinder : Node, ILocalPeerFinder
{
    [Signal]
    public delegate void RegisteredServiceEventHandler();

    [Signal]
    public delegate void RegisterServiceFailedEventHandler();
    
    [Signal]
    public delegate void UnregisteredServiceEventHandler();
    
    [Signal]
    public delegate void PeerFoundEventHandler(string ip, int port);
    
    private float _broadcastTimer = 0f;
    private bool _isBroadcasting;
    private UdpClient _mdnsClient;
    private IPEndPoint _multicastEndpoint;
    private string _serviceName;
    private string _serviceType;
    private int _servicePort;

    private PacketPeerUdp _listeningPeer;
    private bool _isListening;
    private float _listenTimer = 0f;

    public JankyLocalPeerFinder(string serviceName, string serviceType, int servicePort)
    {
        _serviceName = serviceName;
        _serviceType = serviceType;
        _servicePort = servicePort;
    }

    public override void _Process(double delta)
    {
        if (!_isBroadcasting)
            return;

        _broadcastTimer += (float)delta;
        
        if (_broadcastTimer >= 4.0f) // broadcast every 1 second
        {
            _broadcastTimer = 0f;
            // EmitSignalPeerFound("192.168.0.121", 50000);
            // _sendMdnsBroadcast(); //sending mdns does not work, difficult to prepare packet correctly
        }

        if (!_isListening)
            return;
        
        _listenTimer += (float)delta;

        if (_listenTimer >= 0.5f)
        {
            _listenTimer = 0f;
            // _listenForMdnsBroadcast();
        }
    }

    public int StartService()
    {
        _mdnsClient = new UdpClient();
        _mdnsClient.MulticastLoopback = true;
        _multicastEndpoint = new IPEndPoint(IPAddress.Parse("224.0.0.251"), 5353);
        _isBroadcasting = true;
        
        EmitSignalRegisteredService();
        return _servicePort;
    }

    public void StopService()
    {
        // _isBroadcasting = false;
        // _mdnsClient.Client.Close();
        // _mdnsClient.Close();
        // _mdnsClient.Dispose();
        // _mdnsClient = null;
        EmitSignalUnregisteredService();
    }

    public void StartListening()
    {
        _mdnsClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _mdnsClient.ExclusiveAddressUse = false;
        _mdnsClient.Client.Bind(new IPEndPoint(IPAddress.Any, 5353));
        // use to find which interface you want to bind to
        // var interfaces = IP.GetLocalInterfaces();
        // for (var i = 0; i < interfaces.Count; i++)
        // {
        //     GD.Print(interfaces[i]);
        // }
        _mdnsClient.JoinMulticastGroup(IPAddress.Parse("224.0.0.251"));
        _isListening = true;
    }

    public void ConnectSignals(ILocalPeerFinderConnector connector)
    {
        Connect(SignalName.RegisteredService, Callable.From(connector.OnRegisteredService));
        Connect(SignalName.RegisterServiceFailed, Callable.From(connector.OnRegisterServiceFailed));
        Connect(SignalName.UnregisteredService, Callable.From(connector.OnUnregisteredService));
        Connect(SignalName.PeerFound, Callable.From((string ip, int port) => connector.OnServiceFound(ip, port)));
    }

    public void Cleanup(){
    }
}