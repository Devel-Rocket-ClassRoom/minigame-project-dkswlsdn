using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadBaseCamp()
    {
        SaveManager.instance.SaveRequest();
        GameSceneManager.instance.LoadScene(SceneName.BaseCamp);
    }

    public void LoadBattleSpace()
    {
        SaveManager.instance.SaveRequest();
        GameSceneManager.instance.LoadScene(SceneName.Battle);
    }
}
