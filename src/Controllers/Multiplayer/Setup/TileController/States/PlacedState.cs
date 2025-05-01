namespace BattleshipWithWords.Services.Multiplayer.Setup.TileController.States;

public class PlacedState: ITileState
{
    private TileController _controller;

    public PlacedState(TileController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        //TODO display Placed stylebox
    }

    public override void Exit()
    {
    }

    public override void Predict(bool hasConflict, bool isValid, string letter)
    {
        _controller.TransitionTo(new PendingState(_controller, hasConflict, isValid,letter, this));
    }
}