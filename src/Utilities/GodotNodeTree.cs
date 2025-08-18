using Godot;

namespace BattleshipWithWords.Utilities;

public class GodotNodeTree
{
    public static T FindFirstNodeOfType<T>(Node root) where T : Node
    {
        foreach (var child in root.GetChildren())
        {
            if (child is T typedChild)
            {
                GD.Print($"FindFirstNodeOfType<{typeof(T).Name}>()--- found node");
                return typedChild;
                
            }

            if (child is Node node)
            {
                var result = FindFirstNodeOfType<T>(node);
                if (result != null)
                    return result;
            }
        }
        return null;
    }   
}


