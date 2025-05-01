using Godot;

namespace BattleshipWithWords.Services.Multiplayer.Setup.TileController.States;

public class PendingState: ITileState
{
    private TileController _controller;
    private bool _hasConflict;
    private bool _isValid;
    private ITileState _originalState;
    private string _letter;
    private bool _didChangeLetter;

    public PendingState(TileController controller, bool hasConflict, bool isValid, string letter, ITileState originalState)
    {
        _controller = controller;
        _hasConflict = hasConflict;
        _isValid = isValid;
        _originalState = originalState;
        _letter = letter;
    }

    public bool IsPlaceable()
    {
        return !_hasConflict && _isValid;
    }

    public override void Enter()
    {
        GD.Print($"PendingState: hasConflict={_hasConflict} trying to set letter={_letter}");
        if (_hasConflict)
        {
            //TODO display PlacingError stylebox
            if (!_controller.HasLetter())
                _controller.SetLetter(_letter);
        }
        else
        {
            //TODO display Placed stylebox
            _controller.SetLetter(_letter);
        }
    }

    public override void Exit()
    {
    }

    public override void Retract()
    {
        if (_originalState is not PlacedState)
        {
            _controller.RemoveLetter();
        }
        _controller.TransitionTo(_originalState);
    }

    public override void Release()
    {
        if (_originalState is PlacedState || _isValid)
        {
            _controller.TransitionTo(new PlacedState(_controller));
            return;
        }
        _controller.RemoveLetter();
        _controller.TransitionTo(new IdleState(_controller));    
    }
}