using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : MonoBehaviour
{
    private HashSet<int> hitTarget = new HashSet<int>();
    private Character character;
    [HideInInspector] public AttackMethod method;
    private Vector3 targetPoint;

    public bool IsHit { get; private set; }

    public event Action<Character> onHit;
    public event Action<Vector3> onTargetHit;

    private bool isReady;
    private AttackId id = new AttackId();

    public void Activate(Character character, AttackMethod method, Vector3 targetPoint, bool canSpawn = true)
    {
        this.character = character;
        this.targetPoint = targetPoint;
        hitTarget.Clear();
        IsHit = false;

        onHit = null;

        this.method = method;
        
        id.id = character.Id;
        id.team = character.team;
        id.isPlayer = character.isPlayer;
        id.canSpawn = canSpawn;
        id.origin = character.transform;

        if (method.movementType == AttackMovementMethod.Teleport)
            transform.position = targetPoint;
        else
            transform.position = character.transform.position + character.transform.TransformVector(method.positionOffset);

        transform.forward = method.movementType != AttackMovementMethod.Linear ? character.transform.forward : (targetPoint - transform.position).normalized;
        if (method.scale != Vector3.zero)
            transform.localScale = method.scale;

        isReady = true;

        if (canSpawn && method.spawnRules != null && method.spawnRules.Count > 0)
            onHit += (hitCharacter) =>
                StartCoroutine(CoSpawn(SpawnTrigger.OnHit, hitCharacter));

        StartCoroutine(CoLife());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger || !isReady) return;
        if (((1 << other.gameObject.layer) & method.targetLayer) == 0) return;

        var character = other.GetComponent<Character>();
        if (character == null || hitTarget.Contains(character.Id) || character.team == id.team) return;

        hitTarget.Add(character.Id);
        IsHit = true;

        character.Stat.TakeDamage(this.character, method.info, id);

        this.character.State.FreezeFor(method.info.reverseStun);
        onHit?.Invoke(character);
        if (method.isSingleTarget && isReady)
        {
            isReady = false;
            StopAllCoroutines();
            AttackManager.instance.ReleaseAttack(this);
        }
    }


    private void Update()
    {
        if (!isReady) return;

        AttackMove();
    }

    private void AttackMove()
    {
        switch (method.movementType)
        {
            case AttackMovementMethod.FollowCharacter:
                transform.position = character.transform.position + character.transform.TransformVector(method.positionOffset);
                break;
            case AttackMovementMethod.Linear:
                transform.position += transform.forward * method.info.projectileSpeed * Time.deltaTime;
                break;
        }
    }

    IEnumerator CoLife()
    {
        if (method.info.useFrozen) yield return new WaitForSecondsUnfrozen(method.info.activateTime, character.State);
        else yield return new WaitForSeconds(method.info.activateTime);
        if (isReady)
        {
            isReady = false;
            AttackManager.instance.ReleaseAttack(this);
        }
    }

    IEnumerator CoSpawn(SpawnTrigger trigger, Character hitCharacter = null)
    {
        foreach (var rule in method.spawnRules)
        {
            if (rule.trigger != trigger) continue;

            yield return new WaitForSecondsUnfrozen(rule.preDelay, character.State);

            Vector3 targetPoint = rule.position switch
            {
                SpawnPosition.AtOrigin   => character.transform.position,
                SpawnPosition.AtTarget   => hitCharacter != null ? hitCharacter.transform.position : character.transform.position,
                SpawnPosition.AtHitPoint => this.targetPoint,
                _                        => character.transform.position,
            };

            AttackManager.instance.RequestAttack(character, rule.method, targetPoint, canSpawn: false);
        }
    }
    
    public void OnCancled()
    {
        if (isReady)
        {
            isReady = false;
            AttackManager.instance.ReleaseAttack(this);
        }
    }
}

public struct AttackId
{
    public int id;
    public int team;
    public bool isPlayer;
    public bool canSpawn;
    public Transform origin;
}
