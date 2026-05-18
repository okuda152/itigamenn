using UnityEngine;

/// <summary>
/// LineRenderer で描く蝶キャラクター。上翅2枚＋下翅2枚がはばたきアニメーション。
/// </summary>
public class ButterflyRenderer : MonoBehaviour
{
    public Color color     = new Color(0.85f, 0.20f, 0.95f);
    public float lineWidth = 0.06f;
    public float scale     = 1.0f;

    LineRenderer lrBody;
    LineRenderer lrHead;
    LineRenderer lrAntL, lrAntR;
    LineRenderer lrUWingR, lrUWingL;
    LineRenderer lrLWingR, lrLWingL;

    float time;

    void Start()
    {
        lrBody   = Make("Body",    16, loop: true,  w: lineWidth * 1.3f);
        lrHead   = Make("Head",    12, loop: true);
        lrAntL   = Make("AntL",     3);
        lrAntR   = Make("AntR",     3);
        lrUWingR = Make("UWingR",   5, loop: true,  w: lineWidth * 1.1f);
        lrUWingL = Make("UWingL",   5, loop: true,  w: lineWidth * 1.1f);
        lrLWingR = Make("LWingR",   5, loop: true);
        lrLWingL = Make("LWingL",   5, loop: true);
    }

    void Update()
    {
        time += Time.deltaTime;
        foreach (Transform child in transform)
            child.localScale = Vector3.one * scale;

        float flapY = Mathf.Sin(time * 8f) * 0.26f;
        DrawBody();
        DrawHead();
        DrawAntennae();
        DrawWings(flapY);
    }

    void DrawBody()
    {
        int n = lrBody.positionCount;
        for (int i = 0; i < n; i++)
        {
            float a = (float)i / n * Mathf.PI * 2f;
            lrBody.SetPosition(i, new Vector3(Mathf.Cos(a) * 0.09f, Mathf.Sin(a) * 0.30f, 0f));
        }
    }

    void DrawHead()
    {
        Vector3 hc = new Vector3(0f, 0.42f, 0f);
        int n = lrHead.positionCount;
        for (int i = 0; i < n; i++)
        {
            float a = (float)i / n * Mathf.PI * 2f;
            lrHead.SetPosition(i, hc + new Vector3(Mathf.Cos(a) * 0.11f, Mathf.Sin(a) * 0.11f, 0f));
        }
    }

    void DrawAntennae()
    {
        Vector3 top = new Vector3(0f, 0.53f, 0f);
        float bob   = Mathf.Sin(time * 4f) * 0.02f;
        lrAntL.SetPosition(0, top);
        lrAntL.SetPosition(1, top + new Vector3(-0.10f,  0.18f, 0f));
        lrAntL.SetPosition(2, top + new Vector3(-0.14f,  0.32f + bob, 0f));
        lrAntR.SetPosition(0, top);
        lrAntR.SetPosition(1, top + new Vector3( 0.10f,  0.18f, 0f));
        lrAntR.SetPosition(2, top + new Vector3( 0.14f,  0.32f + bob, 0f));
    }

    void DrawWings(float flapY)
    {
        // ---- Upper wings ----
        // offsets[i] = (x, baseY + flapFactor * flapY)
        var upOffsets = new Vector3[]
        {
            new Vector3(0f,    0f,                       0f),
            new Vector3(0.22f, 0.28f + flapY * 0.50f,   0f),
            new Vector3(0.50f, 0.18f + flapY * 1.00f,   0f),
            new Vector3(0.46f,-0.05f + flapY * 0.90f,   0f),
            new Vector3(0.09f,-0.12f + flapY * 0.15f,   0f),
        };
        Vector3 attachUR = new Vector3( 0.05f, 0.14f, 0f);
        Vector3 attachUL = new Vector3(-0.05f, 0.14f, 0f);
        SetWing(lrUWingR, attachUR, upOffsets, flip: false);
        SetWing(lrUWingL, attachUL, upOffsets, flip: true);

        // ---- Lower wings ----
        var loOffsets = new Vector3[]
        {
            new Vector3(0f,    0f,                       0f),
            new Vector3(0.36f,-0.05f + flapY * 0.75f,   0f),
            new Vector3(0.32f,-0.26f + flapY * 1.00f,   0f),
            new Vector3(0.11f,-0.26f + flapY * 0.90f,   0f),
            new Vector3(0.04f,-0.11f + flapY * 0.20f,   0f),
        };
        Vector3 attachLR = new Vector3( 0.05f,-0.07f, 0f);
        Vector3 attachLL = new Vector3(-0.05f,-0.07f, 0f);
        SetWing(lrLWingR, attachLR, loOffsets, flip: false);
        SetWing(lrLWingL, attachLL, loOffsets, flip: true);
    }

    void SetWing(LineRenderer lr, Vector3 attach, Vector3[] offsets, bool flip)
    {
        float fx = flip ? -1f : 1f;
        for (int i = 0; i < offsets.Length && i < lr.positionCount; i++)
        {
            var o = offsets[i];
            lr.SetPosition(i, attach + new Vector3(o.x * fx, o.y, 0f));
        }
    }

    LineRenderer Make(string n, int pts, bool loop = false, float w = -1f)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = pts;
        float width = w < 0f ? lineWidth : w;
        lr.startWidth = lr.endWidth = width;
        lr.useWorldSpace = false;
        lr.loop = loop;
        lr.sortingOrder = 5;
        var shader = Shader.Find("Sprites/Default")
                  ?? Shader.Find("Universal Render Pipeline/Unlit");
        var mat = new Material(shader);
        mat.color = color;
        lr.material = mat;
        lr.startColor = lr.endColor = color;
        return lr;
    }

    public void SetColor(Color c)
    {
        color = c;
        foreach (var lr in GetComponentsInChildren<LineRenderer>())
        {
            lr.startColor = lr.endColor = c;
            if (lr.material) lr.material.color = c;
        }
    }
}
