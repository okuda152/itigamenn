using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Punch")]
    public float punchDamage   = 10f;
    public float punchRange    = 1.2f;
    public float punchCooldown = 0.35f;

    [Header("Kick")]
    public float kickDamage    = 20f;
    public float kickRange     = 1.5f;
    public float kickKnockback = 6f;
    public float kickCooldown  = 0.5f;

    [Header("Throw")]
    public float throwDamage   = 15f;
    public float throwForceX   = 7f;
    public float throwForceY   = 9f;
    public float throwCooldown = 0.8f;

    float punchTimer;
    float kickTimer;
    float throwTimer;

    PlayerController ctrl;

    public bool IsPunching  { get; private set; }
    public bool IsKicking   { get; private set; }
    public bool IsThrowing  { get; private set; }

    void Awake() => ctrl = GetComponent<PlayerController>();

    void Update()
    {
        punchTimer -= Time.deltaTime;
        kickTimer  -= Time.deltaTime;
        throwTimer -= Time.deltaTime;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.zKey.wasPressedThisFrame && punchTimer <= 0f)
            StartCoroutine(PunchRoutine());

        if (kb.eKey.wasPressedThisFrame && kickTimer <= 0f)
            StartCoroutine(KickRoutine());

        if (kb.qKey.wasPressedThisFrame && throwTimer <= 0f)
            StartCoroutine(ThrowRoutine());
    }

    IEnumerator PunchRoutine()
    {
        punchTimer = punchCooldown;
        IsPunching = true;

        yield return new WaitForSeconds(0.08f);

        Vector2 dir    = ctrl.FacingRight ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position + dir * (punchRange * 0.5f);
        bool didHit = false;
        foreach (var hit in Physics2D.OverlapCircleAll(origin, punchRange * 0.5f))
        {
            if (hit.gameObject == gameObject) continue;
            if (hit.GetComponent<IDamageable>() == null) continue;
            hit.GetComponent<IDamageable>().TakeDamage(punchDamage, Vector2.zero);
            didHit = true;
        }
        if (didHit)
        {
            EffectManager.HitSpark(origin, new Color(1f, 0.9f, 0.2f));
            EffectManager.HitRing(origin, Color.white);
        }

        yield return new WaitForSeconds(0.18f);
        IsPunching = false;
    }

    IEnumerator KickRoutine()
    {
        kickTimer = kickCooldown;
        IsKicking = true;

        yield return new WaitForSeconds(0.06f);

        Vector2 dir    = ctrl.FacingRight ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position + dir * (kickRange * 0.55f);
        bool didHit = false;
        foreach (var hit in Physics2D.OverlapCircleAll(origin, kickRange * 0.4f))
        {
            if (hit.gameObject == gameObject) continue;
            var dmg = hit.GetComponent<IDamageable>();
            if (dmg == null) continue;
            dmg.TakeDamage(kickDamage, dir * kickKnockback);
            var enemyRb = hit.GetComponent<Rigidbody2D>();
            if (enemyRb) enemyRb.AddForce(dir * kickKnockback, ForceMode2D.Impulse);
            didHit = true;
        }
        if (didHit)
        {
            EffectManager.HitSpark(origin, new Color(1f, 0.5f, 0.1f));
            EffectManager.HitRing(origin, new Color(1f, 0.7f, 0.2f));
        }

        yield return new WaitForSeconds(0.22f);
        IsKicking = false;
    }

    IEnumerator ThrowRoutine()
    {
        throwTimer = throwCooldown;
        IsThrowing = true;

        // 溜めモーション
        yield return new WaitForSeconds(0.12f);

        float dir = ctrl.FacingRight ? 1f : -1f;

        // 手のワールド座標を直接取得（あれば）
        var fig = GetComponentInChildren<StickFigureRenderer>();
        Vector2 spawn = fig != null
            ? (Vector2)fig.ThrowHandWorld + new Vector2(dir * 0.05f, 0f)
            : (Vector2)transform.position + new Vector2(dir * 0.3f, -0.3f);

        Vector2 vel   = new Vector2(dir * throwForceX, throwForceY);
        var     myCol = GetComponent<Collider2D>();
        Stone.Spawn(spawn, vel, throwDamage, myCol);

        yield return new WaitForSeconds(0.28f);
        IsThrowing = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!ctrl) ctrl = GetComponent<PlayerController>();
        Vector3 dir = ctrl && ctrl.FacingRight ? Vector3.right : Vector3.left;
        Gizmos.color = new Color(0f, 1f, 1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position + dir * punchRange * 0.5f, punchRange * 0.5f);
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position + dir * kickRange * 0.55f, kickRange * 0.4f);
    }
}
