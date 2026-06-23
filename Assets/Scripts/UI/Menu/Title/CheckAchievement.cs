using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CheckAchievement : MonoBehaviour
{
    [SerializeField] private string achievement;
    [SerializeField] private ImageContainer image;
    private DatabaseReference achievementRef;

    private void OnEnable()
    {
        achievementRef = FirebaseInitializer.Instance.Database.GetReference($"users/{AuthManager.Instance.UserId}/Achievement");
        Check().Forget();
    }

    private async UniTaskVoid Check()
    {
        DataSnapshot snapshot = await achievementRef.GetValueAsync();
        if (!snapshot.Exists)
        {
            await achievementRef.SetValueAsync(new Dictionary<string, object>());
            snapshot = await achievementRef.GetValueAsync();
        }

        if (!snapshot.HasChild(achievement))
        {
            await achievementRef.Child(achievement).SetValueAsync(false);
        }

        if (snapshot.Exists)
        {
            try
            {
                bool b = bool.Parse(snapshot.Child(achievement).Value.ToString());

                if (b)
                {
                    image.ChangeSprite("ENABLE");
                    Debug.Log("도전과제 달성됨");
                }
                else
                {
                    image.ChangeSprite("CANCEL");
                    Debug.Log("도전과제 달성안됨");
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"오류 발생 : {ex.Message}");
            }
        }
        else
        {
            Debug.Log("스냅샷 없음");
        }
    }
}
