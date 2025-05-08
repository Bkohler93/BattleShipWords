using System;
using System.Runtime.InteropServices;
using BattleshipWithWords.Networkutils;
using BattleshipWithWords.Nodes.Menus;
using BattleshipWithWords.Services;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer;

public class LocalMatchmakingController
{
    private LocalMatchmaking _matchmakingNode;
    private ILocalMatchmakingState _currentState;
    public long MultiplayerPeerId;
    public long MultiplayerId;

    public LocalMatchmakingController(LocalMatchmaking matchmakingNode)
    {
        _matchmakingNode = matchmakingNode;
    }

    public void OnBackPressed()
    {
        _currentState.OnBackPressed(); 
    }

    private void OnServerDisconnected()
    {
        GD.Print("LocalMatchmakingController: Server disconnected");
        DisplayUpdate("Server disconnected... Go back to try again");
        TransitionTo(new DiscoveringState(this));
    }

    private void OnConnectedToServer()
    {
        GD.Print("LocalMatchmakingController: Connected to server");
    }

    private void OnPeerConnected(long id)
    {
        GD.Print($"Peer connected with id {id}");
    }

    private void OnPeerDisconnected(long id)
    {
        GD.Print($"LocalMatchmakingController::OnPeerDisconnected({id})");
        DisplayUpdate($"Peer disconnected... ");
        AssignMultiplayerPeer(null);
        TransitionTo(new DiscoveringState(this));
    }

    public void TransitionTo(ILocalMatchmakingState newState)
    {
        if (_currentState == newState) return;
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void DisplayStatus(string status)
    {
        _matchmakingNode.StatusLabel.Text = status;
    }

    public void DisplayUpdate(string update)
    {
        _matchmakingNode.UpdateLabel.Text = update;
    }

    public void BackToMenu()
    {
        _currentState?.Exit();
        DetachMultiplayerSignals();
        _matchmakingNode.BackToMainMenu.Invoke();
    }

    public void AssignMultiplayerPeer(ENetMultiplayerPeer peer)
    {
        if (peer == null)
            _matchmakingNode.Multiplayer.MultiplayerPeer.Close();
        _matchmakingNode.Multiplayer.MultiplayerPeer = peer;
    }

    public void DisplayPlayStatus()
    {
        _matchmakingNode.PlayButton.Show();
        _matchmakingNode.StatusLabel.Text = "Press Play to start!";
        _matchmakingNode.UpdateLabel.Text = "=)";
    }

    public void DisconnectMultiplayerPeer()
    {
        _matchmakingNode.Multiplayer.MultiplayerPeer.Close();
        _matchmakingNode.Multiplayer.MultiplayerPeer = null;
    }

    public void ExitMatchmaking()
    {
        DetachMultiplayerSignals();
        _matchmakingNode.StartGame.Invoke();
    }

    public void DetachMultiplayerSignals()
    {
        _matchmakingNode.Multiplayer.PeerDisconnected -= OnPeerDisconnected;
        _matchmakingNode.Multiplayer.PeerConnected -= OnPeerConnected;
        _matchmakingNode.Multiplayer.ConnectedToServer -= OnConnectedToServer;
        _matchmakingNode.Multiplayer.ServerDisconnected -= OnServerDisconnected;
    }

    public void AttachMultiplayerSignals()
    {
        _matchmakingNode.Multiplayer.PeerDisconnected += OnPeerDisconnected;
        _matchmakingNode.Multiplayer.PeerConnected += OnPeerConnected;
        _matchmakingNode.Multiplayer.ConnectedToServer += OnConnectedToServer;
        _matchmakingNode.Multiplayer.ServerDisconnected += OnServerDisconnected;
    }
    

    public void OnPlayPressed()
    {
        _currentState.OnPlayPressed();
    }

    public void OnPeerPressedPlay()
    {
        _currentState.OnPeerPlayPressed();
    }

    public void Init()
    {
        AttachMultiplayerSignals(); 
        TransitionTo(new DiscoveringState(this));
    }

    public bool TestConnection()
    {
        var isConnected =_matchmakingNode.Multiplayer.MultiplayerPeer.GetConnectionStatus() == MultiplayerPeer.ConnectionStatus.Connected;
        if (isConnected)
        {
            MultiplayerId = _matchmakingNode.Multiplayer.GetUniqueId();
            MultiplayerPeerId = _matchmakingNode.Multiplayer.GetPeers()[0];
        }
        return isConnected;
    }

    public void AttachOnPeerConnected(MultiplayerApi.PeerConnectedEventHandler onPeerConnected)
    {
        _matchmakingNode.Multiplayer.PeerConnected += onPeerConnected;
    }

    public void DetachOnPeerConnected(MultiplayerApi.PeerConnectedEventHandler onPeerConnected)
    {
        _matchmakingNode.Multiplayer.PeerConnected -= onPeerConnected;
    }

    public void HidePlayButton()
    {
        _matchmakingNode.PlayButton.Hide();
    }

    public void SetPeerReady()
    {
        PeerReady = true;
    }

    public bool PeerReady { get; set; }
}

public class DiscoveringState : ILocalMatchmakingState,ILocalPeerFinderConnector 
{
    private LocalMatchmakingController _controller;
    private ILocalPeerFinder _peerFinder;
    private ILocalMatchmakingState _nextState;
    private ENetMultiplayerPeer _hostPeer;
    private int _hostPort;
    private ENetMultiplayerPeer _clientPeer;
    
    public DiscoveringState(LocalMatchmakingController localMatchmakingController)
    {
        _controller = localMatchmakingController;
        var factory = new LocalPeerFinderFactory();
        _peerFinder = factory.Create(PlatformUtils.GetPlatform());
        _peerFinder.ConnectSignals(this);
    }

    public override void Enter()
    {
        _hostPort =_peerFinder.StartService();
        _hostPeer = new ENetMultiplayerPeer();
        var err = _hostPeer.CreateServer(_hostPort);
        if (err != Error.Ok)
        {
            GD.Print("LocalMatchmakingController::DiscoveringState::Enter(): Failed to create server");     
        }

        _controller.AttachOnPeerConnected(_onPeerConnected);
        _controller.AssignMultiplayerPeer(_hostPeer);
        _peerFinder.StartListening();
    }

    private void _onPeerConnected(long id)
    {
        GD.Print($"connected to peer {id}"); 
        _controller.DetachOnPeerConnected(_onPeerConnected);
        _nextState = new ConfirmingState(_controller);
        _peerFinder.StopService();
    }

    public override void Exit()
    {
        _peerFinder.Cleanup();
    }

    public override void OnBackPressed()
    {
        _controller.DisplayStatus("Disconnecting from search");
        _hostPeer.Close();
        _peerFinder.StopService();
        _controller.DetachMultiplayerSignals();
    }

    public void OnRegisteredService()
    {
        _controller.DisplayStatus("You are discoverable on your network.");
    }

    public void OnRegisterServiceFailed()
    {
        _controller.DisplayStatus("Unable to broadcast your present on your network.");
        _controller.DisplayUpdate("Go back to try again or wait for you to connect to a peer.");
    }

    public void OnUnregisteredService()
    {
        if (_nextState == null)
            _controller.BackToMenu();
        else
            _controller.TransitionTo(_nextState);
    }

    public override void OnPeerPlayPressed()
    {
        _controller.SetPeerReady();
    }

    public void OnServiceFound(string ip, int port)
    {
        _clientPeer = new ENetMultiplayerPeer();
        var err = _clientPeer.CreateClient(ip, port);
        if (err != Error.Ok)
        {
            GD.Print($"LocalMatchmakingController::DiscoveringState::OnServiceFound() -- error creating _clientPeer: {
                err
            }");
        }
        _controller.AssignMultiplayerPeer(_clientPeer);
        _nextState = new ConfirmingState(_controller);
        _peerFinder.StopService();
        _hostPeer.Close();
    }
}

internal class ConfirmingState : ILocalMatchmakingState
{
    private LocalMatchmakingController _controller;
    private bool _ready;
    private bool _peerReady;
    public ConfirmingState(LocalMatchmakingController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.DisplayStatus("Testing the connection");
        var isConnected = _controller.TestConnection();
        if (!isConnected)
        {
            GD.Print("LocalMatchmakingController::ConfirmingState::Enter: Testing connection failed");
            _controller.TransitionTo(new DiscoveringState(_controller));
        }
        else
        {
            _controller.DisplayPlayStatus();
            if (_controller.PeerReady)
            {
                _controller.DisplayStatus("Peer hit Play! Press Play to continue!");
            }
        }
    }

    public override void Exit()
    {
        _controller.HidePlayButton();
    }

    public override void OnBackPressed()
    {
        _controller.DisconnectMultiplayerPeer();
        _controller.BackToMenu(); 
    }

    public override void OnPlayPressed()
    {
        if (_controller.PeerReady)
        {
            _controller.ExitMatchmaking();
            return;
        }
        _controller.DisplayStatus("Waiting for peer to hit Play!");
        _ready = true;
    }

    public override void OnPeerPlayPressed()
    {
        // if already press play go to next screen
        if (_ready)
        {
            _controller.ExitMatchmaking();
            return;
        }
        
        _controller.DisplayStatus("Your peer is ready! Hit Play!");

        _controller.PeerReady = true;
    }
}

public abstract class ILocalMatchmakingState
{
    public abstract void Enter();
    public abstract void Exit();
    public abstract void OnBackPressed();

    public virtual void OnPlayPressed()
    {
        throw new NotImplementedException($"{GetType().Name} OnPlayPressed() is not implemented.");
    }

    public virtual void OnPeerPlayPressed()
    {
        throw new NotImplementedException($"{GetType().Name} OnPeerPlayPressed() is not implemented.");
    }
}