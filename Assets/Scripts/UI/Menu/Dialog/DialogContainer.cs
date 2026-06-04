using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogContainer : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextContainer characterName;
    [SerializeField] private TextMeshProUGUI textTmp;

    private string currentKey;
    private int currentIndex;
    private IDialogEndHandler endHandler;

    private const float minStayTime = 0.2f;
    private float nextTime;

    public bool isReady => Time.time > nextTime;

    public void StartDialog(string key, IDialogEndHandler handler = null)
    {
        currentKey = key;
        currentIndex = 0;
        endHandler = handler;
        Show();
    }

    public bool Next()
    {
        currentIndex++;
        string nextKey = $"{currentKey}_{currentIndex:D3}";

        if (DataTableManager.DialogTable.TryGet(nextKey, out _))
        {
            Show();
            return true;
        }
        else
        {
            End();
            return false;
        }
    }

    private void Show()
    {
        string key = $"{currentKey}_{currentIndex:D3}";
        var data = DataTableManager.DialogTable.Get(key);

        characterImage.sprite = DataTableManager.SpriteTable.Get(data.characterId);
        characterName.ChangeText(data.characterName);
        textTmp.text = data.dialog;

        nextTime = Time.time + minStayTime;
    }

    private void End()
    {
        gameObject.SetActive(false);
        endHandler?.OnDialogEnd();
        endHandler = null;
    }
}
