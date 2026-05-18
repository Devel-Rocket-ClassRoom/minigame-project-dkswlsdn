using TMPro;
using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager instance;
    [SerializeField] private TextMeshProUGUI damagePrefab;
    private Camera cam;

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

        cam = Camera.main;
    }

    public void Popup(float damage, Vector3 head)
    {
        var g = Instantiate(damagePrefab);
        g.text = ((int)damage).ToString();
        g.transform.position = cam.WorldToScreenPoint(head + Vector3.up);
    }
}
