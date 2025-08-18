using System;
using System.Collections.Generic;
using androidplugintest.ConnectionManager;
using androidplugintest.PeerFinderPlugin;
using BattleshipWithWords.Controllers.Multiplayer.Setup;
using BattleshipWithWords.Nodes.Menus;
using BattleshipWithWords.Utilities;
using Godot;

namespace BattleshipWithWords.Controllers.Multiplayer.LocalMatchmakingController;

public class LocalMatchmakingController 
{
    private LocalMatchmaking _matchmakingNode;
    public P2PConnectionManager ConnectionManager;
    public PeerFinder PeerFinder;
    private LocalMatchmakingState _currentState;

    private readonly List<ServiceInfo> _peerList = [];
    private int _selectedPeerIndex;
    private bool _matchmakingCompleted;

    public LocalMatchmakingController(LocalMatchmaking matchmakingNode)
    {
        _matchmakingNode = matchmakingNode;
    }

    public void OnBackPressed()
    {
        _currentState.OnBackPressed(); 
    }

    public void TransitionTo(LocalMatchmakingState newState)
    {
        if (_currentState == newState) return;
        _currentState?.Exit();
        _currentState = newState;
        ConnectionManager.SetListener(_currentState);
        _currentState.Enter();
    }

    public void DisplayStatus(string status)
    {
        _matchmakingNode.StatusLabel.Text = status;
    }

    public void BackToMenu()
    {
        _currentState?.Exit();
        // GD.Print("LocalMatchmakingController: Back to Menu -- about to detach multiplayer signals");
        // DetachMultiplayerSignals();
        _matchmakingNode.BackToMainMenu.Invoke();
    }

    public void CompleteMatchmaking()
    {
        _matchmakingCompleted = true;
        GD.Print("completing matchmaking");
        _matchmakingNode.StartGame.Invoke();
    }

    public void Init(IP2PPeerService peerService)
    {
        ConnectionManager = new P2PConnectionManager(peerService);
        PeerFinder = new PeerFinder();
        TransitionTo(new InitialState(this));
        // var result = ConnectionManager.CreateServer();
        // if (!result.Success)
        // {
        //     TransitionTo(new ErrorState(this));
        // }
        // TransitionTo(new DiscoveringState(this));
    }
    
    public void HideActionButton()
    {
        _matchmakingNode.ActionButton.Hide();
    }

    public void AddDiscoveredPeer(ServiceInfo serviceInfo)
    {
        if (!_peerList.Exists((info => info.PeerName == serviceInfo.PeerName)))
        {
            _peerList.Add(serviceInfo);
            _matchmakingNode.DiscoveredPeers.AddItem(serviceInfo.PeerName);
        } else if (serviceInfo.IpVersion == IpVersion.IPv4)
        {
            var removeIdx = _peerList.FindIndex(info => info.PeerName == serviceInfo.PeerName);
            _peerList.RemoveAt(removeIdx);
            _matchmakingNode.DiscoveredPeers.RemoveItem(removeIdx);
            _peerList.Add(serviceInfo);
            _matchmakingNode.DiscoveredPeers.AddItem(serviceInfo.PeerName);
        }
    }

    public void SelectItem(long index)
    {
        _selectedPeerIndex = (int)index;
        TransitionTo(new SelectingState(this));
    }

    public void ShowActionButton()
    {
        _matchmakingNode.ActionButton.Show();
    }

    public void OnActionPressed()
    {
        _currentState.OnActionPressed();
    }

    public ServiceInfo GetSelectedPeer()
    {
        return _peerList[_selectedPeerIndex];
    }

    public void ShowAndUpdatePeerLabel(string peerName)
    {
        _matchmakingNode.DiscoveredPeers.Hide();
        _matchmakingNode.PeerLabel.Show();
        _matchmakingNode.PeerLabel.Text = $"{peerName} would like to play";
    }

    public void HidePeerLabel()
    {
        _matchmakingNode.PeerLabel.Hide();
    }

    public void ShowAction(string text)
    {
        _matchmakingNode.ActionButton.Text = text;
        _matchmakingNode.ActionButton.Show();
    }

    public void Shutdown()
    {
        PeerFinder.Shutdown();
        if (!_matchmakingCompleted)
            ConnectionManager.Shutdown();
        ConnectionManager.DisconnectSignals();
    }
}
