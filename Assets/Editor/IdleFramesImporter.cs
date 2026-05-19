using UnityEngine;
using UnityEditor;

/// <summary>
/// Assets/Sprites/Player/IdleFrames/ 以下の PNG を自動的に
/// PPU=100, Point フィルタ, 圧縮なし でインポートする。
/// </summary>
public class IdleFramesImporter : AssetPostprocessor
{
    const string TARGET_PATH = "Assets/Sprites/Player/IdleFrames/";

    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith(TARGET_PATH)) return;

        var ti = (TextureImporter)assetImporter;
        ti.textureType         = TextureImporterType.Sprite;
        ti.spriteImportMode    = SpriteImportMode.Single;
        ti.spritePixelsPerUnit = 100;
        ti.filterMode          = FilterMode.Point;
        ti.textureCompression  = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency = true;

        var settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);
        settings.spriteMeshType  = SpriteMeshType.FullRect;
        settings.spriteAlignment = (int)SpriteAlignment.Center;
        ti.SetTextureSettings(settings);
    }

    [MenuItem("Tools/Reimport Idle Frames")]
    static void ReimportAll()
    {
        for (int i = 0; i < 8; i++)
        {
            string path = $"{TARGET_PATH}idle_{i:D2}.png";
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
        Debug.Log("[IdleFramesImporter] Reimport complete.");
    }
}
