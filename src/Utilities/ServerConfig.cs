using System.Collections.Generic;

namespace BattleshipWithWords.Utilities;

public static class ServerConfig
{
    // Change this to "Local", "LocalPublic", or "Production"
    public static string Environment = "LocalPublic";

    private static readonly Dictionary<string, Dictionary<string, string>> servers = new()
    {
        { "Local", new Dictionary<string, string>
            {
                { "WebsocketServer", "ws://192.168.0.13:8089" },
                { "AuthServer", "http://192.168.0.13:8080/auth" }
            }
        },
        {
            "LocalPublic", new Dictionary<string, string>
            {
                { "WebsocketServer", "ws://localhost:8089" },
                { "AuthServer", "http://localhost:8080/auth" }
            }   
        },
        { "Production", new Dictionary<string, string>
            {
                { "WebsocketServer", "wss://deecegames.com" },
                { "AuthServer", "https://deecegames.com/auth" }
            }
        }
    };

    public static string GetServer(string name)
    {
        return servers[Environment][name];
    }
}
