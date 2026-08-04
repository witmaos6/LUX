using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class CreateDOSGothic2DLitFont
{
    private const string SourceFontPath = "Assets/Font/DOSGothic SDF.asset";
    private const string TargetFontPath = "Assets/Font/DOSGothic SDF - 2D Lit.asset";
    private const string TargetMaterialPath = "Assets/Font/DOSGothic SDF - 2D Lit.mat";
    private const string ShaderName = "LUX/TextMeshPro/Distance Field 2D Lit";

    [MenuItem("Tools/LUX/Create DOSGothic 2D Lit Font")]
    public static void Create()
    {
        TMP_FontAsset sourceFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(SourceFontPath);
        if (sourceFont == null)
        {
            Debug.LogError($"DOSGothic 원본 폰트를 찾지 못했습니다: {SourceFontPath}");
            return;
        }

        Shader litShader = Shader.Find(ShaderName);
        if (litShader == null)
        {
            Debug.LogError($"2D Lit TMP 셰이더를 찾지 못했습니다: {ShaderName}");
            return;
        }

        DeleteExistingGeneratedAssets();

        if (!AssetDatabase.CopyAsset(SourceFontPath, TargetFontPath))
        {
            Debug.LogError($"폰트 에셋 복제에 실패했습니다: {TargetFontPath}");
            return;
        }

        AssetDatabase.ImportAsset(TargetFontPath, ImportAssetOptions.ForceSynchronousImport);
        TMP_FontAsset targetFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(TargetFontPath);
        Material material = new Material(sourceFont.material)
        {
            name = "DOSGothic SDF - 2D Lit",
            shader = litShader
        };

        // CopyAsset로 복제된 폰트의 아틀라스를 명시적으로 사용합니다.
        // 이로써 원본 폰트의 머티리얼은 전혀 변경되지 않습니다.
        if (targetFont.atlasTexture != null)
            material.SetTexture(ShaderUtilities.ID_MainTex, targetFont.atlasTexture);

        AssetDatabase.CreateAsset(material, TargetMaterialPath);
        targetFont.material = material;
        EditorUtility.SetDirty(targetFont);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = targetFont;
        EditorGUIUtility.PingObject(targetFont);
        Debug.Log("DOSGothic SDF - 2D Lit 폰트와 머티리얼을 생성했습니다.");
    }

    private static void DeleteExistingGeneratedAssets()
    {
        if (File.Exists(TargetMaterialPath))
            AssetDatabase.DeleteAsset(TargetMaterialPath);

        if (File.Exists(TargetFontPath))
            AssetDatabase.DeleteAsset(TargetFontPath);
    }
}
