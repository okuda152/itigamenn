using UnityEngine;

/// <summary>
/// 放物線を描く投石。重力に従って飛び、IDamageable に当たるとダメージを与えて消える。
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Stone : MonoBehaviour
{
    public float damage   = 15f;
    public float lifetime = 4f;

    static readonly Color StoneColor = new Color(0.55f, 0.45f, 0.35f);

    public static Stone Spawn(Vector2 pos, Vector2 velocity, float dmg,
                              Collider2D throwerCol)
    {
        var go = new GameObject("Stone");
        go.transform.position = pos;

        // Visual: irregular rock polygon (spins during flight)
        var lr = go.AddComponent<LineRenderer>();
        var rockPts = new Vector3[]
        {
            new Vector3( 0.12f,  0.17f, 0f),
            new Vector3( 0.19f,  0.03f, 0f),
            new Vector3( 0.14f, -0.14f, 0f),
            new Vector3(-0.05f, -0.18f, 0f),
            new Vector3(-0.17f, -0.06f, 0f),
            new Vector3(-0.14f,  0.12f, 0f),
        };
        lr.positionCount = rockPts.Length;
        lr.loop          = true;
        lr.startWidth    = lr.endWidth = 0.06f;
        lr.useWorldSpace = false;
        lr.sortingOrder  = 7;
        for (int i = 0; i < rockPts.Length; i++) lr.SetPosition(i, rockPts[i]);
        var shader = Shader.Find("Sprites/Default")
                  ?? Shader.Find("Universal Render Pipeline/Unlit");
        lr.material = new Material(shader);
        lr.material.color = StoneColor;
        lr.startColor = lr.endColor = StoneColor;

        // Physics — gravity like player/boss
        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale   = 3f;
        rb.freezeRotation = true;
        rb.linearVelocity  = velocity;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.15f;

        // プレイヤー自身とは衝突しない
        if (throwerCol != null)
            Physics2D.IgnoreCollision(col, throwerCol);

        var s    = go.AddComponent<Stone>();
        s.damage = dmg;
        return s;
    }

    void Update()
    {
        // 回転エフェクト
        transform.Rotate(0f, 0f, 320f * Time.deltaTime);

        lifetime -= Time.deltaTime;
        if (lifetime <= 0f) Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        Vector2 knockDir = col.contacts.Length > 0
            ? -col.contacts[0].normal
            : Vector2.right;

        var dmg = col.gameObject.GetComponent<IDamageable>();
        if (dmg != null)
        {
            dmg.TakeDamage(damage, knockDir * 5f);
            EffectManager.HitSpark(transform.position, new Color(0.8f, 0.7f, 0.4f));
        }

        Destroy(gameObject);
    }
}
