using System;
using BattleshipWithWords.Controllers.ScreenManager;
using BattleshipWithWords.Services;
using Godot;

namespace BattleshipWithWords.Networkutils;

public enum SlideTransitionDirection
{
    Forward,
    Backward,
}

public static class SceneTransitions
{
    public static void SlideEnter(Control node, Tween tween, SlideTransitionDirection direction)
    {
        // node.Position = new Vector2(720, 0);
        node.Position = direction switch
        {
            SlideTransitionDirection.Forward => new Vector2(720,0),
            SlideTransitionDirection.Backward => new Vector2(-720,0),
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        tween.TweenProperty(node, "position", new Vector2(0, 0), 0.3)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.Out);
    }

    public static void SlideExit(Control node, Tween tween, SlideTransitionDirection direction)
    {
        float finalX = direction switch
        {
            SlideTransitionDirection.Forward => -720,
            SlideTransitionDirection.Backward => 720,
        };
        tween.TweenProperty(node, "position", new Vector2(finalX, 0), 0.3)
            .SetTrans(Tween.TransitionType.Linear)
            .SetEase(Tween.EaseType.In);
    }

    public static void GameExit(Control node, Tween tween, SlideTransitionDirection direction)
    {
        node.Hide();
    }

    public static void FadeIn(LayerNode node, Tween tween)
    {
        node.Node.Modulate = new Color(node.Node.Modulate.R, node.Node.Modulate.G, node.Node.Modulate.B, 0);

        tween.TweenProperty(node.Node, "modulate:a", 1.0, 0.5)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);
    }
}