using UnityEngine;
using System.Collections;

public class PlayerSpriteAnimator : MonoBehaviour
{
    Animator         anim;
    SpriteRenderer   mainRenderer;
    Material         instanceMat;
    PlayerController ctrl;
    PlayerCombat     combat;
    Rigidbody2D      rb;

    public void Init(string resourcePath, PlayerController controller)
    {
        ctrl   = controller;
        combat = controller.GetComponent<PlayerCombat>();
        rb     = controller.GetComponent<Rigidbody2D>();

        var prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null) { Debug.LogError($"[PSA] prefab not found: {resourcePath}"); return; }

        var inst = Instantiate(prefab, transform);
        inst.transform.localPosition = Vector3.zero;
        inst.transform.localScale    = Vector3.one;

        var pm  = inst.GetComponent<PlayerMovement>();        if (pm)  pm.enabled  = false;
        var cc  = inst.GetComponent<CharacterController2D>(); if (cc)  cc.enabled  = false;
        var atk = inst.GetComponent<Attack>();                if (atk) atk.enabled = false;
        var rb2 = inst.GetComponent<Rigidbody2D>();           if (rb2) rb2.simulated = false;
        foreach (var c in inst.GetComponents<Collider2D>()) c.enabled = false;

        anim         = inst.GetComponentInChildren<Animator>(true);
        mainRenderer = inst.GetComponentInChildren<SpriteRenderer>(true);

        // マテリアルインスタンスを作成（sharedMaterial を汚さない）
        if (mainRenderer != null)
            instanceMat = new Material(mainRenderer.sharedMaterial);
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

    public void Flash(Color c, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(c, duration));
    }

    IEnumerator FlashRoutine(Color c, float duration)
    {
        // Animator を止めて色変化を妨害させない
        if (anim) anim.enabled = false;

        ApplyColor(c);

        yield return new WaitForSeconds(duration);

        ApplyColor(Color.white);

        if (anim) anim.enabled = true;
    }

    void ApplyColor(Color c)
    {
        if (mainRenderer == null) return;

        // 1. SpriteRenderer.color（頂点カラー）
        mainRenderer.color = c;

        // 2. マテリアルインスタンスの _Color
        if (instanceMat != null)
        {
            instanceMat.color = c;
            mainRenderer.material = instanceMat;
        }
    }

    public void SetColor(Color c) { }
}
