using UnityEngine;
using System.Collections.Generic;

public class BackgroundDecorator : MonoBehaviour
{
    public float arenaWidth  = 18f;
    public float arenaHeight = 10f;

    Dictionary<string, Sprite> sprites = new();
    Sprite flatSprite;

    void Start()
    {
        var loaded = Resources.LoadAll<Sprite>("ForestBG/Sprites");
        foreach (var s in loaded) sprites[s.name] = s;

        if (sprites.Count == 0) return;

        flatSprite = MakeFlatSprite();

        float hw      = arenaWidth  * 0.5f;
        float hh      = arenaHeight * 0.5f;
        float groundY = -hh + 0.75f;   // 床壁の上端

        // カメラ背景色を空色に
        if (Camera.main) Camera.main.backgroundColor = new Color(0.45f, 0.72f, 0.88f);

        // ---- 背景パネル ----
        string[] bgPick = { "Background1", "Background3", "Background5" };
        float[]  bgX    = { -hw, 0f, hw };
        for (int i = 0; i < bgPick.Length; i++)
            PlaceGrounded(bgPick[i], bgX[i], groundY, scale: 1.1f, order: -20);

        // ---- 奥の木（小さく・薄め） ----
        PlaceGrounded("Tree5", -hw + 4f, groundY, scale: 0.75f, order: -15, color: new Color(0.75f, 0.82f, 0.75f));
        PlaceGrounded("Tree3",  hw - 4f, groundY, scale: 0.75f, order: -15, color: new Color(0.75f, 0.82f, 0.75f), flipX: true);
        PlaceGrounded("Tree4",  0f,      groundY, scale: 0.65f, order: -16, color: new Color(0.78f, 0.84f, 0.78f));

        // ---- 手前の木 ----
        PlaceGrounded("Tree1", -hw + 0.8f, groundY, scale: 1.05f, order: -8);
        PlaceGrounded("Tree2",  hw - 0.8f, groundY, scale: 1.05f, order: -8, flipX: true);
        PlaceGrounded("Tree6", -hw + 3.5f, groundY, scale: 0.9f,  order: -9);
        PlaceGrounded("Tree7",  hw - 3.5f, groundY, scale: 0.9f,  order: -9, flipX: true);

        // ---- 霧オーバーレイ（プレイヤー視認性確保） ----
        AddColoredRect("BG_Overlay", 0f, 0f, arenaWidth, arenaHeight * 2f,
                       new Color(0.82f, 0.88f, 0.82f, 0.78f), -6);

        // ---- 草ストリップ（床の上端にぴったり重ねる） ----
        // groundY が床壁上端なので、高さ 0.3 の帯の中心を groundY + 0.15 に置く
        AddColoredRect("BG_Grass", 0f, groundY + 0.15f, arenaWidth + 3f, 0.3f,
                       new Color(0.22f, 0.55f, 0.12f), 1);

        // 草の上の細い明るい線（ハイライト）
        AddColoredRect("BG_GrassHL", 0f, groundY + 0.28f, arenaWidth + 3f, 0.06f,
                       new Color(0.38f, 0.75f, 0.22f), 2);

        // ---- 石 ----
        PlaceGrounded("Stone1", -hw + 2.2f, groundY, scale: 0.8f, order: 3);
        PlaceGrounded("Stone3", -hw + 5.0f, groundY, scale: 0.6f, order: 3);
        PlaceGrounded("Stone2",  hw - 2.2f, groundY, scale: 0.8f, order: 3, flipX: true);
        PlaceGrounded("Stone4",  hw - 5.0f, groundY, scale: 0.6f, order: 3);
        PlaceGrounded("Stone5",  1.5f,      groundY, scale: 0.5f, order: 3);

        // ---- 草・植物 ----
        PlaceGrounded("Plant2", -hw + 1.2f, groundY, scale: 0.7f,  order: 4);
        PlaceGrounded("Plant4", -hw + 4.0f, groundY, scale: 0.65f, order: 4);
        PlaceGrounded("Plant1", -2.5f,      groundY, scale: 0.6f,  order: 4);
        PlaceGrounded("Plant3",  hw - 1.2f, groundY, scale: 0.7f,  order: 4, flipX: true);
        PlaceGrounded("Plant5",  hw - 4.0f, groundY, scale: 0.65f, order: 4, flipX: true);
        PlaceGrounded("Plant2",  3.0f,      groundY, scale: 0.55f, order: 4, flipX: true);
    }

    // スプライトの下端が bottomY に来るよう配置
    void PlaceGrounded(string spriteName, float x, float bottomY,
                       float scale = 1f, int order = 0, bool flipX = false,
                       Color? color = null)
    {
        if (!sprites.TryGetValue(spriteName, out var sprite)) return;

        float halfH = sprite.rect.height / sprite.pixelsPerUnit * scale * 0.5f;
        var go = new GameObject($"BG_{spriteName}");
        go.transform.SetParent(transform);
        go.transform.position   = new Vector3(x, bottomY + halfH, 0f);
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = order;
        sr.flipX        = flipX;
        sr.color        = color ?? Color.white;
    }

    void AddColoredRect(string goName, float cx, float cy, float w, float h, Color color, int order)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform);
        go.transform.position   = new Vector3(cx, cy, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = flatSprite;
        sr.color        = color;
        sr.sortingOrder = order;
    }

    static Sprite MakeFlatSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
