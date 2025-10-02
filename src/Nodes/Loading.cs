using Godot;

public partial class Loading : Control
{
    [Export] private Label _progressLabel;

    public void UpdateProgress(string update)
    {
        _progressLabel.Text = update;
    }
}
