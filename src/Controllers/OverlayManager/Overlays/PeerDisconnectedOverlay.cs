using System;
using Godot;

namespace BattleshipWithWords.Controllers;

public class PeerDisconnectedOverlay:IOverlay
{
    private PeerDisconnected _overlay;
    public Action ExitedOverlay;
    public PeerDisconnectedOverlay()
    {
        _overlay =
            ResourceLoader.Load<PackedScene>("res://scenes/overlays/peer_disconnected_overlay.tscn").Instantiate() as PeerDisconnected;
        _overlay.ReturnToMenuButtonPressed += () =>
        {
            ExitedOverlay?.Invoke();
        };
    }

    public CanvasLayer GetNode()
    {
        return _overlay;
    }
}