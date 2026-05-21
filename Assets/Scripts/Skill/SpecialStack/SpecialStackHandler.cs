using System;
using System.Collections.Generic;
using UnityEngine;

public class SpecialStackHandler : MonoBehaviour
{
    private Dictionary<SpecialStackData, int> stacks = new Dictionary<SpecialStackData, int>();

    private Character character;

    public event Action<SpecialStackData, int> onStackChanged;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Request(SpecialStackData data, int amount)
    {
        if (data == null) return;

        if (!stacks.ContainsKey(data))
        {
            if (amount <= 0) return;
            stacks[data] = 0;
        }

        int prev = stacks[data];
        stacks[data] = Mathf.Clamp(stacks[data] + amount, 0, data.maxStack);

        if (stacks[data] == 0)
            stacks.Remove(data);

        if (stacks.TryGetValue(data, out int current) && current != prev)
            onStackChanged?.Invoke(data, current);
        else if (prev != 0 && !stacks.ContainsKey(data))
            onStackChanged?.Invoke(data, 0);
    }

    public int GetCount(SpecialStackData data)
    {
        return stacks.TryGetValue(data, out int count) ? count : 0;
    }

    public bool Has(SpecialStackData data)
    {
        return stacks.ContainsKey(data);
    }

    private void Update()
    {
        foreach (var pair in stacks)
            pair.Key.Apply(character, pair.Value);
    }
}
