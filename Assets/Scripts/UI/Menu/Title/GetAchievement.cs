using Cysharp.Threading.Tasks;
using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GetAchievement : Interactor
{
    [SerializeField] private string achievement;
    private DatabaseReference achievementRef;

    private void Awake()
    {
        try
        {
            achievementRef = FirebaseInitializer.Instance.Database.GetReference($"users/{AuthManager.Instance.UserId}/Achievement");
        }
        catch
        {

        }
    }

    private async UniTaskVoid Check()
    {
        DataSnapshot snapshot = await achievementRef.GetValueAsync();
        if (!snapshot.Exists)
        {
            await achievementRef.SetValueAsync(new Dictionary<string, object>());
            snapshot = await achievementRef.GetValueAsync();
        }

        if (snapshot.HasChild(achievement))
        {
            await achievementRef.Child(achievement).SetValueAsync(true);
        }
        else
        {
            Debug.LogError("해당 도전과제 없음");
        }
    }

    public override bool OnDetected(Character character)
    {
        Check().Forget();
        return true;
    }
}
