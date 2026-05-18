using UnityEngine;

/// <summary>
/// LineRenderer で横向き二足歩行恐竜（T-rex風）を描画します。
/// </summary>
public class DinoRenderer : MonoBehaviour
{
    public Color color       = new Color(0.18f, 0.52f, 0.18f);
    public float lineWidth   = 0.08f;
    public float scale       = 1.0f;
    public bool  facingRight = true;
    public bool  isWalking   = false;
    public bool  isCharging  = false;
    public bool  isWindingUp = false;

    LineRenderer lrBody, lrHead, lrJaw, lrTail, lrArm;
    LineRenderer lrLLegU, lrLLegL, lrRLegU, lrRLegL;

    float time;

    void Start()
    {
        lrBody  = Make("Body",  18, loop: true,  w: lineWidth * 1.4f);
        lrHead  = Make("Head",   8, loop: true);
        lrJaw   = Make("Jaw",    3);
        lrTail  = Make("Tail",   4,              w: lineWidth * 1.2f);
        lrArm   = Make("Arm",    3);
        lrLLegU = Make("LLegU",  2,              w: lineWidth * 1.7f);
        lrLLegL = Make("LLegL",  2,              w: lineWidth * 1.7f);
        lrRLegU = Make("RLegU",  2,              w: lineWidth * 1.7f);
        lrRLegL = Make("RLegL",  2,              w: lineWidth * 1.7f);
    }

    void Update()
    {
        time += Time.deltaTime;
        foreach (Transform child in transform)
            child.localScale = Vector3.one * scale;

        float f    = facingRight ? 1f : -1f;
        float walk = isWalking ? Mathf.Sin(time * 7f) : 0f;

        // 突進中は前傾姿勢・溜め中は後傾
        float lean = isCharging ? 0.18f * f : (isWindingUp ? -0.12f * f : 0f);

        DrawBody(f, walk, lean);
        DrawHead(f, lean);
        DrawTail(f, lean);
        DrawArm(f, walk, lean);

        if (isCharging) DrawChargeLegs(f);
        else            DrawLegs(f, walk);
    }

    // ---- Body ----
    void DrawBody(float f, float walk, float lean)
    {
        float dropY = isCharging ? -0.06f : 0f;
        Vector3 c = new Vector3(lean * 0.3f, 0.12f + Mathf.Abs(walk) * 0.018f + dropY, 0f);
        float bw = 0.46f, bh = 0.28f;
        int n = lrBody.positionCount;
        for (int i = 0; i < n; i++)
        {
            float a = (float)i / n * Mathf.PI * 2f;
            float stretch = 1f + Mathf.Cos(a) * 0.15f * f;
            lrBody.SetPosition(i, c + new Vector3(
                Mathf.Cos(a) * bw * f * stretch,
                Mathf.Sin(a) * bh, 0f));
        }
    }

    // ---- Head ----
    void DrawHead(float f, float lean)
    {
        float dropY = isCharging ? -0.10f : 0f;
        Vector3 hc = new Vector3((0.60f + lean * 0.2f) * f, 0.56f + dropY, 0f);

        // 突進中は口を大きく開く
        float jawMult = isCharging ? 3.0f : 1.0f;
        float jaw = (0.04f + Mathf.Abs(Mathf.Sin(time * (isCharging ? 6f : 0.9f))) * 0.03f) * jawMult;

        Vector3[] pts =
        {
            hc + new Vector3(-0.22f * f,  0.04f, 0f),
            hc + new Vector3(-0.04f * f,  0.18f, 0f),
            hc + new Vector3( 0.18f * f,  0.13f, 0f),
            hc + new Vector3( 0.30f * f,  0.01f, 0f),
            hc + new Vector3( 0.25f * f, -0.10f, 0f),
            hc + new Vector3( 0.02f * f, -0.18f, 0f),
            hc + new Vector3(-0.17f * f, -0.14f, 0f),
            hc + new Vector3(-0.25f * f, -0.02f, 0f),
        };
        if (lrHead.positionCount != pts.Length) lrHead.positionCount = pts.Length;
        for (int i = 0; i < pts.Length; i++) lrHead.SetPosition(i, pts[i]);

        lrJaw.SetPosition(0, hc + new Vector3( 0.30f * f,  0.01f, 0f));
        lrJaw.SetPosition(1, hc + new Vector3( 0.10f * f, -0.09f - jaw, 0f));
        lrJaw.SetPosition(2, hc + new Vector3(-0.08f * f, -0.16f - jaw * 0.5f, 0f));
    }

    // ---- Tail ----
    void DrawTail(float f, float lean)
    {
        // 突進中はしっぽを水平に持ち上げてバランス強調
        float liftY = isCharging ? 0.25f : 0f;
        Vector3 root = new Vector3((-0.42f - lean * 0.15f) * f, 0.08f, 0f);
        lrTail.SetPosition(0, root);
        lrTail.SetPosition(1, root + new Vector3(-0.28f * f, -0.04f + liftY * 0.5f, 0f));
        lrTail.SetPosition(2, root + new Vector3(-0.58f * f, -0.20f + liftY,        0f));
        lrTail.SetPosition(3, root + new Vector3(-0.80f * f, -0.42f + liftY * 1.5f, 0f));
    }

    // ---- Arm ----
    void DrawArm(float f, float walk, float lean)
    {
        // 突進中は腕を後ろに引く
        float pullBack = isCharging ? -0.12f * f : 0f;
        Vector3 base_ = new Vector3((0.28f + lean * 0.1f) * f, 0.10f, 0f);
        lrArm.SetPosition(0, base_);
        lrArm.SetPosition(1, base_ + new Vector3( 0.14f * f + pullBack, -0.11f + walk * 0.04f, 0f));
        lrArm.SetPosition(2, base_ + new Vector3( 0.19f * f + pullBack, -0.25f + walk * 0.02f, 0f));
    }

    // ---- 通常歩行レッグ ----
    void DrawLegs(float f, float walk)
    {
        float lw =  walk;
        float rw = -walk;

        Vector3 lHip  = new Vector3(-0.12f * f, -0.18f, 0f);
        Vector3 lKnee = lHip  + new Vector3(-0.04f * f + lw * 0.10f, -0.32f, 0f);
        Vector3 lFoot = lKnee + new Vector3( 0.06f * f + lw * 0.06f, -0.34f + Mathf.Max(0f, lw * 0.06f), 0f);
        lrLLegU.SetPosition(0, lHip);  lrLLegU.SetPosition(1, lKnee);
        lrLLegL.SetPosition(0, lKnee); lrLLegL.SetPosition(1, lFoot);

        Vector3 rHip  = new Vector3( 0.12f * f, -0.18f, 0f);
        Vector3 rKnee = rHip  + new Vector3( 0.04f * f + rw * 0.10f, -0.32f, 0f);
        Vector3 rFoot = rKnee + new Vector3(-0.06f * f + rw * 0.06f, -0.34f + Mathf.Max(0f, rw * 0.06f), 0f);
        lrRLegU.SetPosition(0, rHip);  lrRLegU.SetPosition(1, rKnee);
        lrRLegL.SetPosition(0, rKnee); lrRLegL.SetPosition(1, rFoot);
    }

    // ---- 突進レッグ（大きなストライド・高速） ----
    void DrawChargeLegs(float f)
    {
        float t  = time * 16f;
        float ls = Mathf.Sin(t);
        float rs = Mathf.Sin(t + Mathf.PI);

        Vector3 lHip  = new Vector3(-0.12f * f, -0.22f, 0f);
        Vector3 lKnee = lHip  + new Vector3(-0.05f * f + ls * 0.20f, -0.28f, 0f);
        Vector3 lFoot = lKnee + new Vector3( 0.08f * f + ls * 0.12f, -0.32f + Mathf.Max(0f, ls * 0.10f), 0f);
        lrLLegU.SetPosition(0, lHip);  lrLLegU.SetPosition(1, lKnee);
        lrLLegL.SetPosition(0, lKnee); lrLLegL.SetPosition(1, lFoot);

        Vector3 rHip  = new Vector3( 0.12f * f, -0.22f, 0f);
        Vector3 rKnee = rHip  + new Vector3( 0.05f * f + rs * 0.20f, -0.28f, 0f);
        Vector3 rFoot = rKnee + new Vector3(-0.08f * f + rs * 0.12f, -0.32f + Mathf.Max(0f, rs * 0.10f), 0f);
        lrRLegU.SetPosition(0, rHip);  lrRLegU.SetPosition(1, rKnee);
        lrRLegL.SetPosition(0, rKnee); lrRLegL.SetPosition(1, rFoot);
    }

    // ---- Utils ----
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
        var shader = Shader.Find("Sprites/Default") ?? Shader.Find("Universal Render Pipeline/Unlit");
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
