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

    private int team;
    private float life;

    public bool IsHit { get; private set; }

    public event Action<Character> onHit;
    public event Action<Vector3> onTargetHit;

    private bool isReady;
    private bool canSpawn;

    public void Activate(Character character, AttackMethod method, Vector3 targetPoint, bool canSpawn = true)
    {
        this.character = character;
        this.targetPoint = targetPoint;
        hitTarget.Clear();
        IsHit = false;
        this.method = method;
        this.method.info = new AttackInfo(method.info);
        this.method.info.id = character.Id;
        this.method.info.isPopup = character.isPlayer;
        this.method.info.origin = character.transform;
        life = method.info.activateTime;

        if (method.movementType == AttackMovementMethod.Teleport)
            transform.position = targetPoint;
        else
            transform.position = character.transform.position + character.transform.TransformVector(method.positionOffset);

        transform.forward = method.movementType != AttackMovementMethod.Linear ? character.transform.forward : (targetPoint - transform.position).normalized;
        if (method.scale != Vector3.zero)
            transform.localScale = method.scale;

        this.team = character.team;
        this.canSpawn = canSpawn;
        isReady = true;

        if (canSpawn && method.spawnRules != null && method.spawnRules.Count > 0)
            onHit += (hitCharacter) =>
                StartCoroutine(CoSpawn(SpawnTrigger.OnHit, hitCharacter));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.isTrigger || !isReady) return;

        var character = other.GetComponent<Character>();
        if (character == null || hitTarget.Contains(character.Id) || character.team == team) return;

        hitTarget.Add(character.Id);
        IsHit = true;

        if (!method.isGrab)
            character.Stat.TakeDamage(this.character, method.info);

        life += method.info.reverseStun;
        this.character.State.FreezeFor(method.info.reverseStun);
        onHit?.Invoke(character);
    }

    private void Update()
    {
        if (!isReady) return;

        if (life <= 0)
        {
            isReady = false;
            if (canSpawn) StartCoroutine(CoSpawn(SpawnTrigger.OnExpire));
            else Destroy(gameObject);
        }

        AttackMove();
        life -= Time.deltaTime;
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

        if (trigger == SpawnTrigger.OnExpire)
            Destroy(gameObject);
    }
}
