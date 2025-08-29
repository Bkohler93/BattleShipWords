using Godot;

namespace BattleshipWithWords.Utilities;

public static class Logger
{
    public static void Print(string msg)
    {
        GD.Print($"[GodotGameLog]:{msg}");
    }
}