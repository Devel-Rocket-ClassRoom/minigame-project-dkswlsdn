using UnityEngine;

public class Anchor : MonoBehaviour
{
    [SerializeField] GameObject rope;
    private int length = 20;

    private void OnEnable()
    {
        for (int i = 0; i < length; i++)
        {
            Instantiate(rope, transform.position + Vector3.down * i, Quaternion.identity);
        }
    }
}
