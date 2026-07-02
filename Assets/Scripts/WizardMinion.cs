using UnityEngine;

public class WizardMinion : MonoBehaviour, IDamageable
{
    public float maxHP = 1f;

    enum State { Hopping, Preparing, Dead }

    float hp;
    State state = State.Hopping;
    Rigidbody2D      rb;
    FantasyCharacterVisual visual;

    // ホップ移動
    const float HOP_SPEED      = 4.5f;
    const float HOP_DURATION   = 0.28f;
    const float PAUSE_DURATION = 0.28f;
    float stateTimer = 0f;
    bool  hopping    = false;
    float hopDir     = 1f;

    // 自爆
    const float DETECT_RANGE     = 1.6f;
    const float PREPARE_TIME     = 1.4f;
    const float EXPLOSION_RADIUS = 2.2f;
    const float EXPLOSION_DAMAGE = 8f;
    float prepareTimer  = 0f;
    float blinkTimer    = 0f;
    bool  blinkOn       = false;

    void Awake()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody2D>();
        stateTimer = Random.Range(0f, 0.3f);
    }

    void Start()
    {
        visual = GetComponentInChildren<FantasyCharacterVisual>();
    }

    void Update()
    {
        if (state == State.Dead) return;

        var player = GameObject.FindWithTag("Player");

        if (state == State.Hopping)
        {
            UpdateHop(player);

            if (player != null &&
                Vector2.Distance(transform.position, player.transform.position) < DETECT_RANGE)
            {
                state        = State.Preparing;
                prepareTimer = PREPARE_TIME;
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }
        else if (state == State.Preparing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            prepareTimer -= Time.deltaTime;
            blinkTimer   -= Time.deltaTime;
            if (blinkTimer <= 0f)
            {
                blinkOn    = !blinkOn;
                float speed = Mathf.Lerp(0.18f, 0.06f, 1f - prepareTimer / PREPARE_TIME);
                blinkTimer = speed;
                if (visual != null)
                    visual.SetColor(blinkOn ? new Color(1f, 0.2f, 0.2f) : Color.white);
            }

            if (prepareTimer <= 0f) Explode(player);
        }
    }

    void UpdateHop(GameObject player)
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            hopping = !hopping;
            if (hopping)
            {
                if (player != null)
                    hopDir = Mathf.Sign(player.transform.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(hopDir * HOP_SPEED, 3.5f);
                stateTimer = HOP_DURATION;
            }
            else
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                stateTimer = PAUSE_DURATION;
            }
        }
        else if (!hopping)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void Explode(GameObject player)
    {
        state = State.Dead;
        EffectManager.DeathBurst(transform.position, new Color(1f, 0.3f, 0.1f));
        EffectManager.HitRing((Vector2)transform.position, new Color(1f, 0.5f, 0.1f));

        foreach (var col in Physics2D.OverlapCircleAll((Vector2)transform.position, EXPLOSION_RADIUS))
        {
            var ph = col.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(EXPLOSION_DAMAGE);
        }

        Destroy(gameObject);
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (state == State.Dead) return;
        hp = Mathf.Max(0f, hp - amount);
        rb.linearVelocity += knockback * 0.4f;
        EffectManager.HitSpark((Vector2)transform.position, new Color(0.8f, 0.4f, 1f));
        if (hp <= 0f) Die();
    }

    void Die()
    {
        state = State.Dead;
        EffectManager.DeathBurst(transform.position, new Color(0.6f, 0.2f, 0.8f));
        Destroy(gameObject);
    }
}
