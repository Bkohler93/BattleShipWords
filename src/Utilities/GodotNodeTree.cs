using Godot;

namespace BattleshipWithWords.Utilities;

public class GodotNodeTree
{
    public static T FindFirstNodeOfType<T>(Node root) where T : Node
    {
        foreach (var child in root.GetChildren())
        {
            switch (child)
            {
                case T typedChild:
                    return typedChild;
                case Node node:
                {
                    var result = FindFirstNodeOfType<T>(node);
                    if (result != null)
                        return result;
                    break;
                }
            }
        }
        return null;
    }   
}


