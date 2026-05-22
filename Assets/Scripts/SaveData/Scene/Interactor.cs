using UnityEngine;
using UnityEngine.SceneManagement;

public class Interactor : MonoBehaviour
{
    [SerializeField] private MenuPanel panel;

    public void OnDetected()
    {
        var manager = panel.transform.root.GetComponent<MenuManager>();
        manager.OpenMenu(panel);
    }
}
