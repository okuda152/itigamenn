using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public enum MovementAbility { None, SpeedBoost, DoubleJump }
public enum SpecialAbility  { None, SubspaceTackle, BulletBarrage }

public class AbilityManager : MonoBehaviour
{
    public const int MoveSlotCount = 2;

    MovementAbility[] moveSlots = { MovementAbility.None, MovementAbility.None };
    SpecialAbility    specialSlot = SpecialAbility.None;

    PlayerController ctrl;
    Rigidbody2D      rb;
    bool             doubleJumpUsed = false;
    float            specialCooldown = 0f;
    const float      TACKLE_CD = 10f;
    const float      BARRAGE_CD = 2f;

    public MovementAbility[] MoveSlots   => moveSlots;
    public SpecialAbility    SpecialSlot => specialSlot;
    public float             SpecialCooldown => specialCooldown;
    public float             MaxSpecialCooldown =>
        specialSlot == SpecialAbility.SubspaceTackle ? TACKLE_CD : BARRAGE_CD;

    void Awake()
    {
        ctrl = GetComponent<PlayerController>();
        rb   = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (specialCooldown > 0f) specialCooldown -= Time.deltaTime;

        var kb = Keyboard.current;
        if (kb == null) return;
        if (kb.rKey.wasPressedThisFrame && specialCooldown <= 0f && specialSlot != SpecialAbility.None)
            TriggerSpecial();
    }

    // PlayerController から呼ぶ：二段ジャンプ試行
    public bool TryDoubleJump()
    {
        foreach (var s in moveSlots)
            if (s == MovementAbility.DoubleJump && !doubleJumpUsed)
            { doubleJumpUsed = true; return true; }
        return false;
    }

    public void ResetDoubleJump() => doubleJumpUsed = false;

    // ---- スロット管理 ----

    public bool HasEmptyMoveSlot()
    {
        foreach (var s in moveSlots) if (s == MovementAbility.None) return true;
        return false;
    }

    public void AddMovementAbility(MovementAbility ability)
    {
        for (int i = 0; i < MoveSlotCount; i++)
        {
            if (moveSlots[i] == MovementAbility.None)
            { moveSlots[i] = ability; ApplyMove(ability); return; }
        }
    }

    public void ReplaceMovementAbility(int slot, MovementAbility newAbility)
    {
        UnapplyMove(moveSlots[slot]);
        moveSlots[slot] = newAbility;
        ApplyMove(newAbility);
    }

    public void SetSpecialAbility(SpecialAbility ability) => specialSlot = ability;

    void ApplyMove(MovementAbility a)
    {
        if (a == MovementAbility.SpeedBoost) ctrl.SpeedMultiplier = Mathf.Min(ctrl.SpeedMultiplier * 1.5f, 4f);
    }

    void UnapplyMove(MovementAbility a)
    {
        if (a == MovementAbility.SpeedBoost) ctrl.SpeedMultiplier = Mathf.Max(ctrl.SpeedMultiplier / 1.5f, 1f);
    }

    // ---- 必殺技 ----

    void TriggerSpecial()
    {
        switch (specialSlot)
        {
            case SpecialAbility.SubspaceTackle: StartCoroutine(DoTackle()); break;
            case SpecialAbility.BulletBarrage:  DoBarrage();                break;
        }
    }

    IEnumerator DoTackle()
    {
        specialCooldown = TACKLE_CD;
        var ph = GetComponent<PlayerHealth>();
        if (ph) ph.SetInvincible(0.5f);

        float dir = ctrl.FacingRight ? 1f : -1f;
        ctrl.LockMovement = true;
        rb.linearVelocity = new Vector2(dir * 24f, rb.linearVelocity.y * 0.2f);
        EffectManager.HitSpark((Vector2)transform.position, new Color(0.5f, 0.1f, 1f));

        var hit = new HashSet<GameObject>();
        float elapsed = 0f;
        while (elapsed < 0.3f)
        {
            elapsed += Time.deltaTime;
            foreach (var c in Physics2D.OverlapCircleAll((Vector2)transform.position, 4.0f))
            {
                if (c.gameObject == gameObject || hit.Contains(c.gameObject)) continue;
                var dmg = c.GetComponent<IDamageable>();
                if (dmg == null) continue;
                dmg.TakeDamage(18f, Vector2.right * dir * 10f);
                EffectManager.HitSpark((Vector2)c.transform.position, Color.white);
                EffectManager.HitRing((Vector2)c.transform.position, new Color(0.5f, 0.1f, 1f));
                hit.Add(c.gameObject);
            }
            yield return null;
        }
        ctrl.LockMovement = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.35f, rb.linearVelocity.y);
    }

    void DoBarrage()
    {
        specialCooldown = BARRAGE_CD;
        float dir = ctrl.FacingRight ? 1f : -1f;
        Vector2 origin = (Vector2)transform.position + new Vector2(dir * 0.4f, 0f);
        var myCol = GetComponent<Collider2D>();

        float[] angles = { 0f, 14f, -14f, 28f, -28f };
        foreach (float a in angles)
        {
            float rad = a * Mathf.Deg2Rad;
            Vector2 vel = new Vector2(dir * Mathf.Cos(rad), Mathf.Sin(rad)) * 16f;
            PlayerBullet.Spawn(origin, vel, 8f, myCol);
        }
        EffectManager.HitSpark(origin, new Color(1f, 0.85f, 0.1f));
    }

    // ---- HUD ----

    void OnGUI()
    {
        float sw = Screen.width;

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 10,
            alignment = TextAnchor.MiddleLeft,
            normal    = { textColor = new Color(0.7f, 0.7f, 0.7f) }
        };

        float x = 20f;
        float y = 20f;
        float slotW = 100f;
        float slotH = 36f;
        float gap = 6f;

        // 移動スロット
        GUI.Label(new Rect(x, y, 60f, 16f), "移動強化", titleStyle);
        for (int i = 0; i < MoveSlotCount; i++)
        {
            float sx = x + i * (slotW + gap);
            Color bg = moveSlots[i] != MovementAbility.None
                ? new Color(0.2f, 0.5f, 1f, 0.7f)
                : new Color(0.2f, 0.2f, 0.2f, 0.55f);
            GUI.color = bg;
            GUI.DrawTexture(new Rect(sx, y + 18f, slotW, slotH), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(sx, y + 18f, slotW, slotH), MoveName(moveSlots[i]), labelStyle);
        }

        // 必殺技スロット
        float rx = x + MoveSlotCount * (slotW + gap) + 18f;
        GUI.Label(new Rect(rx, y, 80f, 16f), "必殺技 [R]", titleStyle);

        Color spBg = specialSlot != SpecialAbility.None
            ? new Color(0.7f, 0.1f, 0.9f, 0.7f)
            : new Color(0.2f, 0.2f, 0.2f, 0.55f);
        GUI.color = spBg;
        GUI.DrawTexture(new Rect(rx, y + 18f, slotW, slotH), Texture2D.whiteTexture);
        GUI.color = Color.white;

        string spLabel = SpecialName(specialSlot);
        if (specialSlot != SpecialAbility.None && specialCooldown > 0f)
        {
            float pct = 1f - specialCooldown / MaxSpecialCooldown;
            GUI.color = new Color(0.4f, 0.4f, 0.4f, 0.8f);
            GUI.DrawTexture(new Rect(rx, y + 18f + slotH * pct, slotW, slotH * (1f - pct)), Texture2D.whiteTexture);
            GUI.color = Color.white;
            spLabel = $"CT {specialCooldown:F1}s";
        }
        GUI.Label(new Rect(rx, y + 18f, slotW, slotH), spLabel, labelStyle);
    }

    public static string MoveName(MovementAbility a) => a switch
    {
        MovementAbility.SpeedBoost => "速度強化",
        MovementAbility.DoubleJump => "二段ジャンプ",
        _                          => "（空）"
    };

    public static string SpecialName(SpecialAbility a) => a switch
    {
        SpecialAbility.SubspaceTackle => "亜空間タックル",
        SpecialAbility.BulletBarrage  => "弾幕攻撃",
        _                             => "（空）"
    };
}
