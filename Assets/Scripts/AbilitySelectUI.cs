using UnityEngine;
using System;

public class AbilitySelectUI : MonoBehaviour
{
    public static AbilitySelectUI Instance { get; private set; }

    public struct AbilityOffer
    {
        public string          name;
        public string          desc;
        public string          slotLabel;
        public bool            isMovement;
        public MovementAbility movement;
        public SpecialAbility  special;
        public Color           color;
    }

    AbilityOffer[] offers;
    AbilityManager abilityMgr;
    Action         onDone;

    bool             isShowing  = false;
    bool             swapPhase  = false;
    MovementAbility  pendingMovement;

    const float CARD_W = 270f;
    const float CARD_H = 210f;

    void Awake() => Instance = this;

    public void Show(AbilityOffer[] offers, AbilityManager mgr, Action callback)
    {
        this.offers     = offers;
        this.abilityMgr = mgr;
        this.onDone     = callback;
        isShowing  = true;
        swapPhase  = false;
        Time.timeScale = 0f;
    }

    void Close()
    {
        isShowing      = false;
        Time.timeScale = 1f;
        onDone?.Invoke();
    }

    void OnGUI()
    {
        if (!isShowing) return;

        int sw = Screen.width;
        int sh = Screen.height;

        GUI.color = new Color(0f, 0f, 0f, 0.8f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (!swapPhase) DrawChoicePhase(sw, sh);
        else            DrawSwapPhase(sw, sh);
    }

    void DrawChoicePhase(int sw, int sh)
    {
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };
        GUI.Label(new Rect(0, sh * 0.1f, sw, 50f), "ボス撃破！　能力を奪え", titleStyle);

        float totalW = CARD_W * offers.Length + 40f * (offers.Length - 1);
        float startX = (sw - totalW) * 0.5f;
        float cardY  = sh * 0.25f;

        for (int i = 0; i < offers.Length; i++)
            DrawCard(offers[i], new Rect(startX + i * (CARD_W + 40f), cardY, CARD_W, CARD_H));
    }

    void DrawCard(AbilityOffer offer, Rect r)
    {
        GUI.color = new Color(offer.color.r, offer.color.g, offer.color.b, 0.22f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);

        // border lines
        GUI.color = new Color(offer.color.r, offer.color.g, offer.color.b, 0.9f);
        GUI.DrawTexture(new Rect(r.x,        r.y,        r.width, 2f),    Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,        r.yMax - 2f, r.width, 2f),   Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,        r.y,        2f,    r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax - 2f, r.y,       2f,    r.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };
        GUI.Label(new Rect(r.x, r.y + 14f, r.width, 34f), offer.name, nameStyle);

        var slotStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 11,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = new Color(offer.color.r, offer.color.g, offer.color.b) }
        };
        GUI.Label(new Rect(r.x, r.y + 48f, r.width, 20f), offer.slotLabel, slotStyle);

        var descStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            alignment = TextAnchor.UpperCenter,
            wordWrap  = true,
            normal    = { textColor = new Color(0.88f, 0.88f, 0.88f) }
        };
        GUI.Label(new Rect(r.x + 14f, r.y + 72f, r.width - 28f, 90f), offer.desc, descStyle);

        var btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 16,
            fontStyle = FontStyle.Bold,
        };
        if (GUI.Button(new Rect(r.x + 28f, r.yMax - 52f, r.width - 56f, 38f), "奪う", btnStyle))
            OnSelect(offer);
    }

    void OnSelect(AbilityOffer offer)
    {
        if (offer.isMovement)
        {
            if (abilityMgr.HasEmptyMoveSlot())
            {
                abilityMgr.AddMovementAbility(offer.movement);
                Close();
            }
            else
            {
                pendingMovement = offer.movement;
                swapPhase = true;
            }
        }
        else
        {
            abilityMgr.SetSpecialAbility(offer.special);
            Close();
        }
    }

    void DrawSwapPhase(int sw, int sh)
    {
        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };
        GUI.Label(new Rect(0, sh * 0.22f, sw, 40f), "移動強化スロットが満杯　どれを捨てる？", titleStyle);

        var slots  = abilityMgr.MoveSlots;
        float btnW = 220f;
        float btnH = 56f;
        float total = btnW * (slots.Length + 1) + 24f * slots.Length;
        float sx    = (sw - total) * 0.5f;
        float btnY  = sh * 0.4f;

        var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 15 };

        for (int i = 0; i < slots.Length; i++)
        {
            float bx = sx + i * (btnW + 24f);
            if (GUI.Button(new Rect(bx, btnY, btnW, btnH),
                $"捨てる: {AbilityManager.MoveName(slots[i])}", btnStyle))
            {
                abilityMgr.ReplaceMovementAbility(i, pendingMovement);
                Close();
            }
        }

        float cancelX = sx + slots.Length * (btnW + 24f);
        if (GUI.Button(new Rect(cancelX, btnY, btnW, btnH), "諦める", btnStyle))
            Close();
    }
}
