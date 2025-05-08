using Godot;

using BattleshipWithWords.Nodes.Menus;

namespace BattleshipWithWords.Services;

public partial class IosLocalPeerFinder: Node, ILocalPeerFinder
{
    private string _serviceType;
    private string _serviceName;
    private int _servicePort;

    public IosLocalPeerFinder(string serviceType, string serviceName, int servicePort)
    {
        _serviceType = serviceType;
        _serviceName = serviceName;
        _servicePort = servicePort;
    }

    public int StartService()
    {
        throw new System.NotImplementedException();
    }

    public void StopService()
    {
        throw new System.NotImplementedException();
    }

    public void StartListening()
    {
        throw new System.NotImplementedException();
    }

    public void ConnectSignals(ILocalPeerFinderConnector connector)
    {
        throw new System.NotImplementedException();
    }

    public void Cleanup()
    {
        throw new System.NotImplementedException();
    }
}