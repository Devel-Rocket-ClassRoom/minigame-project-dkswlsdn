using System;
using UnityEngine;

public class CharacterStat : MonoBehaviour
{
    private CharacterAnchor anchor;

    [SerializeField] private float maxHealth;
    [SerializeField] private float health;
    [SerializeField] private float maxMp;
    [SerializeField] private float mp;
    [SerializeField] private float attack;

    [SerializeField] private ArmorType armor;

    public event Action<AttackInfo> onDamageTake; // 나 이런 공격을 받았어!
    public event Action<float> onTakeDamage; // 나 이만큼의 데미지를 받았어!
    public event Action onSuperArmorActivate; // 나 슈퍼아머 발동했어!
    public event Action onHealthLack; // 나 체력이 부족해!
    public event Action onHealthEnough; // 나 이제 체력 많아!

    [Header("Test")]
    [SerializeField] private bool doTest;
    [SerializeField] private AttackInfo testHitInfo = new AttackInfo();

    [ContextMenu("Test TakeDamage")]
    private void TestTakeDamage() => TakeDamage(testHitInfo);

    private void Awake()
    {
        anchor = GetComponent<CharacterAnchor>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K) && doTest) TakeDamage(testHitInfo);
    }


    public float GetStatPercent(StatType type) 
    {
        return type switch
        {
            StatType.HP => maxHealth > 0 ? health / maxHealth : 1f,
            StatType.MP => maxMp > 0 ? mp / maxMp : 1f,
            _ => 1f
        };
    }

    public void TakeAbsoluteDamage(float damage, out float damageTaken)
    {
        damageTaken = damage;
    }

    public void TakeDamage(AttackInfo hit)
    {
        var myHit = new AttackInfo(hit);

        bool canIgnoreHitStun = (armor & ArmorType.HitStun) != 0 && myHit.reaction == HitReactionType.HitStun;

        bool canIgnoreAirborne = (armor & ArmorType.Airborne) != 0 &&
            (myHit.reaction == HitReactionType.Airborne ||
             myHit.reaction == HitReactionType.Knockdown ||
             myHit.reaction == HitReactionType.Groggy);


        health -= myHit.damage;
        if (hit.id == 0) DamagePopupManager.instance.Popup(myHit.damage, anchor.head.position);

        if (health <= 0)
        {
            Dead();
        }

        if (canIgnoreHitStun || canIgnoreAirborne)
        {
            myHit.reaction = HitReactionType.Gaurded;
        }

        onDamageTake?.Invoke(myHit);
    }

    public void Dead()
    {
        Debug.Log("YouDie!");
    }
}


[Serializable]
public class AttackInfo
{
    public AttackInfo() { }

    public AttackInfo(AttackInfo hit)
    {
        origin = hit.origin;
        id = hit.id;
        damage = hit.damage;
        reaction = hit.reaction;
        stunDuration = hit.stunDuration;
        stunForce = hit.stunForce;
        airborneForce = hit.airborneForce;
        forceDirectionType = hit.forceDirectionType;
    }

    [HideInInspector, NonSerialized]
    public Transform origin;
    [HideInInspector, NonSerialized]
    public int id = -1;
    public float damage;

    public HitReactionType reaction;
    public float stunDuration;
    public float stunForce;
    public Vector2 airborneForce;

    public ForceDirectionType forceDirectionType; 
    public float activateTime;
    public bool isDestroyOnCanceled;
    public bool isReleaseGrab;

    public float projectileSpeed;
}