using UnityEngine;

/// <summary>
/// シーンに何もなくてもプレイボタンを押すだけでゲームが起動します。
/// </summary>
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        // EffectManager は常に起動
        if (Object.FindFirstObjectByType<EffectManager>() == null)
        {
            var go = new GameObject("EffectManager");
            go.AddComponent<EffectManager>();
        }

        // ArenaBuilder はスタートボタン押下時に StartScreen が生成する
        if (Object.FindFirstObjectByType<StartScreen>() == null)
        {
            var go = new GameObject("StartScreen");
            go.AddComponent<StartScreen>();
        }
    }
}
