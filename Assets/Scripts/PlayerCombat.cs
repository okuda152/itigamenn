using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Attack")]
    public float attackDamage   = 10f;
    public float attackRange    = 1.2f;
    public float attackKnockback = 3f;
    public float attackCooldown = 0.35f;

    [Header("Throw")]
    public float throwDamage   = 15f;
    public float throwForceX   = 7f;
    public float throwForceY   = 9f;
    public float throwCooldown = 0.8f;

    float attackTimer;
    float throwTimer;

    PlayerController ctrl;

    public bool IsAttacking { get; private set; }
    public bool IsThrowing  { get; private set; }

    void Awake() => ctrl = GetComponent<PlayerController>();

    void Update()
    {
        attackTimer -= Time.deltaTime;
        throwTimer  -= Time.deltaTime;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame && attackTimer <= 0f)
            StartCoroutine(AttackRoutine());

        if (kb.qKey.wasPressedThisFrame && throwTimer <= 0f)
            StartCoroutine(ThrowRoutine());
    }

    IEnumerator AttackRoutine()
    {
        attackTimer = attackCooldown;
        IsAttacking = true;

        yield return new WaitForSeconds(0.06f);

        Vector2 dir    = ctrl.FacingRight ? Vector2.right : Vector2.left;
        Vector2 origin = (Vector2)transform.position + dir * (attackRange * 0.55f);
        bool didHit = false;
        foreach (var hit in Physics2D.OverlapCircleAll(origin, attackRange * 0.4f))
        {
            if (hit.gameObject == gameObject) continue;
            var dmg = hit.GetComponent<IDamageable>();
            if (dmg == null) continue;
            dmg.TakeDamage(attackDamage, dir * attackKnockback);
            var enemyRb = hit.GetComponent<Rigidbody2D>();
            if (enemyRb) enemyRb.AddForce(dir * attackKnockback, ForceMode2D.Impulse);
            didHit = true;
        }
        if (didHit)
        {
            EffectManager.HitSpark(origin, new Color(1f, 0.5f, 0.1f));
            EffectManager.HitRing(origin, new Color(1f, 0.7f, 0.2f));
        }

        yield return new WaitForSeconds(0.22f);
        IsAttacking = false;
    }

    IEnumerator ThrowRoutine()
    {
        throwTimer = throwCooldown;
        IsThrowing = true;

        yield return new WaitForSeconds(0.12f);

        float dir = ctrl.FacingRight ? 1f : -1f;
        Vector2 spawn = (Vector2)transform.position + new Vector2(dir * 0.3f, -0.3f);
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
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position + dir * attackRange * 0.55f, attackRange * 0.4f);
    }
}
