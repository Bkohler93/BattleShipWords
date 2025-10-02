using System.Collections.Generic;
using BattleshipWithWords.Controllers.Multiplayer.Game;
using Godot;
using Godot.Collections;

namespace BattleshipWithWords.Controllers.Multiplayer.Setup;

public class AllPlacedState: SetupState
{
    private SetupController _controller;

    public AllPlacedState(SetupController controller)
    {
        _controller = controller;
    }

    public override void Enter()
    {
        _controller.SetupNode.ConfirmButton.SetDisabled(false);
    }

    public override void Exit()
    {
    }

    public override void HandleConfirmButton()
    {
        for (var i = 0; i < 3; i++)
        {
            var s =$"'{_controller.SelectedWords[i]}' placed at: ";
            foreach (var coord in _controller.BoardSelection[i])
            {
                s += $"({coord.row},{coord.col}),";
            } 
        }
        // _controller.GameManager.LocalUpdateHandler(new UIEvent
        // {
        //     Type = EventType.SetupCompleted,
        //     Data = new SetupCompletedEventData(_controller.SelectedWords, _controller.BoardSelection) 
        // });
    }

    public override void HandleNextWordsButton()
    {
        _controller.OnNextWordsButtonPressed();
    }
    
    public override void HandlePreviousWordsButton()
    {
        _controller.OnPreviousWordsButtonPressed();
    }
}

public class SetupCompletedEventData : UIEventData, IEventData
{
    public List<string> SelectedWords;
    public List<List<(int row, int col)>> SelectedGridCoordinates;
    public SetupCompletedEventData(List<string> selectedWords, List<List<(int row, int col)>> selectedGridCoordinates)
    {
        SelectedWords = selectedWords;
        SelectedGridCoordinates = selectedGridCoordinates;
    }

    public Dictionary ToDictionary()
    {
        throw new System.NotImplementedException();
    }
}