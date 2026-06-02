using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadBaseCamp()
    {
        SaveManager.instance.SaveRequest();
        GameSceneManager.LoadScene(SceneName.BaseCamp);
    }

    public void LoadBattleSpace()
    {
        SaveManager.instance.SaveRequest();
        GameSceneManager.LoadScene(SceneName.Battle);
    }
}
