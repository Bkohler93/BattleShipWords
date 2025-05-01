using System;

namespace BattleshipWithWords.Games;

public interface IGame
{
    public Action OnPause { get; set; }
    public Action OnFinish { get; set; }
    public Action OnQuit { get; set; }
}