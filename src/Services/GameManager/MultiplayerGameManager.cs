namespace BattleshipWithWords.Services.GameManager;

public class MultiplayerGameManager
{
    private long _peerId;
    private long _id;

    public MultiplayerGameManager(long id, long peerId)
    {
        _id = id;
        _peerId = peerId;
    }
}
