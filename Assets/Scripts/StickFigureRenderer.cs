using UnityEngine;

public class StickFigureRenderer : MonoBehaviour
{
    [Header("Style")]
    public Color color      = Color.black;
    public float lineWidth  = 0.07f;
    public float headRadius = 0.18f;

    [Header("References (optional)")]
    public PlayerController playerController;
    public PlayerCombat     playerCombat;

    enum State { Idle, Run, Jump, Fall, WallSlide, Punch, Kick }
    State state;
    float time;

    LineRenderer lrHead;
    LineRenderer lrBody;
    LineRenderer lrLArmU, lrLArmL;
    LineRenderer lrRArmU, lrRArmL;
    LineRenderer lrLLegU, lrLLegL;
    LineRenderer lrRLegU, lrRLegL;

    Vector3 head, neck, waist;
    Vector3 lShoulder, lElbow, lHand;
    Vector3 rShoulder, rElbow, rHand;
    Vector3 lHip, lKnee, lFoot;
    Vector3 rHip, rKnee, rFoot;

    const float BODY_H     = 0.50f;
    const float ARM_H      = 0.38f;
    const float LEG_H      = 0.48f;
    const float SHOULDER_W = 0.14f;
    const float HIP_W      = 0.10f;

    void Start()
    {
        lrHead  = MakeLR("Head",  18, loop: true);
        lrBody  = MakeLR("Body",   2);
        lrLArmU = MakeLR("LArmU",  2);
        lrLArmL = MakeLR("LArmL",  2);
        lrRArmU = MakeLR("RArmU",  2);
        lrRArmL = MakeLR("RArmL",  2);
        lrLLegU = MakeLR("LLegU",  2);
        lrLLegL = MakeLR("LLegL",  2);
        lrRLegU = MakeLR("RLegU",  2);
        lrRLegL = MakeLR("RLegL",  2);
    }

    void Update()
    {
        time += Time.deltaTime;
        UpdateState();
        SetBaseJoints();
        ApplyPose();
        Draw();
    }

    void UpdateState()
    {
        if (playerController == null) { state = State.Idle; return; }

        bool attacking = playerCombat != null && playerCombat.IsAttacking;

        if      (attacking)                         state = State.Punch;
        else if (playerController.IsWallSliding)    state = State.WallSlide;
        else if (!playerController.IsGrounded)
        {
            var rb = playerController.GetComponent<Rigidbody2D>();
            state = (rb && rb.linearVelocity.y < -0.5f) ? State.Fall : State.Jump;
        }
        else if (Mathf.Abs(playerController.MoveInput) > 0.1f) state = State.Run;
        else                                                     state = State.Idle;
    }

    void SetBaseJoints()
    {
        bool facingRight = playerController == null || playerController.FacingRight;
        float f = facingRight ? 1f : -1f;

        waist     = new Vector3(0f,  0.10f, 0f);
        neck      = new Vector3(0f,  waist.y + BODY_H, 0f);
        head      = new Vector3(0f,  neck.y + headRadius * 1.1f, 0f);
        lShoulder = new Vector3(-SHOULDER_W * f, neck.y - 0.04f, 0f);
        rShoulder = new Vector3( SHOULDER_W * f, neck.y - 0.04f, 0f);
        lHip      = new Vector3(-HIP_W * f, waist.y, 0f);
        rHip      = new Vector3( HIP_W * f, waist.y, 0f);
    }

    void ApplyPose()
    {
        bool facingRight = playerController == null || playerController.FacingRight;
        float f = facingRight ? 1f : -1f;

        switch (state)
        {
            case State.Idle:      PoseIdle(f);      break;
            case State.Run:       PoseRun(f);       break;
            case State.Jump:      PoseJump(f);      break;
            case State.Fall:      PoseFall(f);      break;
            case State.WallSlide: PoseWallSlide(f); break;
            case State.Punch:     PosePunch(f);     break;
            case State.Kick:      PoseKick(f);      break;
        }
    }

    void PoseIdle(float f)
    {
        float b = Mathf.Sin(time * 1.2f) * 0.012f;

        lElbow = lShoulder + new Vector3(-0.06f * f, -ARM_H * 0.50f + b, 0f);
        lHand  = lElbow    + new Vector3(-0.04f * f, -ARM_H * 0.50f,     0f);
        rElbow = rShoulder + new Vector3( 0.06f * f, -ARM_H * 0.50f + b, 0f);
        rHand  = rElbow    + new Vector3( 0.04f * f, -ARM_H * 0.50f,     0f);

        lKnee = lHip + new Vector3(-0.06f * f, -LEG_H * 0.50f, 0f);
        lFoot = lKnee + new Vector3(-0.06f * f, -LEG_H * 0.50f, 0f);
        rKnee = rHip + new Vector3( 0.06f * f, -LEG_H * 0.50f, 0f);
        rFoot = rKnee + new Vector3( 0.06f * f, -LEG_H * 0.50f, 0f);
    }

    void PoseRun(float f)
    {
        float t  = time * 9f;
        float ls = Mathf.Sin(t);
        float rs = Mathf.Sin(t + Mathf.PI);

        lElbow = lShoulder + new Vector3(rs * 0.16f * f, -ARM_H * 0.46f, 0f);
        lHand  = lElbow    + new Vector3(rs * 0.12f * f, -ARM_H * 0.46f, 0f);
        rElbow = rShoulder + new Vector3(ls * 0.16f * f, -ARM_H * 0.46f, 0f);
        rHand  = rElbow    + new Vector3(ls * 0.12f * f, -ARM_H * 0.46f, 0f);

        lKnee = lHip + new Vector3(-0.06f * f + ls * 0.22f * f, -LEG_H * 0.46f, 0f);
        lFoot = lKnee + new Vector3(-0.06f * f + ls * 0.14f * f, -LEG_H * 0.52f, 0f);
        rKnee = rHip + new Vector3( 0.06f * f + rs * 0.22f * f, -LEG_H * 0.46f, 0f);
        rFoot = rKnee + new Vector3( 0.06f * f + rs * 0.14f * f, -LEG_H * 0.52f, 0f);
    }

    void PoseJump(float f)
    {
        lElbow = lShoulder + new Vector3(-0.10f * f,  ARM_H * 0.28f, 0f);
        lHand  = lElbow    + new Vector3(-0.08f * f,  ARM_H * 0.26f, 0f);
        rElbow = rShoulder + new Vector3( 0.10f * f,  ARM_H * 0.28f, 0f);
        rHand  = rElbow    + new Vector3( 0.08f * f,  ARM_H * 0.26f, 0f);

        lKnee = lHip + new Vector3(-0.08f * f, -LEG_H * 0.32f, 0f);
        lFoot = lKnee + new Vector3(-0.06f * f, -LEG_H * 0.36f, 0f);
        rKnee = rHip + new Vector3( 0.08f * f, -LEG_H * 0.32f, 0f);
        rFoot = rKnee + new Vector3( 0.06f * f, -LEG_H * 0.36f, 0f);
    }

    void PoseFall(float f)
    {
        lElbow = lShoulder + new Vector3(-0.12f * f, -ARM_H * 0.14f, 0f);
        lHand  = lElbow    + new Vector3(-0.08f * f, -ARM_H * 0.48f, 0f);
        rElbow = rShoulder + new Vector3( 0.12f * f, -ARM_H * 0.14f, 0f);
        rHand  = rElbow    + new Vector3( 0.08f * f, -ARM_H * 0.48f, 0f);

        lKnee = lHip + new Vector3(-0.06f * f, -LEG_H * 0.48f, 0f);
        lFoot = lKnee + new Vector3(-0.08f * f, -LEG_H * 0.50f, 0f);
        rKnee = rHip + new Vector3( 0.06f * f, -LEG_H * 0.48f, 0f);
        rFoot = rKnee + new Vector3( 0.08f * f, -LEG_H * 0.50f, 0f);
    }

    void PoseWallSlide(float f)
    {
        lElbow = lShoulder + new Vector3( 0.26f * f,  0.04f, 0f);
        lHand  = lElbow    + new Vector3( 0.20f * f, -0.04f, 0f);
        rElbow = rShoulder + new Vector3(-0.06f * f, -ARM_H * 0.44f, 0f);
        rHand  = rElbow    + new Vector3( 0.00f,     -ARM_H * 0.44f, 0f);

        lKnee = lHip + new Vector3( 0.08f * f, -LEG_H * 0.50f, 0f);
        lFoot = lKnee + new Vector3( 0.14f * f, -LEG_H * 0.48f, 0f);
        rKnee = rHip + new Vector3(-0.04f * f, -LEG_H * 0.50f, 0f);
        rFoot = rKnee + new Vector3(-0.10f * f, -LEG_H * 0.48f, 0f);
    }

    void PosePunch(float f)
    {
        lElbow = lShoulder + new Vector3(-0.06f * f, -ARM_H * 0.44f, 0f);
        lHand  = lElbow    + new Vector3(-0.04f * f, -ARM_H * 0.46f, 0f);
        rElbow = rShoulder + new Vector3( ARM_H * 0.46f * f,  0.04f, 0f);
        rHand  = rElbow    + new Vector3( ARM_H * 0.50f * f,  0.00f, 0f);

        lKnee = lHip + new Vector3(-0.08f * f, -LEG_H * 0.50f, 0f);
        lFoot = lKnee + new Vector3(-0.06f * f, -LEG_H * 0.50f, 0f);
        rKnee = rHip + new Vector3( 0.10f * f, -LEG_H * 0.48f, 0f);
        rFoot = rKnee + new Vector3( 0.08f * f, -LEG_H * 0.48f, 0f);
    }

    void PoseKick(float f)
    {
        lElbow = lShoulder + new Vector3(-ARM_H * 0.20f * f,  ARM_H * 0.12f, 0f);
        lHand  = lElbow    + new Vector3(-ARM_H * 0.10f * f, -ARM_H * 0.26f, 0f);
        rElbow = rShoulder + new Vector3(-ARM_H * 0.26f * f,  ARM_H * 0.16f, 0f);
        rHand  = rElbow    + new Vector3(-ARM_H * 0.08f * f,  ARM_H * 0.12f, 0f);

        lKnee = lHip + new Vector3(-0.06f * f,           -LEG_H * 0.50f, 0f);
        lFoot = lKnee + new Vector3(-0.06f * f,           -LEG_H * 0.50f, 0f);
        rKnee = rHip + new Vector3( LEG_H * 0.40f * f,    LEG_H * 0.10f, 0f);
        rFoot = rKnee + new Vector3( LEG_H * 0.44f * f,  -LEG_H * 0.10f, 0f);
    }

    public Vector3 ThrowHandWorld => transform.TransformPoint(rHand);

    void Draw()
    {
        Set2(lrBody,  neck,      waist);
        Set2(lrLArmU, lShoulder, lElbow);
        Set2(lrLArmL, lElbow,    lHand);
        Set2(lrRArmU, rShoulder, rElbow);
        Set2(lrRArmL, rElbow,    rHand);
        Set2(lrLLegU, lHip,      lKnee);
        Set2(lrLLegL, lKnee,     lFoot);
        Set2(lrRLegU, rHip,      rKnee);
        Set2(lrRLegL, rKnee,     rFoot);

        int n = lrHead.positionCount;
        for (int i = 0; i < n; i++)
        {
            float a = (float)i / n * Mathf.PI * 2f;
            lrHead.SetPosition(i, head + new Vector3(
                Mathf.Cos(a) * headRadius,
                Mathf.Sin(a) * headRadius, 0f));
        }
    }

    void Set2(LineRenderer lr, Vector3 a, Vector3 b)
    {
        lr.SetPosition(0, a);
        lr.SetPosition(1, b);
    }

    LineRenderer MakeLR(string goName, int points, bool loop = false)
    {
        var go = new GameObject(goName);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = points;
        lr.startWidth = lr.endWidth = lineWidth;
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
