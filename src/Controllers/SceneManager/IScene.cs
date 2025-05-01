using BattleshipWithWords.Networkutils;
using Godot;

namespace BattleshipWithWords.Services;

public interface IScene
{
    public void Exit(Tween tween, TransitionDirection direction);
    public void Enter(Tween tween, TransitionDirection direction);
    public Node Create();
}