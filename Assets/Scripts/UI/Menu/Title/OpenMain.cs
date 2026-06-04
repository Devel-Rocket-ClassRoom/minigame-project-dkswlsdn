using UnityEngine;

public class OpenMain : MonoBehaviour
{
    // Awake가 아닌 Start에서 호출 → 모든 Awake가 끝나 instance 등록이 보장된 뒤 실행
    private void Start()
    {
        MenuManager.instance.TitleOpen();
    }
}
