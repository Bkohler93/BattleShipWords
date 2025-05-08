using System.Runtime.InteropServices;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

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

    public override void HandlePreviousWordsButton()
    {
        _controller.OnPreviousWordsButtonPressed();
    }

    public override void HandleNextWordsButton()
    {
        _controller.OnNextWordsButtonPressed(); 
    }

    public override void HandleWordSelectionButton(WordSelectionButtonId buttonId)
    {
       _controller.OnWordSelectionButtonPressed(buttonId); 
       _controller.TransitionTo(new WordSelectedState(_controller));
    }
}