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

        float hw  = arenaWidth  * 0.5f;
        float hh  = arenaHeight * 0.5f;
        float groundY = -hh + 0.75f;  // アリーナの床面

        // カメラ背景色を空色に
        if (Camera.main) Camera.main.backgroundColor = new Color(0.45f, 0.72f, 0.88f);

        // ---- 背景パネル (Background1-5) ----
        // 各パネルは 256x272px / 16ppu = 16x17 ワールド単位
        // アリーナ幅18に合わせて2枚横並び + 1枚中央
        string[] bgNames = { "Background1", "Background2", "Background3", "Background4", "Background5" };
        float[] bgXPositions = { -hw, 0f, hw };
        string[] bgPick = { bgNames[0], bgNames[2], bgNames[4] };
        for (int i = 0; i < bgPick.Length; i++)
        {
            PlaceSprite(bgPick[i],
                new Vector3(bgXPositions[i], groundY + 4f, 0f),
                scale: 1.1f,
                order: -20);
        }

        // ---- 木 (Tree) ----
        // 奥の左右に大きめの木を配置
        PlaceSprite("Tree1", new Vector3(-hw + 1.2f, groundY + 4.5f, 0f), scale: 0.9f, order: -15);
        PlaceSprite("Tree3", new Vector3(-hw + 4f,   groundY + 5f,   0f), scale: 0.8f, order: -14);
        PlaceSprite("Tree2", new Vector3( hw - 1.2f, groundY + 4.5f, 0f), scale: 0.9f, order: -15, flipX: true);
        PlaceSprite("Tree4", new Vector3( hw - 4f,   groundY + 5f,   0f), scale: 0.8f, order: -14, flipX: true);
        PlaceSprite("Tree5", new Vector3(0f,         groundY + 5.2f, 0f), scale: 0.75f, order: -13);

        // ---- 手前の木（少し大きく・sortingOrder高め） ----
        PlaceSprite("Tree6", new Vector3(-hw + 0.5f, groundY + 4f, 0f), scale: 1.0f, order: -8);
        PlaceSprite("Tree7", new Vector3( hw - 0.5f, groundY + 4f, 0f), scale: 1.0f, order: -8, flipX: true);

        // ---- 石 (Stone) ----
        PlaceSprite("Stone1", new Vector3(-hw + 2.5f, groundY + 0.2f, 0f), scale: 0.8f, order: -5);
        PlaceSprite("Stone3", new Vector3(-hw + 5.5f, groundY + 0.1f, 0f), scale: 0.6f, order: -5);
        PlaceSprite("Stone2", new Vector3( hw - 2.5f, groundY + 0.2f, 0f), scale: 0.8f, order: -5, flipX: true);
        PlaceSprite("Stone4", new Vector3( hw - 5.5f, groundY + 0.1f, 0f), scale: 0.6f, order: -5);
        PlaceSprite("Stone5", new Vector3(1.5f,       groundY + 0.1f, 0f), scale: 0.5f, order: -5);

        // ---- 草・植物 (Plant) ----
        PlaceSprite("Plant1", new Vector3(-hw + 1f,  groundY + 0.3f, 0f), scale: 0.7f, order: -4);
        PlaceSprite("Plant2", new Vector3(-hw + 3f,  groundY + 0.3f, 0f), scale: 0.65f, order: -4);
        PlaceSprite("Plant3", new Vector3(-2f,       groundY + 0.3f, 0f), scale: 0.6f,  order: -4);
        PlaceSprite("Plant4", new Vector3( hw - 1f,  groundY + 0.3f, 0f), scale: 0.7f,  order: -4, flipX: true);
        PlaceSprite("Plant5", new Vector3( hw - 3f,  groundY + 0.3f, 0f), scale: 0.65f, order: -4, flipX: true);
        PlaceSprite("Plant1", new Vector3( 3.5f,     groundY + 0.3f, 0f), scale: 0.55f, order: -4, flipX: true);
    }

    void PlaceSprite(string spriteName, Vector3 pos, float scale = 1f, int order = 0, bool flipX = false)
    {
        if (!sprites.TryGetValue(spriteName, out var sprite)) return;

        var go = new GameObject($"BG_{spriteName}");
        go.transform.SetParent(transform);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = order;
        sr.flipX        = flipX;
        go.transform.localScale = Vector3.one * scale;
    }
}
