using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage   = 8f;
    public float lifetime = 5f;

    Vector2 velocity;

    // Factory method — creates a bullet with a circle LineRenderer visual
    public static Bullet Spawn(Vector2 pos, Vector2 dir, Color col,
                               float speed = 6f, float dmg = 8f)
    {
        var go = new GameObject("Bullet");
        go.transform.position = pos;

        // Visual: small circle
        var lr = go.AddComponent<LineRenderer>();
        const int seg = 12;
        float vr = 0.14f;
        lr.positionCount  = seg;
        lr.loop           = true;
        lr.startWidth     = lr.endWidth = 0.055f;
        lr.useWorldSpace  = false;
        lr.sortingOrder   = 8;
        var shader = Shader.Find("Sprites/Default")
                  ?? Shader.Find("Universal Render Pipeline/Unlit");
        lr.material = new Material(shader);
        lr.material.color = col;
        lr.startColor = lr.endColor = col;
        for (int i = 0; i < seg; i++)
        {
            float a = (float)i / seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(a) * vr, Mathf.Sin(a) * vr, 0f));
        }

        // Collider (trigger) — slightly smaller than visual for fairness
        var c = go.AddComponent<CircleCollider2D>();
        c.isTrigger = true;
        c.radius    = 0.11f;

        var b      = go.AddComponent<Bullet>();
        b.velocity = dir.normalized * speed;
        b.damage   = dmg;
        return b;
    }

    void Update()
    {
        transform.Translate(velocity * Time.deltaTime, Space.World);
        lifetime -= Time.deltaTime;

        // Arena bounds check (walls at roughly ±9 x, ±5 y)
        var p = transform.position;
        if (lifetime <= 0f || Mathf.Abs(p.x) > 10.5f || p.y < -6f || p.y > 7f)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Bullet>() != null) return;

        // プレイヤーにダメージ
        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) { ph.TakeDamage(damage); Destroy(gameObject); return; }

        // プレイヤーの雑魚にダメージ（弾は貫通）
        var pm = other.GetComponentInParent<PlayerMinion>();
        if (pm != null) { pm.TakeDamage(damage, Vector2.zero); return; }

        // ボス・IDamageable はスルー（ボス自身に当たらないように）
        if (other.GetComponentInParent<IDamageable>() != null) return;

        // 壁などで消える
        if (!other.isTrigger) Destroy(gameObject);
    }
}
