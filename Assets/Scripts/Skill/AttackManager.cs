using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.TextCore.Text;

public class AttackManager : MonoBehaviour
{
    [SerializeField] private Attack[] hitboxes;
    public static AttackManager instance;
    private ObjectPool<Attack>[] pools;

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

        pools = new ObjectPool<Attack>[hitboxes.Length];
        for (int i = 0; i < hitboxes.Length; i++)
        {
            int idx = i; // 클로저 캡처용
            pools[i] = new ObjectPool<Attack>(
                createFunc: () => Instantiate(hitboxes[idx], transform),
                actionOnGet: go => go.gameObject.SetActive(true),
                actionOnRelease: go => go.gameObject.SetActive(false),
                actionOnDestroy: go => Destroy(go.gameObject)
            );
        }
    }

    public Attack RequestAttack(Character character, AttackMethod method, Vector3 targetPoint, bool canSpawn = true)
    {
        var instance = pools[(int)method.type].Get();
        instance.Activate(character, method, targetPoint, canSpawn);
        return instance;
    }

    public void ReleaseAttack(Attack attack)
    {
        pools[(int)attack.method.type].Release(attack);
    }
}
