using TMPro;
using UnityEngine;

public class TextContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private string text;

    private void OnValidate()
    {
        tmp.text = text;
    }
}
