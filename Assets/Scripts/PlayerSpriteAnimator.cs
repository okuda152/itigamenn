using UnityEngine;

/// <summary>
/// Metroidvania Controller の DrawCharacter プレハブを視覚として使うラッパー。
/// 自前の PlayerController の状態を Animator に橋渡しする。
/// </summary>
public class PlayerSpriteAnimator : MonoBehaviour
{
    Animator         anim;
    SpriteRenderer[] renderers;
    PlayerController ctrl;
    Rigidbody2D      rb;

    // ---- 初期化 ----

    public void Init(string resourcePath, PlayerController controller)
    {
        ctrl = controller;
        rb   = controller.GetComponent<Rigidbody2D>();

        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"[PlayerSpriteAnimator] Resources/{resourcePath} が見つかりません");
            return;
        }
        var inst = Instantiate(prefab, transform);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localScale    = Vector3.one;

        // アセット付属のコンポーネントを削除（競合防止）
        Destroy(inst.GetComponent<PlayerMovement>());
        Destroy(inst.GetComponent<CharacterController2D>());
        Destroy(inst.GetComponent<Attack>());
        var rb2 = inst.GetComponent<Rigidbody2D>();
        if (rb2) Destroy(rb2);
        foreach (var c in inst.GetComponents<Collider2D>()) Destroy(c);

        anim      = inst.GetComponent<Animator>();
        renderers = inst.GetComponentsInChildren<SpriteRenderer>(true);
    }

    // ---- 毎フレーム ----

    void Update()
    {
        if (anim == null || ctrl == null) return;

        anim.SetFloat("Speed",     Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool ("IsJumping", !ctrl.IsGrounded);

        // 向き
        float sx = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(ctrl.FacingRight ? sx : -sx, sx, sx);
    }

    // ---- ヒットフラッシュ ----

    public void SetColor(Color c)
    {
        if (renderers == null) return;
        foreach (var r in renderers) r.color = c;
    }
}
