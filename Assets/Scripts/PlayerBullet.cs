using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    float      damage;
    float      lifeTime = 1.8f;
    Collider2D ignore;

    public static void Spawn(Vector2 pos, Vector2 vel, float dmg, Collider2D ignore)
    {
        var go = new GameObject("PlayerBullet");
        go.transform.position   = pos;
        go.transform.localScale = Vector3.one * 0.18f;

        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        var sr    = go.AddComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);
        sr.color  = new Color(1f, 0.9f, 0.2f);
        sr.sortingOrder = 10;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale    = 0f;
        rb.linearVelocity  = vel;
        rb.freezeRotation  = true;

        var col    = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = true;

        var b    = go.AddComponent<PlayerBullet>();
        b.damage = dmg;
        b.ignore = ignore;
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == ignore) return;
        var dmg = other.GetComponent<IDamageable>();
        if (dmg != null)
        {
            var rb  = GetComponent<Rigidbody2D>();
            Vector2 dir = rb ? rb.linearVelocity.normalized : Vector2.right;
            dmg.TakeDamage(damage, dir * 5f);
            EffectManager.HitSpark(transform.position, new Color(1f, 0.9f, 0.2f));
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
