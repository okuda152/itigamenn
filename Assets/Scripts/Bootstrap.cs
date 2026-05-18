using UnityEngine;

/// <summary>
/// シーンに何もなくてもプレイボタンを押すだけでゲームが起動します。
/// </summary>
public static class Bootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (Object.FindFirstObjectByType<ArenaBuilder>() == null)
        {
            var go = new GameObject("ArenaBuilder");
            go.AddComponent<ArenaBuilder>();
        }

        if (Object.FindFirstObjectByType<EffectManager>() == null)
        {
            var go = new GameObject("EffectManager");
            go.AddComponent<EffectManager>();
        }
    }
}
