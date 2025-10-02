using Godot;
using System;

public partial class PauseMenu :Control 
{
    [Export] private Button _continueButton;
    [Export] private Button _quitButton;
    
    [Export] private PanelContainer _pausePanel;
    
    public Action OnQuitButtonPressedEventHandler;
    public Action OnContinueButtonPressedEventHandler;

    public override void _Ready()
    {
        _continueButton.Pressed += _onContinueButtonPressed;
        _quitButton.Pressed += OnQuitButtonPressedEventHandler;
        // _pausePanel.Hide();
        // _blockInputs(this);
    }

    private void _allowInputs(Node node)
    {
        foreach (var child in node.GetChildren(includeInternal:true))
        {
            if (child is Control control && control.Name != "PauseButton")
            {
                control.SetMouseFilter(Control.MouseFilterEnum.Ignore);
            }
            _allowInputs(child);
        }
    }

    private void _onContinueButtonPressed()
    {
        _pausePanel.Hide();
        _allowInputs(this);
        OnContinueButtonPressedEventHandler?.Invoke();
    }

    // private void _onPauseButtonPressed()
    // {
    //     _pausePanel.Show();
    //     _blockInputs(this);
    // }

    private void _blockInputs(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Control control && control.Name != "PauseButton")
            {
                control.SetMouseFilter(Control.MouseFilterEnum.Pass);
            }
            _blockInputs(child);
        }
    }
}
