using TMPro;
using UnityEngine;

public class TextContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    public string text;

    public void ChangeText(string text)
    {
        var t = DataTableManager.StringTable.Get(text);
        if (t == default) tmp.text = text;
        else tmp.text = t;
    }

    private void OnValidate()
    {
        tmp.text = text;
    }
}
