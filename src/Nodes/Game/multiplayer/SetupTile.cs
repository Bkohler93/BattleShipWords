using Godot;
using System;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Services.GameManager;
using BattleshipWithWords.Services.Multiplayer.Setup.TileController;
using BattleshipWithWords.Services.Multiplayer.Setup.TileController.States;

// public enum SetupTileStatus
// {
//     Idle,
//     Selected,
//     SelectedError,
//     Placing,
//     PlacingError,
//     Placed
// }

public enum PlacementDirection
{
    Right,
    Down,
    None,
}

public partial class SetupTile : Panel 
{
    private SetupController _setupController;
    [Export] private int _directionSensitivity = 8;
    [Export] public Label LetterLabel;
    
    private TileController _controller;
    
    // private Vector2 _distanceTraveled = Vector2.Zero;
    // private Vector2 _placementThreshold;
    // private SetupTileStatus _status = SetupTileStatus.Idle;
    private PlacementDirection _placementDirection = PlacementDirection.None;
    
    // before changing status when attempting a placement this will be set for easy restore if placement doesn't work out.
    // private SetupTileStatus _beforePlacementAttemptStatus; 

    public int Row=-1;
    public int Col=-1;
    private float _size;
    
    private StyleBox _idleStyleBox;
    private StyleBox _selectedStyleBox;
    private StyleBox _placingStyleBox;
    private StyleBox _placingFailedStyleBox;
    private StyleBox _placedStyleBox;

    public void Init(SetupController controller, int row, int col, float size, StyleBox idleStyleBox, StyleBox selectedStyleBox, StyleBox placingStyleBox, StyleBox placingFailedStyleBox, StyleBox placedStyleBox)
    {
        _controller = new TileController(this);
        _setupController = controller; 
        Row = row;
        Col = col;
        _size = size;
        // _placementThreshold = new Vector2(_size / 2, _size / 2);
        _idleStyleBox = idleStyleBox;
        _selectedStyleBox = selectedStyleBox;
        _placingStyleBox = placingStyleBox;
        _placedStyleBox = placedStyleBox;
        _placingFailedStyleBox = placingFailedStyleBox;
        _controller.TransitionTo(new IdleState(_controller));
    }

    public void Select(bool hasConflict)
    {
        _controller.HandleSelect(hasConflict);
    }

    public void Release()
    {
        _controller.HandleRelease();
    }

    public void Predict(bool hasConflict, bool isValid, string letter)
    {
        _controller.HandlePrediction(hasConflict, isValid, letter);
    }

    // public bool CanBePlaced()
    // {
    //     return _status == SetupTileStatus.Placing;
    // }

    // public void SetStatus(SetupTileStatus newStatus)
    // {
    //     if (_status == newStatus) return;
    //     
    //     _status = newStatus;
    //     
    //    switch (_status)
    //    {
    //        case SetupTileStatus.Placing:
    //            AddThemeStyleboxOverride("panel", _placingStyleBox);
    //            break;
    //        case SetupTileStatus.PlacingError:
    //            AddThemeStyleboxOverride("panel", _placingFailedStyleBox);
    //            break;
    //        case SetupTileStatus.Placed:
    //            AddThemeStyleboxOverride("panel", _placedStyleBox);
    //            break;
    //        case SetupTileStatus.Idle:
    //            AddThemeStyleboxOverride("panel", _idleStyleBox);
    //            break;
    //        case SetupTileStatus.Selected:
    //            AddThemeStyleboxOverride("panel", _selectedStyleBox);
    //            break;
    //        case SetupTileStatus.SelectedError:
    //            AddThemeStyleboxOverride("panel", _placingFailedStyleBox);
    //            break;
    //    }; 
    // }

    // public void SetLetter(string letter)
    // {
    //     LetterLabel.Text = letter;
    // }

    // public void RemoveLetter()
    // {
    //     LetterLabel.Text = "";
    // }
    
    
    // public void PlacingLetter(string letter, bool isValid)
    // {
    //     _controller.PlacingLetter(letter, isValid);
    //     // _letterLabel.Text = letter;
    //     // _controller.TransitionTo(new PlacingState(_controller, _controller.CurrentState));
    // }
    
    //
    // public void Reset()
    // {
    //     _controller.Reset();
    // }

    public override void _GuiInput(InputEvent @event)
    {
        _setupController.HandleTileGuiEvent(@event, this);
    }
    
    
    // public void Revert()
    // {
    //     _controller.Revert();
    // }
    

    public static bool operator ==(SetupTile a, SetupTile b) => a!.Col == b!.Col && a.Row == b.Row;

    public static bool operator !=(SetupTile a, SetupTile b) => !(a == b);

    // public void SelectingTile()
    // {
    //     _controller.TransitionTo(new SelectedState(_controller, _controller.CurrentState));
    // }

    // public void PlacingLetterError(string letter)
    // {
    //     _controller.PlacingLetterError(letter);
    // }

    // public void Release()
    // {
    //     _controller.Release();
    // }

    // public bool IsPlaced()
    // {
    //     return _controller.CurrentState is PlacedState;
    // }

    // public bool HasLetter(string letter)
    // {
    //     return LetterLabel.Text == letter;
    // }
    //
    // public bool CanBePlaced()
    // {
    //     return _controller.CurrentState is PlacingState;
    // }
    //
    // public void SetPlaced()
    // {
    //     _controller.TransitionTo(new PlacedState(_controller));
    // }
    public void Reset()
    {
        _controller.Reset();
    }

    public bool IsPlaced()
    {
        return _controller.CurrentState is PlacedState;
    }

    public bool HasLetter(string letter)
    {
        return LetterLabel.Text == letter; 
    }

    public bool CanBePlaced()
    {
        return _controller.CurrentState is PendingState currentState && currentState.IsPlaceable();
    }

    public void Retract()
    {
        _controller.HandleRetract();
    }

    public bool HasConflict()
    {
        return _controller.CurrentState is SelectedState currentState && currentState.HasConflict();
    }

    public bool IsPending()
    {
        return _controller.CurrentState is PendingState;
    }

    public bool IsSelected()
    {
        return _controller.CurrentState is SelectedState;
    }
}

