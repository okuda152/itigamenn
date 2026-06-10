using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    Animator         anim;
    SpriteRenderer   mainRenderer;
    PlayerController ctrl;
    PlayerCombat     combat;
    Rigidbody2D      rb;

    Color flashColor = Color.white;
    float flashTimer = 0f;

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

        var pm = inst.GetComponent<PlayerMovement>();        if (pm)  pm.enabled = false;
        var cc = inst.GetComponent<CharacterController2D>(); if (cc)  cc.enabled = false;
        var atk = inst.GetComponent<Attack>();               if (atk) atk.enabled = false;
        var rb2 = inst.GetComponent<Rigidbody2D>();          if (rb2) rb2.simulated = false;
        foreach (var c in inst.GetComponents<Collider2D>()) c.enabled = false;

        anim         = inst.GetComponentInChildren<Animator>(true);
        mainRenderer = inst.GetComponentInChildren<SpriteRenderer>(true);

        if (anim == null)         Debug.LogError("[PlayerSpriteAnimator] Animator が見つかりません");
        if (mainRenderer == null) Debug.LogError("[PlayerSpriteAnimator] SpriteRenderer が見つかりません");
        else                      Debug.Log($"[PlayerSpriteAnimator] SpriteRenderer 取得: {mainRenderer.gameObject.name}");
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

    // Animator 更新後に色を強制上書き
    void LateUpdate()
    {
        if (mainRenderer == null) return;
        mainRenderer.color = flashTimer > 0f ? flashColor : Color.white;
    }

    public void Flash(Color c, float duration)
    {
        flashColor = c;
        flashTimer = duration;
        Debug.Log($"[PlayerSpriteAnimator] Flash called: {c}, {duration}s, renderer={mainRenderer != null}");
    }

    public void SetColor(Color c) { }
}
