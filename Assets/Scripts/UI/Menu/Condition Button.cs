using UnityEngine;
using UnityEngine.UI;

public class ConditionButton : MonoBehaviour
{
    [SerializeField] protected ImplementedCharacter character;
    protected Button button;
    protected ImageContainer image;
    [SerializeField] protected Image lockImage;

    protected virtual void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<ImageContainer>();
    }

    private void OnEnable()
    {
        SaveManager.onSaveModified += Load;
        Load();
    }

    private void OnDisable()
    {
        SaveManager.onSaveModified -= Load;
    }

    private void Load()
    {
        if (SaveManager.CurrentSave.unlockedCharacterList.Contains(SaveData.implementedCharacter[(int)character]))
        {
            button.interactable = true;
            lockImage.gameObject.SetActive(false);
        }
        else
        {
            button.interactable = false;
            lockImage.gameObject.SetActive(true);
        }
    }
}
