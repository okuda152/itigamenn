using UnityEngine;
using System.Collections.Generic;

public class BackgroundDecorator : MonoBehaviour
{
    public float arenaWidth  = 18f;
    public float arenaHeight = 10f;

    Dictionary<string, Sprite> sprites = new();

    void Start()
    {
        var loaded = Resources.LoadAll<Sprite>("ForestBG/Sprites");
        foreach (var s in loaded) sprites[s.name] = s;

        if (sprites.Count == 0) return;

        float hw      = arenaWidth  * 0.5f;
        float hh      = arenaHeight * 0.5f;
        float groundY = -hh + 0.75f;   // 床面（壁の上端）

        // カメラ背景色を空色に
        if (Camera.main) Camera.main.backgroundColor = new Color(0.45f, 0.72f, 0.88f);

        // ---- 背景パネル ----
        string[] bgPick = { "Background1", "Background3", "Background5" };
        float[]  bgX    = { -hw, 0f, hw };
        for (int i = 0; i < bgPick.Length; i++)
            PlaceGrounded(bgPick[i], bgX[i], groundY, scale: 1.1f, order: -20);

        // ---- 奥の木（小さく・暗め） ----
        PlaceGrounded("Tree5", -hw + 4f,   groundY, scale: 0.75f, order: -15, color: new Color(0.75f, 0.82f, 0.75f));
        PlaceGrounded("Tree3",  hw - 4f,   groundY, scale: 0.75f, order: -15, color: new Color(0.75f, 0.82f, 0.75f), flipX: true);
        PlaceGrounded("Tree4",  0f,        groundY, scale: 0.65f, order: -16, color: new Color(0.78f, 0.84f, 0.78f));

        // ---- 手前の木（大きく） ----
        PlaceGrounded("Tree1", -hw + 0.8f, groundY, scale: 1.05f, order: -8);
        PlaceGrounded("Tree2",  hw - 0.8f, groundY, scale: 1.05f, order: -8, flipX: true);
        PlaceGrounded("Tree6", -hw + 3.5f, groundY, scale: 0.9f,  order: -9);
        PlaceGrounded("Tree7",  hw - 3.5f, groundY, scale: 0.9f,  order: -9, flipX: true);

        // ---- 暗めのオーバーレイ（プレイヤーの視認性確保） ----
        AddOverlay(hw, hh, groundY);

        // ---- 地面タイル（床の上端に敷き詰める） ----
        // TileGround は 16x16px / 16ppu = 1x1 ワールド単位
        // 床上端: groundY, タイル上端: groundY なので center は groundY + 0.5
        string[] groundTiles = { "TileGround1", "TileGround2", "TileGround3", "TileGround4", "TileGround5" };
        int tileCount = Mathf.CeilToInt(arenaWidth) + 4;
        float tileStartX = -hw - 1.5f;
        for (int i = 0; i < tileCount; i++)
        {
            string tileName = groundTiles[i % groundTiles.Length];
            PlaceGrounded(tileName, tileStartX + i, groundY - 0.4f, scale: 1f, order: 1);
        }

        // ---- 石 ----
        PlaceGrounded("Stone1", -hw + 2.2f, groundY, scale: 0.8f, order: -3);
        PlaceGrounded("Stone3", -hw + 5.0f, groundY, scale: 0.6f, order: -3);
        PlaceGrounded("Stone2",  hw - 2.2f, groundY, scale: 0.8f, order: -3, flipX: true);
        PlaceGrounded("Stone4",  hw - 5.0f, groundY, scale: 0.6f, order: -3);
        PlaceGrounded("Stone5",  1.5f,      groundY, scale: 0.5f, order: -3);

        // ---- 草・植物 ----
        PlaceGrounded("Plant2", -hw + 1.2f, groundY, scale: 0.7f,  order: -2);
        PlaceGrounded("Plant4", -hw + 4.0f, groundY, scale: 0.65f, order: -2);
        PlaceGrounded("Plant1", -2.5f,      groundY, scale: 0.6f,  order: -2);
        PlaceGrounded("Plant3",  hw - 1.2f, groundY, scale: 0.7f,  order: -2, flipX: true);
        PlaceGrounded("Plant5",  hw - 4.0f, groundY, scale: 0.65f, order: -2, flipX: true);
        PlaceGrounded("Plant2",  3.0f,      groundY, scale: 0.55f, order: -2, flipX: true);
    }

    // スプライトの下端が groundY に来るよう自動計算して配置
    void PlaceGrounded(string spriteName, float x, float groundY,
                       float scale = 1f, int order = 0, bool flipX = false,
                       Color? color = null)
    {
        if (!sprites.TryGetValue(spriteName, out var sprite)) return;

        float halfH = sprite.rect.height / sprite.pixelsPerUnit * scale * 0.5f;
        var pos = new Vector3(x, groundY + halfH, 0f);

        var go = new GameObject($"BG_{spriteName}");
        go.transform.SetParent(transform);
        go.transform.position   = pos;
        go.transform.localScale = Vector3.one * scale;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = order;
        sr.flipX        = flipX;
        sr.color        = color ?? Color.white;
    }

    // プレイヤーと背景の間に半透明の暗いレイヤーを挟む
    void AddOverlay(float hw, float hh, float groundY)
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        var sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

        var go = new GameObject("BG_Overlay");
        go.transform.SetParent(transform);
        go.transform.position   = new Vector3(0f, 0f, 0f);
        go.transform.localScale = new Vector3(arenaWidth, arenaHeight * 2f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = -6;
        sr.color        = new Color(0.82f, 0.88f, 0.82f, 0.78f);
    }
}
