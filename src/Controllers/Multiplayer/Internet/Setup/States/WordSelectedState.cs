using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

class WordSelectedState: SetupState
{
    private SetupController _controller;

    public WordSelectedState(SetupController controller)
    {
        _controller = controller; 
    }
    
    public override void Enter()
    {
    }

    public override void Exit()
    {
    }

    public override void HandleTileGuiEvent(InputEvent @event, SetupTile tile)
    {
        if (@event is InputEventScreenTouch touch)
        {
            if (touch.IsPressed())
            {
                _controller.TransitionTo(new StartingTileSelectedState(_controller, tile));
            }
        }
    }

    public override void HandleWordSelectionButton(WordSelectionButtonId buttonId)
    {
        _controller.OnWordSelectionButtonPressed(buttonId);
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
                Logger.Print("unknown message type during WordSelectedState"); break;
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

    public override void HandleNextWordsButton(int wordLength)
    {
        _controller.OnNextWordsButtonPressed(wordLength);
        _controller.TransitionTo(new InitialState(_controller));
    }

    public override void HandlePreviousWordsButton(int wordLength)
    {
        _controller.OnPreviousWordsButtonPressed(wordLength);
        _controller.TransitionTo(new InitialState(_controller));
    }
}