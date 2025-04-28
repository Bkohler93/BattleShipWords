using System.Threading;
using Godot;

namespace BattleshipWithWords.Services;

public partial class AndroidLocalPeerFinder : Node,
    ILocalPeerFinder
{
    private string _serviceType;
    private string _serviceName;
    private int _servicePort;
    private GodotObject _androidPlugin;

    private Callable _registeredServiceCallable;
    private Callable _registerServiceFailedCallable;
    private Callable _unregisteredServiceCallable;
    private Callable _foundServiceCallable;

    public AndroidLocalPeerFinder(string serviceName, string serviceType, int servicePort)
    {
        _serviceType = serviceType;
        _serviceName = serviceName;
        _servicePort = servicePort;
        _androidPlugin = Engine.GetSingleton("GodotAndroidPeerFinder");
        if (OS.RequestPermissions())
        {
            GD.Print("Permissions already granted");
        } else
        {
            GD.Print("Permissions granted");
        }
    }

    public int StartService()
    {
        _androidPlugin.Call("registerService", _serviceType, _serviceName, _servicePort);
        return _servicePort;
    }

    public void StopService()
    {
        var t = new Thread(() =>
        {
            _androidPlugin.Call("unregisterService");
        });
        t.Start();
    }

    public void StartListening()
    {
        _androidPlugin.Call("listen", _serviceType, _serviceName);
        GD.Print("Listening");
    }

    public void ConnectSignals(LocalMatchmaking matchmaking)
    {
        _registeredServiceCallable = Callable.From(matchmaking.OnRegisteredService);
        _androidPlugin.Connect("RegisteredService", _registeredServiceCallable);

        _registerServiceFailedCallable = Callable.From(matchmaking.OnRegisterServiceFailed);
        _androidPlugin.Connect("RegisterServiceFailed", _registerServiceFailedCallable);
        
        _unregisteredServiceCallable = Callable.From(matchmaking.OnUnregisteredService); 
        _androidPlugin.Connect("UnregisteredService", _unregisteredServiceCallable);

        _foundServiceCallable = Callable.From((string ip, string portStr) =>
            matchmaking.OnServerFound(ip, int.Parse(portStr))
        );
        _androidPlugin.Connect("FoundService", _foundServiceCallable);
    }

    public void Cleanup()
    {
        _androidPlugin.Disconnect("RegisteredService", _registeredServiceCallable);
        _androidPlugin.Disconnect("UnregisteredService", _unregisteredServiceCallable);
        _androidPlugin.Disconnect("FoundService", _foundServiceCallable);
        _androidPlugin.Disconnect("RegisterServiceFailed", _registerServiceFailedCallable);
        _androidPlugin.Dispose();
    }
}