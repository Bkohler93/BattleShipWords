namespace BattleshipWithWords.Services.Multiplayer.Setup;

public class PlaceableState : ITileState
{
    private TileController _controller;
    private ITileState _originalState;

    public PlaceableState(TileController controller, ITileState originalState)
    {
        _controller = controller;
        _originalState = originalState;
    }

    public override void Enter()
    {
        //TODO show placable StyleBox
        _controller.DisplayStyleBox("placeable");
    }

    public override void Exit()
    {
    }

    public override void Retract()
    {
        _controller.TransitionTo(_originalState);
    }

    public override void Release()
    {
       _controller.TransitionTo(new PlacedState(_controller)); 
    }
}