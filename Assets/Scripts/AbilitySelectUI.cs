using UnityEngine;
using System;
using System.Collections.Generic;

public class AbilitySelectUI : MonoBehaviour
{
    public static AbilitySelectUI Instance { get; private set; }

    public struct AbilityOffer
    {
        public string          name;
        public string          desc;
        public string          slotLabel;
        public string          iconName;
        public bool            isMovement;
        public MovementAbility movement;
        public SpecialAbility  special;
        public Color           color;
    }

    AbilityOffer[] offers;
    AbilityManager abilityMgr;
    Action         onDone;

    bool            isShowing  = false;
    bool            swapPhase  = false;
    MovementAbility pendingMovement;
    MovementAbility pendingNewMovement;

    const float ICON_SIZE = 120f;
    const float CARD_W    = 150f;
    const float CARD_H    = 190f;
    const float CARD_GAP  = 50f;

    static readonly Dictionary<string, Texture2D> iconCache = new();

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

    static Texture2D LoadIcon(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (!iconCache.TryGetValue(name, out var tex))
        {
            tex = Resources.Load<Texture2D>($"SkillIcons/{name}");
            iconCache[name] = tex;
        }
        return tex;
    }

    void OnGUI()
    {
        if (!isShowing) return;

        int sw = Screen.width;
        int sh = Screen.height;

        GUI.color = new Color(0f, 0f, 0f, 0.75f);
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        if (!swapPhase) DrawChoicePhase(sw, sh);
        else            DrawSwapPhase(sw, sh);
    }

    void DrawChoicePhase(int sw, int sh)
    {
        float totalW = CARD_W * offers.Length + CARD_GAP * (offers.Length - 1);
        float startX = (sw - totalW) * 0.5f;
        float cardY  = (sh - CARD_H) * 0.5f;

        for (int i = 0; i < offers.Length; i++)
        {
            var r = new Rect(startX + i * (CARD_W + CARD_GAP), cardY, CARD_W, CARD_H);
            if (DrawCard(offers[i], r))
                OnSelect(offers[i]);
        }
    }

    // returns true if clicked
    bool DrawCard(AbilityOffer offer, Rect r)
    {
        bool hover = r.Contains(Event.current.mousePosition);

        // 背景
        GUI.color = hover
            ? new Color(offer.color.r, offer.color.g, offer.color.b, 0.3f)
            : new Color(0f, 0f, 0f, 0.45f);
        GUI.DrawTexture(r, Texture2D.whiteTexture);

        // アイコン
        var icon = LoadIcon(offer.iconName);
        float iconPad = (CARD_W - ICON_SIZE) * 0.5f;
        var iconRect = new Rect(r.x + iconPad, r.y + 14f, ICON_SIZE, ICON_SIZE);
        if (icon != null)
        {
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, icon);
        }

        // 能力名
        var nameStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = Color.white }
        };
        GUI.color = Color.white;
        GUI.Label(new Rect(r.x, iconRect.yMax + 6f, r.width, 26f), offer.name, nameStyle);

        // パッシブ / R キー ラベル
        if (!string.IsNullOrEmpty(offer.name))
        {
            var tagStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = new Color(offer.color.r, offer.color.g, offer.color.b, 0.9f) }
            };
            string tag = offer.isMovement ? "パッシブ" : "R キー";
            GUI.Label(new Rect(r.x, iconRect.yMax + 32f, r.width, 20f), tag, tagStyle);
        }

        // 枠線
        float bw = hover ? 3f : 2f;
        GUI.color = new Color(offer.color.r, offer.color.g, offer.color.b, hover ? 1f : 0.6f);
        GUI.DrawTexture(new Rect(r.x,          r.y,          r.width, bw),       Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,          r.yMax - bw,  r.width, bw),       Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.x,          r.y,          bw,      r.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(r.xMax - bw,  r.y,          bw,      r.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        return Event.current.type == EventType.MouseDown && hover;
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
                pendingNewMovement = offer.movement;
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
        var slots = abilityMgr.MoveSlots;

        float totalW = CARD_W * (slots.Length + 1) + CARD_GAP * slots.Length;
        float startX = (sw - totalW) * 0.5f;
        float cardY  = (sh - CARD_H) * 0.5f;

        // 既存スロットを表示 → クリックで差し替え
        for (int i = 0; i < slots.Length; i++)
        {
            var offer = MakeSwapOffer(slots[i]);
            var r = new Rect(startX + i * (CARD_W + CARD_GAP), cardY, CARD_W, CARD_H);
            if (DrawCard(offer, r))
            {
                abilityMgr.ReplaceMovementAbility(i, pendingNewMovement);
                Close();
            }
        }

        // 諦めるカード
        var cancelOffer = new AbilityOffer
        {
            name     = "諦める",
            iconName = null,
            color    = new Color(0.5f, 0.5f, 0.5f)
        };
        var cancelR = new Rect(startX + slots.Length * (CARD_W + CARD_GAP), cardY, CARD_W, CARD_H);
        if (DrawCard(cancelOffer, cancelR)) Close();
    }

    static AbilityOffer MakeSwapOffer(MovementAbility a) => new AbilityOffer
    {
        name       = AbilityManager.MoveName(a),
        iconName   = MoveIcon(a),
        isMovement = true,
        movement   = a,
        color      = new Color(0.9f, 0.3f, 0.2f)
    };

    static string MoveIcon(MovementAbility a) => a switch
    {
        MovementAbility.SpeedBoost => "UI_Skill_Icon_Buff",
        MovementAbility.DoubleJump => "UI_Skill_Icon_Fly",
        _                          => null
    };
}
