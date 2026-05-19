using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// Player_Idle.anim と Player.controller を生成する。
/// Unity メニュー: Tools > Setup Player Idle Anim
/// </summary>
public class PlayerAnimSetup
{
    const string SPRITE_BASE = "Assets/Sprites/Player/IdleFrames/idle_{0:D2}.png";
    const string CLIP_PATH   = "Assets/Resources/Animations/Player_Idle.anim";
    const string CTRL_PATH   = "Assets/Resources/Animations/Player.controller";

    [MenuItem("Tools/Setup Player Idle Anim")]
    static void Setup()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Animations");

        // ---- スプライトをロード ----
        var sprites = new Sprite[8];
        for (int i = 0; i < 8; i++)
        {
            string path = string.Format(SPRITE_BASE, i);
            sprites[i] = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprites[i] == null)
                Debug.LogError($"[PlayerAnimSetup] Sprite not found: {path}");
        }

        // ---- AnimationClip を作成 ----
        var clip       = new AnimationClip();
        clip.name      = "Player_Idle";
        clip.frameRate = 30f;

        // ループ ON
        var clipSettings     = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);

        // SpriteRenderer.sprite への ObjectReference キーフレーム
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        float[] times = { 0f, 0.3f, 0.6f, 0.9f, 1.2f, 1.5f, 1.8f, 2.1f };
        var keys = new ObjectReferenceKeyframe[8];
        for (int i = 0; i < 8; i++)
            keys[i] = new ObjectReferenceKeyframe { time = times[i], value = sprites[i] };

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        // 上書き保存
        AssetDatabase.DeleteAsset(CLIP_PATH);
        AssetDatabase.CreateAsset(clip, CLIP_PATH);

        // ---- AnimatorController を作成 ----
        AssetDatabase.DeleteAsset(CTRL_PATH);
        var ctrl   = AnimatorController.CreateAnimatorControllerAtPath(CTRL_PATH);
        var sm     = ctrl.layers[0].stateMachine;
        var idle   = sm.AddState("Idle");
        idle.motion      = clip;
        sm.defaultState  = idle;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlayerAnimSetup] Done! Player_Idle.anim + Player.controller を生成しました。");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        AssetDatabase.CreateFolder(path[..slash], path[(slash + 1)..]);
    }
}
