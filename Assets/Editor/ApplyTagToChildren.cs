using UnityEditor;
using UnityEngine;

public static class ApplyTagToChildren
{
    [MenuItem("GameObject/Tag/자식까지 태그 적용", false, 0)]
    private static void ApplyTag()
    { 
        foreach (var root in Selection.gameObjects)
        {
            string tag = root.tag;                                   // 부모의 태그를 기준으로 적용
            var all = root.GetComponentsInChildren<Transform>(true); // 비활성 포함
            Undo.RecordObjects(all, "Apply Tag To Children");

            foreach (var t in all)
                t.gameObject.tag = tag;

            EditorUtility.SetDirty(root);
        }
    }

    // 선택이 있을 때만 메뉴 활성화
    [MenuItem("GameObject/Tag/자식까지 태그 적용", true)]
    private static bool Validate() => Selection.activeGameObject != null;
}
