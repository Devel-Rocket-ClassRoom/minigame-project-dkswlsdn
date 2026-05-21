using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManager : MonoBehaviour
{
    [SerializeField] private Attack[] hitboxes;

    private struct ActiveAttack
    {
        public Attack    attack;
        public float     deactivateTime;
        public Transform origin;
        public int       team;
        public int       id;
    }

    private List<ActiveAttack> activeAttacks = new List<ActiveAttack>();

    public Attack RequestAttack(AttackMethod method, Transform origin, int team, int id, Vector3 targetPoint)
    {
        if (method.type == HitboxType.None) return null;

        var prefab = hitboxes[(int)method.type];
        if (prefab == null) return null;

        var instance = Instantiate(prefab);
        instance.Activate(method, origin, team, id, targetPoint);

        activeAttacks.Add(new ActiveAttack
        {
            attack         = instance,
            deactivateTime = Time.time + method.info.activateTime,
            origin         = origin,
            team           = team,
            id             = id,
        });

        if (method.spawnRules != null && method.spawnRules.Count > 0)
        {
            var rules = method.spawnRules;
            instance.onHit += (character) =>
                StartCoroutine(CoSpawn(rules, SpawnTrigger.OnHit,
                                       origin, character.transform, character.transform.position,
                                       team, id));
        }

        return instance;
    }

    public void DestroyAttack(Attack attack)
    {
        for (int i = activeAttacks.Count - 1; i >= 0; i--)
        {
            if (activeAttacks[i].attack == attack)
            {
                activeAttacks.RemoveAt(i);
                break;
            }
        }
        if (attack != null)
            Destroy(attack.gameObject);
    }

    private void Update()
    {
        for (int i = activeAttacks.Count - 1; i >= 0; i--)
        {
            var entry = activeAttacks[i];
            if (entry.attack == null)
            {
                activeAttacks.RemoveAt(i);
                continue;
            }

            MoveAttack(entry.attack);

            if (Time.time > entry.deactivateTime)
            {
                if (entry.attack.HitInfo.spawnRules != null && entry.attack.HitInfo.spawnRules.Count > 0)
                {
                    var expirePos = entry.attack.transform.position;
                    StartCoroutine(CoSpawn(entry.attack.HitInfo.spawnRules, SpawnTrigger.OnExpire,
                                           entry.origin, null, expirePos, entry.team, entry.id));
                }

                activeAttacks.RemoveAt(i);
                Destroy(entry.attack.gameObject);
            }
        }
    }

    private IEnumerator CoSpawn(List<SpawnRule> rules, SpawnTrigger trigger,
                                 Transform origin, Transform target, Vector3 hitPos,
                                 int team, int id)
    {
        foreach (var rule in rules)
        {
            if (rule.trigger != trigger) continue;
            yield return new WaitForSeconds(rule.spawn.preDelay);

            switch (rule.position)
            {
                case SpawnPosition.AtOrigin:
                    RequestAttack(rule.spawn, origin, team, id, origin.position);
                    break;
                case SpawnPosition.AtTarget:
                    var t = target != null ? target : origin;
                    RequestAttack(rule.spawn, t, team, id, t.position);
                    break;
                case SpawnPosition.AtHitPoint:
                    RequestAttack(rule.spawn, origin, team, id, hitPos);
                    break;
            }
        }
    }

    private void MoveAttack(Attack atk)
    {
        switch (atk.HitInfo.movementType)
        {
            case AttackMovementMethod.FollowCharacter:
                atk.transform.position = atk.HitInfo.info.origin.position +
                    atk.HitInfo.info.origin.TransformVector(atk.HitInfo.positionOffset);
                break;
            case AttackMovementMethod.Linear:
                if (atk.HitInfo.useAim)
                    atk.transform.position += atk.HitInfo.aimDir * atk.HitInfo.info.projectileSpeed * Time.deltaTime;
                else
                    atk.transform.position += atk.transform.forward * atk.HitInfo.info.projectileSpeed * Time.deltaTime;
                break;
        }
    }
}
