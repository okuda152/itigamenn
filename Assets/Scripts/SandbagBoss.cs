using UnityEngine;

// テスト用サンドバッグ。確認が終わったら ArenaBuilder の SpawnSandbag() 呼び出し行を消すこと。
public class SandbagBoss : MonoBehaviour, IDamageable
{
    public float maxHP = 300f;
    float hp;
    bool dead = false;

    void Awake() => hp = maxHP;

    public void TakeDamage(float amount, Vector2 knockback)
    {
        if (dead) return;
        hp = Mathf.Max(0f, hp - amount);
        EffectManager.HitSpark((Vector2)transform.position, new Color(1f, 0.6f, 0.1f));
        if (hp <= 0f) Die();
    }

    void Die()
    {
        dead = true;
        EffectManager.DeathBurst(transform.position, new Color(0.8f, 0.5f, 0.1f));
        Destroy(gameObject);
    }

    void OnGUI()
    {
        if (dead) return;

        // HPバー（頭上）
        var cam = Camera.main;
        if (cam == null) return;
        Vector3 screen = cam.WorldToScreenPoint(transform.position + Vector3.up * 1.6f);
        if (screen.z < 0f) return;

        float barW = 80f;
        float barH = 10f;
        float x = screen.x - barW * 0.5f;
        float y = Screen.height - screen.y - barH * 0.5f;

        GUI.color = new Color(0f, 0f, 0f, 0.6f);
        GUI.DrawTexture(new Rect(x - 1, y - 1, barW + 2, barH + 2), Texture2D.whiteTexture);
        GUI.color = new Color(0.9f, 0.4f, 0.1f);
        GUI.DrawTexture(new Rect(x, y, barW * (hp / maxHP), barH), Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}
