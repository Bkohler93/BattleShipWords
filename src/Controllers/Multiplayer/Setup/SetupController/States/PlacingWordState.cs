using System;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup.States;

public class PlacingWordState : SetupState
{
    private SetupController _controller;
    private SetupTile _originatingFromTile;
    private Vector2 _distanceTraveled;
    private Vector2 _placementThreshold;
    private PlacementDirection _placementDirection;
    private readonly int _directionSensitivity = 10;

    public PlacingWordState(SetupController controller, SetupTile originatingFromTile, Vector2 distanceTraveled, PlacementDirection placementDirection)
    {
        _controller = controller;
        _originatingFromTile = originatingFromTile;
        _distanceTraveled = distanceTraveled;
        _placementThreshold = _originatingFromTile.Size / 2;
        _placementDirection = placementDirection;
    }

    public override void Enter()
    {
        _controller.PredictSelectedWordPlacement(_originatingFromTile, _placementDirection);
    }

    public override void Exit()
    {
    }

    public override void HandleTileGuiEvent(InputEvent @event, SetupTile tile)
    {
        if (_originatingFromTile != tile) return;

        if (@event is InputEventScreenTouch { Pressed: false })
        {
            if (_controller.TryPlaceSelectedWord(_originatingFromTile, _placementDirection))
            {
                if (_controller.AllWordsPlaced())
                    _controller.TransitionTo(new AllPlacedState(_controller));
            }
            _controller.TransitionTo(new WordSelectedState(_controller));
        } else if (@event is InputEventScreenDrag drag)
        {
            _distanceTraveled += drag.Relative;
            if (_distanceTraveled.X < _placementThreshold.X && _distanceTraveled.Y < _placementThreshold.Y)
            {
                _controller.RetractPreviousPrediction(_originatingFromTile, _placementDirection); 
                _controller.TransitionTo(new TileSelectedState(_controller, _originatingFromTile));
            }
            
            if (drag.Relative.X > _directionSensitivity && _placementDirection != PlacementDirection.Right)
            {
                _controller.RetractPreviousPrediction(_originatingFromTile, PlacementDirection.Down);
                _placementDirection = PlacementDirection.Right;
                _controller.PredictSelectedWordPlacement(_originatingFromTile, _placementDirection);
            } else if (drag.Relative.Y > _directionSensitivity && _placementDirection != PlacementDirection.Down)
            {
                _controller.RetractPreviousPrediction(_originatingFromTile, PlacementDirection.Right);
                _placementDirection = PlacementDirection.Down;
                _controller.PredictSelectedWordPlacement(_originatingFromTile, _placementDirection);
            }        
        }
    }
}