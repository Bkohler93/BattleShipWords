using System;
using System.Collections.Generic;
using Godot;

namespace BattleshipWithWords.Services.WordList;

public class FileWordChecker
{
    private List<string> _validWords = [];

    public FileWordChecker(string filepath)
    {
        var wordListFile = FileAccess.Open(filepath, FileAccess.ModeFlags.Read);
        if (FileAccess.GetOpenError() != Error.Ok)
            throw new Exception($"Error opening file: {FileAccess.GetOpenError()}");
            
        while (!wordListFile.EofReached())
        {
            var text = wordListFile.GetLine();
            _validWords.Add(text);
        }
        wordListFile.Close();    
    }

    public bool IsValidWord(string word)
    {
        return _validWords.Contains(word);
    }
}