using System;
using BattleshipWithWords.Services.Multiplayer.Setup.TileController.States;

namespace BattleshipWithWords.Services.Multiplayer.Setup.TileController;

public class TileController
{
    private ITileState _currentState;
    private SetupTile _tileNode;
    
    public ITileState CurrentState => _currentState;

    public TileController(SetupTile tileNode)
    {
        _tileNode = tileNode;
    }

    public void TransitionTo(ITileState newState)
    {
        if (newState == _currentState) return;
        _currentState?.Exit(); 
        _currentState = newState;
        _currentState?.Enter();
    }

    public void Reset()
    {
        _tileNode.LetterLabel.Text = "";
        TransitionTo(new IdleState(this));
    }

    public void HandleSelect(bool hasConflict)
    {
        _currentState?.Select(hasConflict);
    }

    public void HandleRelease()
    {
        _currentState?.Release();
    }

    public void HandlePrediction(bool hasConflict, bool isValid, string letter)
    {
        _currentState?.Predict(hasConflict, isValid, letter);
    }

    public void HandleRetract()
    {
        _currentState?.Retract();
    }

    public void RemoveLetter()
    {
        _tileNode.LetterLabel.Text = "";
    }

    public bool HasLetter()
    {
        return _tileNode.LetterLabel.Text != "";
    }

    public void SetLetter(string letter)
    {
        _tileNode.LetterLabel.Text = letter;
    }
}

public abstract class ITileState
{
    public abstract void Enter();
    public abstract void Exit();

    public virtual void Release()
    {
        throw new Exception($"TileState: {this} - should not call Release()");
    }

    public virtual void Select(bool hasConflict)
    {
        throw new Exception($"TileState: {this}  - should not have called Select");
    }

    public virtual void Predict(bool hasConflict, bool isValid, string letter)
    {
        throw new Exception($"TileState: {this}  - should not have called Predict");
    }

    public virtual void Retract()
    {
        throw new Exception($"TileState: {this}  - should not have called Retract");
    }
}