using UnityEngine;
using UnityEngine.UI;

public class ImageContainer : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private string key;

    private void Start()
    {
        if (!string.IsNullOrEmpty(key))
            ChangeSprite(key);
    }

    public void ChangeSprite(string text)
    {
        key = text;
        var t = DataTableManager.SpriteTable.Get(text);
        if (t != default) img.sprite = t;
    }

    public void SetEnable(bool enable)
    {
        if (img != null) img.enabled = enable;
    }

    public bool IsEnabled => img != null && img.enabled;

    private void OnValidate()
    {
        var t = DataTableManager.SpriteTable.Get(key);
        img.sprite = t;
    }
}
