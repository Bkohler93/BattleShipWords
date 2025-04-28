using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services;

public class LocalPeerFinderFactory
{
    private string _serviceName = "Find a Word Game Matccch";
    private string _serviceType = "_peerfinder._tcp";
    private int _servicePort;

    public LocalPeerFinderFactory()
    {
        _servicePort = NetworkUtils.FindAvailablePort(50000, 200);
    }

    public ILocalPeerFinder Create(Platform platform)
    {
        return platform switch
        {
            Platform.iOS => new IosLocalPeerFinder(_serviceName, _serviceType, _servicePort),
            Platform.Android => new AndroidLocalPeerFinder(_serviceName, _serviceType, _servicePort),
            _ => new JankyLocalPeerFinder(_serviceName, _serviceType, _servicePort)
        };
    }   
}