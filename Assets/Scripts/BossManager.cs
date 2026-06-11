using UnityEngine;
using System.Collections;

/// <summary>
/// ボスの出現順序を管理します。
/// ArenaBuilder から生成され、ボス1撃破後にボス2を登場させます。
/// </summary>
public class BossManager : MonoBehaviour
{
    public float arenaWidth  = 18f;
    public float arenaHeight = 10f;

    bool   showMessage = false;
    string message     = "";

    void Start()
    {
        BossDummy.OnDied     += OnDinoDied;
        ButterflyBoss.OnDied += OnButterflyDied;
        SpawnDinoBoss();
    }

    void OnDestroy()
    {
        BossDummy.OnDied     -= OnDinoDied;
        ButterflyBoss.OnDied -= OnButterflyDied;
    }

    // ---- Event handlers ----

    void OnDinoDied()      => ShowAbilitySelect(DinoOffers(),      () => StartCoroutine(Transition(SpawnButterflyBoss, "N E X T  B O S S")));
    void OnButterflyDied() => ShowAbilitySelect(ButterflyOffers(), () => StartCoroutine(ClearSequence()));

    void ShowAbilitySelect(AbilitySelectUI.AbilityOffer[] offers, System.Action callback)
    {
        var player = GameObject.FindWithTag("Player");
        var am = player?.GetComponent<AbilityManager>();
        if (AbilitySelectUI.Instance != null && am != null)
            AbilitySelectUI.Instance.Show(offers, am, callback);
        else
            callback();
    }

    AbilitySelectUI.AbilityOffer[] DinoOffers() => new[]
    {
        new AbilitySelectUI.AbilityOffer
        {
            name       = "移動速度強化",
            desc       = "移動速度が 1.5倍 になる。\n恐竜の力強さを奪え。",
            slotLabel  = "移動強化スロット",
            isMovement = true,
            movement   = MovementAbility.SpeedBoost,
            color      = new Color(1f, 0.5f, 0.1f)
        },
        new AbilitySelectUI.AbilityOffer
        {
            name       = "亜空間タックル",
            desc       = "Rキーで高速突進し\n接触した敵にダメージ。\n恐竜の突撃を奪え。",
            slotLabel  = "必殺技スロット [R]",
            isMovement = false,
            special    = SpecialAbility.SubspaceTackle,
            color      = new Color(0.5f, 0.1f, 1f)
        }
    };

    AbilitySelectUI.AbilityOffer[] ButterflyOffers() => new[]
    {
        new AbilitySelectUI.AbilityOffer
        {
            name       = "二段ジャンプ",
            desc       = "空中でもう一度\nジャンプできる。\n蝶の飛翔力を奪え。",
            slotLabel  = "移動強化スロット",
            isMovement = true,
            movement   = MovementAbility.DoubleJump,
            color      = new Color(0.2f, 0.8f, 1f)
        },
        new AbilitySelectUI.AbilityOffer
        {
            name       = "弾幕攻撃",
            desc       = "Rキーで前方に\n5発の弾を放つ。\n蝶の乱射を奪え。",
            slotLabel  = "必殺技スロット [R]",
            isMovement = false,
            special    = SpecialAbility.BulletBarrage,
            color      = new Color(0.2f, 1f, 0.4f)
        }
    };

    IEnumerator ClearSequence()
    {
        message     = "C L E A R !";
        showMessage = true;
        yield return new WaitForSeconds(3f);
        ReturnToStart();
    }

    void ReturnToStart()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.GetComponent<Camera>() != null) continue;
            Destroy(go);
        }

        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = Color.black;
            cam.orthographicSize = 5.5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
        }

        new GameObject("EffectManager").AddComponent<EffectManager>();
        new GameObject("StartScreen").AddComponent<StartScreen>();
    }

    IEnumerator Transition(System.Action spawn, string msg)
    {
        message     = msg;
        showMessage = true;
        yield return new WaitForSeconds(2.5f);
        showMessage = false;
        spawn();
    }

    // ---- Boss Spawners ----

    void SpawnDinoBoss()
    {
        float groundY = -arenaHeight * 0.5f + 2f;
        var boss = new GameObject("Boss");
        boss.transform.position = new Vector3(4f, groundY, 0f);

        var rb = boss.AddComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        rb.gravityScale   = 3f;

        var col  = boss.AddComponent<CapsuleCollider2D>();
        col.size   = new Vector2(1.4f, 2.4f);
        col.offset = new Vector2(0f, 0.4f);

        boss.AddComponent<BossDummy>();

        var figGO = new GameObject("DinoFigure");
        figGO.transform.SetParent(boss.transform);
        figGO.transform.localPosition = new Vector3(0f, 0.15f, 0f);
        var vis = figGO.AddComponent<FantasyCharacterVisual>();
        vis.Init("Characters/Character (58)", scale: 1.8f, flipX: true);
    }

    void SpawnButterflyBoss()
    {
        var boss = new GameObject("ButterflyBoss");
        boss.transform.position = new Vector3(0f, arenaHeight * 0.15f, 0f);

        var col  = boss.AddComponent<CapsuleCollider2D>();
        col.size   = new Vector2(1.2f, 1.8f);
        col.offset = new Vector2(0f, 0.1f);

        var bboss         = boss.AddComponent<ButterflyBoss>();
        bboss.arenaHalfW  = arenaWidth  * 0.5f - 1.5f;
        bboss.arenaHalfH  = arenaHeight * 0.5f - 1.0f;
        bboss.minHoverY   = -arenaHeight * 0.5f + 2.5f;

        var figGO = new GameObject("ButterflyFigure");
        figGO.transform.SetParent(boss.transform);
        figGO.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        var vis = figGO.AddComponent<FantasyCharacterVisual>();
        vis.Init("Characters/Character (71)", scale: 1.5f);
    }

    // ---- UI ----

    void OnGUI()
    {
        if (!showMessage) return;

        float sw = Screen.width;
        float sh = Screen.height;

        GUI.color = new Color(0f, 0f, 0f, 0.65f);
        GUI.DrawTexture(new Rect(0f, sh * 0.44f, sw, 64f), Texture2D.whiteTexture);

        var style = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle  = FontStyle.Bold,
            fontSize   = 30,
            normal     = { textColor = Color.white }
        };
        GUI.color = Color.white;
        GUI.Label(new Rect(0f, sh * 0.44f, sw, 64f), message, style);
    }
}
