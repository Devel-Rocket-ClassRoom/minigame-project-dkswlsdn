using UnityEngine;
using UnityEngine.UI;

public class ImageContainer : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private string key;

    public void ChangeSprite(string text)
    {
        key = text;
        var t = DataTableManager.SpriteTable.Get(text);
        if (t == default) img = null;
        else img.sprite = t;
    }

    private void OnValidate()
    {
        var t = DataTableManager.SpriteTable.Get(key);
        img.sprite = t;
    }
}
