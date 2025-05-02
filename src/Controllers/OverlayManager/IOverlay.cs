using Godot;

namespace BattleshipWithWords.Services;

public interface IOverlay
{
    CanvasLayer GetNode();
}