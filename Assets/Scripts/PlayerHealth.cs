using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 100f;

    float hp;
    bool  isDead = false;
    StickFigureRenderer  figure;
    PlayerSpriteAnimator figureSprite;

    static readonly Color HitColor  = new Color(1f, 0.2f, 0.2f);
    static readonly Color BaseColor = Color.black;

    void Awake() => hp = maxHP;

    void Start()
    {
        figure       = GetComponentInChildren<StickFigureRenderer>();
        figureSprite = GetComponentInChildren<PlayerSpriteAnimator>();
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;
        hp = Mathf.Max(0f, hp - amount);
        StopCoroutine("FlashRoutine");
        StartCoroutine("FlashRoutine");
        if (hp <= 0f) StartCoroutine(Die());
    }

    IEnumerator FlashRoutine()
    {
        if (figure)       figure.SetColor(HitColor);
        if (figureSprite) figureSprite.SetColor(HitColor);
        yield return new WaitForSeconds(0.3f);
        if (figure)       figure.SetColor(BaseColor);
        if (figureSprite) figureSprite.SetColor(Color.white);
    }

    IEnumerator Die()
    {
        isDead = true;

        // エフェクト
        EffectManager.DeathBurst(transform.position, new Color(1f, 0.3f, 0.3f));

        // 入力・ビジュアルを止める
        var ctrl = GetComponent<PlayerController>();
        if (ctrl) ctrl.enabled = false;
        if (figure)       figure.gameObject.SetActive(false);
        if (figureSprite) figureSprite.gameObject.SetActive(false);

        // 2.5秒後に全オブジェクトを破棄してスタート画面へ
        yield return new WaitForSeconds(2.5f);
        ReturnToStart();
    }

    void ReturnToStart()
    {
        // ゲーム中に生成された全 GameObject を破棄（カメラは残す）
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.GetComponent<Camera>() != null) continue;
            Destroy(go);
        }

        // カメラ背景を黒にリセット
        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = Color.black;
            cam.orthographicSize = 5.5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        // EffectManager と StartScreen を再生成
        new GameObject("EffectManager").AddComponent<EffectManager>();
        new GameObject("StartScreen").AddComponent<StartScreen>();
    }

    void OnGUI()
    {
        // ---- GAME OVER オーバーレイ ----
        if (isDead)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.65f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 56,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(1f, 0.15f, 0.15f) }
            };
            GUI.color = Color.white;
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "G A M E  O V E R", style);
            return;
        }

        // ---- HP バー（既存）----
        const float barH   = 200f;
        const float barW   = 18f;
        const float margin = 24f;

        float x = Screen.width  - barW - margin;
        float y = (Screen.height - barH) * 0.5f;

        GUI.color = new Color(0f, 0f, 0f, 0.55f);
        GUI.DrawTexture(new Rect(x - 4, y - 4, barW + 8, barH + 8), Texture2D.whiteTexture);

        GUI.color = new Color(0.05f, 0.18f, 0.05f);
        GUI.DrawTexture(new Rect(x, y, barW, barH), Texture2D.whiteTexture);

        float fillH = barH * (hp / maxHP);
        GUI.color = new Color(0.2f, 0.85f, 0.25f);
        GUI.DrawTexture(new Rect(x, y + barH - fillH, barW, fillH), Texture2D.whiteTexture);

        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle  = FontStyle.Bold,
            fontSize   = 11,
            normal     = { textColor = Color.white }
        };
        GUI.color = Color.white;
        GUI.Label(new Rect(x - 4, y - 22f, barW + 8, 18f), "HP", labelStyle);
    }
}
