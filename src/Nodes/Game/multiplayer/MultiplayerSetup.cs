using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Controllers.Multiplayer.Setup.States;
using Godot;

public partial class MultiplayerSetup : MarginContainer
{
    [Export]
    private GridContainer _gridContainer;

    private SetupController _controller;
    
    [Export] public Button ConfirmButton;
    [Export] public Button NextWordsButton;
    [Export] public Button PreviousWordsButton;
    [Export] public Button ThreeLetterWordButton;
    [Export] public Button FourLetterWordButton;
    [Export] public Button FiveLetterWordButton;
    
    public override void _Ready()
    {
        _controller = new SetupController(this);
        _initializeBoard();
        _initializeUI();

        ConfirmButton.Pressed += _controller.HandleConfirmButton;
        NextWordsButton.Pressed += _controller.HandleNextWordsButton;
        PreviousWordsButton.Pressed += _controller.HandlePreviousWordsButton;
        ThreeLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Three);
        FourLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Four);
        FiveLetterWordButton.Pressed += () => _controller.HandleWordSelectionButton(WordSelectionButtonId.Five);
        _controller.TransitionTo(new InitialState(_controller));
    }

    private void _initializeUI()
    {
        ConfirmButton.Text = "\uf058";
        NextWordsButton.Text = "\uf105";
        PreviousWordsButton.Text = "\uf104";
        PreviousWordsButton.Disabled = true;
        ConfirmButton.SetDisabled(true);
        GD.Print("made it here"); 
        ThreeLetterWordButton.Text = _controller.Words[0][_controller.WordSelectionIndex];
        FourLetterWordButton.Text = _controller.Words[1][_controller.WordSelectionIndex];
        FiveLetterWordButton.Text = _controller.Words[2][_controller.WordSelectionIndex];
    }

    private void _onConfirmButtonPressed()
    {
        
    }

    private void _initializeBoard()
    {
        foreach (var name in Theme.GetStyleboxList(GetClass())) {
            GD.Print(name);
        }
    
        // get out of here with the compiler warnings these will be initialized during the first iteration of creating
        // the board below.
        StyleBox idleStyleBox = null!;
        StyleBox selectedStyleBox = null!;
        StyleBox placingStyleBox = null!;
        StyleBox placedStyleBox = null!;
        StyleBox placingFailedStyleBox = null!;
        
        var tilePackedScene = ResourceLoader.Load<PackedScene>("res://scenes/games/multiplayer/setup_tile.tscn");
        var height = GetViewportRect().Size.Y;
        var boardSize = Mathf.Min(height * 0.40f, 600); // board is reserved 40% of screen height 
        var tileSize = boardSize / 7f; // 6 tiles but spaces between
        
        for (var i = 0; i < 6; i++)
        {
            _controller.Board.Add([]);
            for (var j = 0; j < 6; j++)
            {
                var tile = tilePackedScene.Instantiate() as SetupTile;
                _gridContainer.AddChild(tile);
                tile.CustomMinimumSize = new Vector2(tileSize, tileSize);
                if (i == 0 && j == 0)
                {
                    idleStyleBox = tile.GetThemeStylebox("panel_idle");
                    selectedStyleBox = tile.GetThemeStylebox("panel_selected");
                    placingStyleBox = tile.GetThemeStylebox("panel_placing");
                    placedStyleBox = tile.GetThemeStylebox("panel_placed");
                    placingFailedStyleBox = tile.GetThemeStylebox("panel_placing_fail");
                }
                tile.Init(_controller, i,j,tileSize, idleStyleBox,  selectedStyleBox, placingStyleBox, placingFailedStyleBox,placedStyleBox);
                _controller.Board[i].Add(tile);
            }
        }
    }
}
