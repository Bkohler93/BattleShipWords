using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BattleshipWithWords.Services.WordList;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

public class SetupController
{
    public SetupState CurrentState;
    public MultiplayerSetup SetupNode;
    private FileWordListService _wordListService = new("res://data/wordlists");
    
    public readonly List<List<SetupTile>> Board = [];

    public readonly List<List<string>> Words = [new(), new(), new()];
    public int WordSelectionIndex = 0; // used to access correct words from Words using Left/Right controls on MultiplayerSetupNode.
    
    public readonly Random R = new();
    public string SelectedWord;
    public bool IsThreeLetterWordPlaced;
    public bool IsFourLetterWordPlaced;
    public bool IsFiveLetterWordPlaced;

    public SetupController(MultiplayerSetup setupNode)
    {
        SetupNode = setupNode;
        Words[0] = _wordListService.GetWords("threeletters.txt", 3);
        Words[1] = _wordListService.GetWords("fourletters.txt", 4);
        Words[2] = _wordListService.GetWords("fiveletters.txt", 5);
    }
    
    public void TransitionTo(SetupState newState)
    {
        CurrentState?.Exit();

        CurrentState = newState;
        CurrentState.Enter();
    }

    public void HandleTileGuiEvent(InputEvent @event, SetupTile tile)
    {
        CurrentState?.HandleTileGuiEvent(@event, tile);
    }

    public void HandleConfirmButton()
    {
        CurrentState?.HandleConfirmButton();
    }

    public void HandleNextWordsButton()
    {
        CurrentState?.HandleNextWordsButton();
    }

    public void HandlePreviousWordsButton()
    {
        CurrentState?.HandlePreviousWordsButton();
    }

    public void HandleWordSelectionButton(WordSelectionButtonId buttonId)
    {
        CurrentState?.HandleWordSelectionButton(buttonId);
    }

    public void HandleUndoSelectionButton()
    {
        CurrentState?.HandleUndoSelectionButton();
    }

    public void PredictSelectedWordPlacement(SetupTile startingTile, PlacementDirection direction)
    {
        var startingCol = startingTile.Col;
        var startingRow = startingTile.Row;
        GD.Print($"placing at col={startingCol}, row={startingRow}, direction={direction}");
        
        switch (direction)
        {
            case PlacementDirection.Down:
                _predictDown(startingCol, startingRow);
                break;
            case PlacementDirection.Right:
                _predictRight(startingCol, startingRow);
                break;
            case PlacementDirection.None:
                throw new Exception("Should not be trying to place with no direction");
        }
    }
    
    // this method should only be called at some point AFTER PredictSelectedWordPlacement has executed.
    // This method relies on knowledge about the status of tiles that PredictSelectedWordPlacement will have
    // created.
    public bool TryPlaceSelectedWord(SetupTile startingTile, PlacementDirection direction)
    {
        var startingCol = startingTile.Col;
        var startingRow = startingTile.Row;

        switch (direction)
        {
            case PlacementDirection.Down:
                return _tryPlaceDown(startingCol, startingRow);
            case PlacementDirection.Right:
                return _tryPlaceRight(startingCol, startingRow);
            case PlacementDirection.None:
                throw new Exception("Should not be trying to place with no direction");
            default:
                throw new Exception("Placement direction should be set when calling this method");
        }
    }
    
    private bool _tryPlaceRight(int startingCol, int row)
    {
        var canBePlaced = true;
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingCol + i > 5) break;
            var tile = Board[row][startingCol+i];
            if (!tile.CanBePlaced())
               canBePlaced = false; 
        }

        if (canBePlaced)
            _placeRight(startingCol, row);
        else
            _restoreRight(startingCol, row);
        
        return canBePlaced;
    }

    private void _placeRight(int startingCol, int row)
    {
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            var tile = Board[row][startingCol + i];
            tile.Release();
            // tile.SetPlaced();
        }

        if (SelectedWord.Length == 3)
        {
            IsThreeLetterWordPlaced = true;
            SetupNode.ThreeLetterWordButton.SetDisabled(true); 
        }
        else if (SelectedWord.Length == 4)
        {
            IsFourLetterWordPlaced = true;
            SetupNode.FourLetterWordButton.SetDisabled(true); 
        }
        else if (SelectedWord.Length == 5)
        {
            IsFiveLetterWordPlaced = true;
            SetupNode.FiveLetterWordButton.SetDisabled(true); 
        }
        else
            throw new Exception($"There is no way the selected word's length has not been checked: {SelectedWord.Length}");    
    }

    private void _restoreRight(int startingCol, int row)
    {
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingCol + i > 5) break;
            var tile = Board[row][startingCol + i];
            tile.Retract();
            // tile.Revert();
        }
    }

    private bool _tryPlaceDown(int col, int startingRow)
    {
        var canBePlaced = true;
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingRow + i > 5) break;
            var tile = Board[startingRow+i][col];
            if (!tile.CanBePlaced())
               canBePlaced = false; 
        }

        if (canBePlaced)
            _placeDown(col, startingRow);
        else
            _restoreDown(col, startingRow);
        
        return canBePlaced;
    }

    private void _placeDown(int col, int startingRow)
    {
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            var tile = Board[startingRow+i][col];
            // tile.SetPlaced();
            tile.Release();
        }

        if (SelectedWord.Length == 3)
        {
            IsThreeLetterWordPlaced = true;
            SetupNode.ThreeLetterWordButton.SetDisabled(true); 
        }
        else if (SelectedWord.Length == 4)
        {
            IsFourLetterWordPlaced = true;
           SetupNode.FourLetterWordButton.SetDisabled(true); 
        }
        else if (SelectedWord.Length == 5)
        {
            IsFiveLetterWordPlaced = true;
           SetupNode.FiveLetterWordButton.SetDisabled(true); 
        }
        else
            throw new Exception($"There is no way the selected word's length has not been checked: {SelectedWord.Length}");
    }

    private void _restoreDown(int col, int startingRow)
    {
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingRow + i > 5) break;
            var tile = Board[startingRow+i][col];
            tile.Release();
            // tile.Revert();
        }
    }
    
    private void _predictRight(int startingCol, int row)
    {
        //handle error cases first
        var invalidOffsets = new HashSet<int>();
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            var tile = Board[row][startingCol+i];
            
            //check if current tile is already placed and NOT the same letter
            if (tile.IsPlaced() && !tile.HasLetter(SelectedWord[i].ToString()))
            {
                invalidOffsets.Add(i);
                continue;
            }
            
            //check if at edge of board and not at last letter of word
            if (startingCol + i == 5 && i != SelectedWord.Length - 1)
            {  
                invalidOffsets.Add(i);
                break;
            }
            
            //check if first letter has any conflicting tiles to the left 
            if (i == 0 && startingCol != 0)
            {
                var tileToLeft = Board[row][startingCol-1];
                if (tileToLeft.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }
            
            //check if last letter has any conflicting tiles to the right
            if (i == SelectedWord.Length - 1 && startingCol + i < 5)
            {
                var tileToRight = Board[row][startingCol+i+1];
                if (tileToRight.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }
            
            // check if tile above current tile conflicts
            if (row != 0)
            {
                var tileAbove = Board[row-1][startingCol+i];
                if (tileAbove.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile below current tile conflicts
            if (row != 5)
            {
                var tileBelow = Board[row+1][startingCol+i];
                if (tileBelow.IsPlaced())
                    invalidOffsets.Add(i);
            }
        }
    
        switch (invalidOffsets.Count)
        {
            case 0:
            {
                GD.Print("SetupController: valid placement of selected word ", SelectedWord); 
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    var tile = Board[row][startingCol+i];
                    tile.Predict(false, true, SelectedWord[i].ToString());   
                    // tile.PlacingLetter(SelectedWord[i].ToString(), true);
                }

                return;
            }
            default:
            {
                GD.Print($"SetupController: invalid placement of selected word {SelectedWord}\tinvalidOffsets={invalidOffsets}"); 
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    if (startingCol + i > 5) break;
                    GD.Print($"SetupController: Setting {SelectedWord[i]} at row={row}, col={startingCol+i}");
                    var tile = Board[row][startingCol+i];
                    if (invalidOffsets.Contains(i))
                    {
                        tile.Predict(true, false, SelectedWord[i].ToString());
                        // tile.PlacingLetterError(SelectedWord[i].ToString());
                    }
                    else
                    {
                        tile.Predict(false, false, SelectedWord[i].ToString());
                        // tile.PlacingLetter(SelectedWord[i].ToString(), false);
                    }
                }

                break;
            }
        }
    }
    
    // checks for any conflicting tiles. If conflicts are found will set tiles to PlacingError, if no conflicts will set
    // tiles to Placing
    private void _predictDown(int col, int startingRow)
    {
        var invalidOffsets = new HashSet<int>();
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            var tile = Board[startingRow+i][col];
            
            //check if current tile is already placed and NOT the same letter
            if (tile.IsPlaced() && !tile.HasLetter(SelectedWord[i].ToString()))
            {
                invalidOffsets.Add(i);
                continue;
            }
            
            //check if at edge of board and not at last letter of word
            if (startingRow + i == 5 && i != SelectedWord.Length - 1)
            {  
                invalidOffsets.Add(i);
                break;
            }
            
            //check if first letter has any conflicting tiles above it
            if (i == 0 && startingRow != 0)
            {
                var tileAbove = Board[startingRow - 1][col];
                if (tileAbove.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }
            
            //check if last letter has any conflicting tiles beneath it
            if (i == SelectedWord.Length - 1 && startingRow + i >= 5)
            {
                var tileBelow = Board[startingRow + 1][col];
                if (tileBelow.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }
            
            // check if tile right of current tile conflicts
            if (col != 5)
            {
                var tileOnRight = Board[startingRow][col+1];
                if (tileOnRight.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile left of current tile conflicts
            if (col != 0)
            {
                var tileOnLeft = Board[startingRow][col-1];
                if (tileOnLeft.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }
        }

        switch (invalidOffsets.Count)
        {
            case 0:
            {
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    var tile = Board[startingRow+i][col];
                    tile.Predict(false, true, SelectedWord[i].ToString());
                    // tile.PlacingLetter(SelectedWord[i].ToString(), true);
                }

                return;
            }
            default:
            {
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    if (startingRow + i > 5) break;
                    var tile = Board[startingRow+i][col];
                    if (invalidOffsets.Contains(i))
                    {
                        tile.Predict(true, false, SelectedWord[i].ToString());
                        // tile.PlacingLetterError(SelectedWord[i].ToString());
                    }
                    else
                    {
                        tile.Predict(false, false, SelectedWord[i].ToString());   
                        // tile.PlacingLetter(SelectedWord[i].ToString(), false);
                    }
                }

                break;
            }
        }
    }
    
    public void OnNextWordsButtonPressed()
    {
        IsThreeLetterWordPlaced = false;
        IsFourLetterWordPlaced = false;
        IsFiveLetterWordPlaced = false;
        SelectedWord = "";
        SetupNode.PreviousWordsButton.Disabled = false;

        _enableWordButtons();
        _resetBoard(); 
        
        WordSelectionIndex++;
        
        SetupNode.ThreeLetterWordButton.Text = Words[0][WordSelectionIndex];
        SetupNode.FourLetterWordButton.Text = Words[1][WordSelectionIndex];
        SetupNode.FiveLetterWordButton.Text = Words[2][WordSelectionIndex];
        
        if (WordSelectionIndex == 2)
            SetupNode.NextWordsButton.Disabled = true;
    }

    public void OnWordSelectionButtonPressed(WordSelectionButtonId buttonId)
    {
        switch (buttonId)
        {
            case WordSelectionButtonId.Three:
                SetupNode.ThreeLetterWordButton.SetDisabled(true);

                if (!IsFourLetterWordPlaced)
                    SetupNode.FourLetterWordButton.SetDisabled(false);
                if (!IsFiveLetterWordPlaced)
                    SetupNode.FiveLetterWordButton.SetDisabled(false);

                SelectedWord = SetupNode.ThreeLetterWordButton.Text;
                break;
            case WordSelectionButtonId.Four:
                SetupNode.FourLetterWordButton.SetDisabled(true);

                if (!IsThreeLetterWordPlaced)
                    SetupNode.ThreeLetterWordButton.SetDisabled(false);
                if (!IsFiveLetterWordPlaced)
                    SetupNode.FiveLetterWordButton.SetDisabled(false);

                SelectedWord = SetupNode.FourLetterWordButton.Text;
                break;
            case WordSelectionButtonId.Five:
                SetupNode.FiveLetterWordButton.SetDisabled(true);

                if (!IsThreeLetterWordPlaced)
                    SetupNode.ThreeLetterWordButton.SetDisabled(false);
                if (!IsFourLetterWordPlaced)
                    SetupNode.FourLetterWordButton.SetDisabled(false);

                SelectedWord = SetupNode.FiveLetterWordButton.Text;
                break;
        }
    }
    
    public void OnPreviousWordsButtonPressed()
    {
        IsThreeLetterWordPlaced = false;
        IsFourLetterWordPlaced = false;
        IsFiveLetterWordPlaced = false;
        SelectedWord = "";
        SetupNode.NextWordsButton.Disabled = false;

        _enableWordButtons();
        _resetBoard(); 
        
        WordSelectionIndex--;
        
        SetupNode.ThreeLetterWordButton.Text = Words[0][WordSelectionIndex];
        SetupNode.FourLetterWordButton.Text = Words[1][WordSelectionIndex];
        SetupNode.FiveLetterWordButton.Text = Words[2][WordSelectionIndex];
        
        if (WordSelectionIndex == 0)
            SetupNode.PreviousWordsButton.Disabled = true;
    }

    private void _resetBoard()
    {
        for (var i = 0; i < 6; i++)
        {
            for (var j = 0; j < 6; j++)
            {
                var tile = Board[i][j];
                tile.Reset();
            }
        }
        SetupNode.ThreeLetterWordButton.SetDisabled(false);
        SetupNode.FourLetterWordButton.SetDisabled(false);
        SetupNode.FiveLetterWordButton.SetDisabled(false);
        IsThreeLetterWordPlaced = false;
        IsFourLetterWordPlaced = false;
        IsFiveLetterWordPlaced = false;
        SetupNode.ConfirmButton.SetDisabled(true);
    }
    
    private void _enableWordButtons()
    {
        SetupNode.ThreeLetterWordButton.SetDisabled(false);
        SetupNode.FourLetterWordButton.SetDisabled(false);
        SetupNode.FiveLetterWordButton.SetDisabled(false);
    }

    public bool AllWordsPlaced()
    {
        return IsThreeLetterWordPlaced && IsFourLetterWordPlaced && IsFiveLetterWordPlaced; 
    }

    // public void CancelTileSelection(SetupTile selectedTile)
    // {
    //     selectedTile.SetStatus(SetupTileStatus.Idle);
    // }

    public bool IsValidSelection(int selectedTileCol, int selectedTileRow, string letter)   {
        var tile = Board[selectedTileRow][selectedTileCol];
        var tileLetter = tile.LetterLabel.Text;
        
        if (tileLetter.Equals(letter))
            return true;
        if (tileLetter != "") return false;
        
            // check tile to left of selected tile, must be empty
            if (selectedTileCol > 0)
            {
                var tileToLeft = Board[selectedTileRow][selectedTileCol - 1];
                if (tileToLeft.LetterLabel.Text != "")
                    return false;
            }
            
            // check tile to right of selected tile, must be empty
            if (selectedTileCol < 5)
            {
                var tileToRight = Board[selectedTileRow][selectedTileCol + 1];
                if (tileToRight.LetterLabel.Text != "")
                    return false;
            }
            
            // check tile above selected tile, must be empty
            if (selectedTileRow > 0)
            {
                var tileAbove = Board[selectedTileRow - 1][selectedTileCol];
                if (tileAbove.LetterLabel.Text != "")
                    return false;
            }
            
            //check tile below selected tile, must be empty
            if (selectedTileRow < 5)
            {
                var tileBelow = Board[selectedTileRow + 1][selectedTileCol];
                if (tileBelow.LetterLabel.Text != "")
                    return false;
            }

            return true;
    }

    public void RetractPreviousPrediction(SetupTile tile, PlacementDirection direction)
    {
        switch (direction)
        {
            case PlacementDirection.Down:
                _resetPredictionDown(tile);
                break;
            case PlacementDirection.Right:
                _resetPredictionRight(tile);
                break;
            case PlacementDirection.None:
                throw new Exception("Should not try resetting if placement direction is None");
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }

    private void _resetPredictionDown(SetupTile tile)
    {
        var startingRow = tile.Row;
        var col = tile.Col;
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingRow + i > 5) return;
            var t = Board[startingRow + i][col];
            t.Retract();
        }
    }

    private void _resetPredictionRight(SetupTile tile)
    {
        var startingCol = tile.Col;
        var row = tile.Row;

        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingCol + i > 5) return;
            var t = Board[row][startingCol + i];
            t.Retract();
        }
    }
}

public abstract class SetupState
{
    public abstract void Enter();
    public abstract void Exit();
    public virtual void HandleTileGuiEvent(InputEvent @event, SetupTile tile) {}
    public virtual void HandleConfirmButton() {}
    public virtual void HandleNextWordsButton() {}
    public virtual void HandlePreviousWordsButton() {}
    public virtual void HandleWordSelectionButton(WordSelectionButtonId buttonId) {}
    public virtual void HandleUndoSelectionButton() {}
}

public enum WordSelectionButtonId
{
    Three,
    Four,
    Five
}