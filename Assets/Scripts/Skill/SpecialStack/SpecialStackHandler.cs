using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpecialStackHandler : MonoBehaviour
{
    private Dictionary<SpecialStackData, StackHandler> stacks = new Dictionary<SpecialStackData, StackHandler>();
    private List<SpecialStackData> ToRemove = new List<SpecialStackData>();

    private Character character;

    public event Action<SpecialStackData, int> onStackChanged;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    public void Request(SpecialStackData data, int amount, float life)
    {
        if (data == null) return;

        Debug.Log($"{data.name}, {amount}, {life}");

        if (!stacks.ContainsKey(data))
        {
            if (amount <= 0) return;
            stacks[data] = new StackHandler();
            stacks[data].amount = 0;
        }

        if (stacks[data].life < life) stacks[data].life = life;

        var prev = stacks[data].amount;
        stacks[data].amount = Mathf.Clamp(stacks[data].amount + amount, 0, data.maxStack);

        if (stacks[data].amount == 0)
        {
            stacks.Remove(data);
            data.OnRemoved(character);
        }
        else if (prev == 0)
        {
            data.Apply(character, stacks[data].amount);
        }

        if (stacks.TryGetValue(data, out StackHandler current) && current.amount != prev)
            onStackChanged?.Invoke(data, current.amount);
        else if (prev != 0 && !stacks.ContainsKey(data))
            onStackChanged?.Invoke(data, 0);
    }

    public StackHandler GetCount(SpecialStackData data)
    {
        return stacks.TryGetValue(data, out StackHandler count) ? count : null;
    }

    public bool Has(SpecialStackData data)
    {
        return stacks.ContainsKey(data);
    }

    private void Update()
    {
        foreach (var pair in stacks)
        {
            pair.Key.Apply(character, pair.Value.amount);
            pair.Value.life -= Time.deltaTime;
            if (pair.Value.life < 0)
            {
                ToRemove.Add(pair.Key);
            }
        }

        for (int i = ToRemove.Count - 1; i >= 0; i--)
        {
            ToRemove[i].OnRemoved(character);
            stacks.Remove(ToRemove[i]);
            ToRemove.RemoveAt(i);
        }
    }
}

public class StackHandler
{
    public int amount;
    public float life;
}