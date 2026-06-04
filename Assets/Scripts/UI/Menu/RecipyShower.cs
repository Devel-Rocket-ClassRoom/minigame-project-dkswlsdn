using TMPro;
using UnityEngine;

public class RecipyShower : MonoBehaviour
{
    [SerializeField] private ImageContainer image;
    [SerializeField] private TextMeshProUGUI amount;

    public void Init(string item, int amount)
    {
        image.ChangeSprite(item);
        this.amount.text = amount.ToString();
    }
}
