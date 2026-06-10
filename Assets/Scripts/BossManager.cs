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

    void OnDinoDied()     => StartCoroutine(Transition(SpawnButterflyBoss, "N E X T  B O S S"));
    void OnButterflyDied() => StartCoroutine(ClearSequence());

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
        col.size = new Vector2(0.45f, 1.8f);

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
        col.size = new Vector2(0.8f, 0.9f);

        var bboss         = boss.AddComponent<ButterflyBoss>();
        bboss.arenaHalfW  = arenaWidth  * 0.5f - 1.5f;
        bboss.arenaHalfH  = arenaHeight * 0.5f - 1.0f;
        bboss.minHoverY   = -arenaHeight * 0.5f + 2.5f;

        var figGO = new GameObject("ButterflyFigure");
        figGO.transform.SetParent(boss.transform);
        figGO.transform.localPosition = Vector3.zero;
        var vis = figGO.AddComponent<FantasyCharacterVisual>();
        vis.Init("Characters/Character (71)", scale: 1.8f);
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
