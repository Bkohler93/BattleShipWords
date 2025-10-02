namespace BattleshipWithWords.Utilities;

public static class ResourcePaths
{
   public const string AppRoot = "res://scenes/appRoot.tscn";
   public const string LoadingNodePath = "res://scenes/loading.tscn";
   
   private const string GameNodesBase = "res://scenes/game";
   public const string InternetGameNodePath = $"{GameNodesBase}/internet_game.tscn"; 
   public const string InternetSetupNodePath = $"{GameNodesBase}/internet_setup.tscn"; 
   
   private const string ComponentsNodesBase = "res://scenes/components"; 
   public const string PlayerAvatarNodePath = $"{ComponentsNodesBase}/avatar.tscn";
   public const string GameboardNodePath = $"{ComponentsNodesBase}/gameboard.tscn";
   public const string KeyboardKeyNodePath = $"{ComponentsNodesBase}/keyboard_key.tscn";
   public const string SetupTileNodePath = $"{ComponentsNodesBase}/setup_tile.tscn";
   public const string TileNodePath = $"{ComponentsNodesBase}/tile.tscn";
   
   private const string PopupsNodesBase = "res://scenes/popups";
   public const string LoseOverlayNodePath = $"{PopupsNodesBase}/lose_overlay.tscn";
   public const string PauseOverlayNodePath = $"{PopupsNodesBase}/pause_overlay.tscn";
   public const string PeerDisconnectedOverlayNodePath = $"{PopupsNodesBase}/peer_disconnected_overlay.tscn";
   public const string WaitingOverlayNodePath = $"{PopupsNodesBase}/waiting_overlay.tscn";
   public const string WinOverlayNodePath = $"{PopupsNodesBase}/win_overlay.tscn";
   
   private const string UINodesBase = "res://scenes/UI";
   public const string GameOverlayNodePath = $"{UINodesBase}/game_overlay.tscn";
   public const string InternetMatchmakingMenuNodePath = $"{UINodesBase}/internet_matchmaking.tscn";
   public const string LocalMatchmakingMenuNodePath = $"{UINodesBase}/local_matchmaking.tscn";
   public const string MainMenuNodePath = $"{UINodesBase}/main_menu.tscn";
   public const string MultiplayerMenuNodePath = $"{UINodesBase}/multiplayer.tscn";
   public const string MultiplayerGameNodePath = $"{UINodesBase}/multiplayer_game.tscn";
   public const string MultiplayerSetupNodePath = $"{UINodesBase}/multiplayer_setup.tscn";
   public const string MultiplayerTurnDeciderNodePath = $"{UINodesBase}/multiplayer_turn_decider.tscn";
   public const string SettingsMenuNodePath = $"{UINodesBase}/settings.tscn";
   public const string SinglePlayerGameNodePath = $"{UINodesBase}/single_player_game.tscn";
   public const string TutorialNodePath = $"{UINodesBase}/tutorial.tscn";

}