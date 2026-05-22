using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager instance;

    private readonly string[] scenes =
    {
        "TltleScene", "BaseCampScene", "BattleScene"
    };

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 플레이 중지
#else
    Application.Quit();  // 빌드에서 종료
#endif
    }

    public void LoadScene(SceneName scene)
    {
        SceneManager.LoadScene(scenes[(int)scene]);
    }

    public void StartGame(int idx)
    {
        LoadScene(SceneName.BaseCamp);
        SaveManager.instance.LoadRequest(idx);
    }

    public void LoadBaseCamp()
    {
        LoadScene(SceneName.BaseCamp);
    }

    public void LoadBattleSpace()
    {
        LoadScene(SceneName.Battle);
    }
}

public enum SceneName
{
    TitleScene = 0,
    BaseCamp = 1,
    Battle = 2,
}
