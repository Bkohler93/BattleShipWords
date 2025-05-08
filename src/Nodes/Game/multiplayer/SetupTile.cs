using Godot;
using System;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using Godot.Collections;

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
    
    private PlacementDirection _placementDirection = PlacementDirection.None;

    public int Row=-1;
    public int Col=-1;
    private float _size;
    
    private Dictionary<string, StyleBox> _styleBoxDict;

    public void Init(SetupController controller, int row, int col, float size, Dictionary<string,StyleBox> styleBoxDict)
    {
        _controller = new TileController(this);
        _setupController = controller; 
        Row = row;
        Col = col;
        _size = size;
        _styleBoxDict = styleBoxDict;
        _controller.TransitionTo(new TileIdleState(_controller));
    }

    public void Release()
    {
        _controller.HandleRelease();
    }

    public void Predict(bool hasConflict, bool isValid, string letter)
    {
        _controller.HandlePrediction(hasConflict, isValid, letter);
    }

    public override void _GuiInput(InputEvent @event)
    {
        _setupController.HandleTileGuiEvent(@event, this);
    }
    
    public static bool operator ==(SetupTile a, SetupTile b) => a!.Col == b!.Col && a.Row == b.Row;

    public static bool operator !=(SetupTile a, SetupTile b) => !(a == b);

    public void Reset()
    {
        _controller.Reset();
    }

    public bool IsPlaced()
    {
        return _controller.CurrentState is PlacedState || (_controller.CurrentState is TilePendingState state && state.IsOriginallyPlaced());
    }

    public bool HasLetter(string letter)
    {
        return LetterLabel.Text == letter; 
    }

    public bool CanBePlaced()
    {
        return _controller.CurrentState is TilePendingState currentState && currentState.IsPlaceable();
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
        return _controller.CurrentState is TilePendingState;
    }

    public bool IsSelected()
    {
        return _controller.CurrentState is SelectedState;
    }

    public void SetPlaceable()
    {
        _controller.HandleSetPlaceable();
    }

    public void SetPlaced()
    {
        _controller.TransitionTo(new PlacedState(_controller));
    }

    public void ChangeToStyleBox(string name)
    {
        if (!_styleBoxDict.ContainsKey(name))
            throw new Exception($"no stylebox exists with name {name}");
        var styleBox = _styleBoxDict[name];
        
        AddThemeStyleboxOverride("panel", styleBox);
    }
}

