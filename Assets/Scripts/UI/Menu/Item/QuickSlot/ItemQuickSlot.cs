using UnityEngine;

public abstract class ItemQuickSlot : MonoBehaviour
{
    protected Character character;
    protected CharacterCommander commander;
    protected StateManager state;
    protected bool enable = true;

    protected virtual void Awake()
    {
        character = GetComponent<Character>();
        commander = GetComponent<CharacterCommander>();
        state = GetComponent<StateManager>();
        state.onDead += OnDead;
    }

    public abstract void GetItem(Item item);

    public virtual void OnDead() { enable = false; }
}
