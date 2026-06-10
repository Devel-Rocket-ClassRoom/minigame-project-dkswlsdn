using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    // 프리팹별로 풀을 따로 관리
    private Dictionary<GameObject, ObjectPool<GameObject>> pools = new();

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void Play(EffectData data, Transform parent)
    {
        if (data.effect == null) return;

        var pool = GetOrCreatePool(data.effect);
        GameObject go = pool.Get();

        if (parent != null) go.transform.SetParent(parent);
        go.transform.localPosition = data.positionOffset;
        go.transform.localRotation = Quaternion.Euler(data.rotationOffset);
        go.transform.localScale = data.scaleOffset == Vector3.zero ? Vector3.one : data.scaleOffset;

        if (go.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Play();
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(ReturnToPool(go, pool, duration));
        }
    }

    // 부모 없이 월드 좌표에 재생. 캐릭터가 파괴돼도 이펙트가 함께 사라지지 않는다.
    public void Play(EffectData data, Vector3 worldPosition)
    {
        if (data.effect == null) return;

        var pool = GetOrCreatePool(data.effect);
        GameObject go = pool.Get();

        go.transform.SetParent(null);
        go.transform.position = worldPosition + data.positionOffset;
        go.transform.rotation = Quaternion.Euler(data.rotationOffset);
        go.transform.localScale = data.scaleOffset == Vector3.zero ? Vector3.one : data.scaleOffset;

        if (go.TryGetComponent<ParticleSystem>(out var ps))
        {
            ps.Play();
            float duration = ps.main.duration + ps.main.startLifetime.constantMax;
            StartCoroutine(ReturnToPool(go, pool, duration));
        }
    }

    private ObjectPool<GameObject> GetOrCreatePool(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new ObjectPool<GameObject>(
                createFunc: () => Instantiate(prefab, transform),
                actionOnGet: go => go.SetActive(true),
                actionOnRelease: go =>
                {
                    go.transform.SetParent(transform);
                    go.SetActive(false);
                },
                actionOnDestroy: go => Destroy(go)
            );
        }
        return pools[prefab];
    }

    private IEnumerator ReturnToPool(GameObject go, ObjectPool<GameObject> pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        // 재생 중(활성)인 이펙트가 부모와 함께 파괴됐을 수 있다.
        // 죽은 참조를 Release하면 풀이 오염되므로 가드한다.
        if (go != null) pool.Release(go);
    }
}

[Serializable]
public class EffectData
{
    public GameObject effect;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scaleOffset;
    //public bool useFreeze;
}
