using System.Collections.Generic;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Strips unused shader variants during build to reduce build time from 1+ hour to ~5 minutes.
/// This is an Editor-only script — does NOT affect runtime.
/// 
/// The problem: URP + TextMeshPro generates 18+ BILLION shader variant combinations.
/// The fix: Strip all variants we don't need (fog, instancing, lightmaps, etc.)
/// </summary>
public class ShaderVariantStripper : IPreprocessShaders
{
    // Run early in the pipeline
    public int callbackOrder => 0;

    // Keywords we definitely don't use in this VR project
    static readonly HashSet<string> STRIP_KEYWORDS = new HashSet<string>
    {
        // Fog — not used in our indoor VR scenes
        "FOG_LINEAR",
        "FOG_EXP",
        "FOG_EXP2",
        "_FOG_FRAGMENT",

        // Lightmaps — we use realtime lighting only
        "LIGHTMAP_ON",
        "DYNAMICLIGHTMAP_ON",
        "LIGHTMAP_SHADOW_MIXING",
        "SHADOWS_SHADOWMASK",
        "DIRLIGHTMAP_COMBINED",

        // GPU Instancing — not needed for our simple scene
        "INSTANCING_ON",
        "DOTS_INSTANCING_ON",
        "PROCEDURAL_INSTANCING_ON",

        // Debug / Editor only
        "DEBUG_DISPLAY",
        "EDITOR_VISUALIZATION",
        "_ADDITIONAL_LIGHT_SHADOWS",

        // Features we don't use
        "_SCREEN_SPACE_OCCLUSION",
        "_LIGHT_COOKIES",
        "EVALUATE_SH_MIXED",
        "EVALUATE_SH_VERTEX",
    };

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        for (int i = data.Count - 1; i >= 0; i--)
        {
            ShaderKeywordSet keywords = data[i].shaderKeywordSet;

            foreach (string kw in STRIP_KEYWORDS)
            {
                ShaderKeyword keyword = new ShaderKeyword(kw);
                if (keywords.IsEnabled(keyword))
                {
                    data.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
