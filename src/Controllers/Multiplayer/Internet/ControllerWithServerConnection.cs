using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

public abstract class ControllerWithServerConnection
{
    protected ServerConnectionManager ConnectionManager;

    public void SetConnectionManager(ServerConnectionManager connectionManager)
    {
        ConnectionManager = connectionManager;
    }

    public void SetConnectionListener(IServerConnectionListener listener) => ConnectionManager.SetListener(listener);
    public void CloseConnection() => ConnectionManager.Close();
    public ServerConnectionManager GetConnectionManager() => ConnectionManager;

    public Result Connect(string url, TlsOptions options)
    {
        var err = ConnectionManager.Connect(url, options);
        return err != Error.Ok ? Result.Fail(err.ToString()) : Result.Ok();
    }

    public Result Send(BaseServerSendMessage msg)
    {
        var err =ConnectionManager.Send(msg);
        return err != Error.Ok ? Result.Fail(err.ToString()) : Result.Ok();
    }
}