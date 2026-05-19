using UnityEngine;

/// <summary>
/// フレームアニメーション方式のプレイヤー描画。
/// Animator + Player.controller で idle_00〜07 を切り替える。
/// PlayerController.FacingRight に応じてスプライトを左右反転。
/// </summary>
public class FrameAnimPlayer : MonoBehaviour
{
    SpriteRenderer  sr;
    Animator        anim;
    PlayerController ctrl;

    void Start()
    {
        sr   = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        ctrl = GetComponent<PlayerController>();

        // Resources/Animations/Player.controller をロード
        var controller = Resources.Load<RuntimeAnimatorController>("Animations/Player");
        if (controller != null)
            anim.runtimeAnimatorController = controller;
        else
            Debug.LogWarning("[FrameAnimPlayer] Player.controller が見つかりません。" +
                             " Tools > Setup Player Idle Anim を先に実行してください。");
    }

    void LateUpdate()
    {
        // 向きに応じて左右反転（物理には影響しない）
        if (ctrl != null) sr.flipX = !ctrl.FacingRight;
    }

    /// <summary>被弾フラッシュ (PlayerHealth から呼ばれる)</summary>
    public void SetColor(Color c) => sr.color = c;
}
