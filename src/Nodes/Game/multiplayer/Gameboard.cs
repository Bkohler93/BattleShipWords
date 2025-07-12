using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Nodes.Game.multiplayer;
using BattleshipWithWords.Services.GameManager;
using BattleshipWithWords.Services.WordList;

public partial class Gameboard : Control
{
    [Export] private Button _switchBoardBtn;
    [Export] private Button _guessBtn;
    [Export] private Button _emoteBtn;
    [Export] private Label _gameBoardLabel;
    [Export] private Label _wordGuessLabel;
    [Export] private GridContainer _gridContainer;
    [Export] private HBoxContainer _foundIndicatorHBox;
    [Export] private HBoxContainer _topKeyboardRow;
    [Export] private HBoxContainer _middleKeyboardRow;
    [Export] private HBoxContainer _bottomKeyboardRow;
    
    public string GameboardTitle;
    public float InitialRotation;
    private float _separationGap = 40f / 6f;
    
    private List<List<Tile>> _gameBoard;
    private Dictionary<string, KeyboardKey> _keyboard;
    
    public Action OnRotate;
    public bool IsControlledLocally;

    private MultiplayerGameManager _gameManager;
    private FileWordChecker _fileWordChecker;
    private List<string> _guessedWords = [];
    private bool _shouldCheckStuff = true;
    public event Action<string> GuessMade; 
    public event Action<(int row, int col)> TileUncovered;
    public event Action BackspacePressed;
    // public event Action<KeyData> KeyPressed;
    // public event Action<UIUpdate> UIUpdated;
    public event Action<UIEvent> LocalUpdateMade;
    
    
    // _playerOneGameboard.GuessMade += _multiplayerGameManager.GuessMadeEventHandler;
    // _playerOneGameboard.TileUncovered += _multiplayerGameManager.TileUncoveredEventHandler;
    // _playerOneGameboard.BackspacePressed += _multiplayerGameManager.LocalBackspacePressedEventHandler;
    // _playerOneGameboard.KeyPressed += _multiplayerGameManager.LocalKeyPressedEventHandler;

    public void Init(MultiplayerGameManager gameManager)
    {
        _gameManager = gameManager;
        _fileWordChecker = new FileWordChecker("res://data/wordlists/bigwordlist.txt");
    }

    public override void _ExitTree()
    {
        BackspacePressed = null;
        TileUncovered = null;
        GuessMade = null;
    }

    public override void _Ready()
    {
        _gameBoardLabel.Text = GameboardTitle;
        _switchBoardBtn.Pressed += OnRotate;
        _guessBtn.Pressed += OnGuessBtnPressed;

        _setupTiles();
        _setupKeyboard();
        // _setupFoundIndicators();
        CallDeferred(nameof(ApplyInitialRotation));
        if (!IsControlledLocally)
            CallDeferred(nameof(_addInputBlockingPanel));
        
        // KeyPressed += KeyPressedHandler;
        // BackspacePressed += BackspacePressedEventHandler;
    }

    private void OnGuessBtnPressed()
    {
        if (_wordGuessLabel.Text.Length < 3) return;
        
        var word = _wordGuessLabel.Text.ToLower();
        if (!_fileWordChecker.IsValidWord(word))
        {
            //TODO update wordGuessLabel to tell user word is not real
            GD.Print($"Gameboard::OnGuessBtnPressed: word '{word}' not found");
            return;
        }

        if (_guessedWords.Contains(word))
        {
            //TODO update wordGuessLabel to tell user they already guessed it
            GD.Print($"Gameboard::OnGuessBtnPressed: word '{word}' is already guessed");
            return;
        }

        // _updateKeyboardAfterLocalGuess(word);
        // GuessMade?.Invoke(word);
        LocalUpdateMade?.Invoke(new UIEvent
        {
            Type = EventType.GuessedWord,
            Data = new  GuessedWordData
            {
                Word = word, 
            }
        });
        GD.Print($"Gameboard::OnGuessBtnPressed: {word} -- invoked GuessMade");
    }

    private void _updateKeyboardAfterLocalGuess(string word)
    {
        for (var i = 0; i < word.Length; i++)
        {
            string letter = word[i].ToString().ToUpper();
            var key = _keyboard[letter];
            key.SetInPlay();
        }
    }

    

    private void _addInputBlockingPanel()
    {
        var vpWidth = GetViewportRect().Size.X - (28+28);
        var vpHeight = GetViewportRect().Size.Y - (48+34);
        var guessBtnWidth = _guessBtn.Size.X;
        
        var topPanel = new Panel();
        AddChild(topPanel);
        topPanel.ZIndex = 5;
        topPanel.MouseFilter = MouseFilterEnum.Stop;
        topPanel.Size = new Vector2(vpWidth, .6f * vpHeight);
        topPanel.Modulate = new Color(1, 1, 1, 0f); // 20% opaque white
        
        var leftPanel = new Panel();
        AddChild(leftPanel);
        leftPanel.ZIndex = 5;
        leftPanel.MouseFilter = MouseFilterEnum.Stop;
        leftPanel.Size = new Vector2(vpWidth/2+guessBtnWidth/2, vpHeight);
        leftPanel.Modulate = new Color(1, 1, 1, 0f); // 20% opaque white
        
        var bottomPanel = new Panel();
        AddChild(bottomPanel);
        bottomPanel.ZIndex = 5;
        bottomPanel.MouseFilter = MouseFilterEnum.Stop;
        var bottomPanelHeight = 0.24f * vpHeight;
        bottomPanel.Size = new Vector2(vpWidth,bottomPanelHeight);
        bottomPanel.Position = new Vector2(0, vpHeight - bottomPanelHeight);
        bottomPanel.Modulate = new Color(1, 1, 1, 0f); // 20% opaque white
    }

    private void _setupFoundIndicators()
    {
        var threeLetterHBox = new HBoxContainer();
        threeLetterHBox.Alignment = BoxContainer.AlignmentMode.Center;
        for (var i = 0; i < 3; i++)
        {
            var letterIndicator = new LetterIndicator();
            threeLetterHBox.AddChild(letterIndicator);
        }
        _foundIndicatorHBox.AddChild(threeLetterHBox);
        
        var fourLetterHBox = new HBoxContainer();
        fourLetterHBox.Alignment = BoxContainer.AlignmentMode.Center;
        for (var i = 0; i < 4; i++)
        {
            var letterIndicator = new LetterIndicator();
            fourLetterHBox.AddChild(letterIndicator);
        }
        _foundIndicatorHBox.AddChild(fourLetterHBox);
        
        var fiveLetterHBox = new HBoxContainer();
        fiveLetterHBox.Alignment = BoxContainer.AlignmentMode.Center;
        for (var i = 0; i < 5; i++)
        {
            var letterIndicator = new LetterIndicator();
            fiveLetterHBox.AddChild(letterIndicator);
        }
        _foundIndicatorHBox.AddChild(fiveLetterHBox);
    }

    private void ApplyInitialRotation()
    {
        PivotOffset = new Vector2(Size.X / 2f, 0);
        Rotation = InitialRotation;
    }

    private void _setupKeyboard()
    {
        var keyboardKeyScene = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/keyboard_key.tscn");
       var topRowLetters =  new List<string>{"Q","W","E","R","T","Y","U","I","O","P"};
       var middleRowLetters = new List<string>{"A","S","D","F","G","H","J","K","L"};
       var bottomRowLetters = new List<string>{"Z","X","C","V","B","N","M"};

       _keyboard = new Dictionary<string, KeyboardKey>();
       foreach (var letter in topRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           key.ButtonPressed += (l) => LocalUpdateMade?.Invoke(new UIEvent
           {
               Type = EventType.KeyPressed,
               Data = new EventKeyPressedData
               {
                   Key = l
               }
           });
           _topKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }
        
       foreach (var letter in middleRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           key.ButtonPressed += (l) => LocalUpdateMade?.Invoke(new UIEvent
           {
               Type = EventType.KeyPressed,
               Data = new EventKeyPressedData
               {
                   Key = l
               }
           });
           _middleKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }

       foreach (var letter in bottomRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           key.ButtonPressed += (l) => LocalUpdateMade?.Invoke(new UIEvent
           {
               Type = EventType.KeyPressed,
               Data = new EventKeyPressedData
               {
                   Key = l
               }
           });
           _bottomKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }
       var backspaceBtn = new BackspaceButton();
       // backspaceBtn.ButtonPressed += () => KeyPressed?.Invoke(new KeyData("backspace"));
       backspaceBtn.ButtonPressed += () => LocalUpdateMade?.Invoke(new UIEvent
       {
           Type = EventType.KeyPressed,
           Data = new EventKeyPressedData
           {
               Key = "backspace" 
           }
       });
       _bottomKeyboardRow.AddChild(backspaceBtn);
    }

    private void _setupTiles()
    {
        var tilePackedScene = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/tile.tscn");
        var contentScreenHeight = GetViewportRect().Size.Y - (48 + 34);
        var boardSize = Mathf.Min(contentScreenHeight * 0.40f, 600); // board is reserved 40% of screen height 
        var tileSize = boardSize / 6f - _separationGap;

        _gameBoard = [];
        for (var i = 0; i < 6; i++)
        {
            _gameBoard.Add([]);
            for (var j = 0; j < 6; j++)
            {
                var tile = tilePackedScene.Instantiate() as Tile;
                tile.CustomMinimumSize = new Vector2(tileSize, tileSize);
                tile.Init(i,j, tileSize);
                // tile.ButtonPressed += (row, col) => TileUncovered?.Invoke((row, col));
                tile.ButtonPressed += (row, col) => LocalUpdateMade?.Invoke(new UIEvent
                {
                    Type = EventType.UncoveredTile,
                    Data = new UncoveredTileData
                    {
                        Row = row,
                        Col = col
                    }
                });
                _gridContainer.AddChild(tile);
                _gameBoard[i].Add(tile);
            }
        }
    }

    public void ProcessGuessResult(WordGuessedResponseData result)
    {
        _guessedWords.Add(_wordGuessLabel.Text);
        _wordGuessLabel.Text = "";
        foreach (var (letter, letterStatus) in result.ResponseLetters)
        {
            var key = _keyboard[letter.ToString().ToUpper()];
            switch (letterStatus.KeyboardStatus)
            {
                case KeyboardLetterStatus.AllFound:
                    key.SetAllFound();
                    break;
                case KeyboardLetterStatus.InPlay:
                    key.SetInPlay();
                    break;
                case KeyboardLetterStatus.OutOfPlay:
                    key.SetOutOfPlay();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            foreach (var (row, col, status) in letterStatus.FoundCoords)
            {
                var tile = _gameBoard[row][col];

                switch (status)
                {
                    case GameTileStatus.WordFound:
                        tile.SetOutOfPlay(letter.ToString());
                        break;
                    case GameTileStatus.Uncovered:
                        tile.SetInPlay(letter.ToString());
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }

    public void ProcessUncoverTile(UncoveredTileResponse result)
    {
        foreach (var (letter, keyboardStatus) in result.KeyboardStatus)
        {
            var key = _keyboard[letter.ToString().ToUpper()];
            switch (keyboardStatus)
            {
                case KeyboardLetterStatus.AllFound:
                    key.SetAllFound();
                    break;
                case KeyboardLetterStatus.InPlay:
                    key.SetInPlay();
                    break;
                case KeyboardLetterStatus.OutOfPlay:
                    key.SetOutOfPlay();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        foreach (var (row, col, letter, status) in result.GameboardStatus)
        {
            var tile = _gameBoard[row][col];
            switch (status)
            {
                case GameTileStatus.WordFound:
                    tile.SetOutOfPlay(letter.ToUpper());
                    break;
                case GameTileStatus.Uncovered:
                    tile.SetInPlay(letter.ToUpper());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }    
        }
    }

    public void ProcessKeyPress(string key)
    {
        if (key == "backspace")
        {
            if (_wordGuessLabel.Text.Length > 0)
                _wordGuessLabel.Text = _wordGuessLabel.Text[..^1];    
        } else if (key.Length > 1)
        {
            GD.PrintErr($"Attempting to process invalid key: {key}");
        }
        else //must be keyboard key
        {
            if (_wordGuessLabel.Text.Length == 5)
                return;
            _wordGuessLabel.Text += key;
        }
    }
    
    public void UIUpdatedHandler(UIUpdate @event)
    {
        switch (@event.Type)
        {
            case UIUpdateType.UncoveredTile:
            {
                if (@event.Data is UncoveredTileResponse result)
                    ProcessUncoverTile(result);
                else
                    throw new Exception("Invalid UncoverTile UIEvent Data attempted to be processed");
                break;
            }
            case UIUpdateType.GuessedWord:
            {
                if (@event.Data is WordGuessedResponseData result)
                    ProcessGuessResult(result); 
                else
                    throw new Exception("Invalid GuessedWord UIEvent Data attempted to be processed");
                break;
            }
            case UIUpdateType.KeyPressed:
            {
                if (@event.Data  is KeyData result)
                    ProcessKeyPress(result.Key);
                else
                    throw new Exception("Invalid KeyPressed UIEvent Data attempted to be processed");
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}

public class GuessedWordEventData : UIEventData
{
    public string Word;

    public GuessedWordEventData(string word)
    {
        Word = word;
    }
}


public abstract class UIEventData{}

public record class UIEvent
{
    public required EventType Type;
    public required IEventData Data;
}

public enum UIUpdateType
{
    UncoveredTile,
    GuessedWord,
    KeyPressed
}

public abstract class UIUpdateData{}

public record class UIUpdate
{
       public required UIUpdateType Type;
       public required UIUpdateData Data;
}

public class KeyData(string key) : UIUpdateData
{
    public string Key = key;
}

