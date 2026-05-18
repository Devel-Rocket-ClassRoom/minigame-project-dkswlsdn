using System;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private HashSet<int> hitTarget = new HashSet<int>();
    [HideInInspector] public AttackInfo HitInfo;
    [HideInInspector] public bool isGrab;
    public int team;
    private float deactivateTime;
    private bool isActive;

    public bool IsHit { get; private set; }
    public event Action<Character> onHit;

    public void Activate(AttackInfo hitInfo, Vector3 origin, Quaternion rotation, int team, bool isGrab = false)
    {
        hitTarget.Clear();
        IsHit = false;
        HitInfo = hitInfo;
        transform.position = origin;
        transform.rotation = rotation;
        this.team = team;
        this.isGrab = isGrab;
        deactivateTime = Time.time + hitInfo.activateTime;
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
    }

    private void OnTriggerEnter(Collider other)
    {
        var character = other.GetComponent<Character>();
        if (character == null || hitTarget.Contains(character.Id) || character.team == team) return;

        hitTarget.Add(character.Id);
        IsHit = true;

        if (!isGrab)
            character.Stat.TakeDamage(HitInfo);

        onHit?.Invoke(character);
    }
}
