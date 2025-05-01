namespace BattleshipWithWords.Controllers.Multiplayer.Setup.States;

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

    public override void HandleNextWordsButton()
    {
        _controller.OnNextWordsButtonPressed();
    }

    public override void HandlePreviousWordsButton()
    {
        _controller.OnPreviousWordsButtonPressed();
    }
}