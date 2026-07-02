using UnityEngine;

public class WizardMinion : MonoBehaviour, IDamageable
{
    public float maxHP = 30f;

    float hp;
    bool  dead = false;
    Rigidbody2D rb;

    const float MOVE_SPEED      = 3.5f;
    const float CONTACT_DAMAGE  = 8f;
    const float CONTACT_CD      = 0.8f;
    float contactTimer = 0f;

    void Awake()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (dead) return;
        contactTimer -= Time.deltaTime;

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        float dir = Mathf.Sign(player.transform.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * MOVE_SPEED, rb.linearVelocity.y);
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (dead || contactTimer > 0f) return;
        var health = col.gameObject.GetComponent<PlayerHealth>();
        if (health == null) return;
        health.TakeDamage(CONTACT_DAMAGE);
        contactTimer = CONTACT_CD;
    }

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (dead) return;
        hp = Mathf.Max(0f, hp - amount);
        rb.linearVelocity += knockback * 0.4f;
        EffectManager.HitSpark((Vector2)transform.position, new Color(0.8f, 0.4f, 1f));
        if (hp <= 0f) Die();
    }

    void Die()
    {
        dead = true;
        EffectManager.DeathBurst(transform.position, new Color(0.6f, 0.2f, 0.8f));
        Destroy(gameObject);
    }
}
