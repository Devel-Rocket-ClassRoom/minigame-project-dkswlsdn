using UnityEngine;

public class DamageFadeout : MonoBehaviour
{
    private void Awake()
    {
        Destroy(gameObject, 1f);
    }
    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime;
    }
}
