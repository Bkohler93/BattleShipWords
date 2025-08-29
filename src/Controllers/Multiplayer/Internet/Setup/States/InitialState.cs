using System.Runtime.InteropServices;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

class InitialState : SetupState
{
    private SetupController _controller;

    public InitialState(SetupController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void HandlePreviousWordsButton(int wordLength)
    {
        _controller.OnPreviousWordsButtonPressed(wordLength);
    }

    public override void HandleNextWordsButton(int wordLength)
    {
        _controller.OnNextWordsButtonPressed(wordLength); 
    }

    public override void HandleWordSelectionButton(WordSelectionButtonId buttonId)
    {
       _controller.OnWordSelectionButtonPressed(buttonId); 
       _controller.TransitionTo(new WordSelectedState(_controller));
    }

    public override void Connecting()
    {
        throw new System.NotImplementedException();
    }

    public override void Connected()
    {
        throw new System.NotImplementedException();
    }

    public override void UnableToConnect()
    {
        throw new System.NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Reconnecting()
    {
        throw new System.NotImplementedException();
    }

    public override void Reconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Receive(IServerReceivable message)
    {
        switch (message)
        {
            default:
                Logger.Print("Unknown message type during InitialState");
                break;
        }
    }

    public override void Disconnecting()
    {
        throw new System.NotImplementedException();
    }

    public override void HttpResponse(string response)
    {
        throw new System.NotImplementedException();
    }
}