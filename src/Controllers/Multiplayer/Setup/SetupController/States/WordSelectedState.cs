using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

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

    public override void HandleNextWordsButton()
    {
        _controller.OnNextWordsButtonPressed();
        _controller.TransitionTo(new InitialState(_controller));
    }

    public override void HandlePreviousWordsButton()
    {
        _controller.OnPreviousWordsButtonPressed();
        _controller.TransitionTo(new InitialState(_controller));
    }
}