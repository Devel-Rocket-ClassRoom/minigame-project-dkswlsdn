using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AttackPreview))]
public class AttackPreviewEditor : Editor
{
    private static GameObject lastPreview;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(6);
        if (GUILayout.Button("Preview", GUILayout.Height(28)))
            SpawnPreview((AttackPreview)target);
    }

    private void SpawnPreview(AttackPreview preview)
    {
        var prefab = preview.GetPrefab(preview.type);
        if (prefab == null)
        {
            Debug.LogWarning($"HitboxType.{preview.type} 에 할당된 프리팹이 없습니다.");
            return;
        }

        if (lastPreview != null)
            DestroyImmediate(lastPreview);

        lastPreview = (GameObject)PrefabUtility.InstantiatePrefab(prefab.gameObject);
        lastPreview.transform.position = preview.positionOffset;
        lastPreview.transform.rotation = Quaternion.Euler(preview.rotation);
        if (preview.scale != Vector3.zero)
            lastPreview.transform.localScale = preview.scale;
        lastPreview.name = $"[Preview] {preview.type}";

        Undo.RegisterCreatedObjectUndo(lastPreview, "Preview Attack");
        Selection.activeGameObject = lastPreview;
    }
}
