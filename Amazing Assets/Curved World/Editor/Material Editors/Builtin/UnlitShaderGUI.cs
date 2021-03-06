using System.Linq;
using UnityEngine;
using UnityEditor;


namespace AmazingAssets.CurvedWorldEditor
{
    internal class UnlitShaderGUI : ShaderGUI
    {
        static MaterialProperty _IncludeVertexColor = null;
        static MaterialProperty _Color = null;
        static MaterialProperty _MainTex = null;
        static MaterialProperty _MainTex_Scroll = null;
        static MaterialProperty _Cutoff = null;
         
        static MaterialProperty _TextureMix = null;
        static MaterialProperty _SecondaryTex = null;
        static MaterialProperty _SecondaryTex_Scroll = null;
        static MaterialProperty _SecondaryTex_Blend = null;

        static MaterialProperty _NormalMapStrength = null;
        static MaterialProperty _NormalMap = null;
        static MaterialProperty _NormalMap_UV_Scale = null;
        static MaterialProperty _SecondaryNormalMap = null;
        static MaterialProperty _SecondaryNormalMap_UV_Scale = null;
        
        static MaterialProperty _ReflectionColor = null;
        static MaterialProperty _ReflectionMaskOffset = null;
        static MaterialProperty _ReflectionCubeMap = null;
        static MaterialProperty _ReflectionFresnelBias = null;

        static MaterialProperty _RimColor = null;
        static MaterialProperty _RimBias = null;

        static MaterialProperty _EmissionColor = null;
        static MaterialProperty _EmissionMap = null;
        static MaterialProperty _EmissionMap_UV_Scale = null;

        static MaterialProperty _MatcapMap = null;
        static MaterialProperty _MatcapIntensity = null;
        static MaterialProperty _MatcapBlendMode = null;


        static MaterialProperty _OutlineColor = null;
        static MaterialProperty _OutlineWidth = null;
        static MaterialProperty _OutlineSizeIsFixed = null;


        static bool isOutline;


        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            isOutline = _OutlineColor != null;


            CurvedWorldEditor.MaterialProperties.InitCurvedWorldMaterialProperties(properties);
            CurvedWorldEditor.MaterialProperties.DrawCurvedWorldMaterialProperties(materialEditor, MaterialProperties.STYLE.HelpBox, isOutline ? false : true, true);


            Material material = (Material)materialEditor.target;


            DrawOutline(materialEditor, material);
            DrawAlbedo(materialEditor, material);
            DrawCutout(materialEditor, material);
            DrawBumpMap(materialEditor, material);
            DrawReflection(materialEditor, material);
            DrawMatcap(materialEditor, material);
            DrawRimFresnel(materialEditor, material);
            DrawEmission(materialEditor, material);


            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);
            EditorGUI.LabelField(rect, "Advanced Rendering Options", EditorStyles.boldLabel);
            base.OnGUI(materialEditor, properties);
        }
        
        void DrawAlbedo(MaterialEditor materialEditor, Material material)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);


            EditorGUI.LabelField(rect, "Albedo", EditorStyles.boldLabel);
            using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
            {
                materialEditor.ShaderProperty(_IncludeVertexColor, "Vertex Color");
                materialEditor.ShaderProperty(_Color, "Tint Color");

                materialEditor.TexturePropertySingleLine(new GUIContent("Main Map"), _MainTex);
                using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                {
                    materialEditor.TextureScaleOffsetProperty(_MainTex);
                    materialEditor.ShaderProperty(_MainTex_Scroll, string.Empty);
                }

                GUILayout.Space(10);
                materialEditor.ShaderProperty(_TextureMix, "Texture Blend");
                if (material.shaderKeywords.Contains("_TEXTUREMIX_BY_MAIN_ALPHA") ||
                    material.shaderKeywords.Contains("_TEXTUREMIX_BY_SECONDARY_ALPHA") ||
                    material.shaderKeywords.Contains("_TEXTUREMIX_MULTIPLE") ||
                    material.shaderKeywords.Contains("_TEXTUREMIX_ADDITIVE"))
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Secondary Map"), _SecondaryTex);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                    {
                        materialEditor.TextureScaleOffsetProperty(_SecondaryTex);
                        materialEditor.ShaderProperty(_SecondaryTex_Scroll, string.Empty);
                    }
                }
                else if (material.shaderKeywords.Contains("_TEXTUREMIX_BY_VERTEX_ALPHA"))
                {
                    materialEditor.ShaderProperty(_SecondaryTex_Blend, "Vertex Alpha Offset");
                    materialEditor.TexturePropertySingleLine(new GUIContent("Secondary Map"), _SecondaryTex);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                    {
                        materialEditor.TextureScaleOffsetProperty(_SecondaryTex);
                        materialEditor.ShaderProperty(_SecondaryTex_Scroll, string.Empty);
                    }
                }
            }
        }

        void DrawCutout(MaterialEditor materialEditor, Material material)
        {
            if (isOutline)
                return;


            if (material.shaderKeywords.Contains("_ALPHATEST_ON"))
            {
                Rect rect = EditorGUILayout.GetControlRect();
                EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);

                EditorGUI.LabelField(rect, "Cutout", EditorStyles.boldLabel);
                using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                {
                    materialEditor.ShaderProperty(_Cutoff, "Alpha Cutoff");
                }
            }
        }

        void DrawBumpMap(MaterialEditor materialEditor, Material material)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);


            bool value = material.shaderKeywords.Contains("_NORMALMAP");

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.ToggleLeft(rect, "Bump", value, EditorStyles.boldLabel);
            if (EditorGUI.EndChangeCheck())
            {
                if (value)
                    material.EnableKeyword("_NORMALMAP");
                else
                    material.DisableKeyword("_NORMALMAP");
            }

            if (value)
            {
                if (material.shaderKeywords.Contains("_REFLECTION") == false && material.shaderKeywords.Contains("_MATCAP") == false)
                {
                    EditorGUILayout.HelpBox("Bump has effect only with Reflection or MatCap enabled.", MessageType.Info);
                }
                else
                {
                    using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                    {
                        materialEditor.ShaderProperty(_NormalMapStrength, "Strength");
                        materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map"), _NormalMap);
                        using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                        {
                            materialEditor.ShaderProperty(_NormalMap_UV_Scale, "UV Scale");
                        }

                        if (material.shaderKeywords.Contains("_TEXTUREMIX_BY_MAIN_ALPHA") ||
                            material.shaderKeywords.Contains("_TEXTUREMIX_BY_SECONDARY_ALPHA") ||
                            material.shaderKeywords.Contains("_TEXTUREMIX_MULTIPLE") ||
                            material.shaderKeywords.Contains("_TEXTUREMIX_ADDITIVE") ||
                            material.shaderKeywords.Contains("_TEXTUREMIX_BY_VERTEX_ALPHA"))
                        {
                            materialEditor.TexturePropertySingleLine(new GUIContent("Secondary Normal Map"), _SecondaryNormalMap);
                            using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                            {
                                materialEditor.ShaderProperty(_SecondaryNormalMap_UV_Scale, "UV Scale");
                            }
                        }
                    }
                }
            }
        }

        void DrawReflection(MaterialEditor materialEditor, Material material)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);


            bool value = material.shaderKeywords.Contains("_REFLECTION");

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.ToggleLeft(rect, "Reflection", value, EditorStyles.boldLabel);
            if (EditorGUI.EndChangeCheck())
            {
                if (value)
                    material.EnableKeyword("_REFLECTION");
                else
                    material.DisableKeyword("_REFLECTION");
            }

            if (value)
            {
                using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                {
                    materialEditor.ShaderProperty(_ReflectionColor, "Color");
                    materialEditor.ShaderProperty(_ReflectionMaskOffset, "Mask Offset");
                    materialEditor.TexturePropertySingleLine(new GUIContent("CubeMap"), _ReflectionCubeMap);
                    materialEditor.ShaderProperty(_ReflectionFresnelBias, "Fresnel Bias");
                }
            }
        }

        void DrawRimFresnel(MaterialEditor materialEditor, Material material)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);


            bool value = material.shaderKeywords.Contains("_RIM");

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.ToggleLeft(rect, "Rim/Fresnel", value, EditorStyles.boldLabel);
            if (EditorGUI.EndChangeCheck())
            {
                if (value)
                    material.EnableKeyword("_RIM");
                else
                    material.DisableKeyword("_RIM");
            }

            if (value)
            {
                using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                {
                    materialEditor.ShaderProperty(_RimColor, "Color");
                    materialEditor.ShaderProperty(_RimBias, "Bias");
                }
            }
        }

        void DrawEmission(MaterialEditor materialEditor, Material material)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);


            bool value = material.shaderKeywords.Contains("_EMISSION");

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.ToggleLeft(rect, "Emission", value, EditorStyles.boldLabel);
            if (EditorGUI.EndChangeCheck())
            {
                if (value)
                    material.EnableKeyword("_EMISSION");
                else
                    material.DisableKeyword("_EMISSION");
            }

            if (value)
            {
                using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                {
                    materialEditor.ShaderProperty(_EmissionColor, "Color");
                    materialEditor.TexturePropertySingleLine(new GUIContent("Emission Map"), _EmissionMap);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                    {
                        materialEditor.ShaderProperty(_EmissionMap_UV_Scale, "UV Scale");
                    }
                }
            }
        }

        void DrawMatcap(MaterialEditor materialEditor, Material material)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);


            bool value = material.shaderKeywords.Contains("_MATCAP");

            EditorGUI.BeginChangeCheck();
            value = EditorGUI.ToggleLeft(rect, "MatCap", value, EditorStyles.boldLabel);
            if (EditorGUI.EndChangeCheck())
            {
                if (value)
                    material.EnableKeyword("_MATCAP");
                else
                    material.DisableKeyword("_MATCAP");
            }

            if (value)
            {
                using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("MatCap Map"), _MatcapMap);
                    materialEditor.ShaderProperty(_MatcapIntensity, "Intensity");
                    materialEditor.ShaderProperty(_MatcapBlendMode, "Mode");
                }
            }
        }


        void DrawOutline(MaterialEditor materialEditor, Material material)
        {
            if (_OutlineColor == null)
                return;

            Rect rect = EditorGUILayout.GetControlRect();
            EditorGUI.DrawRect(new Rect(rect.xMin - 2, rect.yMin, rect.width + 2, rect.height), Color.gray * 0.25f);

            EditorGUI.LabelField(rect, "Outline", EditorStyles.boldLabel);
            using (new AmazingAssets.EditorGUIUtility.EditorGUIIndentLevel(1))
            {
                materialEditor.ShaderProperty(_OutlineColor, "Color");
                materialEditor.ShaderProperty(_OutlineWidth, "Width");
                materialEditor.ShaderProperty(_OutlineSizeIsFixed, "Fixed Size");
            }
        }


        void FindProperties(MaterialProperty[] properties)
        {
            _IncludeVertexColor = CurvedWorldEditor.MaterialProperties.FindProperty("_IncludeVertexColor", properties, true);
            _Color = CurvedWorldEditor.MaterialProperties.FindProperty("_Color", properties, true);
            _MainTex = CurvedWorldEditor.MaterialProperties.FindProperty("_MainTex", properties, true);
            _MainTex_Scroll = CurvedWorldEditor.MaterialProperties.FindProperty("_MainTex_Scroll", properties, true);
            _Cutoff = CurvedWorldEditor.MaterialProperties.FindProperty("_Cutoff", properties, true);

            _TextureMix = CurvedWorldEditor.MaterialProperties.FindProperty("_TextureMix", properties, true);
            _SecondaryTex = CurvedWorldEditor.MaterialProperties.FindProperty("_SecondaryTex", properties, true);
            _SecondaryTex_Scroll = CurvedWorldEditor.MaterialProperties.FindProperty("_SecondaryTex_Scroll", properties, true);
            _SecondaryTex_Blend = CurvedWorldEditor.MaterialProperties.FindProperty("_SecondaryTex_Blend", properties, true);

            _NormalMapStrength = CurvedWorldEditor.MaterialProperties.FindProperty("_NormalMapStrength", properties, true);
            _NormalMap = CurvedWorldEditor.MaterialProperties.FindProperty("_NormalMap", properties, true);
            _NormalMap_UV_Scale = CurvedWorldEditor.MaterialProperties.FindProperty("_NormalMap_UV_Scale", properties, true);
            _SecondaryNormalMap = CurvedWorldEditor.MaterialProperties.FindProperty("_SecondaryNormalMap", properties, true);
            _SecondaryNormalMap_UV_Scale = CurvedWorldEditor.MaterialProperties.FindProperty("_SecondaryNormalMap_UV_Scale", properties, true);
            
            _ReflectionColor = CurvedWorldEditor.MaterialProperties.FindProperty("_ReflectionColor", properties, true);
            _ReflectionMaskOffset = CurvedWorldEditor.MaterialProperties.FindProperty("_ReflectionMaskOffset", properties, true);
            _ReflectionCubeMap = CurvedWorldEditor.MaterialProperties.FindProperty("_ReflectionCubeMap", properties, true);
            _ReflectionFresnelBias = CurvedWorldEditor.MaterialProperties.FindProperty("_ReflectionFresnelBias", properties, true);

            _RimColor = CurvedWorldEditor.MaterialProperties.FindProperty("_RimColor", properties, true);
            _RimBias = CurvedWorldEditor.MaterialProperties.FindProperty("_RimBias", properties, true);

            _EmissionColor = CurvedWorldEditor.MaterialProperties.FindProperty("_EmissionColor", properties, true);
            _EmissionMap = CurvedWorldEditor.MaterialProperties.FindProperty("_EmissionMap", properties, true);
            _EmissionMap_UV_Scale = CurvedWorldEditor.MaterialProperties.FindProperty("_EmissionMap_UV_Scale", properties, true);

            _MatcapMap = CurvedWorldEditor.MaterialProperties.FindProperty("_MatcapMap", properties, true);
            _MatcapIntensity = CurvedWorldEditor.MaterialProperties.FindProperty("_MatcapIntensity", properties, true);
            _MatcapBlendMode = CurvedWorldEditor.MaterialProperties.FindProperty("_MatcapBlendMode", properties, true);


            //Available only in Outline shader
            _OutlineColor = CurvedWorldEditor.MaterialProperties.FindProperty("_OutlineColor", properties, false);
            _OutlineWidth = CurvedWorldEditor.MaterialProperties.FindProperty("_OutlineWidth", properties, false);
            _OutlineSizeIsFixed = CurvedWorldEditor.MaterialProperties.FindProperty("_OutlineSizeIsFixed", properties, false);
        }
    }
}