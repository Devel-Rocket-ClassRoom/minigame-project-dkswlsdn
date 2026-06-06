using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    protected CharacterAnchor anchor;
    protected StateManager state;

    [SerializeField] protected float maxHealth;
    [SerializeField] protected float health;
    [SerializeField] protected float attack;
    [SerializeField] protected float crit;
    [SerializeField] protected float defense;
    [SerializeField] protected float dodgy;
    [SerializeField] protected float sightRange;

    public float Attack => attack;
    public float Critical => crit;

    public float SightRange => sightRange;

    [SerializeField] private ArmorType armor;
    [SerializeField] private int grabImmuneLevel;
    public ArmorType Armor => armor;
    public bool IsImmune { get; private set; }
    [SerializeField] private bool isObstacle;

    public event Action<Character, AttackInfo> onDamageTake; // 나 이런 공격을 받았어!
    public event Action<float> onTakeDamage; // 나 이만큼의 데미지를 받았어!
    public event Action onSuperArmorActivate; // 나 슈퍼아머 발동했어!
    public event Action onHealthLack; // 나 체력이 부족해!
    public event Action onHealthEnough; // 나 이제 체력 많아!
    public event Action onStatChanged;

    public float HPRatio => Mathf.Clamp01(health / maxHealth);


    protected virtual void Awake()
    {
        anchor = GetComponent<CharacterAnchor>();
        state = GetComponent<StateManager>();
    }

    // 스탯이 갱신되었음을 파생 클래스가 알릴 수 있도록 하는 훅. (event는 선언 클래스 외부에서 직접 호출 불가)
    protected void RaiseStatChanged()
    {
        onStatChanged?.Invoke();
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

        var stat = character.Stat;
        bool crit = false;
        float finalCrit = myHit.crit + stat.Critical - dodgy;
        if (finalCrit <= 0) crit = false;
        else if (finalCrit >= 100f) crit = true;
        else crit = UnityEngine.Random.Range(0f, 100f) <= finalCrit;

        ArmorType requiredArmor = myHit.range switch
        {
            RangeType.None => ArmorType.Full,
            RangeType.Short => ArmorType.Short | ArmorType.Full,
            RangeType.Long => ArmorType.Long | ArmorType.Full,
            RangeType.Middle => ArmorType.Full,
            _ => ArmorType.Full
        };

        bool ignoreReaction = (armor & requiredArmor) != 0;

        float originDamage = stat.Attack * myHit.mult + myHit.add;
        float defendedDamage = originDamage * (100f / (100 + defense));
        float finalDamage = crit ? defendedDamage * 1.3f : defendedDamage;
        health -= finalDamage;
        if (hit.isPopup) DamagePopupManager.instance.Popup(finalDamage, crit, anchor.head.position);

        // 어택인포에 설정된 카메라 흔들림을 공격자 카메라에 적용(플레이어가 때렸을 때만 실제로 흔들림)
        character.Camera?.Shake(myHit.cameraShake);

        if (ignoreReaction)
        {
            myHit.reaction = HitReactionType.Gaurded;
        }

        bool willDie = health <= 0 || (isObstacle && myHit.isBreakable);
        if (willDie && myHit.isBreakable)
        {
            if (isObstacle) health = 0;
            state.Die(true);
            return;
        }

        onDamageTake?.Invoke(character, myHit);

        if (isObstacle && myHit.isBreakable) { health = 0; }

        if (health <= 0)
        {
            state.Die(myHit.isBreakable);
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


