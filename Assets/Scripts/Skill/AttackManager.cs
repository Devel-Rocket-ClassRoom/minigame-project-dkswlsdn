using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.TextCore.Text;

public class AttackManager : MonoBehaviour
{
    [SerializeField] private Attack[] hitboxes;
    public static AttackManager instance;
    private List<Attack> attackList = new List<Attack>();
    private Dictionary<Attack, ObjectPool<Attack>> pools = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Attack RequestAttack(Character character, AttackMethod method, Vector3 targetPoint, bool canSpawn = true)
    {
        var instance = GetOrCreatePool(hitboxes[(int)method.type]).Get();
        instance.Activate(character, method, targetPoint, canSpawn);
        attackList.Add(instance);
        return instance;
    }

    private ObjectPool<Attack> GetOrCreatePool(Attack attack)
    {
        if (!pools.ContainsKey(attack))
        {
            pools[attack] = new ObjectPool<Attack>(
                createFunc: () => Instantiate(attack, transform),
                actionOnGet: go => go.gameObject.SetActive(true),
                actionOnRelease: go =>
                {
                    go.transform.SetParent(transform);
                    go.gameObject.SetActive(false);
                },
                actionOnDestroy: go => Destroy(go)
            );
        }
        return pools[attack];
    }

    private void Update()
    {
        for (int i = attackList.Count - 1; i >= 0; i--)
        {
            if (attackList[i] == null)
            {
                attackList.RemoveAt(i);
            }
        }
    }

    public void DestroyAttack(Attack attack)
    {
        attackList.Remove(attack);
        if (attack != null) pools[attack].Release(attack);
}
}
