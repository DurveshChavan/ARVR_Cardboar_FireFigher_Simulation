using UnityEngine;
using UnityEditor;

public class MaterialFixer
{
    [MenuItem("VR Firefighter/Fix Pink Materials")]
    public static void FixMaterials()
    {
        // Find all materials in the Assets/Materials folder
        string[] guids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials" });
        Shader standardShader = Shader.Find("Standard");
        
        int count = 0;
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat != null && mat.shader.name.Contains("Universal Render Pipeline"))
            {
                // URP uses _BaseColor, Standard uses _Color. We need to save the color before swapping!
                Color originalColor = Color.white;
                if (mat.HasProperty("_BaseColor"))
                {
                    originalColor = mat.GetColor("_BaseColor");
                }
                
                // Swap the shader to the Built-in Standard Shader
                mat.shader = standardShader;
                
                // Apply the saved color
                mat.SetColor("_Color", originalColor);
                
                EditorUtility.SetDirty(mat);
                count++;
            }
        }
        AssetDatabase.SaveAssets();
        Debug.Log($"Successfully fixed {count} pink materials. Your scene is restored!");
    }
}
