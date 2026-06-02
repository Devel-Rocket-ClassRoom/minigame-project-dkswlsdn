using TMPro;
using UnityEngine;

public class ShowWeight : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private void OnEnable()
    {
        SetWeight();
        SaveManager.onSaveModified += SetWeight;
        Item.onGet += SetWeight;
        Item.onUse += SetWeight;
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= SetWeight;
        Item.onGet -= SetWeight;
        Item.onUse -= SetWeight;
    }

    private void SetWeight()
    {
        text.text = $"{WeightManager.Sum:F2} / {WeightManager.Max}";
    }
}
