using Newtonsoft.Json;
using System.IO;
using System;
using UnityEngine;
using SaveDataVC = SaveDataV1;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public int currentVersion;

    public int CurrentSlot { get; private set; }
    public SaveDataVC CurrentSave { get; private set; }

    private string mainPath;
    private readonly string[] fileName =
    {
        "Absolute", "Save1", "Save2", "Save3"
    };

    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
            mainPath = Path.Combine(Application.persistentDataPath);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    

    public void SaveRequest()
    {
        var data = CurrentSave;

        try
        {
            var path = Path.Combine(mainPath, fileName[CurrentSlot]);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
        }
    }

    public SaveDataVC LoadRequest(int slot)
    {
        if (slot < 0 || slot >= fileName.Length) throw new IndexOutOfRangeException("세이브데이터 슬롯 오류");

        CurrentSlot = slot;

        try
        {
            var path = Path.Combine(mainPath, fileName[slot]);
            if (!File.Exists(path))
            {
                CurrentSave = new SaveDataVC();
                return CurrentSave;
            }

            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<SaveDataVC>(json);
            CurrentSave = data;
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"불러오기 실패: {e.Message}");
            CurrentSave = new SaveDataVC();
            return CurrentSave;
        }
    }
}