using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Assets/Resources/Sprites/Player/ 以下の PNG を自動検出し、
/// PPU・フィルタ・ピボットを一括設定する AssetPostprocessor。
/// </summary>
public class PlayerPartsImporter : AssetPostprocessor
{
    const string TARGET_PATH = "Assets/Resources/Sprites/Player/";

    // ファイル名（拡張子なし） → Pivot UV座標
    static readonly Dictionary<string, Vector2> Pivots = new()
    {
        { "01_head",         new Vector2(0.500f, 0.750f) },
        { "02_torso",        new Vector2(0.500f, 0.734f) },
        { "03_arm_L_upper",  new Vector2(0.500f, 0.703f) },
        { "04_arm_L_lower",  new Vector2(0.422f, 0.547f) },
        { "05_arm_R_upper",  new Vector2(0.500f, 0.703f) },
        { "06_arm_R_lower",  new Vector2(0.578f, 0.547f) },
        { "07_leg_L_upper",  new Vector2(0.500f, 0.422f) },
        { "08_leg_L_lower",  new Vector2(0.453f, 0.234f) },
        { "09_leg_R_upper",  new Vector2(0.500f, 0.422f) },
        { "10_leg_R_lower",  new Vector2(0.547f, 0.234f) },
    };

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(TARGET_PATH)) return;

        string key = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (!Pivots.TryGetValue(key, out Vector2 pivot)) return;

        var ti = (TextureImporter)assetImporter;
        ti.textureType          = TextureImporterType.Sprite;
        ti.spriteImportMode     = SpriteImportMode.Single;
        ti.spritePixelsPerUnit  = 100;
        ti.filterMode           = FilterMode.Point;
        ti.textureCompression   = TextureImporterCompression.Uncompressed;
        ti.spriteMeshType       = SpriteMeshType.FullRect;
        ti.alphaIsTransparency  = true;

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spritePivot     = pivot;
        ti.SetTextureSettings(settings);
    }

    // Unity メニューから手動で再インポートする場合
    [MenuItem("Tools/Reimport Player Parts")]
    static void ReimportAll()
    {
        foreach (var key in Pivots.Keys)
        {
            string path = TARGET_PATH + key + ".png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        Debug.Log("[PlayerPartsImporter] Reimport complete.");
    }
}
