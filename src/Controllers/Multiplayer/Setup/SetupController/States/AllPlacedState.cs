using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

public class AllPlacedState: SetupState
{
    private SetupController _controller;

    public AllPlacedState(SetupController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.SetupNode.ConfirmButton.SetDisabled(false);
    }

    public override void Exit()
    {
    }

    public override void HandleConfirmButton()
    {
        GD.Print("======== State To Send To Other Player =========");
        for (var i = 0; i < 3; i++)
        {
            var s =$"'{_controller.SelectedWords[i]}' placed at: ";
            foreach (var coord in _controller.BoardSelection[i])
            {
                s += $"({coord.row},{coord.col}),";
            } 
            GD.Print(s);
        }
        GD.Print("================================================");
        // _controller.GameManager.InitPlayerTwoGameState(_controller.SelectedWords, _controller.BoardSelection);
        _controller.SetupNode.SetupCompleteCallback(_controller.SelectedWords, _controller.BoardSelection);
    }

    public override void HandleNextWordsButton()
    {
        _controller.OnNextWordsButtonPressed();
    }
    
    public override void HandlePreviousWordsButton()
    {
        _controller.OnPreviousWordsButtonPressed();
    }
}