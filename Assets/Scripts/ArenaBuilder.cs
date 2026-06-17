using UnityEngine;

/// <summary>
/// ランタイムでアリーナ・プレイヤー・ボスを生成します。
/// SampleScene の空の GameObject にアタッチするか、Bootstrap.cs を使って自動起動させてください。
/// </summary>
public class ArenaBuilder : MonoBehaviour
{
    [Header("Arena")]
    public float arenaWidth  = 18f;
    public float arenaHeight = 10f;

    void Awake()
    {
        SetupCamera();
        BuildArena();
        SpawnPlayer();

        new GameObject("AbilitySelectUI").AddComponent<AbilitySelectUI>();

        SpawnSandbag(); // ← テスト終わったらこの行を消す

        var bmGO = new GameObject("BossManager");
        var bm   = bmGO.AddComponent<BossManager>();
        bm.arenaWidth  = arenaWidth;
        bm.arenaHeight = arenaHeight;
    }

    // ---- Camera ----

    void SetupCamera()
    {
        var cam = Camera.main;
        if (!cam) return;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.orthographicSize   = arenaHeight * 0.5f + 0.5f;
        cam.backgroundColor    = Color.white;
        cam.clearFlags         = CameraClearFlags.SolidColor;
    }

    // ---- Arena ----

    void BuildArena()
    {
        float hw = arenaWidth  * 0.5f;
        float hh = arenaHeight * 0.5f;

        const float wallThick = 1.5f;
        BuildWall("Floor",     new Vector2(0f,       -hh),  new Vector2(arenaWidth + wallThick * 2f, wallThick));
        BuildWall("Ceiling",   new Vector2(0f,        hh),  new Vector2(arenaWidth + wallThick * 2f, wallThick));
        BuildWall("LeftWall",  new Vector2(-hw, 0f),        new Vector2(wallThick, arenaHeight));
        BuildWall("RightWall", new Vector2( hw, 0f),        new Vector2(wallThick, arenaHeight));
    }

    void BuildWall(string wallName, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(wallName);
        go.tag = "Untagged";
        go.transform.position   = pos;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        go.AddComponent<BoxCollider2D>().size = Vector2.one;

        var sr     = go.AddComponent<SpriteRenderer>();
        sr.sprite  = CreatePixelSprite();
        sr.color   = new Color(0.15f, 0.15f, 0.20f);
        sr.sortingOrder = 0;
    }

    // ---- Player ----

    void SpawnPlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-5f, -arenaHeight * 0.5f + 2f, 0f);

        var rb = player.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale   = 3f;

        var col  = player.AddComponent<CapsuleCollider2D>();
        col.size           = new Vector2(0.45f, 1.8f);
        col.sharedMaterial = CreateNoFrictionMaterial();

        var ctrl = player.AddComponent<PlayerController>();
        var combat = player.AddComponent<PlayerCombat>();
        player.AddComponent<PlayerHealth>();
        player.AddComponent<AbilityManager>();

        var figGO = new GameObject("PlayerFigure");
        figGO.transform.SetParent(player.transform);
        figGO.transform.localPosition = new Vector3(0f, 0.0f, 0f);
        var fig = figGO.AddComponent<PlayerSpriteAnimator>();
        fig.Init("Player/DrawCharacter", ctrl);
    }

    // ---- Sandbag (テスト用) ----

    void SpawnSandbag()
    {
        float groundY = -arenaHeight * 0.5f + 2f;
        var go = new GameObject("Sandbag");
        go.transform.position = new Vector3(3f, groundY, 0f);

        var rb = go.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale   = 3f;
        rb.constraints    = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;

        var col  = go.AddComponent<CapsuleCollider2D>();
        col.size   = new Vector2(1f, 2f);
        col.offset = new Vector2(0f, 0.2f);

        go.AddComponent<SandbagBoss>();

        // 見た目（茶色い丸太っぽい矩形）
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CreatePixelSprite();
        sr.color  = new Color(0.55f, 0.35f, 0.15f);
        sr.transform.localScale = new Vector3(1f, 2f, 1f);
        sr.sortingOrder = 1;
    }

    // ---- Util ----

    static PhysicsMaterial2D CreateNoFrictionMaterial()
    {
        var mat         = new PhysicsMaterial2D("NoFriction");
        mat.friction    = 0f;
        mat.bounciness  = 0f;
        return mat;
    }

    static Sprite CreatePixelSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
