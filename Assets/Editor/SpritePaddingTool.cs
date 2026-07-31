using System.IO;
using UnityEditor;
using UnityEngine;

public static class SpritePaddingTool
{
    private const int Padding = 16;

    [MenuItem("Tools/LUX2D/Create Padded Sprite")]
    private static void CreatePaddedSprite()
    {
        Texture2D sourceAsset = Selection.activeObject as Texture2D;
        if (sourceAsset == null)
        {
            EditorUtility.DisplayDialog(
                "Create Padded Sprite",
                "Project 창에서 이미지 파일을 선택하세요.",
                "확인");
            return;
        }

        string sourcePath = AssetDatabase.GetAssetPath(sourceAsset);
        byte[] sourceBytes = File.ReadAllBytes(sourcePath);

        Texture2D sourceTexture = new Texture2D(2, 2);
        sourceTexture.LoadImage(sourceBytes);

        int targetWidth = sourceTexture.width + Padding * 2;
        int targetHeight = sourceTexture.height + Padding * 2;

        Texture2D targetTexture = new Texture2D(
            targetWidth,
            targetHeight,
            TextureFormat.RGBA32,
            false);

        Color32[] clearPixels =
            new Color32[targetWidth * targetHeight];

        targetTexture.SetPixels32(clearPixels);
        targetTexture.SetPixels(
            Padding,
            Padding,
            sourceTexture.width,
            sourceTexture.height,
            sourceTexture.GetPixels());

        targetTexture.Apply();

        string directory = Path.GetDirectoryName(sourcePath);
        string fileName = Path.GetFileNameWithoutExtension(sourcePath);
        string targetPath =
            $"{directory}/{fileName}_Padded.png";

        File.WriteAllBytes(
            targetPath,
            targetTexture.EncodeToPNG());

        Object.DestroyImmediate(sourceTexture);
        Object.DestroyImmediate(targetTexture);

        AssetDatabase.ImportAsset(
            targetPath,
            ImportAssetOptions.ForceUpdate);

        CopyImportSettings(sourcePath, targetPath);
        AssetDatabase.Refresh();

        Selection.activeObject =
            AssetDatabase.LoadAssetAtPath<Texture2D>(targetPath);
    }

    private static void CopyImportSettings(
        string sourcePath,
        string targetPath)
    {
        TextureImporter sourceImporter =
            AssetImporter.GetAtPath(sourcePath) as TextureImporter;

        TextureImporter targetImporter =
            AssetImporter.GetAtPath(targetPath) as TextureImporter;

        if (sourceImporter == null || targetImporter == null)
            return;

        targetImporter.textureType = TextureImporterType.Sprite;
        targetImporter.spriteImportMode =
            SpriteImportMode.Single;
        targetImporter.spritePixelsPerUnit =
            sourceImporter.spritePixelsPerUnit;
        targetImporter.spritePivot =
            sourceImporter.spritePivot;
        targetImporter.spriteBorder =
            sourceImporter.spriteBorder;
        TextureImporterSettings textureSettings =
            new TextureImporterSettings();
        targetImporter.ReadTextureSettings(textureSettings);
        textureSettings.spriteMeshType = SpriteMeshType.FullRect;
        targetImporter.SetTextureSettings(textureSettings);
        targetImporter.filterMode =
            sourceImporter.filterMode;
        targetImporter.textureCompression =
            sourceImporter.textureCompression;
        targetImporter.mipmapEnabled = false;
        targetImporter.alphaIsTransparency = true;
        targetImporter.wrapMode = TextureWrapMode.Clamp;

        targetImporter.SaveAndReimport();
    }
}
