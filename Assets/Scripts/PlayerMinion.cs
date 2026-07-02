using UnityEngine;

public class PlayerMinion : MonoBehaviour, IDamageable
{
    enum State { Hopping, Preparing, Dead }

    State state = State.Hopping;
    Rigidbody2D            rb;
    FantasyCharacterVisual visual;

    // ホップ移動
    const float HOP_SPEED      = 4.5f;
    const float HOP_DURATION   = 0.28f;
    const float PAUSE_DURATION = 0.28f;
    float stateTimer = 0f;
    bool  hopping    = false;
    float hopDir     = 1f;

    // 自爆
    const float DETECT_RANGE      = 1.6f;
    const float PREPARE_TIME      = 1.4f;
    const float EXPLOSION_RADIUS  = 2.2f;
    const float EXPLOSION_DAMAGE  = 10f;
    float prepareTimer = 0f;
    float blinkTimer   = 0f;
    bool  blinkOn      = false;

    void Awake()
    {
        rb         = GetComponent<Rigidbody2D>();
        stateTimer = Random.Range(0f, 0.3f);
    }

    void Start()
    {
        visual = GetComponentInChildren<FantasyCharacterVisual>();

        // ボス・敵雑魚とは物理衝突しない（攻撃は貫通させる）
        var myCol = GetComponent<Collider2D>();
        if (myCol == null) return;
        foreach (var col in FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
        {
            if (col.gameObject == gameObject) continue;
            if (col.GetComponentInParent<IDamageable>() != null)
                Physics2D.IgnoreCollision(myCol, col);
        }
    }

    void Update()
    {
        if (state == State.Dead) return;

        var target = FindTarget();

        if (state == State.Hopping)
        {
            UpdateHop(target);

            if (target != null &&
                Vector2.Distance(transform.position, target.position) < DETECT_RANGE)
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
                blinkTimer  = speed;
                if (visual != null)
                    visual.SetColor(blinkOn ? new Color(0.3f, 1f, 0.3f) : Color.white);
            }

            if (prepareTimer <= 0f) Explode();
        }
    }

    void UpdateHop(Transform target)
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            hopping = !hopping;
            if (hopping)
            {
                if (target != null)
                    hopDir = Mathf.Sign(target.position.x - transform.position.x);
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

    void Explode()
    {
        state = State.Dead;
        EffectManager.DeathBurst(transform.position, new Color(0.3f, 1f, 0.3f));
        EffectManager.HitRing((Vector2)transform.position, new Color(0.3f, 1f, 0.3f));

        foreach (var col in Physics2D.OverlapCircleAll((Vector2)transform.position, EXPLOSION_RADIUS))
        {
            if (col.GetComponent<PlayerHealth>() != null) continue;
            if (col.GetComponent<WizardMinion>()  != null) continue;
            if (col.GetComponent<PlayerMinion>()  != null) continue;
            var dmg = col.GetComponent<IDamageable>();
            if (dmg != null) dmg.TakeDamage(EXPLOSION_DAMAGE, Vector2.zero);
        }

        Destroy(gameObject);
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (state == State.Dead) return;
        state = State.Dead;
        EffectManager.DeathBurst(transform.position, new Color(0.3f, 1f, 0.3f));
        Destroy(gameObject);
    }

    Transform FindTarget()
    {
        float best = float.MaxValue;
        Transform found = null;
        foreach (var dmg in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (dmg is not IDamageable)                              continue;
            if (dmg is PlayerHealth or WizardMinion or PlayerMinion) continue;
            float d = Vector2.Distance(transform.position, dmg.transform.position);
            if (d < best) { best = d; found = dmg.transform; }
        }
        return found;
    }
}
