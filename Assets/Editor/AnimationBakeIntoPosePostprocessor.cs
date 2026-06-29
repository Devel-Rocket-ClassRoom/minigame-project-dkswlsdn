using UnityEditor;
using UnityEngine;

public class AnimationBakeIntoPosePostprocessor : AssetPostprocessor
{
    private const string TARGET_PATH = "Assets/Imported/Animation/Action 1";

    void OnPreprocessAnimation()
    {
        if (!assetPath.Replace("\\", "/").StartsWith(TARGET_PATH)) return;

        var importer = assetImporter as ModelImporter;
        if (importer == null) return;

        var clips = importer.clipAnimations;

        // clipAnimations가 비어있으면 defaultClipAnimations(FBX에서 읽은 기본값)로 채움
        if (clips == null || clips.Length == 0)
            clips = importer.defaultClipAnimations;

        string clipName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        foreach (var clip in clips)
        {
            clip.name                = clipName;
            clip.lockRootRotation    = true;
            clip.keepOriginalOrientation = true;
            clip.lockRootHeightY     = true;
            clip.lockRootPositionXZ  = true;
        }

        importer.clipAnimations = clips;
    }
}
