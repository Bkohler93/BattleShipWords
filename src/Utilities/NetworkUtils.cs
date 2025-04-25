using Godot;

namespace BattleshipWithWords.Networkutils;

public class NetworkUtils
{
   // retrieves local IPv4 IP address or empty string if not found
   public static string GetLocalIp()
   {
      foreach (string ip in IP.GetLocalAddresses())
      {
         if (System.Net.IPAddress.TryParse(ip, out var parsedIp))
         {
            // Check it's IPv4 and not a loopback address
            if (parsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                !ip.StartsWith("127."))
            {
               return ip;
            }
         }
      }

      return "";
   }
}