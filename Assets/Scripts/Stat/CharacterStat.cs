using Newtonsoft.Json.Linq;
using System;
using System.Collections;
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

    public ArmorType Armor => armor;
    [SerializeField] private ArmorType armor;

    public int GrabImmuneLevel => grabImmuneLevel;
    [SerializeField] private int defaultGrabImmuneLevel;
    private int grabImmuneLevel;

    public bool IsImmune { get; private set; }
    [SerializeField] private bool isObstacle;
    public bool IsObstacle => isObstacle;

    private const float wakeUpImmuneDuration = 1.1f;
    private Coroutine wakeUpImmuneCoroutine;

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

    protected virtual void OnEnable()
    {
        if (state != null) state.onWakeUp += OnWakeUp;
    }

    protected virtual void OnDisable()
    {
        if (state != null) state.onWakeUp -= OnWakeUp;
    }

    // 녹다운에서 일어날 때 잠시 무적
    private void OnWakeUp()
    {
        if (wakeUpImmuneCoroutine != null) StopCoroutine(wakeUpImmuneCoroutine);
        wakeUpImmuneCoroutine = StartCoroutine(CoWakeUpImmune());
    }

    private IEnumerator CoWakeUpImmune()
    {
        ApplyImmune(true);
        yield return new WaitForSeconds(wakeUpImmuneDuration);
        ApplyImmune(false);
        wakeUpImmuneCoroutine = null;
    }

    // 스탯이 갱신되었음을 파생 클래스가 알릴 수 있도록 하는 훅. (event는 선언 클래스 외부에서 직접 호출 불가)
    protected void RaiseStatChanged()
    {
        onStatChanged?.Invoke();
    }

    // 캐릭터 강화 1회당 증가량 (StatusBar 표시와 동일).
    protected const float AttackPerPoint   = 50f;
    protected const float CriticalPerPoint = 30f;
    protected const float HealthPerPoint   = 1000f;
    protected const float DefensePerPoint  = 25f;
    protected const float DodgyPerPoint    = 30f;

    // 캐릭터 테이블 기본값 + 세이브의 강화 투자량으로 최종 스탯을 계산해 적용한다.
    // 플레이어(PlayerStat)와 AI 아군(AllyStat)이 공유한다.
    protected void ApplyCharacterStats(string characterId)
    {
        CharacterData baseStat = DataTableManager.CharacterTable.Get(characterId);
        var dict = SaveManager.CurrentSave.characterData;
        if (!dict.TryGetValue(characterId, out CharacterEntry add))
            throw new System.Exception($"해당 캐릭터의 데이터 없음: {characterId}");

        attack    = baseStat.attack   + AttackPerPoint   * add.consumedStat[(int)StatType.Attack];
        crit      = baseStat.critical + CriticalPerPoint * add.consumedStat[(int)StatType.Critical];
        maxHealth = baseStat.health   + HealthPerPoint   * add.consumedStat[(int)StatType.Health];
        defense   = baseStat.defense  + DefensePerPoint  * add.consumedStat[(int)StatType.Defense];
        dodgy     = baseStat.dodgy    + DodgyPerPoint    * add.consumedStat[(int)StatType.Dodgy];
        health    = maxHealth;

        RaiseStatChanged();
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
        if (IsImmune || state.State == CharacterState.Dead) return;   // 무적이거나 이미 죽은 대상은 타격 무시

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
        grabImmuneLevel = level;
    }

    public void ResetGrabImmune() => grabImmuneLevel = defaultGrabImmuneLevel;
}


