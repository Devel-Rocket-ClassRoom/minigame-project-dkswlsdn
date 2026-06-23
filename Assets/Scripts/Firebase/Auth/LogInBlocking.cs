using UnityEngine;

public class LogInBlocking : MonoBehaviour
{
    [SerializeField] private GameObject[] menu;
    bool logedIn = false;

    void Update()
    {
        if (logedIn == AuthManager.Instance.IsLogedIn) return;
        logedIn = AuthManager.Instance.IsLogedIn;

        foreach (GameObject go in menu)
        {
            go.SetActive(logedIn);
        }
    }
}
