using System.Collections.Generic;
using UnityEngine;

public abstract class DataTable<T>
{
    protected Dictionary<string, T> table = new(); 

    public abstract void Load(string csv);

    public T Get(string key)
    {
        if (table.TryGetValue(key, out T value)) return value;
        return default;
    }

    public bool TryGet(string key, out T value)
    {
        return table.TryGetValue(key, out value);
    }
}