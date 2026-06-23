using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public void LoadBaseCamp()
    {
        SaveManager.SaveRequest();
        GameSceneManager.LoadScene(SceneName.BaseCamp);
    }

    public void LoadBattleSpace()
    {
        SaveManager.SaveRequest();
        GameSceneManager.LoadScene(SceneName.Battle);
    }
}
