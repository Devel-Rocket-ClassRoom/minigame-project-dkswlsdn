using Newtonsoft.Json;
using System.IO;
using System;
using UnityEngine;
using SaveDataVC = SaveDataV1;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public int currentVersion;

    private int currentSlot = 0;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Debug.Log(mainPath);
            Debug.Log(CurrentSave.version);
        }
    }

    public void SaveRequest(SaveDataVC data)
    {
        try
        {
            var path = Path.Combine(mainPath, fileName[currentSlot]);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            CurrentSave = data;
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message}");
        }
    }

    public SaveDataVC LoadRequest(int slot)
    {
        if (slot < 0 || slot >= fileName.Length) throw new IndexOutOfRangeException("세이브데이터 슬롯 오류");

        currentSlot = slot;

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