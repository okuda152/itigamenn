using UnityEngine;
using System.IO;

/// <summary>
/// スタート画面。StreamingAssets/title.png を表示し、
/// STARTボタンが押されたらゲームを開始する。
/// </summary>
public class StartScreen : MonoBehaviour
{
    Texture2D titleTex;
    bool      started = false;

    void Start()
    {
        // title.png を読み込む
        string path = Path.Combine(Application.streamingAssetsPath, "title.png");
        if (File.Exists(path))
        {
            titleTex = new Texture2D(2, 2);
            titleTex.LoadImage(File.ReadAllBytes(path));
        }
    }

    void OnGUI()
    {
        if (started) return;

        int sw = Screen.width;
        int sh = Screen.height;

        // 黒背景
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0, 0, sw, sh), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // タイトル絵
        if (titleTex != null)
        {
            float maxW  = sw * 0.78f;
            float maxH  = sh * 0.60f;
            float scale = Mathf.Min(maxW / titleTex.width, maxH / titleTex.height);
            float imgW  = titleTex.width  * scale;
            float imgH  = titleTex.height * scale;
            float imgX  = (sw - imgW) * 0.5f;
            float imgY  = sh * 0.06f;
            GUI.DrawTexture(new Rect(imgX, imgY, imgW, imgH), titleTex);
        }
        else
        {
            // 絵がない場合はタイトル文字を表示
            var titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 48,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white }
            };
            GUI.Label(new Rect(0, sh * 0.25f, sw, 80f), "I T I G A M E N N  F I G H T E R", titleStyle);
        }

        // STARTボタン
        var btnStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize  = 26,
            fontStyle = FontStyle.Bold,
        };
        float btnW = 220f;
        float btnH = 60f;
        float btnX = (sw - btnW) * 0.5f;
        float btnY = sh * 0.78f;

        if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), "S T A R T", btnStyle))
        {
            StartGame();
        }

        // 操作説明
        var infoStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 13,
            alignment = TextAnchor.MiddleCenter,
            normal    = { textColor = new Color(0.7f, 0.7f, 0.7f) }
        };
        GUI.Label(new Rect(0, sh * 0.89f, sw, 22f), "移動: A/D  ジャンプ: Space  パンチ: Z  キック: E  投石: Q", infoStyle);
    }

    void StartGame()
    {
        started = true;

        var go = new GameObject("ArenaBuilder");
        go.AddComponent<ArenaBuilder>();

        Destroy(gameObject);
    }
}
