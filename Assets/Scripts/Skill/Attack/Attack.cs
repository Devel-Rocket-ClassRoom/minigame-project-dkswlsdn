using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class Attack : MonoBehaviour
{
    private HashSet<int> hitTarget = new HashSet<int>();
    [HideInInspector] public AttackMethod HitInfo;
    [HideInInspector] public bool isGrab;
    private int team;
    private float deactivateTime;
    private bool isActive;

    public bool IsHit { get; private set; }
    public event Action<Character> onHit;

    public void Activate(SkillContext context, AttackMethod attackInfo, Transform origin, int team, int id)
    {
        hitTarget.Clear();
        IsHit = false;
        HitInfo = attackInfo;
        HitInfo.info = new AttackInfo(attackInfo.info);
        HitInfo.info.id = id;
        HitInfo.info.origin = origin;
        if (attackInfo.movementType == AttackMovementMethod.Teleport) transform.position = context.targetPoint;
        else transform.position = origin.position + origin.TransformVector(attackInfo.positionOffset);
        transform.forward = origin.forward;
        this.team = team;
        isGrab = attackInfo.isGrab;
        deactivateTime = Time.time + attackInfo.info.activateTime;
        isActive = true;
    }

    public void DeActivate()
    {
        isActive = false;
        Destroy(gameObject);
    }

    private void Update()
    {
        if (isActive && deactivateTime < Time.time)
            DeActivate();
        if (HitInfo.movementType == AttackMovementMethod.FollowCharacter)
        {
            transform.position = HitInfo.info.origin.position + HitInfo.info.origin.TransformVector(HitInfo.positionOffset); ;
        }
        else if (HitInfo.movementType == AttackMovementMethod.Linear)
        {
            if (HitInfo.useAim)
            {
                transform.position += HitInfo.aimDir * HitInfo.info.projectileSpeed * Time.deltaTime;
            }
            else
            {
                transform.position += transform.forward * HitInfo.info.projectileSpeed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger) return;

        var character = other.GetComponent<Character>();
        if (character == null || hitTarget.Contains(character.Id) || character.team == team) return;

        hitTarget.Add(character.Id);
        IsHit = true;

        if (!isGrab)
        {
            character.Stat.TakeDamage(HitInfo.info);
        }

        onHit?.Invoke(character);
    }
}
