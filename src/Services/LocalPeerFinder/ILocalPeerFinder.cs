using BattleshipWithWords.Nodes.Menus;

namespace BattleshipWithWords.Services;

public interface ILocalPeerFinder
{
    // returns the port the service is advertising for 
    int StartService();
    void StopService();
    void StartListening();
    void ConnectSignals(ILocalPeerFinderConnector connector);
    void Cleanup();
}

public interface ILocalPeerFinderConnector
{
    public void OnRegisteredService();
    public void OnRegisterServiceFailed();
    public void OnUnregisteredService();
    public void OnServiceFound(string ip, int port);
}
