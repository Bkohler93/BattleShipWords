using System;
using System.Collections.Generic;
using Godot;

namespace BattleshipWithWords.Services.WordList;

public class FileWordRetrieverService
{
    private string _basePath;
    private Random _random; 
    private Dictionary<int, List<string>> _words = new();
    
    public FileWordRetrieverService(string basePath)
    {
        _basePath = basePath; 
        _random = new Random();
    }

    public List<string> GetWords(string filename, int numWords)
    {
            var allWords = new List<string>();
            var wordListFile = FileAccess.Open($"{_basePath}/{filename}", FileAccess.ModeFlags.Read);
            if (FileAccess.GetOpenError() != Error.Ok)
                throw new Exception($"Error opening file: {FileAccess.GetOpenError()}");
            
            while (!wordListFile.EofReached())
            {
                var text = wordListFile.GetLine();
                allWords.Add(text); 
            }
            wordListFile.Close();

            var words = new List<string>();

            for (var i = 0; i < numWords; i++)
            {
                string word;
                do
                {
                    word = allWords[_random.Next(allWords.Count)];
                } while (words.Contains(word));
                
                words.Add(word);
            }
            
            return words;
    }
    
}