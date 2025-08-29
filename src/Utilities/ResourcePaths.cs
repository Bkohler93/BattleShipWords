namespace BattleshipWithWords.Utilities;

public static class ResourcePaths
{
   private const string GameNodesBase = "res://scenes/games";
   public const string SinglePlayerGameNodePath = $"{GameNodesBase}/single_player_game.tscn";
   public const string TutorialNodePath = $"{GameNodesBase}/tutorial.tscn";

   private const string GameComponentNodes = $"{GameNodesBase}/components";
   public const string GameboardNodePath = $"{GameComponentNodes}/gameboard.tscn";
   public const string KeyboardKeyNodePath = $"{GameComponentNodes}/keyboard_key.tscn";
   public const string SetupTileNodePath = $"{GameComponentNodes}/setup_tile.tscn";
   public const string TileNodePath = $"{GameComponentNodes}/tile.tscn";
   
   private const string MultiplayerGameNodes = $"{GameNodesBase}/multiplayer";
   public const string MultiplayerGameNodePath = $"{MultiplayerGameNodes}/multiplayer_game.tscn";
   public const string MultiplayerSetupNodePath = $"{MultiplayerGameNodes}/multiplayer_setup.tscn";
   public const string MultiplayerTurnDeciderNodePath = $"{MultiplayerGameNodes}/multiplayer_turn_decoder.tscn";
  
   private const string InternetGameNodes = $"{GameNodesBase}/multiplayer/internet";
   public const string InternetSetupNodePath = $"{InternetGameNodes}/internet_setup.tscn";
   public const string InternetTurnDeciderNodePath = $"{InternetGameNodes}/internet_turn_decider.tscn";
   
   private const string MenuNodesBase = "res://scenes/menus";
   public const string InternetMatchmakingMenuNodePath = $"{MenuNodesBase}/internet_matchmaking.tscn";
   public const string LocalMatchmakingMenuNodePath = $"{MenuNodesBase}/local_matchmaking.tscn";
   public const string MainMenuNodePath = $"{MenuNodesBase}/main_menu.tscn";
   public const string MultiplayerMenuNodePath = $"{MenuNodesBase}/multiplayer.tscn";
   public const string SettingsMenuNodePath = $"{MenuNodesBase}/settings.tscn";
   
   private const string OverlayNodesBase = "res://scenes/overlays";
   public const string WinOverlayNodePath = $"{OverlayNodesBase}/win_overlay.tscn";
   public const string LoseOverlayNodePath = $"{OverlayNodesBase}/lose_overlay.tscn";
   public const string PauseOverlayNodePath = $"{OverlayNodesBase}/pause_overlay.tscn";
   public const string PeerDisconnectedOverlayNodePath = $"{OverlayNodesBase}/peer_disconnected_overlay.tscn";
   public const string WaitingOverlayNodePath = $"{OverlayNodesBase}/waiting_overlay.tscn";
    

   public const string AppRoot = "res://scenes/appRoot.tscn";
}