using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// 使用するキャラプレハブを Resources/Characters/ に移動するセットアップ用ツール。
/// メニュー: Game > Setup Character Prefabs
/// </summary>
public static class CharacterSetup
{
    static readonly int[] CharacterNumbers = { 58, 71 };

    [MenuItem("Game/Setup Character Prefabs")]
    static void Setup()
    {
        string srcDir = "Assets/Blackthornprod/100 Fantasy Characters Pack/Prefabs";
        string dstDir = "Assets/Resources/Characters";

        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder(dstDir))
            AssetDatabase.CreateFolder("Assets/Resources", "Characters");

        int moved = 0;
        foreach (int n in CharacterNumbers)
        {
            string name    = $"Character ({n})";
            string srcPath = $"{srcDir}/{name}.prefab";
            string dstPath = $"{dstDir}/{name}.prefab";

            if (!File.Exists(Path.GetFullPath(srcPath)))
            {
                Debug.LogWarning($"[CharacterSetup] 見つかりません: {srcPath}");
                continue;
            }
            if (File.Exists(Path.GetFullPath(dstPath)))
            {
                Debug.Log($"[CharacterSetup] 既に移動済み: {name}");
                continue;
            }

            string err = AssetDatabase.MoveAsset(srcPath, dstPath);
            if (string.IsNullOrEmpty(err))
            {
                Debug.Log($"[CharacterSetup] 移動完了: {name}");
                moved++;
            }
            else
            {
                Debug.LogError($"[CharacterSetup] 移動失敗 ({name}): {err}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Setup Complete",
            moved > 0
                ? $"{moved} 個のプレハブを Resources/Characters/ に移動しました。"
                : "すべて移動済みです。",
            "OK");
    }
}
