namespace BattleshipWithWords.Nodes.Globals;

using Godot;

public partial class Auth : Node
{
    public string UserId { get; private set; }
    public string JwtToken { get; private set; }
    public string Username { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(UserId);

    public void SetUser(string userId, string token, string username = "")
    {
        UserId = userId;
        JwtToken = token;
        Username = username;
        GD.Print("User logged in:", userId);
    }

    public void Logout()
    {
        UserId = null;
        JwtToken = null;
        Username = null;
        GD.Print("User logged out");
    }
}
