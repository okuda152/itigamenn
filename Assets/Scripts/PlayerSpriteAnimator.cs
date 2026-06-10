using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    Animator         anim;
    PlayerController ctrl;
    PlayerCombat     combat;
    Rigidbody2D      rb;

    Color  flashColor = Color.white;
    float  flashTimer = 0f;

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

        var pm = inst.GetComponent<PlayerMovement>();
        if (pm) pm.enabled = false;
        var cc = inst.GetComponent<CharacterController2D>();
        if (cc) cc.enabled = false;
        var atk = inst.GetComponent<Attack>();
        if (atk) atk.enabled = false;
        var rb2 = inst.GetComponent<Rigidbody2D>();
        if (rb2) rb2.simulated = false;
        foreach (var c in inst.GetComponents<Collider2D>()) c.enabled = false;

        anim = inst.GetComponentInChildren<Animator>(true);
        if (anim == null)
            Debug.LogError("[PlayerSpriteAnimator] Animator が見つかりません");
    }

    void Update()
    {
        if (anim == null || ctrl == null) return;

        bool isAttacking = combat != null && combat.IsAttacking;

        anim.SetFloat("Speed",         Mathf.Abs(rb.linearVelocity.x));
        anim.SetBool ("IsJumping",     !ctrl.IsGrounded);
        anim.SetBool ("IsWallSliding", ctrl.IsWallSliding);
        anim.SetBool ("IsAttacking",   isAttacking);

        bool faceRight = ctrl.IsWallSliding ? !ctrl.FacingRight : ctrl.FacingRight;
        float sx = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(faceRight ? sx : -sx, sx, sx);

        if (flashTimer > 0f) flashTimer -= Time.deltaTime;
    }

    // Animator の更新後に色を上書きする
    void LateUpdate()
    {
        Color target = flashTimer > 0f ? flashColor : Color.white;
        foreach (var r in GetComponentsInChildren<SpriteRenderer>(true))
            r.color = target;
    }

    // PlayerHealth から呼ぶ: c=赤で開始、c=white でリセット
    public void Flash(Color c, float duration)
    {
        flashColor = c;
        flashTimer = duration;
    }

    // 後方互換
    public void SetColor(Color c) { }
}
