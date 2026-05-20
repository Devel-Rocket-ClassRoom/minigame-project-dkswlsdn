using TMPro;
using UnityEngine;

public class TextContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    public string text;

    public void ChangeText(string text)
    {
        tmp.text = text;
    }

    private void OnValidate()
    {
        tmp.text = text;
    }
}
