using System.Collections.Generic;
using System.Linq;
using BattleshipWithWords.Controllers.Multiplayer.Internet.Matchmaking;
using BattleshipWithWords.Services.ConnectionManager.Server;
using BattleshipWithWords.Utilities;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.Internet;

public class AllPlacedState: SetupState
{
    private SetupController _controller;

    public AllPlacedState(SetupController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        Logger.Print("entered AllPlacedState");
        _controller.SetupNode.ConfirmButton.SetDisabled(false);
    }

    public override void Exit()
    {
    }

    public override void HandleConfirmButton()
    {
        _controller.ServerConnectionManager.Send(new SetupFinalize
        {
            SelectedWords = _controller.SelectedWords,
            WordPlacements =_controller.BoardSelection,
            UserId = _controller.SetupNode.Auth.UserId
        });
        Logger.Print("sent SetupFinalize message");
        _controller.SetupNode.OnLocalSetupComplete?.Invoke();
        // _controller.OverlayManager.Add("waiting", new WaitingOverlay(), 2);
        // _controller.GameManager.LocalUpdateHandler(new UIEvent
        // {
        //     Type = EventType.SetupCompleted,
        //     Data = new SetupCompletedEventData(_controller.SelectedWords, _controller.BoardSelection) 
        // });
    }

    public override void HandleNextWordsButton(int wordLength)
    {
        _controller.OnNextWordsButtonPressed(wordLength);
    }
    
    public override void HandlePreviousWordsButton(int wordLength)
    {
        _controller.OnPreviousWordsButtonPressed(wordLength);
    }

    public override void Connecting()
    {
        throw new System.NotImplementedException();
    }

    public override void Connected()
    {
        throw new System.NotImplementedException();
    }

    public override void UnableToConnect()
    {
        throw new System.NotImplementedException();
    }

    public override void Disconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Reconnecting()
    {
        throw new System.NotImplementedException();
    }

    public override void Reconnected()
    {
        throw new System.NotImplementedException();
    }

    public override void Receive(IServerReceivable message)
    {
        switch (message)
        {
            case StartOrderDecider msg:
                // _controller.OverlayManager.Remove("waiting");
                _controller.SetupNode.OnSetupComplete?.Invoke();
                break;
            default:
                Logger.Print($"Unknown message type during AllPlacedState - {message.GetType().Name}");
                break;
        }
    }

    public override void Disconnecting()
    {
        throw new System.NotImplementedException();
    }

    public override void HttpResponse(string response)
    {
        throw new System.NotImplementedException();
    }
}

// public class SetupCompletedEventData : UIEventData, IEventData
// {
//     public List<string> SelectedWords;
//     public List<List<(int row, int col)>> SelectedGridCoordinates;
//     public SetupCompletedEventData(List<string> selectedWords, List<List<(int row, int col)>> selectedGridCoordinates)
//     {
//         SelectedWords = selectedWords;
//         SelectedGridCoordinates = selectedGridCoordinates;
//     }
//
//     public Dictionary ToDictionary()
//     {
//         throw new System.NotImplementedException();
//     }
// }