namespace BattleshipWithWords.Services;

public interface ILocalPeerFinder
{
    // returns the port the service is advertising for 
    int StartService();
    void StopService();
    void StartListening();
    void ConnectSignals(LocalMatchmaking matchmaking);
    void Cleanup();
}
