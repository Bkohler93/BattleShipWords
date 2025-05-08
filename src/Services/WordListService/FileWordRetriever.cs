using System;
using System.Collections.Generic;
using Godot;

namespace BattleshipWithWords.Services.WordList;

public class FileWordRetriever
{
    private string _basePath;
    private Random _random; 
    private Dictionary<int, List<string>> _words = new();
    
    public FileWordRetriever(string basePath, List<string> wordListFileNames)
    {
        _basePath = basePath; 
        _random = new Random();

        foreach (var filename in wordListFileNames)
        {
            var wordListFile = FileAccess.Open($"{_basePath}/{filename}", FileAccess.ModeFlags.Read);
            if (FileAccess.GetOpenError() != Error.Ok)
                throw new Exception($"Error opening file: {FileAccess.GetOpenError()}");
            
            while (!wordListFile.EofReached())
            {
                var text = wordListFile.GetLine();
                if (!_words.ContainsKey(text.Length))
                    _words[text.Length] = [];
                _words[text.Length].Add(text);
            }
            wordListFile.Close();    
        }
    }

    public List<string> GetWords(int wordLength, int numWords)
    {
        if (!_words.ContainsKey(wordLength)) 
            throw new Exception($"FileWordRetriever: No words of length '{wordLength}' stored");
        
        var words = new List<string>();

        for (var i = 0; i < numWords; i++)
        {
            string word;
            do
            {
                word = _words[wordLength][_random.Next(_words[wordLength].Count)];
            } while (words.Contains(word));
                
            words.Add(word);
        }
            
        return words;
    }
}