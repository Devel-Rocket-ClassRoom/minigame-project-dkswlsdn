using Cysharp.Threading.Tasks;
using Firebase.Database;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayTimeChecker : MonoBehaviour
{
    private static PlayTimeChecker instance;
    private DatabaseReference playTimeRef;
    private float elapsed = 0f;
    private bool isTracking = false;
    private CancellationTokenSource cts;

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
            return;
        }
    }

    private async UniTaskVoid Start()
    {
        await UniTask.WaitUntil(() => AuthManager.Instance.IsInitialized);
        playTimeRef = FirebaseInitializer.Instance.Database
            .GetReference($"users/{AuthManager.Instance.UserId}/stats/playTime");

        SceneManager.activeSceneChanged += OnSceneChanged;
        TryStartTracking(SceneManager.GetActiveScene().name);
    }

    private void OnSceneChanged(Scene prev, Scene next)
    {
        SaveAsync().Forget();
        TryStartTracking(next.name);
    }

    private void TryStartTracking(string sceneName)
    {
        if (sceneName == "TitleScene")
        {
            isTracking = false;
            cts?.Cancel();
        }
        else
        {
            isTracking = true;
            cts = new CancellationTokenSource();
            TrackLoop(cts.Token).Forget();
        }
    }

    private void Update()
    {
        if (isTracking) elapsed += Time.deltaTime;
    }

    private async UniTaskVoid TrackLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await UniTask.Delay(30000, cancellationToken: token);
            await SaveAsync();
        }
    }

    private async UniTask SaveAsync()
    {
        DataSnapshot snapshot = await playTimeRef.GetValueAsync();
        float saved = snapshot.Exists ? float.Parse(snapshot.Value.ToString()) : 0f;
        await playTimeRef.SetValueAsync(saved + elapsed);
        elapsed = 0f;
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        cts?.Cancel();
    }
}
