#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class AnimationPathFixer : AssetPostprocessor
{
    void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        var bindings = AnimationUtility.GetCurveBindings(clip);
        bool modified = false;

        foreach (var binding in bindings)
        {
            if (!binding.path.StartsWith("Armature")) continue;

            var newBinding  = binding;
            newBinding.path = binding.path == "Armature"
                ? ""
                : binding.path.Substring("Armature/".Length);

            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            AnimationUtility.SetEditorCurve(clip, binding,    null);
            AnimationUtility.SetEditorCurve(clip, newBinding, curve);
            modified = true;
        }

        if (modified)
            Debug.Log($"[AnimationPathFixer] {clip.name} 경로 자동 수정 완료");
    }
}
#endif
