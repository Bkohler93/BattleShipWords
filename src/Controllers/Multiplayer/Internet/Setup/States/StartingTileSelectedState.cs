using System.Collections.Generic;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

public class StartingTileSelectedState: SetupState
{
    private SetupController _controller;
    private SetupTile _selectedTile;
    private Vector2 _distanceTraveled = Vector2.Zero;
    private Vector2 _placementThreshold;
    private HashSet<PlacementDirection> _availablePlacementDirections;

    public StartingTileSelectedState(SetupController controller, SetupTile selectedTile)
    {
        _controller = controller;
        _selectedTile = selectedTile;
        _placementThreshold = _selectedTile.Size / 2;
    }

    public override void Enter()
    {
        _availablePlacementDirections = _controller.ShowAvailablePlacements(_selectedTile.Col, _selectedTile.Row);
        // var hasConflict = !_controller.IsValidSelection(_selectedTile.Col, _selectedTile.Row, _controller.SelectedWord[0].ToString()); 
        // _selectedTile.Select(hasConflict);
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
            // _selectedTile.Release();
            _controller.RemovePlacements(_selectedTile.Col, _selectedTile.Row);
            _controller.TransitionTo(new WordSelectedState(_controller));
        } else if (@event is InputEventScreenDrag drag)
        {
            // if (tile.HasConflict()) return;
            _distanceTraveled += drag.Relative;
            if (_distanceTraveled.X > _placementThreshold.X && _availablePlacementDirections.Contains(PlacementDirection.Right))
                _controller.TransitionTo(new PlacingWordState(_controller, _selectedTile, _distanceTraveled, PlacementDirection.Right));
            else if (_distanceTraveled.Y > _placementThreshold.Y && _availablePlacementDirections.Contains(PlacementDirection.Down))
                _controller.TransitionTo(new PlacingWordState(_controller, _selectedTile, _distanceTraveled, PlacementDirection.Down));
                
        }
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
                Logger.Print("Unknown message type during StartingTileSelectedState");
                break;
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
}