using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    float       damage;
    float       lifeTime = 1.8f;
    Collider2D  ignore;
    Rigidbody2D rb;
    Vector2     prevPos;

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
        rb.gravityScale   = 0f;
        rb.linearVelocity = vel;
        rb.freezeRotation = true;

        var b     = go.AddComponent<PlayerBullet>();
        b.damage  = dmg;
        b.ignore  = ignore;
        b.rb      = rb;
        b.prevPos = pos;
    }

    void FixedUpdate()
    {
        lifeTime -= Time.fixedDeltaTime;
        if (lifeTime <= 0f) { Destroy(gameObject); return; }

        Vector2 curPos = rb.position;
        Vector2 dir    = curPos - prevPos;
        float   dist   = dir.magnitude;

        if (dist > 0.001f)
        {
            foreach (var hit in Physics2D.CircleCastAll(prevPos, 0.25f, dir.normalized, dist))
            {
                if (hit.collider == ignore) continue;

                var dmg = hit.collider.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    dmg.TakeDamage(damage, dir.normalized * 5f);
                    EffectManager.HitSpark(curPos, new Color(1f, 0.9f, 0.2f));
                    Destroy(gameObject);
                    return;
                }
                if (!hit.collider.isTrigger)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        prevPos = curPos;
    }
}
