using TMPro;
using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager instance;
    [SerializeField] private TextMeshProUGUI damagePrefab;
    [SerializeField] private Canvas canvas;
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
        var g = Instantiate(damagePrefab, canvas.transform);
        g.text = ((int)damage).ToString();
        var point = cam.WorldToScreenPoint(head);
        point.z = 0f;
        g.transform.position = point;
        Debug.Log(damage);
    }
}
