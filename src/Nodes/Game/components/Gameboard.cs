using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Nodes.Game;
using BattleshipWithWords.Nodes.Game.multiplayer;
using BattleshipWithWords.Services.GameManager;
using BattleshipWithWords.Services.WordList;
using BattleshipWithWords.Utilities;

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

    // private MultiplayerGameManager _gameManager;
    private FileWordChecker _fileWordChecker;
    private List<string> _guessedWords = [];
    private bool _shouldCheckStuff = true;
    public event Action<string> GuessMade; 
    public event Action<(int row, int col)> TileUncovered;
    public event Action BackspacePressed;
    // public event Action<KeyData> KeyPressed;
    // public event Action<UIUpdate> UIUpdated;
    public Func<IUIEvent, bool> LocalUpdateMade;
    
    // _playerOneGameboard.GuessMade += _multiplayerGameManager.GuessMadeEventHandler;
    // _playerOneGameboard.TileUncovered += _multiplayerGameManager.TileUncoveredEventHandler;
    // _playerOneGameboard.BackspacePressed += _multiplayerGameManager.LocalBackspacePressedEventHandler;
    // _playerOneGameboard.KeyPressed += _multiplayerGameManager.LocalKeyPressedEventHandler;

    public void Init(MultiplayerGameManager gameManager)
    {
        // _gameManager = gameManager;
        _fileWordChecker = new FileWordChecker("res://data/wordlists/bigwordlist.txt");
    }

    public override void _ExitTree()
    {
        BackspacePressed = null;
        TileUncovered = null;
        GuessMade = null;
    }

    public void SetGameBoardTurn(bool isPlayerTurn)
    {
        if (isPlayerTurn)
        {
            Logger.Print("setting player's turn");
            _gameBoardLabel.Text += "(YOUR TURN)";
        }
        else
        {
            Logger.Print("not this player's turn");
            _gameBoardLabel.Text = _gameBoardLabel.Text.Replace("(YOUR TURN)", "");
        }
    }

    private class GuessAnimatorController
    {
        private bool _isAnimating;
        private Gameboard _gameboard;

        public GuessAnimatorController(Gameboard gameboard)
        {
            _gameboard = gameboard;
            _isAnimating = false;
        }

        private Tween _flashAndShake(Color color)
        {
            var label = _gameboard._wordGuessLabel;
            var shakeTween = _gameboard.CreateTween();
            var colorTween = _gameboard.CreateTween();
               
            var originalColor = label.Modulate;
            var originalPos = label.Position;

            colorTween.TweenProperty(label, "modulate", color, 0.2f)
                .SetTrans(Tween.TransitionType.Sine);
            colorTween.TweenProperty(label, "modulate", originalColor, 0.2f).SetTrans(Tween.TransitionType.Sine);

            shakeTween.TweenProperty(label, "position", originalPos + new Vector2(-10,0), 0.1f)
                .SetTrans(Tween.TransitionType.Sine);
            shakeTween.TweenProperty(label, "position", originalPos + new Vector2(10,0), 0.1f)
                .SetTrans(Tween.TransitionType.Sine);
            shakeTween.TweenProperty(label, "position", originalPos + new Vector2(-10,0), 0.1f)
                .SetTrans(Tween.TransitionType.Sine);
            shakeTween.TweenProperty(label, "position", originalPos, 0.1f)
                .SetTrans(Tween.TransitionType.Sine);
            return shakeTween;
        }

        public void OnGuessBtnPressed()
        {
            if (_isAnimating) return;
            if (_gameboard._wordGuessLabel.Text.Length < 3) return;
        
            var word = _gameboard._wordGuessLabel.Text.ToLower();
            if (!_gameboard._fileWordChecker.IsValidWord(word))
            {
                _isAnimating = true;
                var tween = _flashAndShake(new Color(1,0,0));
                tween.Finished += () => { _isAnimating = false; };
                GD.Print($"Gameboard::OnGuessBtnPressed: word '{word}' not found");
                return;
            }

            if (_gameboard._guessedWords.Contains(word))
            {
                _isAnimating = true;
                var tween = _flashAndShake(new Color(1,1,0));
                tween.Finished += () => { _isAnimating = false; };
                GD.Print($"Gameboard::OnGuessBtnPressed: word '{word}' is already guessed");
                return;
            }

            // _updateKeyboardAfterLocalGuess(word);
            // GuessMade?.Invoke(word);
            // LocalUpdateMade?.Invoke(new UIEvent
            // {
            //     Type = EventType.GuessedWord,
            //     Data = new  GuessedWordData
            //     {
            //         Word = word, 
            //     }
            // });
            var isSuccessful = _gameboard.LocalUpdateMade(new GuessedWordEvent
            {
                Word = word 
            });
            if (isSuccessful)
            {
                _gameboard._guessedWords.Add(word);
                _gameboard._wordGuessLabel.Text = "";
            }
            GD.Print($"Gameboard::OnGuessBtnPressed: {word} -- invoked GuessMade");
        }
    }

    public override void _Ready()
    {
        _gameBoardLabel.Text = GameboardTitle;
        _switchBoardBtn.Pressed += OnRotate;
        var guessAnimatorController = new GuessAnimatorController(this);
        _guessBtn.Pressed += guessAnimatorController.OnGuessBtnPressed;

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
        Logger.Print("Gameboard::_addInputBlockingPanel");
        // var vpWidth = GetViewportRect().Size.X - (28+28);
        var vpWidth = GetViewportRect().Size.X;
        // var vpHeight = GetViewportRect().Size.Y - (48+34);
        var vpHeight = GetViewportRect().Size.Y;
        var guessBtnWidth = _guessBtn.Size.X;
        
        var topPanel = new Panel();
        AddChild(topPanel);
        topPanel.ZIndex = 5;
        topPanel.MouseFilter = MouseFilterEnum.Stop;
        topPanel.Size = new Vector2(vpWidth, .6f * vpHeight);
        topPanel.Modulate = new Color(1, 1, 1, .2f); // 20% opaque white
        
        var leftPanel = new Panel();
        AddChild(leftPanel);
        leftPanel.ZIndex = 5;
        leftPanel.MouseFilter = MouseFilterEnum.Stop;
        leftPanel.Size = new Vector2(vpWidth/2+guessBtnWidth/2, vpHeight);
        leftPanel.Modulate = new Color(1, 1, 1, .2f); // 20% opaque white
        
        var bottomPanel = new Panel();
        AddChild(bottomPanel);
        bottomPanel.ZIndex = 5;
        bottomPanel.MouseFilter = MouseFilterEnum.Stop;
        var bottomPanelHeight = 0.24f * vpHeight;
        bottomPanel.Size = new Vector2(vpWidth,bottomPanelHeight);
        bottomPanel.Position = new Vector2(0, vpHeight - bottomPanelHeight);
        bottomPanel.Modulate = new Color(1, 1, 1, .2f); // 20% opaque white
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
        var keyboardKeyScene = ResourceLoader.Load<PackedScene>(ResourcePaths.KeyboardKeyNodePath);
       var topRowLetters =  new List<string>{"Q","W","E","R","T","Y","U","I","O","P"};
       var middleRowLetters = new List<string>{"A","S","D","F","G","H","J","K","L"};
       var bottomRowLetters = new List<string>{"Z","X","C","V","B","N","M"};

       _keyboard = new Dictionary<string, KeyboardKey>();
       foreach (var letter in topRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           // key.ButtonPressed += (l) => LocalUpdateMade?.Invoke(new UIEvent
           // {
           //     Type = EventType.KeyPressed,
           //     Data = new EventKeyPressedData
           //     {
           //         Key = l
           //     }
           // });
           key.ButtonPressed += (l) => LocalUpdateMade(new KeyPressedEvent
           {
               Key = l
           });
           _topKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }
        
       foreach (var letter in middleRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           // key.ButtonPressed += (l) => LocalUpdateMade?.Invoke(new UIEvent
           // {
           //     Type = EventType.KeyPressed,
           //     Data = new EventKeyPressedData
           //     {
           //         Key = l
           //     }
           // });
           key.ButtonPressed += (l) => LocalUpdateMade?.Invoke(new KeyPressedEvent
           {
               Key = l  
           });
           _middleKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }

       foreach (var letter in bottomRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           // key.ButtonPressed += (l) => LocalUpdateMade?.Invoke(new UIEvent
           // {
           //     Type = EventType.KeyPressed,
           //     Data = new EventKeyPressedData
           //     {
           //         Key = l
           //     }
           // });
           key.ButtonPressed += l => LocalUpdateMade?.Invoke(new KeyPressedEvent
           {
               Key = l
           });
           _bottomKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }
       var backspaceBtn = new BackspaceButton();
       // backspaceBtn.ButtonPressed += () => KeyPressed?.Invoke(new KeyData("backspace"));
       // backspaceBtn.ButtonPressed += () => LocalUpdateMade?.Invoke(new UIEvent
       // {
       //     Type = EventType.KeyPressed,
       //     Data = new EventKeyPressedData
       //     {
       //         Key = "backspace" 
       //     }
       // });
       backspaceBtn.ButtonPressed += () => LocalUpdateMade?.Invoke(new KeyPressedEvent
       {
           Key = "backspace" 
       });
       _bottomKeyboardRow.AddChild(backspaceBtn);
    }

    private void _setupTiles()
    {
        var tilePackedScene = ResourceLoader.Load<PackedScene>(ResourcePaths.TileNodePath);
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
                // tile.ButtonPressed += (row, col) => LocalUpdateMade?.Invoke(new UIEvent
                // {
                //     Type = EventType.UncoveredTile,
                //     Data = new UncoveredTileData
                //     {
                //         Row = row,
                //         Col = col
                //     }
                // });
                tile.ButtonPressed += (row, col) => LocalUpdateMade?.Invoke(new UncoveredTileEvent
                {
                    Row = row,
                    Col = col
                });
                _gridContainer.AddChild(tile);
                _gameBoard[i].Add(tile);
            }
        }
    }

    public void ProcessTurnResponse(TurnTakenUpdate result)
    {
        foreach (var (letter, letterStatus) in result.LetterGameStatuses)
        {
            if (letter == " ")
                continue;
                
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
            
            foreach (var letterFoundCoordinate in letterStatus.UncoveredGameTiles)
            {
                var tile = _gameBoard[letterFoundCoordinate.Coordinate.Row][letterFoundCoordinate.Coordinate.Col];

                switch (letterFoundCoordinate.GameTileStatus)
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

        _wordGuessLabel.Text = "";
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
        switch (@event)
        {
            case  KeyPressedUpdate keyPressed:
                ProcessKeyPress(keyPressed.Key);
                break;
            case TurnTakenUpdate turnTaken:
                ProcessTurnResponse(turnTaken);
                break;
            // case UncoveredTileResponse uncoveredTile:
            //     ProcessUncoverTile(uncoveredTile);
            //     break;
            // case WordGuessedResponse wordGuessed:
            //     ProcessGuessResult(wordGuessed);
            //     break;
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

[JsonDerivedType(typeof(KeyPressedEvent), typeDiscriminator: "KeyPressed"),
 JsonDerivedType(typeof(GuessedWordEvent), typeDiscriminator: "GuessedWord"),
 JsonDerivedType(typeof(UncoveredTileEvent), typeDiscriminator: "UncoveredTile")]
public interface IUIEvent
{
    public String ToString();
    // public required EventType Type;
    // public required IEventData Data;
}

public enum UIUpdateType
{
    TurnTaken,
    KeyPressed
}

[JsonDerivedType(typeof(KeyPressedUpdate), typeDiscriminator: "KeyPressedUpdate"),
 JsonDerivedType(typeof(TurnTakenUpdate), typeDiscriminator: "TurnTakenUpdate"),
JsonDerivedType(typeof(TimeUpdate), typeDiscriminator: "TimeUpdate")]
public abstract class UIUpdate
{
       // public required UIUpdateType Type;
       // public required UIUpdateData Data;
}

public class KeyPressedUpdate() : UIUpdate
{
    [JsonPropertyName("key")]
    public string Key { get; set; }
}

