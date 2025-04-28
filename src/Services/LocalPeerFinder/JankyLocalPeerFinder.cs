using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services;

using System.Net;
using System.Net.Sockets;
using System.Text;

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
    
    private int SkipDnsName(byte[] data, int offset)
    {
        while (data[offset] != 0)
        {
            byte length = data[offset];
            offset += length + 1;
        }
        return offset + 1;
    }
    
    string ParseDnsName(byte[] data, int offset)
    {
        List<string> labels = new List<string>();

        while (data[offset] != 0)
        {
            int length = data[offset];
            offset++;
            string label = System.Text.Encoding.UTF8.GetString(data, offset, length);
            labels.Add(label);
            offset += length;
        }
        return string.Join(".", labels);
    }

    private void _listenForMdnsBroadcast()
    {
        while (_mdnsClient.Available > 0)
        {
            IPEndPoint endpoint = null;
            var p = _mdnsClient.Receive(ref endpoint);
        }
    }
    
    private void _sendMdnsBroadcast()
{
    if (_mdnsClient == null)
        return;

    // Step 1: Construct DNS Header
    byte[] header = new byte[12]; // DNS header is 12 bytes (simplified version)
    // Here you would normally fill in the actual DNS header fields like transaction ID, flags, etc.
    // For simplicity, we'll assume a minimal mDNS header

    // Step 2: Construct Query Section (service type query)
    byte[] query = BuildQuerySection();

    // Step 3: Construct Answer Section (response with service details)
    byte[] answer = BuildAnswerSection();

    // Step 4: Combine Header, Query, and Answer Sections
    byte[] packet = new byte[header.Length + query.Length + answer.Length];
    Buffer.BlockCopy(header, 0, packet, 0, header.Length);
    Buffer.BlockCopy(query, 0, packet, header.Length, query.Length);
    Buffer.BlockCopy(answer, 0, packet, header.Length + query.Length, answer.Length);

    // Step 5: Send the packet (multicast address: 224.0.0.251, port: 5353)
    _mdnsClient.Send(packet, packet.Length, _multicastEndpoint);
}

// Build the DNS query section (asking for a service)
private byte[] BuildQuerySection()
{
    byte[] serviceTypeBytes = Encoding.UTF8.GetBytes(_serviceType);
    byte[] query = new byte[serviceTypeBytes.Length + 5]; // Service query with length field
    query[0] = (byte)(serviceTypeBytes.Length >> 8); // Length byte for the name
    query[1] = (byte)(serviceTypeBytes.Length & 0xFF);
    Buffer.BlockCopy(serviceTypeBytes, 0, query, 2, serviceTypeBytes.Length);
    query[query.Length - 3] = 0; // Null byte (end of name)
    query[query.Length - 2] = 0; // Type field (PTR = 0x0C)
    query[query.Length - 1] = 1; // Class field (IN = 0x01)
    
    return query;
}

// Build the DNS answer section (responding with the service details)
private byte[] BuildAnswerSection()
{
    byte[] serviceAddress = new byte[] { 192, 168, 1, 100 }; // IP of the device offering service
    byte[] servicePort = BitConverter.GetBytes((short)_servicePort); // Port of the service

    // Answer section would include the service IP address and port information
    // We're simplifying here, but it would typically follow the DNS response format
    byte[] answer = new byte[serviceAddress.Length + servicePort.Length + 10]; // Extra space for DNS answer header

    // DNS response header (simplified)
    answer[0] = 0; // Sequence number (simplified)
    answer[1] = 0;
    // Answer contains the PTR (pointer) record for the service type
    answer[2] = (byte)serviceAddress.Length; // Length of address
    answer[3] = 0; // Pointer
    answer[4] = 0;

    Buffer.BlockCopy(serviceAddress, 0, answer, 5, serviceAddress.Length);
    Buffer.BlockCopy(servicePort, 0, answer, 5 + serviceAddress.Length, servicePort.Length);

    return answer;
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

    public void ConnectSignals(LocalMatchmaking matchmaking)
    {
        Connect(SignalName.RegisteredService, Callable.From(matchmaking.OnRegisteredService));
        Connect(SignalName.RegisterServiceFailed, Callable.From(matchmaking.OnRegisterServiceFailed));
        Connect(SignalName.UnregisteredService, Callable.From(matchmaking.OnUnregisteredService));
        Connect(SignalName.PeerFound, Callable.From((string ip, int port) => matchmaking.OnServerFound(ip, port)));
    }

    public void Cleanup(){
    }
}