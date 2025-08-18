using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Nodes.Game.multiplayer;

public partial class LetterIndicator : Control
{
    public bool Found = false;
    public Color FoundColor = Utilities.Colors.GetColor(ColorName.Primary);
    public Color NotFoundColor = Utilities.Colors.GetColor(ColorName.OffWhite);
    public float Radius = 10f;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(Radius * 2 + 4, Radius * 2 + 4);
    }

    public override void _Draw()
    {
        var center = Size / 2;
        if (Found)
        {
            DrawCircle(center, Radius, FoundColor);
        }
        else
        {
            DrawArc(center, Radius, 0, Mathf.Tau, 32, NotFoundColor, 2f);
        }
    }

    public void SetFound(bool isFound)
    {
        Found = isFound;
        QueueRedraw();
    }
}
