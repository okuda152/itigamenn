using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;

    float hp;
    float flashTimer;
    FrameAnimPlayer figure;

    static readonly Color BaseColor = Color.white;

    void Awake() => hp = maxHP;

    void Start() => figure = GetComponent<FrameAnimPlayer>();

    void Update()
    {
        if (flashTimer <= 0f) return;
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f && figure) figure.SetColor(BaseColor);
    }

    public void TakeDamage(float amount)
    {
        hp = Mathf.Max(0f, hp - amount);
        if (figure) { figure.SetColor(new Color(1f, 0.3f, 0.3f)); flashTimer = 0.15f; }
        if (hp <= 0f) gameObject.SetActive(false);
    }

    void OnGUI()
    {
        const float barH   = 200f;
        const float barW   = 18f;
        const float margin = 24f;

        float x = Screen.width  - barW - margin;
        float y = (Screen.height - barH) * 0.5f;

        // 外枠
        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(x - 4, y - 4, barW + 8, barH + 8), Texture2D.whiteTexture);

        // 空バー
        GUI.color = new Color(0.05f, 0.18f, 0.05f);
        GUI.DrawTexture(new Rect(x, y, barW, barH), Texture2D.whiteTexture);

        // HP（下から上に伸びる）
        float fillH = barH * (hp / maxHP);
        GUI.color = new Color(0.2f, 0.85f, 0.25f);
        GUI.DrawTexture(new Rect(x, y + barH - fillH, barW, fillH), Texture2D.whiteTexture);

        // ラベル
        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle  = FontStyle.Bold,
            fontSize   = 11,
            normal     = { textColor = Color.white }
        };
        GUI.color = Color.white;
        GUI.Label(new Rect(x - 4, y - 22f, barW + 8, 18f), "HP", style);
    }
}
