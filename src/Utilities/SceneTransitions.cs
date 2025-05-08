using System;
using BattleshipWithWords.Services;
using Godot;

namespace BattleshipWithWords.Networkutils;

public enum TransitionDirection
{
    Forward,
    Backward,
}

public static class SceneTransitions
{
    public static void MenuEnter(Control node, Tween tween, TransitionDirection direction)
    {
        // node.Position = new Vector2(720, 0);
        node.Position = direction switch
        {
            TransitionDirection.Forward => new Vector2(720,0),
            TransitionDirection.Backward => new Vector2(-720,0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        tween.TweenProperty(node, "position", new Vector2(0, 0), 0.3)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.Out);
    }

    public static void MenuExit(Control node, Tween tween, TransitionDirection direction)
    {
        float finalX = direction switch
        {
            TransitionDirection.Forward => -720,
            TransitionDirection.Backward => 720,
        };
        tween.TweenProperty(node, "position", new Vector2(finalX, 0), 0.3)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.In);
    }

    public static void GameExit(Control node, Tween tween, TransitionDirection direction)
    {
        node.Hide();
    }
}