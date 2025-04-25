using Godot;
using static System.Int32;

namespace BattleshipWithWords;

public partial class Config: Node
{
   
   // Local ports used when testing on single local machine
   public int LocalSrcUdpPort;
   public int LocalDstUdpPort;
   public int LocalTcpPort;

   public void ParseConfig(string[] args)
   {
      foreach (var arg in args)
      {
         var vals = arg.Split("=");
         if (vals[0] == "--destUdp")
            LocalDstUdpPort= Parse(vals[1]);

         if (vals[0] == "--srcUdp")
            LocalSrcUdpPort = Parse(vals[1]);
         
         if (vals[0] == "--tcp")
            LocalTcpPort = Parse(vals[1]);
      }
   }

   public bool AreLocalUdpPortsSet()
   {
      return LocalSrcUdpPort != 0 && LocalDstUdpPort != 0;
   }
}