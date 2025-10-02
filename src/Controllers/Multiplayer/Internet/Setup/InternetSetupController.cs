using System;
using System.Collections.Generic;
using System.Linq;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

public class SetupController : ISetupTileEventHandler
{
    public ServerConnectionManager ServerConnectionManager;
    public SetupState CurrentState;
    public readonly InternetSetup SetupNode;
    // public OverlayManager OverlayManager;

    public readonly List<List<SetupTile>> Board = [];

    // public readonly List<List<string>> WordOptions = [new(), new(), new()];
    public readonly Dictionary<int, List<string>> WordOptions = new();

    // public int
    //     WordsSelectionIndex =
    //         0; // used to access correct words from Words using Left/Right controls on MultiplayerSetupNode.

    public Dictionary<int,int> WordSelectionIndices = new();

    // public List<List<(int row, int col)>> BoardSelection = [[], [], []];
    public Dictionary<int, List<Coordinate>> BoardSelection = new();
    // public List<string> SelectedWords;
    public Dictionary<int, string> SelectedWords = new();

    public string SelectedWord;
    // public bool IsThreeLetterWordPlaced;
    // public bool IsFourLetterWordPlaced;
    // public bool IsFiveLetterWordPlaced;
    public Dictionary<int, bool> IsWordPlaced = new();

    public SetupController(InternetSetup setupNode, ServerConnectionManager connectionManager,
        StartSetup startSetupData)
    {
        // OverlayManager = overlayManager;
        BoardSelection[3] = [];
        BoardSelection[4] = [];
        BoardSelection[5] = [];
        ServerConnectionManager = connectionManager;
        SetupNode = setupNode;
        WordOptions[3] = startSetupData.ThreeLetterWords.ToList();
        WordOptions[4] = startSetupData.FourLetterWords.ToList();
        WordOptions[5] = startSetupData.FiveLetterWords.ToList();
        SelectedWords[3] = WordOptions[3][0];
        SelectedWords[4] = WordOptions[4][0];
        SelectedWords[5] = WordOptions[5][0];
        // SelectedWords = [WordOptions[0][0], WordOptions[1][0], WordOptions[2][0]];
        WordSelectionIndices[3] = 0;
        WordSelectionIndices[4] = 0;
        WordSelectionIndices[5] = 0;
        IsWordPlaced[3] = false;
        IsWordPlaced[4] = false;
        IsWordPlaced[5] = false;
        
        foreach (var (len, btn) in SetupNode.PreviousWordButtons)
        {
                btn.SetDisabled(true); 
        }
    }

    public void TransitionTo(SetupState newState)
    {
        CurrentState?.Exit();
        CurrentState = newState;
        ServerConnectionManager.Listener = CurrentState;
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

    public void HandleNextWordsButton(int wordLength)
    {
        CurrentState?.HandleNextWordsButton(wordLength);
    }

    public void HandlePreviousWordsButton(int wordLength)
    {
        CurrentState?.HandlePreviousWordsButton(wordLength);
    }

    public void HandleWordSelectionButton(WordSelectionButtonId buttonId)
    {
        CurrentState?.HandleWordSelectionButton(buttonId);
    }

    public void HandleUndoSelectionButton()
    {
        CurrentState?.HandleUndoSelectionButton();
    }

    public HashSet<PlacementDirection> ShowAvailablePlacements(int selectedTileCol, int selectedTileRow)
    {
        var result = new HashSet<PlacementDirection>();

        if (_canPlaceRight(selectedTileCol, selectedTileRow))
        {
            result.Add(PlacementDirection.Right);
        }

        if (_canPlaceDown(selectedTileCol, selectedTileRow))
        {
            result.Add(PlacementDirection.Down);
        }

        return result;
    }

    private bool _canPlaceDown(int col, int startingRow)
    {
        var invalidOffsets = new HashSet<int>();
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            var tile = Board[startingRow + i][col];

            //check if at edge of board and not at last letter of word
            if (startingRow + i == 5 && i != SelectedWord.Length - 1)
            {
                invalidOffsets.Add(i);
                break;
            }

            //check if current tile is already placed and NOT the same letter
            if (tile.IsPlaced())
            {
                if (tile.HasLetter(SelectedWord[i].ToString()))
                    continue;
                invalidOffsets.Add(i);
                continue;
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
            if (i == SelectedWord.Length - 1 && startingRow + i < 5)
            {
                var tileBelow = Board[startingRow + i + 1][col];
                if (tileBelow.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile right of current tile conflicts
            if (col != 5)
            {
                var tileOnRight = Board[startingRow + i][col + 1];
                if (tileOnRight.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile left of current tile conflicts
            if (col != 0)
            {
                var tileOnLeft = Board[startingRow + i][col - 1];
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
                    var tile = Board[startingRow + i][col];
                    if (tile.IsPending()) continue;
                    tile.Predict(false, true, SelectedWord[i].ToString());
                    // tile.PlacingLetter(SelectedWord[i].ToString(), true);
                }

                return true;
            }
            default:
            {
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    if (startingRow + i > 5) break;
                    var tile = Board[startingRow + i][col];
                    if (tile.IsPending()) continue;
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

                return false;
            }
        }
    }

    private bool _canPlaceRight(int startingCol, int row)
    {
        //handle error cases first
        var invalidOffsets = new HashSet<int>();
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            var tile = Board[row][startingCol + i];

            //check if at edge of board and not at last letter of word
            if (startingCol + i == 5 && i != SelectedWord.Length - 1)
            {
                invalidOffsets.Add(i);
                break;
            }

            if (tile.IsPlaced())
            {
                if (tile.HasLetter(SelectedWord[i].ToString()))
                    continue;
                invalidOffsets.Add(i);
                continue;
            }



            //check if first letter has any conflicting tiles to the left 
            if (i == 0 && startingCol != 0)
            {
                var tileToLeft = Board[row][startingCol - 1];
                if (tileToLeft.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            //check if last letter has any conflicting tiles to the right
            if (i == SelectedWord.Length - 1 && startingCol + i < 5)
            {
                var tileToRight = Board[row][startingCol + i + 1];
                if (tileToRight.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile above current tile conflicts
            if (row != 0)
            {
                var tileAbove = Board[row - 1][startingCol + i];
                if (tileAbove.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile below current tile conflicts
            if (row != 5)
            {
                var tileBelow = Board[row + 1][startingCol + i];
                if (tileBelow.IsPlaced())
                    invalidOffsets.Add(i);
            }
        }

        switch (invalidOffsets.Count)
        {
            case 0:
            {
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    var tile = Board[row][startingCol + i];
                    if (tile.IsPending()) continue;
                    tile.Predict(false, true, SelectedWord[i].ToString());
                    // tile.PlacingLetter(SelectedWord[i].ToString(), true);
                }

                return true;
            }
            default:
            {
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    if (startingCol + i > 5) break;
                    var tile = Board[row][startingCol + i];
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

                return false;
            }
        }
    }

    public void RemovePlacements(int startingCol, int startingRow)
    {
        // remove right placements
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingCol + i > 5) break;
            var tile = Board[startingRow][startingCol + i];
            if (tile.IsPending())
                tile.Retract();
        }

        // remove down placements
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingRow + i > 5) break;
            var tile = Board[startingRow + i][startingCol];
            if (tile.IsPending())
                tile.Retract();
        }
    }

    public void RemoveOtherDirectionPlacements(int startingCol, int startingRow, PlacementDirection placementDirection)
    {
        var retractInDirection = placementDirection switch
        {
            PlacementDirection.Right => PlacementDirection.Down,
            PlacementDirection.Down => PlacementDirection.Right,
            _ => throw new Exception("RemoveOtherDirectionPlacements: Requires direction of Right or Down")
        };

        if (retractInDirection == PlacementDirection.Right)
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingCol + i > 5) break;
                if (i == 0) continue;
                var tile = Board[startingRow][startingCol + i];
                if (tile.IsPending())
                    tile.Retract();
            }
        }
        else
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingRow + i > 5) break;
                if (i == 0) continue;
                var tile = Board[startingRow + i][startingCol];
                if (tile.IsPending())
                    tile.Retract();
            }
        }
    }

    public void SetWordToPlaceable(int startingCol, int startingRow, PlacementDirection placementDirection)
    {
        if (placementDirection == PlacementDirection.Right)
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingCol + i > 5) break;
                var tile = Board[startingRow][startingCol + i];
                tile.SetPlaceable();
            }
        }
        else
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingRow + i > 5) break;
                var tile = Board[startingRow + i][startingCol];
                tile.SetPlaceable();
            }
        }
    }

    public void ConfirmPlacements(int startingCol, int startingRow, PlacementDirection placementDirection)
    {
        if (placementDirection == PlacementDirection.Right)
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingCol + i > 5) break;
                var tile = Board[startingRow][startingCol + i];
                BoardSelection[SelectedWord.Length]
                    // .Add((startingRow, startingCol + i));
                    .Add(new Coordinate
                    {
                        Row = startingRow,
                        Col = startingCol + i,
                    });
                tile.SetPlaced();
            }
        }
        else
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingRow + i > 5) break;
                var tile = Board[startingRow + i][startingCol];
                BoardSelection[SelectedWord.Length]
                    // .Add((startingRow + i, startingCol));
                    .Add(new Coordinate
                    {
                        Row = startingRow+i,
                        Col = startingCol
                    });
                tile.SetPlaced();
            }
        }

        if (SelectedWord.Length == 3)
        {
            IsWordPlaced[3] = true;
            // IsThreeLetterWordPlaced = true;
            SetupNode.WordButtons[3].SetDisabled(true);
        }
        else if (SelectedWord.Length == 4)
        {
            IsWordPlaced[4] = true;
            // IsFourLetterWordPlaced = true;
            // SetupNode.FourLetterWordButton.SetDisabled(true);
            SetupNode.WordButtons[4].SetDisabled(true);
        }
        else if (SelectedWord.Length == 5)
        {
            IsWordPlaced[5] = true;
            // IsFiveLetterWordPlaced = true;
            SetupNode.WordButtons[5].SetDisabled(true);
            // SetupNode.FiveLetterWordButton.SetDisabled(true);
        }
        else
            throw new Exception(
                $"There is no way the selected word's length has not been checked: {SelectedWord.Length}");
    }

    public void RetractPlaceableTiles(int startingCol, int startingRow, PlacementDirection placementDirection)
    {
        if (placementDirection == PlacementDirection.Right)
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingCol + i > 5) break;
                var tile = Board[startingRow][startingCol + i];
                tile.Retract();
            }
        }
        else
        {
            for (var i = 0; i < SelectedWord.Length; i++)
            {
                if (startingRow + i > 5) break;
                var tile = Board[startingRow + i][startingCol];
                tile.Retract();
            }
        }
    }

    public void OnNextWordsButtonPressed(int wordLength)
    {
        IsWordPlaced[wordLength] = false;
        // IsThreeLetterWordPlaced = false;
        // IsFourLetterWordPlaced = false;
        // IsFiveLetterWordPlaced = false;
        SelectedWord = "";
        SetupNode.PreviousWordButtons[wordLength].Disabled = false;
        // SetupNode.PreviousWordsButton.Disabled = false;

        _resetButtons(wordLength);
        _resetBoard(wordLength);
        TransitionTo(new InitialState(this));

        WordSelectionIndices[wordLength]++;
        // WordsSelectionIndex++;
        
        SetupNode.WordButtons[wordLength].Text = WordOptions[wordLength][WordSelectionIndices[wordLength]];
        // SetupNode.ThreeLetterWordButton.Text = WordOptions[0][WordsSelectionIndex];
        // SetupNode.FourLetterWordButton.Text = WordOptions[1][WordsSelectionIndex];
        // SetupNode.FiveLetterWordButton.Text = WordOptions[2][WordsSelectionIndex];
        SelectedWords[wordLength] = WordOptions[wordLength][WordSelectionIndices[wordLength]];
        // SelectedWords =
        // [
        //     WordOptions[0][WordsSelectionIndex], WordOptions[1][WordsSelectionIndex],
        //     WordOptions[2][WordsSelectionIndex]
        // ];

        if (WordSelectionIndices[wordLength] == 2)
            SetupNode.NextWordButtons[wordLength].Disabled = true;
    }

    public void OnWordSelectionButtonPressed(WordSelectionButtonId buttonId)
    {
        switch (buttonId)
        {
            case WordSelectionButtonId.Three:
                // SetupNode.ThreeLetterWordButton.SetDisabled(true);
                SetupNode.WordButtons[3].SetDisabled(true);

                if (!IsWordPlaced[4])
                    SetupNode.WordButtons[4].SetDisabled(false);
                    // SetupNode.FourLetterWordButton.SetDisabled(false);
                if (!IsWordPlaced[5])
                    // SetupNode.FiveLetterWordButton.SetDisabled(false);
                    SetupNode.WordButtons[5].SetDisabled(false);

                SelectedWord = SetupNode.WordButtons[3].Text;
                break;
            case WordSelectionButtonId.Four:
                // SetupNode.FourLetterWordButton.SetDisabled(true);
                SetupNode.WordButtons[4].SetDisabled(true);

                if (!IsWordPlaced[3])
                    // SetupNode.ThreeLetterWordButton.SetDisabled(false);
                    SetupNode.WordButtons[3].SetDisabled(false);
                if (!IsWordPlaced[5])
                    // SetupNode.FiveLetterWordButton.SetDisabled(false);
                    SetupNode.WordButtons[5].SetDisabled(false);

                // SelectedWord = SetupNode.FourLetterWordButton.Text;
                SelectedWord = SetupNode.WordButtons[4].Text;
                break;
            case WordSelectionButtonId.Five:
                // SetupNode.FiveLetterWordButton.SetDisabled(true);
                SetupNode.WordButtons[5].SetDisabled(true);

                if (!IsWordPlaced[3])
                    // SetupNode.ThreeLetterWordButton.SetDisabled(false);
                    SetupNode.WordButtons[3].SetDisabled(false);
                if (!IsWordPlaced[4])
                    // SetupNode.FourLetterWordButton.SetDisabled(false);
                    SetupNode.WordButtons[4].SetDisabled(false);

                SelectedWord = SetupNode.WordButtons[5].Text;
                break;
        }
    }

    public void OnPreviousWordsButtonPressed(int wordLength)
    {
        IsWordPlaced[wordLength] = false;
        // IsWordPlaced[4] = false;
        // IsWordPlaced[5] = false;
        // SelectedWord = "";
        SetupNode.NextWordButtons[wordLength].Disabled = false;

        _resetButtons(wordLength);
        _resetBoard(wordLength);
        TransitionTo(new InitialState(this));

        WordSelectionIndices[wordLength]--;

        SetupNode.WordButtons[wordLength].Text = WordOptions[wordLength][WordSelectionIndices[wordLength]];
        SelectedWords[wordLength] = WordOptions[wordLength][WordSelectionIndices[wordLength]];
        // SelectedWords =
        // [
        //     WordOptions[0][WordsSelectionIndex], WordOptions[1][WordsSelectionIndex],
        //     WordOptions[2][WordsSelectionIndex]
        // ];

        if (WordSelectionIndices[wordLength] == 0)
            SetupNode.PreviousWordButtons[wordLength].Disabled = true;
    }

    private void _resetBoard(int wordLength)
    {
        // for (var i = 0; i < 6; i++)
        // {
        //     for (var j = 0; j < 6; j++)
        //     {
        //         var tile = Board[i][j];
        //         tile.Reset();
        //     }
        // }
        
        foreach (var coord in BoardSelection[wordLength])
        {
            if (BoardSelection
                .Where(kv => kv.Key != wordLength)
                .Any(kv => kv.Value.Contains(new Coordinate
                {
                    Row = coord.Row,
                    Col =coord.Col 
                }))) continue;
            var tile = Board[coord.Row][coord.Col];
            tile.Reset();
        }

        // BoardSelection[0].Clear();
        // BoardSelection[1].Clear();
        // BoardSelection[2].Clear();
        BoardSelection[wordLength].Clear();
    }

    private void _resetButtons(int wordLength)
    {
        SetupNode.WordButtons[wordLength].SetDisabled(false);
        IsWordPlaced[wordLength] = false;
        // IsThreeLetterWordPlaced = false;
        // IsFourLetterWordPlaced = false;
        // IsFiveLetterWordPlaced = false;
        SetupNode.ConfirmButton.SetDisabled(true);
    }

    public bool AllWordsPlaced()
    {
        return IsWordPlaced.Values.All(placed => placed); 
        // return IsThreeLetterWordPlaced && IsFourLetterWordPlaced && IsFiveLetterWordPlaced;
    }

    public void AutoSetup()
    {
        SelectedWords = [];
        for (var i = 0; i < 3; i++)
        {
            var word = WordOptions[i+3][WordSelectionIndices[i+3]];
            for (var j = 0; j < word.Length; j++)
            {
                // BoardSelection[i+3].Add((i * 2, j));
                BoardSelection[i+3].Add(new Coordinate
                {
                    Row = i*2,
                    Col = j
                });
            }

            SelectedWords[i + 3] = word;
        }
        TransitionTo(new AllPlacedState(this));
        // GameManager.CompleteLocalSetup(SelectedWords, BoardSelection);
        // GameManager.LocalUpdateHandler(new UIEvent
        // {
        //     Type = EventType.SetupCompleted,
        //     Data = new SetupCompletedEventData(SelectedWords, BoardSelection) 
        // });
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
            var tile = Board[row][startingCol + i];
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
            IsWordPlaced[SelectedWord.Length] = true;
            // IsThreeLetterWordPlaced = true;
            // SetupNode.ThreeLetterWordButton.SetDisabled(true);
            SetupNode.WordButtons[3].SetDisabled(true);
        }
        else if (SelectedWord.Length == 4)
        {
            IsWordPlaced[SelectedWord.Length] = true;
            // IsFourLetterWordPlaced = true;
            // SetupNode.FourLetterWordButton.SetDisabled(true);
            SetupNode.WordButtons[4].SetDisabled(true);
        }
        else if (SelectedWord.Length == 5)
        {
            IsWordPlaced[SelectedWord.Length] = true;
            // IsFiveLetterWordPlaced = true;
            // SetupNode.FiveLetterWordButton.SetDisabled(true);
            SetupNode.WordButtons[5].SetDisabled(true);
        }
        else
            throw new Exception(
                $"There is no way the selected word's length has not been checked: {SelectedWord.Length}");
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
            var tile = Board[startingRow + i][col];
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
            var tile = Board[startingRow + i][col];
            // tile.SetPlaced();
            tile.Release();
        }

        if (SelectedWord.Length == 3)
        {
            IsWordPlaced[SelectedWord.Length] = true;
            // IsThreeLetterWordPlaced = true;
            SetupNode.WordButtons[3].SetDisabled(true);
        }
        else if (SelectedWord.Length == 4)
        {
            IsWordPlaced[SelectedWord.Length] = true;
            // IsFourLetterWordPlaced = true;
            // SetupNode.FourLetterWordButton.SetDisabled(true);
            SetupNode.WordButtons[4].SetDisabled(true);
        }
        else if (SelectedWord.Length == 5)
        {
            IsWordPlaced[SelectedWord.Length] = true;
            // IsFiveLetterWordPlaced = true;
            SetupNode.WordButtons[5].SetDisabled(true);
            // SetupNode.FiveLetterWordButton.SetDisabled(true);
        }
        else
            throw new Exception(
                $"There is no way the selected word's length has not been checked: {SelectedWord.Length}");
    }

    private void _restoreDown(int col, int startingRow)
    {
        for (var i = 0; i < SelectedWord.Length; i++)
        {
            if (startingRow + i > 5) break;
            var tile = Board[startingRow + i][col];
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
            var tile = Board[row][startingCol + i];

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
                var tileToLeft = Board[row][startingCol - 1];
                if (tileToLeft.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            //check if last letter has any conflicting tiles to the right
            if (i == SelectedWord.Length - 1 && startingCol + i < 5)
            {
                var tileToRight = Board[row][startingCol + i + 1];
                if (tileToRight.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile above current tile conflicts
            if (row != 0)
            {
                var tileAbove = Board[row - 1][startingCol + i];
                if (tileAbove.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile below current tile conflicts
            if (row != 5)
            {
                var tileBelow = Board[row + 1][startingCol + i];
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
                    var tile = Board[row][startingCol + i];
                    tile.Predict(false, true, SelectedWord[i].ToString());
                    // tile.PlacingLetter(SelectedWord[i].ToString(), true);
                }

                return;
            }
            default:
            {
                GD.Print(
                    $"SetupController: invalid placement of selected word {SelectedWord}\tinvalidOffsets={invalidOffsets}");
                for (var i = 0; i < SelectedWord.Length; i++)
                {
                    if (startingCol + i > 5) break;
                    GD.Print($"SetupController: Setting {SelectedWord[i]} at row={row}, col={startingCol + i}");
                    var tile = Board[row][startingCol + i];
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
            var tile = Board[startingRow + i][col];

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
                var tileOnRight = Board[startingRow][col + 1];
                if (tileOnRight.IsPlaced())
                {
                    invalidOffsets.Add(i);
                    continue;
                }
            }

            // check if tile left of current tile conflicts
            if (col != 0)
            {
                var tileOnLeft = Board[startingRow][col - 1];
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
                    var tile = Board[startingRow + i][col];
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
                    var tile = Board[startingRow + i][col];
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



    // public void CancelTileSelection(SetupTile selectedTile)
    // {
    //     selectedTile.SetStatus(SetupTileStatus.Idle);
    // }

    public bool IsValidSelection(int selectedTileCol, int selectedTileRow, string letter)
    {
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

public abstract class SetupState:IServerConnectionListener
{
    public abstract void Enter();
    public abstract void Exit();
    public virtual void HandleTileGuiEvent(InputEvent @event, SetupTile tile) {}
    public virtual void HandleConfirmButton() {}
    public virtual void HandleNextWordsButton(int wordLength) {}
    public virtual void HandlePreviousWordsButton(int wordLength) {}
    public virtual void HandleWordSelectionButton(WordSelectionButtonId buttonId) {}
    public virtual void HandleUndoSelectionButton() {}
    public abstract void Connecting();
    public abstract void Connected();
    public abstract void UnableToConnect();
    public abstract void Disconnected();
    public abstract void Reconnecting();
    public abstract void Reconnected();
    public abstract void Receive(IServerReceivable message);
    public abstract void Disconnecting();
    public abstract void HttpResponse(string response);
}

public enum WordSelectionButtonId
{
    Three,
    Four,
    Five
}
