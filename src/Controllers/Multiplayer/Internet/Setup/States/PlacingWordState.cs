using System;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

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
        _controller.RemoveOtherDirectionPlacements(_originatingFromTile.Col, _originatingFromTile.Row, _placementDirection);
        _controller.SetWordToPlaceable(_originatingFromTile.Col, _originatingFromTile.Row, _placementDirection);
    }

    public override void Exit()
    {
    }

    public override void HandleTileGuiEvent(InputEvent @event, SetupTile tile)
    {
        if (_originatingFromTile != tile) return;

        if (@event is InputEventScreenTouch { Pressed: false })
        {
            
            _controller.ConfirmPlacements(_originatingFromTile.Col, _originatingFromTile.Row, _placementDirection);
            if (_controller.AllWordsPlaced())
            {
                Logger.Print("all words placed");
                _controller.TransitionTo(new AllPlacedState(_controller));
                
            }
            else
            {
                Logger.Print("all words not placed yet");
                _controller.TransitionTo(new InitialState(_controller));
                
            }
        } else if (@event is InputEventScreenDrag drag)
        {
            _distanceTraveled += drag.Relative;
            if (_distanceTraveled.X < _placementThreshold.X && _distanceTraveled.Y < _placementThreshold.Y)
            {
                // _controller.RetractPreviousPrediction(_originatingFromTile, _placementDirection); 
                _controller.RetractPlaceableTiles(_originatingFromTile.Col, _originatingFromTile.Row, _placementDirection);
                _controller.TransitionTo(new StartingTileSelectedState(_controller, _originatingFromTile));
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

    public override void Connecting()
    {
        throw new NotImplementedException();
    }

    public override void Connected()
    {
        throw new NotImplementedException();
    }

    public override void UnableToConnect()
    {
        throw new NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new NotImplementedException();
    }

    public override void Reconnecting()
    {
        throw new NotImplementedException();
    }

    public override void Reconnected()
    {
        throw new NotImplementedException();
    }

    public override void Receive(IServerReceivable message)
    {
        switch (message)
        {
            default:
                Logger.Print("unknown message type during PlacingWordState");
                break;
        }
    }

    public override void Disconnecting()
    {
        throw new NotImplementedException();
    }

    public override void HttpResponse(string response)
    {
        throw new NotImplementedException();
    }
}