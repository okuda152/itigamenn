using UnityEngine;
using System.Collections;

/// <summary>
/// 空中を飛び回りながら弾幕を撃つ蝶ボス。
/// Phase2 (HP 50%以下) で弾速・弾数が増え螺旋弾幕を追加。
/// </summary>
public class ButterflyBoss : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHP = 150f;

    [Header("Flight")]
    public float moveSpeed   = 3.5f;
    public float arenaHalfW  = 7.5f;
    public float arenaHalfH  = 4.0f;
    public float minHoverY   = 0.5f;

    [Header("Bullets")]
    public float bulletSpeed1 = 5.5f;
    public float bulletSpeed2 = 7.5f;
    public float bulletDamage = 8f;
    public Color bulletColor  = new Color(1f, 0.40f, 0.80f);

    public static event System.Action OnDied;

    enum State { Moving, Waiting, Attacking }

    State            state      = State.Waiting;
    float            hp;
    float            waitTimer;
    Vector2          targetPos;
    float            flashTimer;
    bool             phase2     = false;
    bool             dead       = false;

    ButterflyRenderer bfly;
    PlayerHealth      playerHealth;

    static readonly Color BaseColor  = new Color(0.85f, 0.20f, 0.95f);

    // ---- Lifecycle ----

    void Awake()
    {
        hp = maxHP;

        // Flying — no gravity, kinematic movement
        var rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType       = RigidbodyType2D.Kinematic;
        rb.gravityScale   = 0f;
        rb.freezeRotation = true;

        var col  = GetComponent<CapsuleCollider2D>();
        if (col) col.size = new Vector2(0.8f, 0.9f);
    }

    void Start()
    {
        bfly         = GetComponentInChildren<ButterflyRenderer>();
        playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        targetPos    = (Vector2)transform.position;
        waitTimer    = 1.2f;
    }

    void Update()
    {
        if (dead) return;

        // Hit flash
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0f && bfly) bfly.SetColor(BaseColor);
        }

        // Phase 2 transition
        if (!phase2 && hp <= maxHP * 0.5f)
        {
            phase2     = true;
            moveSpeed *= 1.25f;
        }

        switch (state)
        {
            case State.Moving:  DoMove();  break;
            case State.Waiting: DoWait();  break;
            // Attacking is handled by coroutine; no per-frame logic needed
        }
    }

    void DoMove()
    {
        Vector2 cur  = transform.position;
        Vector2 next = Vector2.MoveTowards(cur, targetPos, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(next.x, next.y, 0f);

        if (Vector2.Distance(cur, targetPos) < 0.12f)
        {
            state     = State.Waiting;
            waitTimer = phase2 ? Random.Range(0.3f, 0.7f) : Random.Range(0.7f, 1.3f);
        }
    }

    void DoWait()
    {
        // Gentle hover bob
        float bob = Mathf.Sin(Time.time * 2.5f) * 0.06f;
        transform.position = new Vector3(targetPos.x, targetPos.y + bob, 0f);

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
            StartCoroutine(AttackSequence());
    }

    // ---- Attack State Machine ----

    IEnumerator AttackSequence()
    {
        state = State.Attacking;

        int roll = phase2 ? Random.Range(0, 3) : Random.Range(0, 2);
        switch (roll)
        {
            case 0: yield return StartCoroutine(SpreadAttack()); break;
            case 1: yield return StartCoroutine(CircleBurst());  break;
            case 2: yield return StartCoroutine(SpiralAttack()); break;
        }

        PickNewTarget();
        state = State.Moving;
    }

    IEnumerator SpreadAttack()
    {
        yield return new WaitForSeconds(0.35f);

        int   count  = phase2 ? 9 : 6;
        float spread = 50f;
        float spd    = phase2 ? bulletSpeed2 : bulletSpeed1;

        Vector2 toPlayer = playerHealth
            ? ((Vector2)playerHealth.transform.position - (Vector2)transform.position).normalized
            : Vector2.down;
        float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;

        for (int i = 0; i < count; i++)
        {
            float deg = baseAngle + Mathf.Lerp(-spread * 0.5f, spread * 0.5f,
                                               (float)i / Mathf.Max(1, count - 1));
            Vector2 dir = new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad),
                                      Mathf.Sin(deg * Mathf.Deg2Rad));
            Bullet.Spawn((Vector2)transform.position, dir, bulletColor, spd, bulletDamage);
        }
        EffectManager.HitSpark(transform.position, bulletColor);

        yield return new WaitForSeconds(phase2 ? 0.8f : 1.2f);
    }

    IEnumerator CircleBurst()
    {
        yield return new WaitForSeconds(0.30f);

        int   count = phase2 ? 18 : 12;
        float spd   = (phase2 ? bulletSpeed2 : bulletSpeed1) * 0.85f;

        for (int i = 0; i < count; i++)
        {
            float deg = (float)i / count * 360f;
            Vector2 dir = new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad),
                                      Mathf.Sin(deg * Mathf.Deg2Rad));
            Bullet.Spawn((Vector2)transform.position, dir, bulletColor, spd, bulletDamage);
        }
        EffectManager.HitRing(transform.position, bulletColor);

        yield return new WaitForSeconds(phase2 ? 1.0f : 1.5f);
    }

    IEnumerator SpiralAttack()
    {
        yield return new WaitForSeconds(0.20f);

        float duration  = 2.8f;
        float angleStep = 22f;
        float interval  = 0.07f;
        float angle     = 0f;
        float elapsed   = 0f;
        float nextShot  = 0f;

        while (elapsed < duration)
        {
            elapsed  += Time.deltaTime;
            nextShot -= Time.deltaTime;
            if (nextShot <= 0f)
            {
                nextShot = interval;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad),
                                          Mathf.Sin(angle * Mathf.Deg2Rad));
                Bullet.Spawn((Vector2)transform.position, dir,
                             bulletColor, bulletSpeed2, bulletDamage);
                angle += angleStep;
            }
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
    }

    void PickNewTarget()
    {
        float x = Random.Range(-arenaHalfW * 0.70f, arenaHalfW * 0.70f);
        float y = Random.Range(minHoverY,            arenaHalfH * 0.80f);
        targetPos = new Vector2(x, y);
    }

    // ---- IDamageable ----

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (dead) return;
        hp = Mathf.Max(0f, hp - amount);
        if (bfly) { bfly.SetColor(Color.white); flashTimer = 0.12f; }
        if (hp <= 0f) Die();
    }

    void Die()
    {
        dead = true;
        StopAllCoroutines();
        EffectManager.DeathBurst(transform.position, BaseColor);
        EffectManager.DeathBurst(transform.position + Vector3.right * 0.6f, bulletColor);
        EffectManager.DeathBurst(transform.position + Vector3.left  * 0.6f, bulletColor);
        OnDied?.Invoke();
        gameObject.SetActive(false);
    }

    // ---- HP Bar (OnGUI) ----

    void OnGUI()
    {
        float sw = Screen.width;
        const float panelW = 400f, panelH = 52f;
        float panelX = (sw - panelW) * 0.5f;
        const float panelY = 12f;

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle  = FontStyle.Bold,
            fontSize   = 13,
            normal     = { textColor = new Color(0.95f, 0.80f, 1f) }
        };
        GUI.color = Color.white;
        string label = phase2 ? "B U T T E R F L Y  [ P H A S E  2 ]" : "B U T T E R F L Y";
        GUI.Label(new Rect(panelX, panelY + 4f, panelW, 18f), label, style);

        const float barW = 360f, barH = 14f;
        float barX = (sw - barW) * 0.5f;
        const float barY = panelY + 26f;

        GUI.color = new Color(0.25f, 0.05f, 0.30f);
        GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);
        GUI.color = new Color(0.85f, 0.20f, 0.95f);
        GUI.DrawTexture(new Rect(barX, barY, barW * (hp / maxHP), barH), Texture2D.whiteTexture);

        GUI.color = new Color(1f, 1f, 1f, 0.25f);
        GUI.DrawTexture(new Rect(barX - 1,    barY - 1,    barW + 2, 1),        Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barX - 1,    barY + barH, barW + 2, 1),        Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barX - 1,    barY - 1,    1,        barH + 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barX + barW, barY - 1,    1,        barH + 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
