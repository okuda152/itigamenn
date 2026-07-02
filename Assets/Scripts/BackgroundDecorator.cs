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
        float groundY = -hh + 0.75f;  // 床壁の上端 = プレイヤーが立つ面

        // ---- 地面タイル（土のみ） ----
        int   tileCount  = Mathf.CeilToInt(arenaWidth) + 24;
        float tileStartX = -hw - 12f;

        for (int row = 0; row <= 2; row++)
        {
            float rowCenterY = groundY - 0.5f - row;
            for (int i = 0; i < tileCount; i++)
                PlaceAt("TileGround8",
                        new Vector3(tileStartX + i, rowCenterY, 0f), order: 2);
        }

    }

    // 指定の中心座標に配置
    void PlaceAt(string spriteName, Vector3 center, float scale = 1f, int order = 0,
                 bool flipX = false, bool flipY = false, float rotZ = 0f)
    {
        if (!sprites.TryGetValue(spriteName, out var sprite)) return;

        var go = new GameObject($"BG_{spriteName}");
        go.transform.SetParent(transform);
        go.transform.position   = center;
        go.transform.localScale = Vector3.one * scale;
        if (rotZ != 0f) go.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = order;
        sr.flipX        = flipX;
        sr.flipY        = flipY;
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

}
