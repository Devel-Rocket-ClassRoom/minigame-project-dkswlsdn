using TMPro;
using UnityEngine;

public class TextContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    public string text;

    private void Start()
    {
        if (!string.IsNullOrEmpty(text))
            ChangeText(text);
    }

    public void ChangeText(string text)
    {
        this.text = text;
        var t = DataTableManager.StringTable.Get(text);
        if (t == default) tmp.text = text;
        else tmp.text = t;
    }

    private void OnValidate()
    {
        tmp.text = text;
    }
}
