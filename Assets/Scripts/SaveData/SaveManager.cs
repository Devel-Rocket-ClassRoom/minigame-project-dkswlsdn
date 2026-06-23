using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SaveDataVC = SaveDataV3;

public static class SaveManager
{
    private static int currentVersion = 3;
    public static event Action onSaveModified;

    public static int CurrentSlot { get; private set; }
    private static SaveDataVC current;
    public static SaveDataVC CurrentSave
    {
        get
        {
            if (current == null)
            {
                LoadRequest(0);
            }

            return current;
        }
        private set
        {
            current = value;
        }
    }

    private static string mainPath;
    private static readonly string[] fileName =
    {
        "Absolute", "Save1", "Save2", "Save3"
    };

    static SaveManager()
    {
        mainPath = Path.Combine(Application.persistentDataPath);
    }


    public static void SaveRequest()
    {
        if (CurrentSave == null)
        {
            Debug.LogError("[SaveManager] CurrentSave가 null — 저장 불가");
            return;
        }

        var data = CurrentSave;

        try
        {
            var path = Path.Combine(mainPath, fileName[CurrentSlot]);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
            onSaveModified?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"저장 실패: {e.Message} / {fileName[CurrentSlot]}\n{e.StackTrace}");
        }
    }

    public static SaveDataVC LoadRequest(int slot)
    {
        if (slot < 0 || slot >= fileName.Length) throw new IndexOutOfRangeException("세이브데이터 슬롯 오류");

        CurrentSlot = slot;

        try
        {
            var path = Path.Combine(mainPath, fileName[slot]);

            if (!File.Exists(path))
            {
                Debug.Log($"[Save] 파일 없음, 새 저장 데이터 생성: {path}");
                CurrentSave = new SaveDataVC().Init();
                return CurrentSave;
            }

            Debug.Log($"[Save] 파일 로드 시작: {path}");
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                throw new Exception($"파일 읽기 실패 (path: {path})", e);
            }

            SaveData ver;
            try
            {
                ver = JsonConvert.DeserializeObject<SaveDataVC>(json);
                if (ver == null) throw new Exception("역직렬화 결과가 null (JSON이 비어있거나 형식 불일치)");
                Debug.Log($"[Save] 역직렬화 성공 - 저장 버전: {ver.version}, 현재 버전: {currentVersion}");
            }
            catch (Exception e)
            {
                throw new Exception($"JSON 역직렬화 실패\nJSON 내용: {json}", e);
            }

            try
            {
                int migrationStep = 0;
                while (ver.version != currentVersion)
                {
                    int beforeVersion = ver.version;
                    ver = ver.NextVersion();
                    if (ver == null) throw new Exception($"NextVersion()이 null 반환 (step {migrationStep}, before version: {beforeVersion})");
                    Debug.Log($"[Save] 마이그레이션 step {++migrationStep}: v{beforeVersion} → v{ver.version}");

                    if (migrationStep > 100) throw new Exception("마이그레이션 무한루프 의심 (100회 초과)");
                }
            }
            catch (Exception e)
            {
                throw new Exception($"버전 마이그레이션 실패 (현재 ver.version: {ver?.version})", e);
            }

            CurrentSave = ver as SaveDataVC;
            if (CurrentSave == null)
                throw new Exception($"SaveDataVC 캐스팅 실패 - 실제 타입: {ver.GetType().Name}");

            Debug.Log($"[Save] 로드 완료 (version: {CurrentSave.version})");
            return CurrentSave;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Save] 불러오기 실패\n원인: {e.Message}\n스택:\n{e.StackTrace}");
            if (e.InnerException != null)
                Debug.LogError($"[Save] 내부 원인: {e.InnerException.Message}\n{e.InnerException.StackTrace}");

            CurrentSave = new SaveDataVC().Init();
            SaveRequest();
            return CurrentSave;
        }
    }

    public static void DeleteSave(int slot)
    {
        var path = Path.Combine(mainPath, fileName[slot]);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }

    public static void ResetSave()
    {
        CurrentSave = new SaveDataVC().Init();
    }

    public static void InventoryIO(string item, int amount, bool isStorage)
    {
        Dictionary<string, int> dict = null;

        if (isStorage)
        {
            dict = CurrentSave.itemInStorage;
        }
        else
        {
            dict = CurrentSave.itemInCharacter;
        }

        if (dict.ContainsKey(item))
        {
            if (dict[item] + amount < 0) throw new InvalidOperationException("아이템을 음수개로 가질 수 없습니다");
            dict[item] += amount;
            if (dict[item] == 0)
            {
                dict.Remove(item);
            }
        }
        else
        {
            if (amount <= 0) throw new InvalidOperationException("아이템을 음수개로 가질 수 없습니다");
            dict.Add(item, amount);
        }
    }
}