using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stat;
    [SerializeField] private Image originBar;
    [SerializeField] private Image additionalBar;
    [SerializeField] private TextMeshProUGUI sum;

    public void Init(float original, float additional, int count)
    {
        sum.text = $"{original} + {additional * count}";
        float max = original + additional * 2;
        originBar.fillAmount = original / max;
        additionalBar.fillAmount = (original + additional * count) / max;
    }
}
