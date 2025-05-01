using Godot;
using System;

public partial class InternetMatchmaking : Control
{
    [Export]
    private Button _cancelButton;
    
    [Export]
    private Label _statusLabel;
    
    [Export]
    private Label _infoLabel;
    
    public Action OnCancelButtonPressed;

    public override void _Ready()
    {
        _cancelButton.Pressed += OnCancelButtonPressed;
    }
}
