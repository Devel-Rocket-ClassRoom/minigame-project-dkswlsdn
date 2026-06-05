using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSceneManager
{
    public static bool IsInBattle { get; private set; }

    private static readonly string[] scenes =
    {
        "TitleScene", "BaseCampScene", "BattleScene", "TutorialScene", "BossScene"
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InitOnLoad()
    {
        var current = SceneManager.GetActiveScene().name;
        IsInBattle = current != scenes[(int)SceneName.TitleScene]
                  && current != scenes[(int)SceneName.BaseCamp];
    }

    public static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;  // 에디터에서 플레이 중지
#else
    Application.Quit();  // 빌드에서 종료
#endif
    }

    public static void LoadScene(SceneName scene)
    {
        IsInBattle = scene != SceneName.TitleScene && scene != SceneName.BaseCamp;
        SceneManager.LoadScene(scenes[(int)scene]);
    }

    public static bool StartGame(int idx)
    {
        SaveManager.LoadRequest(idx);

        if (SaveManager.CurrentSave.anchCount < 0)
        {
            return false;
        }

        if (SaveManager.CurrentSave.isTutorialCleared)
        {
            IsInBattle = false;
            LoadScene(SceneName.BaseCamp);
        }
        else
        {
            IsInBattle = true;
            SaveManager.ResetSave();
            LoadScene(SceneName.Tutorial);
        }

        return true;
    }

    public static void LoadBaseCamp()
    {
        LoadScene(SceneName.BaseCamp);
    }

    public static void LoadBattleSpace()
    {
        IsInBattle = true;
        LoadScene(SceneName.Battle);
    }

    public static void LoadBoss()
    {
        IsInBattle = true;
        LoadScene(SceneName.Boss);
    }
}

public enum SceneName
{
    TitleScene = 0,
    BaseCamp = 1,
    Battle = 2,
    Tutorial = 3,
    Boss = 4,
}
