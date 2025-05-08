using System.Reflection.PortableExecutable;
using Godot;

namespace BattleshipWithWords.Controllers;

public class WaitingOverlay: IOverlay
{
    private Waiting _overlay;
    public WaitingOverlay()
    {
        _overlay =
            ResourceLoader.Load<PackedScene>("res://scenes/overlays/waiting_overlay.tscn").Instantiate() as Waiting;
    } 
    public CanvasLayer GetNode()
    {
        return _overlay;
    }
}