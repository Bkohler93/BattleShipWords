using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

public class SelectedState : ITileState
{
    private TileController _controller;
    private bool _hasConflict;

    public SelectedState(TileController controller, bool hasConflict)
    {
       GD.Print($"SelectedState: transitioning to selectedstate, hasConflict={hasConflict}"); 
        _controller = controller;
        _hasConflict = hasConflict;
    }

    public override void Enter()
    {
        if (_hasConflict)
        {
            //TODO display SelectedError StyleBox
        }
        else
        {
            //TODO display Selected StyleBox
        }
    }

    public override void Exit()
    {
    }

    // public override void Select(bool hasConflict)
    // {
    //     GD.Print("re-selecting a selected tile... shouldn't happen too often");
    // }

    public override void Release()
    {
        _controller.TransitionTo(new TileIdleState(_controller));
    }

    public override void Predict(bool hasConflict, bool isValid, string letter)
    {
       GD.Print($"SelectedState: new hasConflict={hasConflict}, original _hasConflict={_hasConflict}, letter={letter}"); 
        if (_hasConflict) return; 
        _controller.TransitionTo(new TilePendingState(_controller, false, isValid, letter, this));
    }

    // public override void Revert()
    // {
    //     _controller.TransitionTo(OriginalState);
    // }
    //
    // public override void PlacingLetter(string letter, bool isValid)
    // {
    //     _controller.TransitionTo(new PlacingState(_controller, OriginalState, isValid));
    // }
    public bool HasConflict()
    {
        return _hasConflict;
    }
}
