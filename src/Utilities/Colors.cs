using System;
using Godot;

namespace BattleshipWithWords.Networkutils;

public enum ColorName
{
    Primary,
    Secondary,
    White,
    Background,
    BackgroundDark,
    PrimaryDark,
    OffWhite,
    Pink,
}

public static class Colors
{
    public static Color GetColor(ColorName name)
    {
        return name switch
        {
            ColorName.Primary => new Color(20/255f, 204/255f, 94/255f),
            ColorName.Secondary => new Color(248/255f, 51/255f, 60/255f),
            ColorName.White => new Color(1, 1, 1),
            ColorName.Background => new Color(42/255f, 69/255f, 80/255f),
            ColorName.BackgroundDark => new Color(20/255f, 36/255f, 43/255f), 
            ColorName.PrimaryDark => new Color(9/255f, 109/255f, 47/255f),
            ColorName.OffWhite => new Color(147/255f, 168/255f, 172/255f),
            ColorName.Pink => new Color(226/255f, 180/255f, 189/255f),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, null)
        };
    }
}