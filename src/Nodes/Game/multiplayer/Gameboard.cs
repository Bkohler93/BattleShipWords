using Godot;
using System;
using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
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
    public event Action<string> KeyPressed;

    public void Init(MultiplayerGameManager gameManager)
    {
        _gameManager = gameManager;
        _fileWordChecker = new FileWordChecker("res://data/wordlists/bigwordlist.txt");
    }

    public override void _ExitTree()
    {
        BackspacePressed = null;
        KeyPressed = null;
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

        KeyPressed += KeyPressedEventHandler;
        BackspacePressed += BackspacePressedEventHandler;
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
        GuessMade?.Invoke(word);
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

    public void BackspacePressedEventHandler()
    {
        if (_wordGuessLabel.Text.Length > 0)
            _wordGuessLabel.Text = _wordGuessLabel.Text[..^1];
    }

    public void KeyPressedEventHandler(string letter)
    {
        if (_wordGuessLabel.Text.Length == 5)
            return;
        _wordGuessLabel.Text += letter;
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
           key.ButtonPressed += (l) => KeyPressed?.Invoke(l);
           _topKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }
        
       foreach (var letter in middleRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           key.ButtonPressed += (l) => KeyPressed?.Invoke(l);
           _middleKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }

       foreach (var letter in bottomRowLetters)
       {
           var key = keyboardKeyScene.Instantiate() as KeyboardKey;
           key.Initialize(letter);
           key.ButtonPressed += (l) => KeyPressed?.Invoke(l);
           _bottomKeyboardRow.AddChild(key);
           _keyboard[letter] = key;
       }
       var backspaceBtn = new BackspaceButton();
       backspaceBtn.ButtonPressed += () => BackspacePressed?.Invoke();
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
                tile.ButtonPressed += (row, col) => TileUncovered?.Invoke((row, col));
                _gridContainer.AddChild(tile);
                _gameBoard[i].Add(tile);
            }
        }
    }

    public void GuessResultReceivedEventHandler(WordGuessedResponseData result)
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

    public void TileUncoverResultReceivedEventHandler(UncoveredTileResponse result)
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
}
