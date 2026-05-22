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
    [SerializeField] private float sightRange;

    public float SightRange => sightRange;

    [SerializeField] private ArmorType armor;

    public event Action<AttackInfo> onDamageTake; // 나 이런 공격을 받았어!
    public event Action<float> onTakeDamage; // 나 이만큼의 데미지를 받았어!
    public event Action onSuperArmorActivate; // 나 슈퍼아머 발동했어!
    public event Action onHealthLack; // 나 체력이 부족해!
    public event Action onHealthEnough; // 나 이제 체력 많아!
    public event Action onStatChanged;

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
        //if ( && doTest) TakeDamage(testHitInfo);
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
        if (hit.isPopup) DamagePopupManager.instance.Popup(myHit.damage, anchor.head.position);

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

    public void RestoreHP(float amount)
    {
        health = Mathf.Min(health + amount, maxHealth);
    }
}


