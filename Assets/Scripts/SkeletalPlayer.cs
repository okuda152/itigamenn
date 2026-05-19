using UnityEngine;

/// <summary>
/// Resources/Sprites/Player/ の10パーツPNGを読み込み、
/// スケルタル階層を構築して Idle アニメを再生する。
/// ArenaBuilder から子オブジェクトとして生成される。
/// </summary>
public class SkeletalPlayer : MonoBehaviour
{
    const string BASE = "Sprites/Player/";

    void Start()
    {
        BuildHierarchy();
        SetupIdleAnimation();
    }

    // ---- 階層構築 ----

    void BuildHierarchy()
    {
        // Player (this.gameObject)
        // └── Torso
        //     ├── Head
        //     ├── ArmL_Upper  ← sortingOrder -1 (背面)
        //     │   └── ArmL_Lower
        //     ├── ArmR_Upper  ← sortingOrder  2 (前面)
        //     │   └── ArmR_Lower
        //     ├── LegL_Upper
        //     │   └── LegL_Lower
        //     └── LegR_Upper
        //         └── LegR_Lower

        var torso   = Make("Torso",       transform,       "02_torso",        0);
        /*         */ Make("Head",        torso.transform, "01_head",          1);

        var armLU   = Make("ArmL_Upper",  torso.transform, "03_arm_L_upper",  -1);
        /*         */ Make("ArmL_Lower",  armLU.transform, "04_arm_L_lower",  -1);

        var armRU   = Make("ArmR_Upper",  torso.transform, "05_arm_R_upper",   2);
        /*         */ Make("ArmR_Lower",  armRU.transform, "06_arm_R_lower",   2);

        var legLU   = Make("LegL_Upper",  torso.transform, "07_leg_L_upper",   1);
        /*         */ Make("LegL_Lower",  legLU.transform, "08_leg_L_lower",   1);

        var legRU   = Make("LegR_Upper",  torso.transform, "09_leg_R_upper",   1);
        /*         */ Make("LegR_Lower",  legRU.transform, "10_leg_R_lower",   1);
    }

    // ---- Idle アニメ (Legacy Animation) ----

    void SetupIdleAnimation()
    {
        var clip       = new AnimationClip();
        clip.legacy    = true;
        clip.wrapMode  = WrapMode.Loop;
        clip.name      = "Idle";

        // ArmL_Upper: 0° → +3° → 0° → -3° → 0°  (2秒ループ)
        // ArmR_Upper: 逆位相
        clip.SetCurve("Torso/ArmL_Upper", typeof(Transform),
                      "localEulerAngles.z", SinCurve( 3f));
        clip.SetCurve("Torso/ArmR_Upper", typeof(Transform),
                      "localEulerAngles.z", SinCurve(-3f));

        var anim  = gameObject.AddComponent<Animation>();
        anim.AddClip(clip, "Idle");
        anim.clip = clip;
        anim.Play("Idle");
    }

    // ---- ユーティリティ ----

    /// <summary>amplitude の振れ幅で 2秒sin波カーブを生成</summary>
    static AnimationCurve SinCurve(float amplitude)
    {
        var curve = new AnimationCurve(
            new Keyframe(0.0f,  0f),
            new Keyframe(0.5f,  amplitude),
            new Keyframe(1.0f,  0f),
            new Keyframe(1.5f, -amplitude),
            new Keyframe(2.0f,  0f)
        );
        for (int i = 0; i < curve.length; i++)
            curve.SmoothTangents(i, 0);
        return curve;
    }

    /// <summary>GameObject を生成して SpriteRenderer を付ける</summary>
    static GameObject Make(string goName, Transform parent, string spriteName, int order)
    {
        var sp = Resources.Load<Sprite>(BASE + spriteName);
        if (sp == null)
            Debug.LogWarning($"[SkeletalPlayer] Sprite not found: {BASE + spriteName}");

        var go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale    = Vector3.one;
        go.transform.localRotation = Quaternion.identity;

        var sr          = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sp;
        sr.sortingOrder = order;
        return go;
    }

    /// <summary>被弾フラッシュ用 (PlayerHealth から呼ばれる)</summary>
    public void SetColor(Color c)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.color = c;
    }
}
