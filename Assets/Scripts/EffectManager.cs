using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour
{
    static EffectManager instance;

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
    }

    // ---- Public API ----

    /// <summary>パンチ・キックのヒットスパーク（放射線）</summary>
    public static void HitSpark(Vector2 pos, Color color)
        => instance?.StartCoroutine(instance.HitSparkCo(pos, color));

    /// <summary>ヒット時に広がる衝撃リング</summary>
    public static void HitRing(Vector2 pos, Color color)
        => instance?.StartCoroutine(instance.HitRingCo(pos, color));

    /// <summary>着地ほこり（左右に弧）</summary>
    public static void LandDust(Vector2 pos)
        => instance?.StartCoroutine(instance.LandDustCo(pos));

    /// <summary>突進中の速度線残像（都度呼ぶ）</summary>
    public static void ChargePuff(Vector2 pos, bool facingRight, Color color)
        => instance?.StartCoroutine(instance.ChargePuffCo(pos, facingRight, color));

    /// <summary>溜め中の集中線エフェクト</summary>
    public static void FocusLines(Vector2 pos, float duration)
        => instance?.StartCoroutine(instance.FocusLinesCo(pos, duration));

    /// <summary>死亡爆発</summary>
    public static void DeathBurst(Vector2 pos, Color color)
        => instance?.StartCoroutine(instance.DeathBurstCo(pos, color));

    // ---- ヒットスパーク ----

    IEnumerator HitSparkCo(Vector2 pos, Color color)
    {
        int n = 8;
        var gos = new GameObject[n];
        var lrs = new LineRenderer[n];
        for (int i = 0; i < n; i++)
        {
            float angle  = (i / (float)n * 360f + Random.Range(-22f, 22f)) * Mathf.Deg2Rad;
            float inner  = Random.Range(0.05f, 0.12f);
            float outer  = inner + Random.Range(0.2f, 0.5f);
            var   dir    = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            gos[i] = Spawn("Spark", pos);
            lrs[i] = MakeLR(gos[i], color, Random.Range(0.03f, 0.06f));
            lrs[i].SetPosition(0, dir * inner);
            lrs[i].SetPosition(1, dir * outer);
        }
        yield return Fade(lrs, color, 0.18f);
        foreach (var go in gos) if (go) Destroy(go);
    }

    // ---- ヒットリング ----

    IEnumerator HitRingCo(Vector2 pos, Color color)
    {
        const int segs = 24;
        var go = Spawn("HitRing", pos);
        var lr = MakeLR(go, color, 0.04f, segs);
        lr.loop = true;

        float dur = 0.25f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float p = t / dur;
            float r = Mathf.Lerp(0.08f, 0.75f, p);
            SetAlpha(lr, color, 1f - p);
            for (int i = 0; i < segs; i++)
            {
                float a = (float)i / segs * Mathf.PI * 2f;
                lr.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
            }
            yield return null;
        }
        Destroy(go);
    }

    // ---- 着地ほこり ----

    IEnumerator LandDustCo(Vector2 pos)
    {
        Color dust = new Color(0.45f, 0.42f, 0.38f);
        int   cnt  = 4;
        var   gos  = new GameObject[cnt * 2];
        var   lrs  = new LineRenderer[cnt * 2];

        for (int i = 0; i < cnt; i++)
        {
            float ratio = (float)(i + 1) / cnt;
            float angle = Mathf.Lerp(8f, 65f, ratio) * Mathf.Deg2Rad;
            float len   = Mathf.Lerp(0.35f, 0.15f, ratio);
            var   dirR  = new Vector3( Mathf.Cos(angle) * len, Mathf.Sin(angle) * len * 0.6f, 0f);
            var   origR = new Vector3( ratio * 0.25f, 0f, 0f);

            gos[i]       = Spawn("DustR", pos); lrs[i]       = MakeLR(gos[i], dust, 0.035f);
            lrs[i].SetPosition(0, origR); lrs[i].SetPosition(1, origR + dirR);

            gos[cnt + i] = Spawn("DustL", pos); lrs[cnt + i] = MakeLR(gos[cnt + i], dust, 0.035f);
            lrs[cnt + i].SetPosition(0, new Vector3(-origR.x, 0, 0));
            lrs[cnt + i].SetPosition(1, new Vector3(-origR.x - dirR.x, dirR.y, 0));
        }
        yield return Fade(lrs, dust, 0.28f);
        foreach (var go in gos) if (go) Destroy(go);
    }

    // ---- 突進速度線 ----

    IEnumerator ChargePuffCo(Vector2 pos, bool facingRight, Color color)
    {
        float dir = facingRight ? -1f : 1f;
        int   cnt = 5;
        var   gos = new GameObject[cnt];
        var   lrs = new LineRenderer[cnt];

        for (int i = 0; i < cnt; i++)
        {
            float oy  = Random.Range(-0.5f, 0.7f);
            float len = Random.Range(0.25f, 0.65f);
            gos[i] = Spawn("Puff", pos + new Vector2(0f, oy));
            lrs[i] = MakeLR(gos[i], color, 0.04f);
            lrs[i].SetPosition(0, Vector3.zero);
            lrs[i].SetPosition(1, new Vector3(dir * len, 0f, 0f));
        }
        yield return Fade(lrs, color, 0.12f);
        foreach (var go in gos) if (go) Destroy(go);
    }

    // ---- 溜め集中線 ----

    IEnumerator FocusLinesCo(Vector2 pos, float duration)
    {
        Color fc   = new Color(1f, 0.88f, 0.1f);
        int   cnt  = 16;
        var   gos  = new GameObject[cnt];
        var   lrs  = new LineRenderer[cnt];

        for (int i = 0; i < cnt; i++)
        {
            gos[i] = Spawn("Focus", pos);
            lrs[i] = MakeLR(gos[i], fc, 0.03f);
        }

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float p     = t / duration;
            float pulse = Mathf.Sin(t * 9f) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(0.25f, 0.9f, pulse) * Mathf.Clamp01(p * 4f);

            for (int i = 0; i < cnt; i++)
            {
                if (!lrs[i]) continue;
                float angle  = (float)i / cnt * Mathf.PI * 2f;
                float inner  = Mathf.Lerp(0.25f, 0.7f, pulse);
                float outer  = inner + Mathf.Lerp(0.4f, 1.4f, p);
                var   d      = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                lrs[i].SetPosition(0, d * inner);
                lrs[i].SetPosition(1, d * outer);
                SetAlpha(lrs[i], fc, alpha);
            }
            yield return null;
        }

        // フェードアウト
        yield return Fade(lrs, fc, 0.12f);
        foreach (var go in gos) if (go) Destroy(go);
    }

    // ---- 死亡爆発 ----

    IEnumerator DeathBurstCo(Vector2 pos, Color color)
    {
        // 3 波のスパーク＋リング
        for (int wave = 0; wave < 3; wave++)
        {
            StartCoroutine(HitSparkCo(pos + (Vector2)Random.insideUnitCircle * 0.4f, color));
            StartCoroutine(HitRingCo(pos, color));
            yield return new WaitForSeconds(0.08f);
        }

        // 最後に大爆発ライン
        int cnt = 14;
        var gos  = new GameObject[cnt];
        var lrs  = new LineRenderer[cnt];
        var dirs = new Vector3[cnt];
        var lens = new float[cnt];

        for (int i = 0; i < cnt; i++)
        {
            float a = (float)i / cnt * Mathf.PI * 2f;
            dirs[i] = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f);
            lens[i] = Random.Range(0.7f, 1.4f);
            gos[i]  = Spawn("Burst", pos);
            lrs[i]  = MakeLR(gos[i], color, 0.07f);
            lrs[i].SetPosition(0, dirs[i] * 0.08f);
            lrs[i].SetPosition(1, dirs[i] * lens[i]);
        }

        float dur = 0.45f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float p = t / dur;
            for (int i = 0; i < cnt; i++)
            {
                if (!lrs[i]) continue;
                lrs[i].SetPosition(0, dirs[i] * Mathf.Lerp(0.08f, lens[i] * 0.4f, p));
                lrs[i].SetPosition(1, dirs[i] * Mathf.Lerp(lens[i], lens[i] * 1.6f, p));
                SetAlpha(lrs[i], color, 1f - p);
            }
            yield return null;
        }
        foreach (var go in gos) if (go) Destroy(go);
    }

    // ---- Helpers ----

    static GameObject Spawn(string n, Vector2 pos)
    {
        var go = new GameObject(n);
        go.transform.position = pos;
        return go;
    }

    LineRenderer MakeLR(GameObject go, Color color, float width, int points = 2)
    {
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = points;
        lr.startWidth = lr.endWidth = width;
        lr.useWorldSpace = false;
        lr.sortingOrder  = 10;
        var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
        lr.material = new Material(shader);
        lr.startColor = lr.endColor = color;
        return lr;
    }

    void SetAlpha(LineRenderer lr, Color base_, float a)
    {
        var c = new Color(base_.r, base_.g, base_.b, a);
        lr.startColor = lr.endColor = c;
        if (lr.material) lr.material.color = c;
    }

    IEnumerator Fade(LineRenderer[] lrs, Color base_, float dur)
    {
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float a = 1f - t / dur;
            foreach (var lr in lrs) if (lr) SetAlpha(lr, base_, a);
            yield return null;
        }
    }
}
