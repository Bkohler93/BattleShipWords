using System;
using System.Text.RegularExpressions;
using Godot;

namespace androidplugintest.PeerFinderPlugin;

internal enum Platform
{
    Ios,
    Android,
}

public interface IPeerFinderListener
{
    public void OnStarted();
    public void OnStopped();
    public void OnStartErrorOccurred(string error);

    public void OnStopErrorOccured(string error)
    {
        GD.PrintErr($"PeerFinderPlugin - Error occurred while stopping PeerFinder: {error}");
    }

    public void OnFailedToResolveService(string error)
    {
        GD.PrintErr($"PeerFinderPlugin - Error occurred while resolving service: {error}");
    }
    public void OnFoundService(ServiceInfo serviceInfo);
}

public enum IpVersion
{
    IPv4,
    IPv6
}

public static class IpVersionExt
{
    public static IpVersion From(string ip) => ip.Contains(':') ? IpVersion.IPv6 : IpVersion.IPv4;
}

public record ServiceInfo
{
    public required string Ip { get; set; }
    public required IpVersion IpVersion { get; set; }
    public required int Port { get; set; }
    public required string PeerName { get; set; }
}

public class PeerFinder 
{
    private IPeerFinderListener _listener;
    private GodotObject _plugin;
    private Platform _platform;
    private string _serviceType = "_peerfinder._tcp.";
    public string DisplayName { get; private set; }

    public bool IsActive { get; private set; }

    public event Action Started;
    public event Action Stopped;
    public event Action<string> StartErrorOccured;
    public event Action<string> StopErrorOccured;
    public event Action<string> FailedToResolveService;
    public event Action<ServiceInfo> FoundService;

    private readonly Callable _startedCallable;
    private readonly Callable _stoppedCallable;
    private readonly Callable _startErrorOccurredCallable;
    private readonly Callable _stopErrorOccurredCallable;
    private readonly Callable _failedToResolveServiceCallable;
    private readonly Callable _foundServiceIosCallable;
    private readonly Callable _foundServiceAndroidCallable;

    private const string StartedSignal = "Started";
    private const string StoppedSignal = "Stopped";
    private const string StartErrorOccurredSignal = "StartErrorOccurred";
    private const string StopErrorOccurredSignal = "StopErrorOccurred";
    private const string FailedToResolveServiceSignal = "FailedToResolveService";
    private const string FoundServiceSignal = "FoundService";

    public PeerFinder()
    {
        if (Engine.HasSingleton("GodotAndroidPeerFinder"))
        {
            _plugin = Engine.GetSingleton("GodotAndroidPeerFinder");
            _platform = Platform.Android;
        } else if (Engine.HasSingleton("IosPeerFinderPlugin"))
        {
            _plugin = Engine.GetSingleton("IosPeerFinderPlugin");
            _platform = Platform.Ios;
        } else
        {
            throw new Exception("No PeerFinderPlugin has been found, or invalid platform is being used with the plugin.");
        }

        _startedCallable = Callable.From(() =>
        {
            GD.Print("PeerFinder started");
            IsActive = true;
            Started?.Invoke();
            _listener?.OnStarted();
        });
        _plugin.Connect(StartedSignal, _startedCallable);

        _stoppedCallable = Callable.From(() =>
        {
            GD.Print("PeerFinder stopped");
            Stopped?.Invoke();
            _listener?.OnStopped();
        });
        _plugin.Connect(StoppedSignal, _stoppedCallable);

        _startErrorOccurredCallable = Callable.From((string msg) =>
        {
            GD.Print("PeerFinder Start error occurred");
            StartErrorOccured?.Invoke(msg);
            _listener?.OnStartErrorOccurred(msg);
        });
        _plugin.Connect(StartErrorOccurredSignal, _startErrorOccurredCallable);

        _stopErrorOccurredCallable = Callable.From((string msg) =>
        {
            GD.Print("PeerFinder Stop error occurred");
            StopErrorOccured?.Invoke(msg);
            _listener?.OnStopErrorOccured(msg);
        });
        _plugin.Connect(StopErrorOccurredSignal, _stopErrorOccurredCallable);

        _failedToResolveServiceCallable = Callable.From((string msg) =>
        {
            GD.Print("PeerFinder failed to resolve service");
            FailedToResolveService?.Invoke(msg);
            _listener?.OnFailedToResolveService(msg);
        });
        _plugin.Connect(FailedToResolveServiceSignal, _failedToResolveServiceCallable);

        _foundServiceIosCallable = Callable.From((string peerName, string ip, int port) =>
        {
            GD.Print("PeerFinder found service");
            var serviceInfo = new ServiceInfo
            {
                Ip = ip,
                PeerName = peerName,
                Port = port,
                IpVersion = IpVersionExt.From(ip)
            };
            FoundService?.Invoke(serviceInfo);
            _listener?.OnFoundService(serviceInfo);
        });
        _foundServiceAndroidCallable = Callable.From((string peerName, string ip, string port) =>
        {
            GD.Print("PeerFinder found service");
            int portInt = Int32.Parse(port);
            var serviceInfo = new ServiceInfo
            {
                Ip = ip,
                Port = portInt,
                PeerName = peerName,
                IpVersion = IpVersionExt.From(ip)
            };
            FoundService?.Invoke(serviceInfo);
            _listener?.OnFoundService(serviceInfo);
        });
        _plugin.Connect(FoundServiceSignal, _platform == Platform.Ios ? _foundServiceIosCallable : _foundServiceAndroidCallable);
    }

    private void DisconnectSignals()
    {
        _plugin.Disconnect(StartedSignal, _startedCallable);
        _plugin.Disconnect(StoppedSignal, _stoppedCallable);
        _plugin.Disconnect(StartErrorOccurredSignal, _startErrorOccurredCallable);
        _plugin.Disconnect(StopErrorOccurredSignal, _stopErrorOccurredCallable);
        _plugin.Disconnect(FailedToResolveServiceSignal, _failedToResolveServiceCallable);
        if (_platform == Platform.Android)
            _plugin.Disconnect(FoundServiceSignal, _foundServiceAndroidCallable);
        else
            _plugin.Disconnect(FoundServiceSignal, _foundServiceIosCallable);
    }

    public void Shutdown()
    {
        DisconnectSignals();
        SetListener(null);
        _plugin.Dispose();
    }

    public void SetListener(IPeerFinderListener listener)
    {
        _listener = listener;
    }

    public void Stop()
    {
        _plugin.Call("stop");
    }

    public void Start(string displayName, string serviceName, int port)
    {
        DisplayName = displayName;
        if (_platform == Platform.Android) 
            _plugin.Call("start", displayName, _serviceType, serviceName, port);
        else if (_platform == Platform.Ios)
        {
            var advertiseParams = new Godot.Collections.Dictionary()
            {
                {"serviceType",_serviceType},
                {"serviceName", serviceName},
                { "servicePort", port},
                {"displayName", displayName}
            };
            _plugin.Call("start", advertiseParams);    
        }
    }
}