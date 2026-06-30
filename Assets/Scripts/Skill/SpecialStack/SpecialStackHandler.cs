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

    public void Request(SpecialStackData data, int amount, float life, Character grantor = null)
    {
        if (data == null) return;

        if (!stacks.ContainsKey(data))
        {
            if (amount <= 0) return;
            stacks[data] = new StackHandler();
            stacks[data].amount = 0;
        }

        if (stacks[data].life < life) stacks[data].life = life;
        if (grantor != null) stacks[data].grantor = grantor;

        var prev = stacks[data].amount;
        stacks[data].amount = Mathf.Clamp(stacks[data].amount + amount, 0, data.maxStack);

        if (stacks[data].amount == 0)
        {
            var g = stacks[data].grantor;
            stacks.Remove(data);
            data.OnRemoved(character, prev, g);
        }

        var gained = stacks.TryGetValue(data, out var cur) ? cur.amount - prev : 0;
        if (gained > 0) data.OnGained(character, gained, cur.grantor);

        if (stacks.TryGetValue(data, out StackHandler current) && current.amount != prev)
            onStackChanged?.Invoke(data, current.amount);
        else if (prev != 0 && !stacks.ContainsKey(data))
            onStackChanged?.Invoke(data, 0);
    }

    public StackHandler GetCount(SpecialStackData data)
    {
        return stacks.TryGetValue(data, out StackHandler count) ? count : null;
    }

    // 풀 재사용/부활 시 초기화. 그냥 비우면 적용된 효과가 안 풀리므로
    // 각 스택의 OnRemoved를 호출해 효과를 되돌린 뒤 비운다.
    public void ResetState()
    {
        foreach (var kv in new List<KeyValuePair<SpecialStackData, StackHandler>>(stacks))
        {
            kv.Key.OnRemoved(character, kv.Value.amount, kv.Value.grantor);
            onStackChanged?.Invoke(kv.Key, 0);
        }
        stacks.Clear();
        ToRemove.Clear();
    }

    public bool Has(SpecialStackData data)
    {
        return stacks.ContainsKey(data);
    }

    private void Update()
    {
        foreach (var pair in stacks)
        {
            pair.Key.Apply(character, pair.Value.amount, pair.Value.grantor);
            if (!pair.Key.useFreeze || !character.State.IsFrozen)
            {
                pair.Value.life -= Time.deltaTime;
            }

            if (pair.Value.life < 0)
            {
                ToRemove.Add(pair.Key);
            }
        }

        for (int i = ToRemove.Count - 1; i >= 0; i--)
        {
            ToRemove[i].OnRemoved(character, stacks[ToRemove[i]].amount, stacks[ToRemove[i]].grantor);
            stacks.Remove(ToRemove[i]);
            ToRemove.RemoveAt(i);
        }
    }
}

public class StackHandler
{
    public int amount;
    public float life;
    public Character grantor;
}