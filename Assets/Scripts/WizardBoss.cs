using UnityEngine;
using System;
using System.Collections;

public class WizardBoss : MonoBehaviour, IDamageable
{
    public static event Action OnDied;

    public float maxHP      = 250f;
    public float arenaHalfW = 8f;
    public float arenaHalfH = 4f;

    float hp;
    bool  dead = false;
    Rigidbody2D rb;

    Vector2 targetPos;
    float   moveTimer   = 0f;
    float   summonTimer = 0f;

    const float MOVE_SPEED       = 2.5f;
    const float MOVE_INTERVAL    = 2.2f;
    const float SUMMON_INTERVAL  = 2.5f;
    const int   MAX_MINIONS      = 6;

    void Awake()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody2D>();
        PickNewTarget();
        summonTimer = 1.5f;  // 少し間を置いてから最初の召喚
    }

    void Update()
    {
        if (dead) return;

        // 移動
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f) PickNewTarget();

        Vector2 toTarget = targetPos - (Vector2)transform.position;
        rb.linearVelocity = toTarget.magnitude > 0.3f
            ? toTarget.normalized * MOVE_SPEED
            : Vector2.zero;

        // 召喚
        summonTimer -= Time.deltaTime;
        if (summonTimer <= 0f)
        {
            summonTimer = SUMMON_INTERVAL;
            if (MinionCount() < MAX_MINIONS)
                SummonMinion();
        }
    }

    void PickNewTarget()
    {
        moveTimer = MOVE_INTERVAL;
        targetPos = new Vector2(
            UnityEngine.Random.Range(-arenaHalfW * 0.65f, arenaHalfW * 0.65f),
            UnityEngine.Random.Range(0.5f, arenaHalfH * 0.6f)
        );
    }

    int MinionCount() =>
        GameObject.FindObjectsByType<WizardMinion>(FindObjectsSortMode.None).Length;

    void SummonMinion()
    {
        float groundY = -arenaHalfH + 0.75f + 0.5f;
        float spawnX  = UnityEngine.Random.value > 0.5f
            ? -arenaHalfW + 1.5f
            :  arenaHalfW - 1.5f;

        var go = new GameObject("WizardMinion");
        go.transform.position = new Vector3(spawnX, groundY + 0.4f, 0f);

        var mRb = go.AddComponent<Rigidbody2D>();
        mRb.freezeRotation = true;
        mRb.gravityScale   = 3f;

        var col  = go.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.4f, 0.8f);

        go.AddComponent<WizardMinion>();

        var figGO = new GameObject("Figure");
        figGO.transform.SetParent(go.transform);
        figGO.transform.localPosition = Vector3.zero;
        var vis = figGO.AddComponent<FantasyCharacterVisual>();
        vis.Init("Characters/Character (44)", scale: 0.9f);

        EffectManager.HitSpark((Vector2)go.transform.position, new Color(0.6f, 0.2f, 0.9f));
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (dead) return;
        hp = Mathf.Max(0f, hp - amount);
        EffectManager.HitSpark((Vector2)transform.position, new Color(0.5f, 0.1f, 1f));
        if (hp <= 0f) Die();
    }

    void Die()
    {
        dead = true;
        rb.linearVelocity = Vector2.zero;

        foreach (var m in FindObjectsByType<WizardMinion>(FindObjectsSortMode.None))
            Destroy(m.gameObject);

        EffectManager.DeathBurst(transform.position, new Color(0.5f, 0.1f, 1f));
        OnDied?.Invoke();
        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (dead) return;
        var cam = Camera.main;
        if (cam == null) return;

        Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 1.4f);
        if (screen.z < 0f) return;

        float barW = 100f, barH = 10f;
        float x = screen.x - barW * 0.5f;
        float y = Screen.height - screen.y - barH * 0.5f;

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(x - 1, y - 1, barW + 2, barH + 2), Texture2D.whiteTexture);
        GUI.color = new Color(0.5f, 0.1f, 1f);
        GUI.DrawTexture(new Rect(x, y, barW * (hp / maxHP), barH), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }

}
