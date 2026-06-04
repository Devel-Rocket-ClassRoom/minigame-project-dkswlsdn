using System;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    private CharacterAnchor anchor;
    private StateManager state;

    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private float attack;
    [SerializeField] private float sightRange;

    public float SightRange => sightRange;

    [SerializeField] private ArmorType armor;
    [SerializeField] private int grabImmuneLevel;
    public ArmorType Armor => armor;
    public bool IsImmune { get; private set; }

    public event Action<Character, AttackInfo> onDamageTake; // 나 이런 공격을 받았어!
    public event Action<float> onTakeDamage; // 나 이만큼의 데미지를 받았어!
    public event Action onSuperArmorActivate; // 나 슈퍼아머 발동했어!
    public event Action onHealthLack; // 나 체력이 부족해!
    public event Action onHealthEnough; // 나 이제 체력 많아!
    public event Action onStatChanged;

    public float HPRatio => Mathf.Clamp01(health / maxHealth);


    private void Awake()
    {
        anchor = GetComponent<CharacterAnchor>();
        state = GetComponent<StateManager>();
    }


    public float GetStatPercent(StatType type) 
    {
        return type switch
        {
            StatType.Health => maxHealth > 0 ? health / maxHealth : 1f,
            _ => 1f
        };
    }

    public void TakeAbsoluteDamage(float damage, out float damageTaken)
    {
        damageTaken = damage;
    }

    public void TakeDamage(Character character, AttackInfo hit)
    {
        if (IsImmune) return;

        var myHit = new AttackInfo(hit);

        ArmorType requiredArmor = myHit.range switch
        {
            RangeType.None => ArmorType.Full,
            RangeType.Short => ArmorType.Short | ArmorType.Full,
            RangeType.Long => ArmorType.Long | ArmorType.Full,
            RangeType.Middle => ArmorType.Full,
            _ => ArmorType.Full
        };

        bool ignoreReaction = (armor & requiredArmor) != 0;


        health -= myHit.damage;
        if (hit.isPopup) DamagePopupManager.instance.Popup(myHit.damage, anchor.head.position);

        if (ignoreReaction)
        {
            myHit.reaction = HitReactionType.Gaurded;
        }

        onDamageTake?.Invoke(character, myHit);

        if (health <= 0)
        {
            state.ChangeState(CharacterState.Dead);
        }
    }


    public void RestoreHP(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }

    // 풀 재사용/부활 시 초기화. 일단은 체력만 최대로 채운다.
    public void ResetState()
    {
        health = maxHealth;
    }

    public void ApplyArmor(ArmorType armor)
    {
        this.armor |= armor;
    }

    public void RemoveArmor(ArmorType armor)
    {
        this.armor &= ~armor;
    }

    public void ApplyImmune(bool useImmune)
    {
        IsImmune = useImmune;
    }

    public void ApplyGrabImmune(int level)
    {

    }
}


