using UnityEngine;

public class PlayerMinion : MonoBehaviour
{
    Rigidbody2D rb;

    const float MOVE_SPEED     = 4f;
    const float CONTACT_DAMAGE = 12f;
    const float CONTACT_CD     = 1f;
    const float LIFETIME       = 15f;

    float contactTimer = 0f;
    float lifetime     = LIFETIME;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void Update()
    {
        contactTimer -= Time.deltaTime;
        lifetime     -= Time.deltaTime;
        if (lifetime <= 0f) { Destroy(gameObject); return; }

        var target = FindTarget();
        if (target == null) return;

        float dir = Mathf.Sign(target.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dir * MOVE_SPEED, rb.linearVelocity.y);
    }

    Transform FindTarget()
    {
        float best = float.MaxValue;
        Transform found = null;
        foreach (var dmg in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (dmg is not IDamageable)                         continue;
            if (dmg is PlayerHealth or WizardMinion or PlayerMinion) continue;
            float d = Vector2.Distance(transform.position, dmg.transform.position);
            if (d < best) { best = d; found = dmg.transform; }
        }
        return found;
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (contactTimer > 0f) return;
        var dmg = col.gameObject.GetComponent<IDamageable>();
        if (dmg == null
            || col.gameObject.GetComponent<PlayerHealth>() != null
            || col.gameObject.GetComponent<WizardMinion>()  != null
            || col.gameObject.GetComponent<PlayerMinion>()  != null) return;

        dmg.TakeDamage(CONTACT_DAMAGE, Vector2.zero);
        EffectManager.HitSpark((Vector2)transform.position, new Color(0.5f, 1f, 0.4f));
        contactTimer = CONTACT_CD;
    }
}
