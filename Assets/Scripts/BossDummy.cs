using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class BossDummy : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float maxHP = 100f;

    [Header("Patrol")]
    public float patrolSpeed = 3f;

    [Header("Charge")]
    public float chargeSpeed       = 16f;
    public float chargeDetectRange = 9f;
    public float windUpDuration    = 0.7f;
    public float chargeDuration    = 0.9f;
    public float cooldownDuration  = 1.8f;
    public float chargeDamage      = 15f;

    enum State { Patrol, WindUp, Charging, Cooldown }
    State state = State.Patrol;
    float stateTimer;
    float chargeDir;

    float hp;
    float flashTimer;
    float chargePuffTimer;
    Rigidbody2D rb;
    FantasyCharacterVisual dino;
    PlayerHealth playerHealth;
    int patrolDir = 1;
    bool dealtDamageThisCharge;

    static readonly Color BaseColor = new Color(0.18f, 0.52f, 0.18f);

    void Awake()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale   = 3f;
    }

    void Start()
    {
        dino         = GetComponentInChildren<FantasyCharacterVisual>();
        playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
        stateTimer   = Random.Range(1f, 2.5f);
    }

    void Update()
    {
        if (flashTimer <= 0f) return;
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f && dino) dino.SetColor(Color.white);
    }

    void FixedUpdate()
    {
        stateTimer -= Time.fixedDeltaTime;

        switch (state)
        {
            case State.Patrol:    DoPatrol();    break;
            case State.WindUp:    DoWindUp();    break;
            case State.Charging:  DoCharge();    break;
            case State.Cooldown:  DoCooldown();  break;
        }
    }

    // ---- 状態ごとの処理 ----

    void DoPatrol()
    {
        rb.linearVelocity = new Vector2(patrolDir * patrolSpeed, rb.linearVelocity.y);
        if (dino) { dino.FacingRight = patrolDir > 0; dino.IsMoving = true; }

        if (stateTimer <= 0f && PlayerInRange(chargeDetectRange))
            EnterWindUp();
    }

    void DoWindUp()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        float shake = Mathf.Sin(Time.time * 40f) * 0.04f;
        rb.MovePosition(rb.position + new Vector2(shake * Time.fixedDeltaTime, 0f));

        if (stateTimer <= 0f) EnterCharge();
    }

    void DoCharge()
    {
        rb.linearVelocity = new Vector2(chargeDir * chargeSpeed, rb.linearVelocity.y);

        chargePuffTimer -= Time.fixedDeltaTime;
        if (chargePuffTimer <= 0f)
        {
            chargePuffTimer = 0.06f;
            EffectManager.ChargePuff(transform.position, chargeDir > 0, BaseColor);
        }

        if (!dealtDamageThisCharge && playerHealth != null)
        {
            float dist = Mathf.Abs(transform.position.x - playerHealth.transform.position.x);
            if (dist < 1.0f)
            {
                playerHealth.TakeDamage(chargeDamage);
                dealtDamageThisCharge = true;
            }
        }

        // 突進中に PlayerMinion を薙ぎ倒す
        foreach (var pm in FindObjectsByType<PlayerMinion>(FindObjectsSortMode.None))
        {
            float dist = Mathf.Abs(transform.position.x - pm.transform.position.x);
            if (dist < 1.2f)
                pm.TakeDamage(999f, Vector2.zero);
        }

        if (stateTimer <= 0f) EnterCooldown();
    }

    void DoCooldown()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        if (dino) dino.IsMoving = false;
        if (stateTimer <= 0f) EnterPatrol();
    }

    // ---- 状態遷移 ----

    void EnterWindUp()
    {
        state      = State.WindUp;
        stateTimer = windUpDuration;
        if (playerHealth != null)
            chargeDir = playerHealth.transform.position.x > transform.position.x ? 1f : -1f;
        if (dino) { dino.FacingRight = chargeDir > 0; dino.IsMoving = false; dino.TriggerAttack(); }
        EffectManager.FocusLines(transform.position, windUpDuration);
    }

    void EnterCharge()
    {
        state                  = State.Charging;
        stateTimer             = chargeDuration;
        dealtDamageThisCharge  = false;
        if (dino) dino.IsMoving = true;
    }

    void EnterCooldown()
    {
        state      = State.Cooldown;
        stateTimer = cooldownDuration;
        if (dino) dino.IsMoving = false;
    }

    void EnterPatrol()
    {
        state      = State.Patrol;
        stateTimer = Random.Range(1.5f, 3f);
        patrolDir  = -(int)Mathf.Sign(chargeDir);
        if (dino) dino.IsMoving = true;
    }

    // ---- 壁で反転 / チャージ中断 ----

    void OnCollisionEnter2D(Collision2D col)
    {
        foreach (var contact in col.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if (state == State.Charging)
                    EnterCooldown();
                else
                    patrolDir = contact.normal.x > 0f ? 1 : -1;
                break;
            }
        }
    }

    // ---- Damage ----

    public void TakeDamage(float amount, Vector2 knockback)
    {
        hp = Mathf.Max(0f, hp - amount);
        if (dino) { dino.SetColor(Color.white); flashTimer = 0.12f; }
        if (hp <= 0f) Die();
    }

    public static event System.Action OnDied;

    void Die()
    {
        EffectManager.DeathBurst(transform.position, BaseColor);
        OnDied?.Invoke();
        gameObject.SetActive(false);
    }

    bool PlayerInRange(float range) =>
        playerHealth != null &&
        Mathf.Abs(transform.position.x - playerHealth.transform.position.x) < range;

    // ---- HP UI ----

    void OnGUI()
    {
        float sw = Screen.width;
        const float panelW = 400f, panelH = 52f;
        float panelX = (sw - panelW) * 0.5f;
        const float panelY = 12f;

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(panelX, panelY, panelW, panelH), Texture2D.whiteTexture);

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle  = FontStyle.Bold,
            fontSize   = 13,
            normal     = { textColor = new Color(1f, 0.85f, 0.85f) }
        };
        GUI.color = Color.white;
        GUI.Label(new Rect(panelX, panelY + 4f, panelW, 18f), "B O S S", labelStyle);

        const float barW = 360f, barH = 14f;
        float barX = (sw - barW) * 0.5f;
        const float barY = panelY + 26f;

        GUI.color = new Color(0.35f, 0.05f, 0.05f);
        GUI.DrawTexture(new Rect(barX, barY, barW, barH), Texture2D.whiteTexture);
        GUI.color = new Color(0.9f, 0.15f, 0.15f);
        GUI.DrawTexture(new Rect(barX, barY, barW * (hp / maxHP), barH), Texture2D.whiteTexture);

        GUI.color = new Color(1f, 1f, 1f, 0.25f);
        GUI.DrawTexture(new Rect(barX - 1,    barY - 1,    barW + 2, 1),        Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barX - 1,    barY + barH, barW + 2, 1),        Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barX - 1,    barY - 1,    1,        barH + 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(barX + barW, barY - 1,    1,        barH + 2), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
