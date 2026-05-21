using System;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private HashSet<int> hitTarget = new HashSet<int>();
    [HideInInspector] public AttackMethod HitInfo;
    [HideInInspector] public bool isGrab;
    private int team;

    public bool IsHit { get; private set; }

    public event Action<Character> onHit;
    public event Action<Vector3> onTargetHit;

    public void Activate(AttackMethod attackInfo, Transform origin, int team, int id, Vector3 targetPoint)
    {
        hitTarget.Clear();
        IsHit = false;
        HitInfo = attackInfo;
        HitInfo.info = new AttackInfo(attackInfo.info);
        HitInfo.info.id = id;
        HitInfo.info.origin = origin;
        if (attackInfo.movementType == AttackMovementMethod.Teleport)
            transform.position = targetPoint;
        else
            transform.position = origin.position + origin.TransformVector(attackInfo.positionOffset);
        transform.forward = origin.forward;
        if (attackInfo.scale != Vector3.zero)
            transform.localScale = attackInfo.scale;
        this.team = team;
        isGrab = attackInfo.isGrab;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger) return;

        var character = other.GetComponent<Character>();
        if (character == null || hitTarget.Contains(character.Id) || character.team == team) return;

        hitTarget.Add(character.Id);
        IsHit = true;

        if (!isGrab)
            character.Stat.TakeDamage(HitInfo.info);

        onHit?.Invoke(character);
    }
}
