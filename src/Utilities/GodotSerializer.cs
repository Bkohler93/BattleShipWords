using System;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Utilities;

public class GodotSerializer
{
    public static byte[] Serialize(IGodotSerializable serializable)
    {
        var dict = serializable.ToDictionary();
        return Json.Stringify(dict).ToUtf8Buffer();
    }

    public static T Deserialize<T>(byte[] bytes) where T : IGodotSerializable
    {
        var dict = (Dictionary)Json.ParseString(bytes.GetStringFromUtf8());

        var method = typeof(T).GetMethod("FromDictionary",
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (method == null)
            throw new InvalidOperationException($"{typeof(T).Name} must implement public static T FromDictionary(Dictionary)");

        return (T)method.Invoke(null, [dict]);
    } 
}

public interface IGodotSerializable
{
    Dictionary ToDictionary();
}