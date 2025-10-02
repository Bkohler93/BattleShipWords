using System;
using System.Collections.Generic;

namespace BattleshipWithWords.Services.SharedData;

public class SharedData
{
    private Dictionary<Type, object> _data;

    public SharedData()
    {
        _data = new Dictionary<Type, object>();
    }
    
    public T Get<T>()
    {
        return (T)_data[typeof(T)];
    }

    public T Consume<T>()
    {
        var data = (T)_data[typeof(T)];
        _data.Remove(typeof(T));
        return data;
    }

    public void Set<T>(T value)
    {
        _data[typeof(T)] = value;
    }
}