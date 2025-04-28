using Godot;

namespace BattleshipWithWords.Networkutils;

public class PlatformUtils
{
    public static Platform GetPlatform()
    {
        if (OS.GetName() == "Android") 
            return Platform.Android;
        if (OS.GetName() == "iOS")
            return Platform.iOS;
        else
            return Platform.Other;
    }
}

public enum Platform
{
    Android,
    iOS,
    Other
}

public static class PlatformExtensions 
{
    public static bool IsAndroid(this Platform type)
    {
        return type == Platform.Android;
    }
    
    public static bool IsiOS(this Platform type)
    {
        return type == Platform.iOS;
    }
    
    public static bool IsOther(this Platform type)
    {
        return type == Platform.Other;
    }
}
