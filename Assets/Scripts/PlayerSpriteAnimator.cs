using UnityEngine;
using System.Collections;

public class PlayerSpriteAnimator : MonoBehaviour
{
    Animator         anim;
    SpriteRenderer   mainRenderer;
    SpriteRenderer   hitOverlay;
    PlayerController ctrl;
    PlayerCombat     combat;
    Rigidbody2D      rb;

    bool isFlashing = false;

    public void Init(string resourcePath, PlayerController controller)
    {
        ctrl   = controller;
        combat = controller.GetComponent<PlayerCombat>();
        rb     = controller.GetComponent<Rigidbody2D>();

        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null) { Debug.LogError($"[PSA] not found: {resourcePath}"); return; }

        var inst = Instantiate(prefab, transform);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localScale    = Vector3.one;

        var pm  = inst.GetComponent<PlayerMovement>();        if (pm)  pm.enabled    = false;
        var cc  = inst.GetComponent<CharacterController2D>(); if (cc)  cc.enabled    = false;
        var atk = inst.GetComponent<Attack>();                if (atk) atk.enabled   = false;
        var rb2 = inst.GetComponent<Rigidbody2D>();           if (rb2) rb2.simulated = false;
        foreach (var c in inst.GetComponents<Collider2D>()) c.enabled = false;

        anim         = inst.GetComponentInChildren<Animator>(true);
        mainRenderer = inst.GetComponentInChildren<SpriteRenderer>(true);

        // キャラ上に重ねる赤オーバーレイ用 SpriteRenderer を作成
        if (mainRenderer != null)
        {
            var overlayGO = new GameObject("HitOverlay");
            overlayGO.transform.SetParent(mainRenderer.transform);
            overlayGO.transform.localPosition = Vector3.zero;
            overlayGO.transform.localScale    = Vector3.one;

            hitOverlay              = overlayGO.AddComponent<SpriteRenderer>();
            hitOverlay.sprite       = mainRenderer.sprite;
            hitOverlay.sortingLayerID = mainRenderer.sortingLayerID;
            hitOverlay.sortingOrder = mainRenderer.sortingOrder + 1;
            hitOverlay.color        = new Color(1f, 0.1f, 0.1f, 0f); // 初期は透明
        }
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
    }

    void LateUpdate()
    {
        if (hitOverlay == null || mainRenderer == null) return;
        // Animator がスプライトを切り替えても追従する
        hitOverlay.sprite = mainRenderer.sprite;
        hitOverlay.color  = isFlashing
            ? new Color(1f, 0.1f, 0.1f, 0.85f)
            : new Color(1f, 0.1f, 0.1f, 0f);
    }

    public void Flash(Color c, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FlashCo(duration));
    }

    IEnumerator FlashCo(float duration)
    {
        isFlashing = true;
        yield return new WaitForSeconds(duration);
        isFlashing = false;
    }

    public void SetColor(Color c) { }
}
