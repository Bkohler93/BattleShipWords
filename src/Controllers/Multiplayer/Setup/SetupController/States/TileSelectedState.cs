using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup.States;

public class TileSelectedState: SetupState
{
    private SetupController _controller;
    private SetupTile _selectedTile;
    private Vector2 _distanceTraveled = Vector2.Zero;
    private Vector2 _placementThreshold;

    public TileSelectedState(SetupController controller, SetupTile selectedTile)
    {
        _controller = controller;
        _selectedTile = selectedTile;
        _placementThreshold = _selectedTile.Size / 2;
    }

    public override void Enter()
    {
        var hasConflict = !_controller.IsValidSelection(_selectedTile.Col, _selectedTile.Row, _controller.SelectedWord[0].ToString()); 
        _selectedTile.Select(hasConflict);
    }

    public override void Exit()
    {
    }

    public override void HandleTileGuiEvent(InputEvent @event, SetupTile tile)
    {
        if (tile != _selectedTile)
            return;
        
        if (@event is InputEventScreenTouch { Pressed: false })
        {
            _selectedTile.Release();
            _controller.TransitionTo(new WordSelectedState(_controller));
        } else if (@event is InputEventScreenDrag drag)
        {
            if (tile.HasConflict()) return;
            _distanceTraveled += drag.Relative;
            if (_distanceTraveled.X > _placementThreshold.X)
                _controller.TransitionTo(new PlacingWordState(_controller, _selectedTile, _distanceTraveled, PlacementDirection.Right));
            else if (_distanceTraveled.Y > _placementThreshold.Y)
                _controller.TransitionTo(new PlacingWordState(_controller, _selectedTile, _distanceTraveled, PlacementDirection.Down));
                
        }
    }
}