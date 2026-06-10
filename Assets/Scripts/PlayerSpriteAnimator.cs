using UnityEngine;

/// <summary>
/// Metroidvania Controller の DrawCharacter プレハブを視覚として使うラッパー。
/// 自前の PlayerController/PlayerCombat の状態を Animator に橋渡しする。
/// </summary>
public class PlayerSpriteAnimator : MonoBehaviour
{
    Animator         anim;
    SpriteRenderer[] renderers;
    PlayerController ctrl;
    PlayerCombat     combat;
    Rigidbody2D      rb;

    // ---- 初期化 ----

    public void Init(string resourcePath, PlayerController controller)
    {
        ctrl   = controller;
        combat = controller.GetComponent<PlayerCombat>();
        rb     = controller.GetComponent<Rigidbody2D>();

        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[PlayerSpriteAnimator] Resources/{resourcePath} が見つかりません");
            return;
        }
        var inst = Instantiate(prefab, transform);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localScale    = Vector3.one;

        // 競合するコンポーネントを即座に無効化
        var pm = inst.GetComponent<PlayerMovement>();
        if (pm) pm.enabled = false;
        var cc = inst.GetComponent<CharacterController2D>();
        if (cc) cc.enabled = false;
        var atk = inst.GetComponent<Attack>();
        if (atk) atk.enabled = false;
        var rb2 = inst.GetComponent<Rigidbody2D>();
        if (rb2) rb2.simulated = false;
        foreach (var c in inst.GetComponents<Collider2D>()) c.enabled = false;

        anim      = inst.GetComponentInChildren<Animator>(true);
        renderers = inst.GetComponentsInChildren<SpriteRenderer>(true);

        if (anim == null)
            Debug.LogError("[PlayerSpriteAnimator] Animator が見つかりません");
    }

    // ---- 毎フレーム ----

    void Update()
    {
        if (anim == null || ctrl == null) return;

        bool isAttacking = combat != null && combat.IsAttacking;

        anim.SetFloat("Speed",         Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool ("IsJumping",     !ctrl.IsGrounded);
        anim.SetBool ("IsWallSliding", ctrl.IsWallSliding);
        anim.SetBool ("IsAttacking",   isAttacking);

        // 向き（壁張り付き中は反転）
        bool faceRight = ctrl.IsWallSliding ? !ctrl.FacingRight : ctrl.FacingRight;
        float sx = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(faceRight ? sx : -sx, sx, sx);
    }

    // ---- ヒットフラッシュ ----

    public void SetColor(Color c)
    {
        foreach (var r in GetComponentsInChildren<SpriteRenderer>(true))
            r.color = c;
    }
}
