// SafeAreaManager.cs

using BattleshipWithWords.Utilities;
using Godot;

public partial class SafeAreaManager : Control
{
    private Rect2I _lastSafeArea;

    public override void _Ready()
    {
        // Connect to the window's size_changed signal
        GetTree().Root.SizeChanged += OnViewportSizeChanged;
        
        // Initial setup
        UpdateSafeArea();
    }

    private void OnViewportSizeChanged()
    {
        UpdateSafeArea();
    }

    private void UpdateSafeArea()
    {
        Rect2I safeArea;

        // Get the appropriate safe area based on the platform
        if (OS.HasFeature("ios") || OS.HasFeature("android"))
        {
            safeArea = DisplayServer.GetDisplaySafeArea();
        }
        else // Desktop platform
        {
            var windowSize = DisplayServer.WindowGetSize();
            safeArea = new Rect2I((int)Vector2.Zero.X,(int)Vector2.Zero.Y, windowSize.X, windowSize.Y);
        }
        
        var size = DisplayServer.WindowGetSize();
        Logger.Print($"screen width={size.X}, height={size.Y}");
        
        // Only update if the safe area has changed
        if (safeArea.Equals(_lastSafeArea))
        {
            return;
        }
        _lastSafeArea = safeArea;
        
        // Calculate the offsets based on the safe area.
        // The offsets are relative to the parent's rect.
        // We need to set the anchors to full rect (0,0,1,1) first
        // to make the offsets work correctly as margins.
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);

        // Apply the safe area offsets directly to the Control node.
        OffsetLeft = safeArea.Position.X;
        OffsetTop = safeArea.Position.Y;
        OffsetRight = - (DisplayServer.WindowGetSize().X - (safeArea.Position.X + safeArea.Size.X));
        OffsetBottom = - (DisplayServer.WindowGetSize().Y - (safeArea.Position.Y + safeArea.Size.Y));
    }
}