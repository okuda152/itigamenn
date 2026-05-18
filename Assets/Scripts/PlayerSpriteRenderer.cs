using UnityEngine;
using System.IO;

/// <summary>
/// StreamingAssets/player.png を読み込み、白背景を透過して SpriteRenderer で表示する。
/// アニメーションなし — 左右反転のみ行う。
/// </summary>
public class PlayerSpriteRenderer : MonoBehaviour
{
    public PlayerController playerController;

    SpriteRenderer sr;

    void Awake()
    {
        sr              = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;
        sr.color        = Color.white;
    }

    void Start()
    {
        LoadSprite();
    }

    void LoadSprite()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "player.png");
        if (!File.Exists(path))
        {
            Debug.LogWarning("[PlayerSpriteRenderer] player.png not found at: " + path);
            return;
        }

        byte[] bytes = File.ReadAllBytes(path);

        // ① 一時テクスチャに読み込む（LoadImage はフォーマットを上書きするため）
        Texture2D tmp = new Texture2D(2, 2);
        if (!tmp.LoadImage(bytes))
        {
            Debug.LogWarning("[PlayerSpriteRenderer] LoadImage failed.");
            return;
        }

        int w = tmp.width, h = tmp.height;
        Debug.Log($"[PlayerSpriteRenderer] Loaded {w}x{h}, format={tmp.format}");

        // ② RGBA32 の新テクスチャにコピー（alpha チャンネルを確保）
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = tmp.GetPixels();
        Destroy(tmp);

        // ③ 元画像にアルファがなければ白背景を透過処理
        bool hasAlpha = false;
        for (int i = 0; i < pixels.Length; i++)
        {
            if (pixels[i].a < 0.99f) { hasAlpha = true; break; }
        }

        if (!hasAlpha)
        {
            // 輝度 0.85 以上 → 透明、0.30 以下 → 不透明、その間はなめらかに遷移
            for (int i = 0; i < pixels.Length; i++)
            {
                float lum    = pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f;
                pixels[i].a  = Mathf.Clamp01(Mathf.InverseLerp(0.85f, 0.30f, lum));
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // ④ 不透明ピクセルの境界を検出してキャラクター領域を特定
        int minY = h, maxY = 0, minX = w, maxX = 0;
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (pixels[y * w + x].a > 0.1f)
                {
                    if (y < minY) minY = y;
                    if (y > maxY) maxY = y;
                    if (x < minX) minX = x;
                    if (x > maxX) maxX = x;
                }
            }
        }
        if (maxY <= minY) { minY = 0; maxY = h - 1; minX = 0; maxX = w - 1; }

        int charPixH = maxY - minY + 1;
        int charPixW = maxX - minX + 1;
        Debug.Log($"[PlayerSpriteRenderer] Char bounds ({minX},{minY})-({maxX},{maxY}), size={charPixW}x{charPixH}");

        // ⑤ キャラクター身長 = 1.8 world units になる PPU を計算
        float ppu = Mathf.Max(charPixH / 1.8f, 10f);

        // ⑥ ピボット = キャラクターの足元中央
        float pivotX = (minX + charPixW * 0.5f) / w;
        float pivotY = (float)minY / h;

        sr.sprite              = Sprite.Create(tex, new Rect(0, 0, w, h),
                                               new Vector2(pivotX, pivotY), ppu);
        transform.localPosition = new Vector3(0f, -0.9f, 0f);
    }

    void Update()
    {
        if (playerController == null || sr == null) return;
        sr.flipX = !playerController.FacingRight;
    }

    /// <summary>ヒットフラッシュ用。白=通常、赤=被弾</summary>
    public void SetColor(Color c) => sr.color = c;
}
