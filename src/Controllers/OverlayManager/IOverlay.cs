using Godot;

namespace BattleshipWithWords.Controllers;

public interface IOverlay
{
    CanvasLayer GetNode();
}