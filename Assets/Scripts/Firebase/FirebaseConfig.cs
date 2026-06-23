using UnityEngine;

[CreateAssetMenu(fileName = "FirebaseConfig", menuName = "Firebase Study/Firebase Config")]
public class FirebaseConfig : ScriptableObject
{
    public string apiKey;        // 참고/확장용
    public string appId;         // 참고/확장용
    public string projectId;     // 참고/확장용
    public string databaseUrl;   // 코드가 실제로 쓰는 값: Realtime Database URL
    public string storageBucket; // 참고/확장용

    public bool IsValid => !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(databaseUrl);
}