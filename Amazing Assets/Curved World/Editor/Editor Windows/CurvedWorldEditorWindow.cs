using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;


namespace AmazingAssets
{
    namespace CurvedWorldEditor
    {
        public class CurvedWorldEditorWindow : EditorWindow
        {
            public static CurvedWorldEditorWindow activeWindow;

            public enum TAB { Manage, Controllers, RenderersOverview, CurvedWorldKeywords, Activator, ShaderPackages, About }
            static string[] preferencesNames = new string[] { "Manage", "Controllers", "Renderers Overview", "Curved World Keywords", "Activator", "Shader Packages", "About" };

            enum RENDERERS_OVERVIEW_MODE { Scene, SelectedOjects }
            enum SEARCH_OPTION { ShaderName, MaterialName, Keyword }

            public TAB gTab;



            public CurvedWorld.BEND_TYPE gBendType = CurvedWorld.BEND_TYPE.ClassicRunner_X_Positive;
            public int gBendID = 1;

            static CurvedWorld.CurvedWorldController[] gSceneControllers;

            RENDERERS_OVERVIEW_MODE gRenderersOverviewMode;
            List<EditorUtilities.ShaderOverview> gRenderersOverviewList;
            SEARCH_OPTION gRenderersOverviewSearchOption;
            string gRenderersOverviewFilter;
            bool gRenderersOverviewFilterExclude;
            static int gRenderersOverviewModeRectWidth;
            static EditorUtilities.ShaderOverview gRenderersOverviewEditIndex = null;
            static string gRenderersOverviewEditString = string.Empty;

            public Shader gCurvedWorldKeywordsShader;
            public static EditorUtilities.ShaderCurvedWorldKeywordsInfo gCurvedWorldKeywordsShaderInfo;

            static List<Shader> gActivatorShaders;
            string gActivatorPath;

            static string[] shaderPackages;


            static GUIStyle guiStyleOptionsHeader;
            static int guiStyleOptionsHeaderHeight = 0;
            static GUIStyle guiStyleControllersButton;
            static GUIStyle guiStyleAnalyzeSaveButton;

            static Vector2 scroll;
            static Vector2 mousePosition;

            static GUIStyle guiStyleButtonTab;



            [MenuItem("Window/Amazing Assets/Curved World", false, 513)]
            static public void ShowWindow()
            {
                EditorWindow window = EditorWindow.GetWindow(typeof(CurvedWorldEditorWindow));
                window.titleContent = new GUIContent("Curved World");

                window.minSize = new Vector2(750, 600);

                activeWindow = (CurvedWorldEditorWindow)window;



                //Rebuild
                EditorUtilities.RebuildProjectBendsInfo();
            }


            private void OnDestroy()
            {
                activeWindow = null;

                if (CurvedWorldMaterialDuplicateEditorWindow.window != null)
                    CurvedWorldMaterialDuplicateEditorWindow.window.Close();

                if (CurvedWorldBendSettingsEditorWindow.window != null)
                    CurvedWorldBendSettingsEditorWindow.window.Close();
            }

            private void OnDisable()
            {
                activeWindow = null;
            }

            void OnFocus()
            {
                gSceneControllers = null;

                if (gTab == TAB.RenderersOverview)
                    RebuildSceneShadersOverview();

                if(gTab == TAB.CurvedWorldKeywords)
                    gCurvedWorldKeywordsShaderInfo = new EditorUtilities.ShaderCurvedWorldKeywordsInfo(gCurvedWorldKeywordsShader);


                shaderPackages = null;

                Undo.undoRedoPerformed -= CallbackUndo;
                Undo.undoRedoPerformed += CallbackUndo;


                UnityEditor.EditorUtility.ClearProgressBar();
            }

            void OnGUI()
            {
                activeWindow = this;


                if (Event.current != null)
                    mousePosition = Event.current.mousePosition;


                LoadResources();

                GUILayout.Space(10);
                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                {
                    GUILayout.Space(5);
                    DrawOptions();

                    GUILayout.Space(5);
                    scroll = EditorGUILayout.BeginScrollView(scroll);
                    DrawTabs();
                    EditorGUILayout.EndScrollView();
                }


                if (Event.current.keyCode == KeyCode.Escape)
                {
                    gRenderersOverviewEditIndex = null;
                    gRenderersOverviewEditString = string.Empty;
                    GUIUtility.keyboardControl = 0;

                    Repaint();
                }
            }

            void LoadResources()
            {
                if (guiStyleOptionsHeader == null)
                {
                    guiStyleOptionsHeader = new GUIStyle((GUIStyle)"SettingsHeader");
                }

                if (guiStyleOptionsHeaderHeight == 0)
                    guiStyleOptionsHeaderHeight = Mathf.CeilToInt(guiStyleOptionsHeader.CalcSize(new GUIContent("Manage")).y);


                if (guiStyleControllersButton == null)
                {
                    guiStyleControllersButton = new GUIStyle(EditorStyles.miniButtonRight);
                    guiStyleControllersButton.alignment = TextAnchor.MiddleLeft;
                }

                if (guiStyleAnalyzeSaveButton == null)
                {
                    guiStyleAnalyzeSaveButton = new GUIStyle(EditorStyles.miniButtonLeft);
                    guiStyleAnalyzeSaveButton.richText = true;
                }


                if (gRenderersOverviewModeRectWidth == 0)
                    gRenderersOverviewModeRectWidth = Mathf.CeilToInt(EditorStyles.popup.CalcSize(new GUIContent("Selected Objects")).x + 10);


                if (guiStyleButtonTab == null)
                    guiStyleButtonTab = new GUIStyle(GUIStyle.none);
                if (UnityEditor.EditorGUIUtility.isProSkin)
                    guiStyleButtonTab.normal.textColor = Color.white * 0.95f;
            }

            void DrawOptions()
            {
                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(GUILayout.MaxWidth(100)))
                {

                    for (int i = 0; i < preferencesNames.Length; i++)
                    {
                        EditorGUILayout.LabelField(string.Empty);
                        Rect rc = GUILayoutUtility.GetLastRect();


                        EditorGUI.DrawRect(rc, gTab == (TAB)i ? (EditorWindow.focusedWindow == this ? GUI.skin.settings.selectionColor : (UnityEditor.EditorGUIUtility.isProSkin ? Color.white * 0.45f : Color.gray * 0.1f)) : Color.clear);
                        if (GUI.Button(rc, " " + preferencesNames[i], guiStyleButtonTab))
                        {
                            gTab = (TAB)i;

                            if (gTab == TAB.Controllers)
                                FindSceneCurvedWorldControllers();


                            if (gTab == TAB.RenderersOverview)
                                RebuildSceneShadersOverview();
                        }
                    }


                    EditorGUI.DrawRect(new Rect(GUILayoutUtility.GetLastRect().xMax, 0, 1, this.position.height), Color.gray * 0.2f);
                }
            }

            void DrawTabs()
            {
                EditorGUILayout.BeginVertical();

                switch (gTab)
                {
                    case TAB.Manage: DrawTab_Manage(); break;
                    case TAB.Controllers: DrawTab_Controllers(); break;
                    case TAB.RenderersOverview: DrawTab_RenderersOverview(); break;
                    case TAB.CurvedWorldKeywords: DrawTab_CurvedWorldKeywords(); break;
                    case TAB.Activator: DrawTab_Activator(); break;
                    case TAB.ShaderPackages: Draw_ShaderPackages(); break;
                    case TAB.About: Draw_AboutTab(); break;

                    default: break;
                }


                EditorGUILayout.EndVertical();
            }

            void DrawTab_Manage()
            {
                EditorGUILayout.LabelField(preferencesNames[(int)TAB.Manage], guiStyleOptionsHeader, GUILayout.Height(guiStyleOptionsHeaderHeight));
                GUILayout.Space(15);



                EditorGUILayout.LabelField("Bend Type");
                Rect rc = GUILayoutUtility.GetLastRect();
                rc.xMin += UnityEditor.EditorGUIUtility.labelWidth;
                if (GUI.Button(rc, EditorUtilities.GetBendTypeNameInfo(gBendType).forLable, EditorStyles.popup))
                {
                    GenericMenu menu = EditorUtilities.BuildBendTypesMenu(gBendType, CallbackBendTypeMenu);

                    menu.DropDown(new Rect(rc.xMin, rc.yMin, 200, UnityEditor.EditorGUIUtility.singleLineHeight));
                }

                gBendID = EditorGUILayout.IntSlider("Bend ID", gBendID, 1, EditorUtilities.MAX_SUPPORTED_BEND_IDS);



                GUILayout.Space(25);

                string mainCGINCFilePath = EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.cginc);
                if (File.Exists(mainCGINCFilePath) == false)
                {
                    using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                    {
                        if (GUILayout.Button("Generate"))
                        {
                            EditorUtilities.CreateCGINCFile(gBendType, gBendID);

                            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


                            //Rebuild
                            EditorUtilities.RebuildProjectBendsInfo();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Source", EditorStyles.miniLabel);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                    {
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.ObjectField("CGINC File", AssetDatabase.LoadAssetAtPath(mainCGINCFilePath, typeof(UnityEngine.Object)), typeof(UnityEngine.Object), false);

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy Path", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "\"" + mainCGINCFilePath + "\"";
                                    te.text = te.text.Replace(Path.DirectorySeparatorChar, '/');
                                    te.text = "#include " + te.text.Replace('\\', '/');
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }

                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.TextField("Method Name", Path.GetFileNameWithoutExtension(mainCGINCFilePath));

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = Path.GetFileNameWithoutExtension(mainCGINCFilePath);
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }
                    }

                    GUILayout.Space(15);
                    EditorGUILayout.LabelField("Keywords", EditorStyles.miniLabel);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                    {
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.TextField("Bend Type", EditorUtilities.shaderKeywordPrefix_BendType + gBendType.ToString().ToUpperInvariant());

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = EditorUtilities.shaderKeywordPrefix_BendType + gBendType.ToString().ToUpperInvariant();
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }

                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.TextField("Bend ID", EditorUtilities.shaderKeywordPrefix_BendID + gBendID);

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = EditorUtilities.shaderKeywordPrefix_BendID + gBendID;
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }
                    }


                    GUILayout.Space(15);
                    EditorGUILayout.LabelField("Custom Shader", EditorStyles.miniLabel);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                    {
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.LabelField("Material Properties");

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "[CurvedWorldBendSettings] _CurvedWorldBendSettings(\"" + (int)gBendType + "|" + gBendID + "|1\", Vector) = (0, 0, 0, 0)";
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }

                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.LabelField("Definitions");

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "#define " + EditorUtilities.shaderKeywordPrefix_BendType + (gBendType).ToString().ToUpperInvariant();
                                    te.text += System.Environment.NewLine + "#define " + EditorUtilities.shaderKeywordPrefix_BendID + gBendID;
                                    te.text += System.Environment.NewLine + "#pragma shader_feature_local " + EditorUtilities.shaderKeywordName_CurvedWorldDisabled;
                                    te.text += System.Environment.NewLine + "#pragma shader_feature_local " + EditorUtilities.shaderKeywordName_BendTransformNormal;


                                    te.text += System.Environment.NewLine + EditorUtilities.GetCoreTransformFilePathForShader();

                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }

                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.LabelField("Vertex Transformations");

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)";
                                    te.text += System.Environment.NewLine + "   #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON";


                                    te.text += System.Environment.NewLine + "      CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)";
                                    te.text += System.Environment.NewLine + "   #else";
                                    te.text += System.Environment.NewLine + "      CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)";


                                    te.text += System.Environment.NewLine + "   #endif";
                                    te.text += System.Environment.NewLine + "#endif";

                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }
                    }


                    GUILayout.Space(15);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                    {
                        //Unity Shader Graph
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField("Unity Shader Sub-Graph", EditorStyles.miniLabel);

                            using (new AmazingAssets.EditorGUIUtility.GUIEnabled(EditorUtilities.CanGenerateUnityShaderGrap()))
                            {
                                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                                {
                                    if (GUILayout.Button(new GUIContent("Vertex", "Vertex Transformation"), EditorStyles.miniButtonLeft))
                                    {
                                        string subGrapFileLocation = EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.UnityShaderGraphVertex);

                                        if (File.Exists(subGrapFileLocation))
                                        {
                                            PingObject(subGrapFileLocation);
                                        }
                                        else
                                        {
                                            string fileGIUD = AssetDatabase.AssetPathToGUID(mainCGINCFilePath);

                                            if (string.IsNullOrEmpty(fileGIUD) == false)
                                            {
                                                EditorUtilities.CreateSubGraphFile(gBendType, gBendID, fileGIUD, EditorUtilities.EXTENSTION.UnityShaderGraphVertex);

                                                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


                                                PingObject(EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.UnityShaderGraphVertex));
                                            }
                                        }
                                    }

                                    if (GUILayout.Button(new GUIContent("Vertex + Normal", "Vertex And Normal Transformation"), EditorStyles.miniButtonRight))
                                    {
                                        string subGrapFileLocation = EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.UnityShaderGraphNormal);

                                        if (File.Exists(subGrapFileLocation))
                                        {
                                            PingObject(subGrapFileLocation);
                                        }
                                        else
                                        {
                                            string fileGIUD = AssetDatabase.AssetPathToGUID(mainCGINCFilePath);

                                            if (string.IsNullOrEmpty(fileGIUD) == false)
                                            {
                                                EditorUtilities.CreateSubGraphFile(gBendType, gBendID, fileGIUD, EditorUtilities.EXTENSTION.UnityShaderGraphNormal);

                                                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


                                                PingObject(EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.UnityShaderGraphNormal));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Amplify Shader Editor
                        GUILayout.Space(5);
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                        {
                            EditorGUILayout.LabelField("Amplify Shader Function", EditorStyles.miniLabel);

                            using (new AmazingAssets.EditorGUIUtility.GUIEnabled(EditorUtilities.CanGenerateAmplifyShaderFuntion()))
                            {
                                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                                {
                                    if (GUILayout.Button(new GUIContent("Vertex", "Vertex Transformation"), EditorStyles.miniButtonLeft))
                                    {
                                        string subGrapFileLocation = EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.AmplifyShaderEditorVertex);

                                        if (File.Exists(subGrapFileLocation))
                                        {
                                            PingObject(subGrapFileLocation);
                                        }
                                        else
                                        {
                                            string fileGIUD = AssetDatabase.AssetPathToGUID(mainCGINCFilePath);

                                            if (string.IsNullOrEmpty(fileGIUD) == false)
                                            {
                                                EditorUtilities.CreateSubGraphFile(gBendType, gBendID, fileGIUD, EditorUtilities.EXTENSTION.AmplifyShaderEditorVertex);

                                                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


                                                PingObject(EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.AmplifyShaderEditorVertex));
                                            }
                                        }
                                    }

                                    if (GUILayout.Button(new GUIContent("Vertex + Normal", "Vertex And Normal Transformation"), EditorStyles.miniButtonMid))
                                    {
                                        string subGrapFileLocation = EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.AmplifyShaderEditorNormal);

                                        if (File.Exists(subGrapFileLocation))
                                        {
                                            PingObject(subGrapFileLocation);
                                        }
                                        else
                                        {
                                            string fileGIUD = AssetDatabase.AssetPathToGUID(mainCGINCFilePath);

                                            if (string.IsNullOrEmpty(fileGIUD) == false)
                                            {
                                                EditorUtilities.CreateSubGraphFile(gBendType, gBendID, fileGIUD, EditorUtilities.EXTENSTION.AmplifyShaderEditorNormal);

                                                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


                                                PingObject(EditorUtilities.GetBendFileLocation(gBendType, gBendID, EditorUtilities.EXTENSTION.AmplifyShaderEditorNormal));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }


                    GUILayout.Space(15);
                    EditorGUILayout.LabelField("Definitions", EditorStyles.miniLabel);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                    {
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.TextField("Curved World Installed", "CURVEDWORLD_IS_INSTALLED");

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "CURVEDWORLD_IS_INSTALLED";
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }

                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.TextField("Normal Transformation On", "CURVEDWORLD_NORMAL_TRANSFORMATION_ON");

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "CURVEDWORLD_NORMAL_TRANSFORMATION_ON";
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }
                    }


                    GUILayout.Space(15);
                    EditorGUILayout.LabelField("Transformation Macros", EditorStyles.miniLabel);
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                    {
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.TextField("Vertex", "CURVEDWORLD_TRANSFORM_VERTEX");

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "CURVEDWORLD_TRANSFORM_VERTEX";
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }

                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.TextField("Vertex And Normal", "CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL");

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("Copy", GUILayout.MaxWidth(100)))
                                {
                                    TextEditor te = new TextEditor();
                                    te.text = "CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL";
                                    te.SelectAll();
                                    te.Copy();
                                }
                            }
                        }
                    }




                    //if (GUILayout.Button("Regenrerate Core Transform File"))
                    //{
                    //    GenerateCoreTransformFile();
                    //}
                }
            }

            void DrawTab_Controllers()
            {
                EditorGUILayout.LabelField(preferencesNames[(int)TAB.Controllers], guiStyleOptionsHeader, GUILayout.Height(guiStyleOptionsHeaderHeight));
                GUILayout.Space(15);

                if (gSceneControllers == null)
                    FindSceneCurvedWorldControllers();

                if (gSceneControllers.Length == 0 || (gSceneControllers.Length == 1 && gSceneControllers[0] == null))
                {
                    using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                    {
                        if (GUILayout.Button("Create"))
                        {
                            GameObject gameObject = new GameObject("Curved World Controller");
                            Undo.RegisterCreatedObjectUndo(gameObject, "Created Curved World Controller");

                            gameObject.transform.position = Vector3.zero;
                            gameObject.transform.rotation = Quaternion.identity;
                            gameObject.transform.localScale = Vector3.one;

                            gameObject.AddComponent<CurvedWorld.CurvedWorldController>();


                            gSceneControllers = null;
                            Repaint();
                        }
                    }
                }
                else
                {

                    for (int i = 0; i < gSceneControllers.Length; i++)
                    {
                        if (gSceneControllers[i] != null)
                        {
                            using (new AmazingAssets.EditorGUIUtility.GUILayoutBeginVertical(EditorStyles.helpBox))
                            {
                                string headerName = " [ID: " + gSceneControllers[i].bendID + "]   " + EditorUtilities.GetBendTypeNameInfo(gSceneControllers[i].bendType).forLable;

                                using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                                {
                                    using (new AmazingAssets.EditorGUIUtility.GUIEnabled(gSceneControllers[i].enabled && gSceneControllers[i].gameObject.activeInHierarchy))
                                    {
                                        if (GUILayout.Button(headerName, guiStyleControllersButton))
                                        {
                                            gSceneControllers[i].isExpanded = !gSceneControllers[i].isExpanded;
                                        }
                                    }
                                }



                                if (gSceneControllers[i].isExpanded)
                                {
                                    using (new AmazingAssets.EditorGUIUtility.GUIEnabled(gSceneControllers[i].gameObject.activeInHierarchy))
                                    {
                                        EditorGUILayout.ObjectField("Object", gSceneControllers[i].gameObject, typeof(GameObject), false);


                                        bool isEnabled = gSceneControllers[i].enabled;
                                        EditorGUI.BeginChangeCheck();
                                        isEnabled = EditorGUILayout.Toggle("Enable", gSceneControllers[i].enabled);
                                        if (EditorGUI.EndChangeCheck())
                                        {
                                            Undo.RecordObject(gSceneControllers[i], "Script enabled");
                                            gSceneControllers[i].enabled = isEnabled;
                                        }
                                    }

                                    Editor editor = Editor.CreateEditor(gSceneControllers[i]);
                                    editor.OnInspectorGUI();
                                }
                            }
                        }
                    }
                }
            }

            void DrawTab_RenderersOverview()
            {
                EditorGUILayout.LabelField(preferencesNames[(int)TAB.RenderersOverview], guiStyleOptionsHeader, GUILayout.Height(guiStyleOptionsHeaderHeight));
                GUILayout.Space(15);



                if (gRenderersOverviewList == null)
                    RebuildSceneShadersOverview();


                Rect materialsAndShadersCountRect;
                int totalMaterialsCount = 0;
                List<Shader> totalShadersCount = new List<Shader>();


                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                {
                    EditorGUI.BeginChangeCheck();
                    using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(gRenderersOverviewMode == RENDERERS_OVERVIEW_MODE.SelectedOjects ? Color.yellow : Color.white))
                    {
                        gRenderersOverviewMode = (RENDERERS_OVERVIEW_MODE)EditorGUILayout.EnumPopup(gRenderersOverviewMode, GUILayout.Width(gRenderersOverviewModeRectWidth));
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        RebuildSceneShadersOverview();
                    }

                    using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(string.IsNullOrEmpty(gRenderersOverviewFilter) ? Color.white : Color.yellow))
                    {
                        gRenderersOverviewFilter = EditorGUILayout.TextField(string.Empty, gRenderersOverviewFilter, EditorStyles.toolbarSearchField);
                    }

                    using (new AmazingAssets.EditorGUIUtility.GUIEnabled(!string.IsNullOrEmpty(gRenderersOverviewFilter)))
                    {
                        gRenderersOverviewSearchOption = (SEARCH_OPTION)EditorGUILayout.EnumPopup(gRenderersOverviewSearchOption, GUILayout.MaxWidth(110));


                        Rect incudeRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(129));
                        if (GUI.Toggle(new Rect(incudeRect.xMin, incudeRect.yMin, incudeRect.width * 0.5f, incudeRect.height), !gRenderersOverviewFilterExclude, "Include", "Button"))
                        {
                            gRenderersOverviewFilterExclude = false;
                        }
                        if (GUI.Toggle(new Rect(incudeRect.xMin + incudeRect.width * 0.5f, incudeRect.yMin, incudeRect.width * 0.5f, incudeRect.height), gRenderersOverviewFilterExclude, "Exclude", "Button"))
                        {
                            gRenderersOverviewFilterExclude = true;
                        }
                    }
                }

                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                {
                    EditorGUILayout.LabelField(string.Empty);

                    //Material & Shaders icon
                    materialsAndShadersCountRect = GUILayoutUtility.GetLastRect();


                    using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                    {
                        Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(129));

                        if (GUI.Button(new Rect(buttonRect.xMin, buttonRect.yMin, buttonRect.width - 20, buttonRect.height), "Refresh", EditorStyles.miniButtonLeft))
                        {
                            RebuildSceneShadersOverview();
                            Repaint();
                        }

                        if (GUI.Button(new Rect(buttonRect.xMax - 20, buttonRect.yMin, 20, buttonRect.height), "≡", EditorStyles.miniButtonRight))
                        {
                            gRenderersOverviewEditIndex = null;


                            GenericMenu menu = new GenericMenu();

                            menu.AddItem(new GUIContent("Replace Materials With Duplicates"), false, CallbackRenderersOverviewDuplicateMaterials, null);
                            menu.AddItem(new GUIContent("Change Curved World Bend Settings"), false, CallbackRenderersOverviewAdjustCurvedWorld, null);
                            menu.AddSeparator(string.Empty);
                            menu.AddItem(new GUIContent("Generate Missing Curved World CGINC Files"), false, CallbackGenerateMissingCurvedWorldFiles);

                            menu.ShowAsContext();
                        }
                    }
                }


                GUILayout.Space(20);

                for (int i = 0; i < gRenderersOverviewList.Count; i++)
                {
                    if (gRenderersOverviewList[i] == null || gRenderersOverviewList[i].shader == null || gRenderersOverviewList[i].materialsInfo == null || gRenderersOverviewList[i].materialsInfo.Count == 0)
                        continue;


                    if (string.IsNullOrEmpty(gRenderersOverviewFilter) == false)
                    {
                        if (gRenderersOverviewSearchOption == SEARCH_OPTION.ShaderName)
                        {
                            if (gRenderersOverviewList[i].shader.name.Contains(gRenderersOverviewFilter, true) == gRenderersOverviewFilterExclude)
                                continue;
                        }
                        else if (gRenderersOverviewSearchOption == SEARCH_OPTION.MaterialName)
                        {
                            if (gRenderersOverviewList[i].materialsInfo.Where(m => m != null && m.material != null && m.material.name.Contains(gRenderersOverviewFilter, true) != gRenderersOverviewFilterExclude).Count() == 0)
                                continue;
                        }
                        else if (gRenderersOverviewSearchOption == SEARCH_OPTION.Keyword)
                        {
                            if (gRenderersOverviewList[i].keywordsTooltip.Contains(gRenderersOverviewFilter, true) == gRenderersOverviewFilterExclude)
                                continue;
                        }
                    }


                    //Count shaders
                    if (totalShadersCount.Contains(gRenderersOverviewList[i].shader) == false)
                        totalShadersCount.Add(gRenderersOverviewList[i].shader);



                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                    {
                        //Shader Name
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            Rect changeRect = EditorGUILayout.GetControlRect();
                            EditorGUI.DrawRect(changeRect, GUI.skin.settings.selectionColor);
                            EditorGUI.LabelField(changeRect, " " + gRenderersOverviewList[i].shader.name);


                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUI.Button(GUILayoutUtility.GetLastRect(), string.Empty, GUIStyle.none))
                                {
                                    UnityEditor.EditorGUIUtility.PingObject(gRenderersOverviewList[i].shader);
                                }

                                using (new AmazingAssets.EditorGUIUtility.GUIEnabled(gRenderersOverviewList[i].AllMaterialsAreBuiltInResources() ? false : true))
                                {
                                    if (GUILayout.Button("Change", GUILayout.MaxWidth(125)))
                                    {
                                        ShaderInfo[] allShaderInfo = ShaderUtil.GetAllShaderInfo();
                                        List<string> mainShaders = new List<string>();
                                        List<string> legacyShaders = new List<string>();

                                        foreach (ShaderInfo shaderInfo in allShaderInfo)
                                        {
                                            if (!shaderInfo.name.StartsWith("Deprecated") && !shaderInfo.name.StartsWith("Hidden"))
                                            {
                                                if (shaderInfo.hasErrors == false &&
                                                    shaderInfo.supported &&
                                                    string.IsNullOrEmpty(shaderInfo.name) == false &&
                                                    shaderInfo.name.StartsWith("Hidden/") == false)
                                                {
                                                    if (shaderInfo.name.StartsWith("Legacy Shaders/"))
                                                        legacyShaders.Add(shaderInfo.name);
                                                    else
                                                        mainShaders.Add(shaderInfo.name);
                                                }
                                            }
                                        }
                                        mainShaders.Sort();
                                        legacyShaders.Sort();


                                        GenericMenu menu = new GenericMenu();
                                        foreach (var item in mainShaders)
                                        {
                                            menu.AddItem(new GUIContent(item), gRenderersOverviewList[i].shader.name == item, CallBackRenderersOverviewShaderChanged, i + "_" + item);
                                        }

                                        if (legacyShaders.Count > 0)
                                        {
                                            menu.AddSeparator(string.Empty);

                                            foreach (var item in legacyShaders)
                                            {
                                                menu.AddItem(new GUIContent(item), gRenderersOverviewList[i].shader.name == item, CallBackRenderersOverviewShaderChanged, i + "_" + item);
                                            }
                                        }

                                        menu.ShowAsContext();
                                    }
                                }
                            }
                        }


                        //Keywords
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            if (gRenderersOverviewEditIndex != null && gRenderersOverviewEditIndex == gRenderersOverviewList[i])
                            {
                                using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(Color.yellow))
                                {
                                    gRenderersOverviewEditString = EditorGUILayout.TextArea(gRenderersOverviewEditString);
                                }

                                using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(Color.yellow))
                                {
                                    if (GUILayout.Button("Done", GUILayout.MaxWidth(125)))
                                    {
                                        gRenderersOverviewList[i].SetNewKeywords(gRenderersOverviewEditString);

                                        gRenderersOverviewEditIndex = null;
                                        gRenderersOverviewEditString = string.Empty;

                                        GUIUtility.keyboardControl = 0;


                                        gRenderersOverviewList.Clear();
                                        gRenderersOverviewList = null;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                EditorGUILayout.LabelField(new GUIContent(gRenderersOverviewList[i].keywordsString, gRenderersOverviewList[i].keywordsTooltip), EditorStyles.miniLabel);


                                using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                                {
                                    if (GUILayout.Button("Edit", GUILayout.MaxWidth(125)))
                                    {
                                        gRenderersOverviewEditIndex = gRenderersOverviewList[i];
                                        gRenderersOverviewEditString = gRenderersOverviewList[i].keywordsTooltip;
                                    }
                                }
                            }
                        }


                        //Materials
                        int materialsCount = gRenderersOverviewList[i].materialsInfo.Count;
                        if (string.IsNullOrEmpty(gRenderersOverviewFilter) == false && gRenderersOverviewSearchOption == SEARCH_OPTION.MaterialName)
                        {
                            materialsCount = gRenderersOverviewList[i].materialsInfo.Where(m => m != null && m.material != null && m.material.name.Contains(gRenderersOverviewFilter, true) != gRenderersOverviewFilterExclude).Count();
                        }

                        gRenderersOverviewList[i].foldout = EditorGUILayout.Foldout(gRenderersOverviewList[i].foldout, " Materials (" + materialsCount + ")", false);

                        totalMaterialsCount += materialsCount;


                        if (gRenderersOverviewList[i].foldout)
                        {
                            for (int m = 0; m < gRenderersOverviewList[i].materialsInfo.Count; m++)
                            {
                                if (gRenderersOverviewList[i].materialsInfo[m] == null)
                                    continue;


                                if (string.IsNullOrEmpty(gRenderersOverviewFilter) ||
                                    gRenderersOverviewSearchOption != SEARCH_OPTION.MaterialName ||
                                    (gRenderersOverviewSearchOption == SEARCH_OPTION.MaterialName && gRenderersOverviewList[i].materialsInfo[m].material.name.Contains(gRenderersOverviewFilter, true) != gRenderersOverviewFilterExclude))
                                {
                                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                                    {
                                        using (new AmazingAssets.EditorGUIUtility.GUIEnabled(gRenderersOverviewList[i].materialsInfo[m].isBuiltInresource ? false : true))
                                        {
                                            EditorGUILayout.ObjectField(string.Empty, gRenderersOverviewList[i].materialsInfo[m].material, typeof(Material), false);
                                        }

                                        Rect buttonRect = EditorGUILayout.GetControlRect(GUILayout.MaxWidth(125));

                                        //Find References
                                        if (GUI.Button(new Rect(buttonRect.xMin, buttonRect.yMin, buttonRect.width - 40, buttonRect.height), "Find Ref", EditorStyles.miniButtonLeft))
                                        {
                                            Transform saveCurrentTransform = Selection.activeTransform;

                                            Selection.instanceIDs = new int[] { gRenderersOverviewList[i].materialsInfo[m].material.GetInstanceID() };
                                            EditorApplication.ExecuteMenuItem("Assets/Find References In Scene");

                                            Selection.activeTransform = saveCurrentTransform;
                                        }

                                        if (GUI.Button(new Rect(buttonRect.xMax - 40, buttonRect.yMin, 20, buttonRect.height), "☼", EditorStyles.miniButtonLeft))
                                        {
                                            PingObject(gRenderersOverviewList[i].materialsInfo[m].material);
                                        }

                                        if (GUI.Button(new Rect(buttonRect.xMax - 20, buttonRect.yMin, 20, buttonRect.height), "≡", EditorStyles.miniButtonRight))
                                        {
                                            GenericMenu menu = new GenericMenu();

                                            menu.AddItem(new GUIContent("Replace Material With Duplicate"), false, CallbackRenderersOverviewDuplicateMaterials, gRenderersOverviewList[i].materialsInfo[m].material);

                                            if (EditorUtilities.HasShaderCurvedWorldBendSettingsProperty(gRenderersOverviewList[i].materialsInfo[m].material.shader))
                                                menu.AddItem(new GUIContent("Change Curved World Bend Settings"), false, CallbackRenderersOverviewAdjustCurvedWorld, gRenderersOverviewList[i].materialsInfo[m].material);
                                            else
                                                menu.AddDisabledItem(new GUIContent("Change Curved World Bend Settings"));

                                            menu.ShowAsContext();
                                        }
                                    }
                                }
                            }
                        }
                    }

                    GUILayout.Space(5);
                }



                materialsAndShadersCountRect.yMin += 2;
                materialsAndShadersCountRect.yMax += 2;
                if (gRenderersOverviewMode == RENDERERS_OVERVIEW_MODE.SelectedOjects)
                {
                    GUI.Box(materialsAndShadersCountRect, new GUIContent(Selection.gameObjects == null ? "0" : Selection.gameObjects.Length.ToString(), UnityEditor.EditorGUIUtility.IconContent("Selectable Icon").image), EditorStyles.label);
                    materialsAndShadersCountRect.xMin += 55;
                }

                //Shaders count                
                GUI.Box(materialsAndShadersCountRect, new GUIContent(totalShadersCount.Count.ToString(), UnityEditor.EditorGUIUtility.IconContent("Shader Icon").image), EditorStyles.label);

                //Material count
                materialsAndShadersCountRect.xMin += 55;
                GUI.Box(materialsAndShadersCountRect, new GUIContent(totalMaterialsCount.ToString(), UnityEditor.EditorGUIUtility.IconContent("Material Icon").image), EditorStyles.label);
            }

            void DrawTab_CurvedWorldKeywords()
            {
                EditorGUILayout.LabelField(preferencesNames[(int)TAB.CurvedWorldKeywords], guiStyleOptionsHeader, GUILayout.Height(guiStyleOptionsHeaderHeight));
                GUILayout.Space(15);



                EditorGUI.BeginChangeCheck();
                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                {
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                    {
                        gCurvedWorldKeywordsShader = (Shader)EditorGUILayout.ObjectField("Shader", gCurvedWorldKeywordsShader, typeof(Shader), true);


                        using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                        {
                            using (new AmazingAssets.EditorGUIUtility.GUIEnabled(gCurvedWorldKeywordsShader != null))
                            {
                                if (GUILayout.Button("Reload", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(100)))
                                {
                                    //Rebuild info
                                    EditorUtilities.RebuildProjectBendsInfo();


                                    gCurvedWorldKeywordsShaderInfo = new EditorUtilities.ShaderCurvedWorldKeywordsInfo(gCurvedWorldKeywordsShader);

                                    Repaint();
                                }
                            }

                            GUILayout.Space(3);
                            using (new AmazingAssets.EditorGUIUtility.GUIEnabled(gCurvedWorldKeywordsShader != null && gCurvedWorldKeywordsShaderInfo != null && gCurvedWorldKeywordsShaderInfo.IsCurvedWorldShader() && EditorUtilities.IsShaderCurvedWorldTerrain(gCurvedWorldKeywordsShader) == false))
                            {
                                if (GUILayout.Button("≡", EditorStyles.miniButtonRight, GUILayout.MaxWidth(20)))
                                {
                                    GenericMenu menu = new GenericMenu();

                                    menu.AddItem(new GUIContent("Uncheck all"), false, CallbackCurvedWorldKeywordsUnckechAll);
                                    menu.AddSeparator(string.Empty);


                                    string label = EditorUtilities.BendSettingsToString(gCurvedWorldKeywordsShaderInfo.GetSelectedBendTypes(), gCurvedWorldKeywordsShaderInfo.GetSelectedBendIDs(), false);

                                    menu.AddItem(new GUIContent("Rewrite all project Curved World shaders"), false, CallbackCurvedWorldKeywordsRewriteAllProjectShaders, label);

                                    menu.ShowAsContext();
                                }
                            }
                        }
                    }
                }
                if (EditorGUI.EndChangeCheck() || gCurvedWorldKeywordsShaderInfo == null)
                {
                    gCurvedWorldKeywordsShaderInfo = new EditorUtilities.ShaderCurvedWorldKeywordsInfo(gCurvedWorldKeywordsShader);
                }

                bool shaderIsCurvedWorldTerrain = EditorUtilities.IsShaderCurvedWorldTerrain(gCurvedWorldKeywordsShader);


                if (gCurvedWorldKeywordsShader != null && gCurvedWorldKeywordsShaderInfo.IsCurvedWorldShader())
                {
                    Rect saveButtonRect;

                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                    {
                        using (new AmazingAssets.EditorGUIUtility.GUIEnabled(shaderIsCurvedWorldTerrain == false))
                        {
                            EditorGUILayout.LabelField(string.Empty, GUILayout.MaxWidth(150));

                            saveButtonRect = EditorGUILayout.GetControlRect();

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                            {
                                if (GUILayout.Button("≡", EditorStyles.miniButtonRight, GUILayout.MaxWidth(20)))
                                {
                                    GenericMenu menu = new GenericMenu();

                                    menu.AddItem(new GUIContent("shader_feature"), false, CallbackCurvedWorldKeywordsMultiCompile, false);
                                    menu.AddItem(new GUIContent("multi_compile"), false, CallbackCurvedWorldKeywordsMultiCompile, true);

                                    menu.ShowAsContext();
                                }
                            }
                        }
                    }




                    #region Save Button
                    int usedKeywordsCount = gCurvedWorldKeywordsShaderInfo.selectedBendTypes.Count(x => x == true) == 1 ? 0 : gCurvedWorldKeywordsShaderInfo.selectedBendTypes.Count(x => x == true);
                    usedKeywordsCount += gCurvedWorldKeywordsShaderInfo.selectedBendIDs.Count(x => x == true) == 1 ? 0 : gCurvedWorldKeywordsShaderInfo.selectedBendIDs.Count(x => x == true);

                    bool isValed = false;
                    if (usedKeywordsCount < 65 &&
                        gCurvedWorldKeywordsShaderInfo.selectedBendIDs.Count(x => x == true) > 0 &&
                        gCurvedWorldKeywordsShaderInfo.selectedBendTypes.Count(x => x == true) > 0 &&
                        shaderIsCurvedWorldTerrain == false)
                    {
                        isValed = true;
                    }


                    using (new AmazingAssets.EditorGUIUtility.GUIEnabled(isValed))
                    {
                        using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(GUI.skin.settings.selectionColor))
                        {
                            if (GUI.Button(saveButtonRect, "Save   <size=10>(Keywords: " + usedKeywordsCount + ")   " + (gCurvedWorldKeywordsShaderInfo.selecedMultiCompile ? "multi_compile" : "shader_feature") + "</size>", guiStyleAnalyzeSaveButton))
                            {

                                List<CurvedWorld.BEND_TYPE> selectedBendTypes = new List<CurvedWorld.BEND_TYPE>();
                                for (int i = 0; i < EditorUtilities.MAX_SUPPORTED_BEND_TYPES; i++)
                                {
                                    if (gCurvedWorldKeywordsShaderInfo.selectedBendTypes[i])
                                        selectedBendTypes.Add((CurvedWorld.BEND_TYPE)i);
                                }

                                List<int> selectedIDs = new List<int>();
                                for (int i = 0; i < EditorUtilities.MAX_SUPPORTED_BEND_IDS; i++)
                                {
                                    if (gCurvedWorldKeywordsShaderInfo.selectedBendIDs[i])
                                        selectedIDs.Add(i + 1);    //ID indexes start from 1, not 0
                                }


                                //Create CGING files
                                UnityEditor.EditorUtility.DisplayProgressBar("Hold On", string.Empty, 0);
                                foreach (var itemBendType in selectedBendTypes)
                                {
                                    foreach (var itemBendID in selectedIDs)
                                    {
                                        EditorUtilities.CreateCGINCFile(itemBendType, itemBendID);
                                    }
                                }

                                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                                EditorUtilities.SetShaderBendSettings(gCurvedWorldKeywordsShader, selectedBendTypes.ToArray(), selectedIDs.ToArray(), gCurvedWorldKeywordsShaderInfo.selecedMultiCompile ? EditorUtilities.KEYWORDS_COMPILE.MultiCompile : EditorUtilities.KEYWORDS_COMPILE.ShaderFeature, true);

                                UnityEditor.EditorUtility.ClearProgressBar();
                            }
                        }
                    }
                    #endregion


                    #region Bend IDs
                    GUILayout.Space(5);
                    using (new AmazingAssets.EditorGUIUtility.GUIEnabled(shaderIsCurvedWorldTerrain == false))
                    {
                        int buttonsCountHorizontal = 16;

                        GUIContent[] buttonNames = new GUIContent[EditorUtilities.MAX_SUPPORTED_BEND_IDS];
                        for (int i = 0; i < EditorUtilities.MAX_SUPPORTED_BEND_IDS; i++)
                        {
                            buttonNames[i] = new GUIContent((i + 1).ToString());
                        }
                        Rect rc1 = EditorGUILayout.GetControlRect();
                        Rect rc4 = EditorGUILayout.GetControlRect();

                        Rect position = new Rect(rc1.xMin, rc1.yMin, rc1.width, (rc4.yMax - rc1.yMin));


                        Rect[] rectArray = CalcButtonRects(position, buttonNames, buttonsCountHorizontal);

                        for (int i = 0; i < rectArray.Length; i++)
                        {
                            bool isBendIDSupported = true;
                            if (gCurvedWorldKeywordsShaderInfo.selectedBendIDs[i])
                            {
                                for (int j = 0; j < EditorUtilities.MAX_SUPPORTED_BEND_TYPES; j++)
                                {
                                    if (gCurvedWorldKeywordsShaderInfo.selectedBendTypes[j])
                                    {
                                        string bendFileLocation = EditorUtilities.GetBendFileLocation((CurvedWorld.BEND_TYPE)j, i + 1, EditorUtilities.EXTENSTION.cginc);
                                        if (File.Exists(bendFileLocation) == false)
                                        {
                                            isBendIDSupported = false;

                                            break;
                                        }
                                    }
                                }
                            }

                            using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(gCurvedWorldKeywordsShaderInfo.selectedBendIDs[i] ? (isBendIDSupported ? GUI.skin.settings.selectionColor : Color.red) : Color.white))
                            {
                                if (GUI.Button(rectArray[i], buttonNames[i]))
                                    gCurvedWorldKeywordsShaderInfo.selectedBendIDs[i] = !gCurvedWorldKeywordsShaderInfo.selectedBendIDs[i];
                            }
                        }
                    }
                    #endregion


                    #region Bend Types
                    GUILayout.Space(5);
                    using (new AmazingAssets.EditorGUIUtility.GUIEnabled(shaderIsCurvedWorldTerrain == false))
                    {
                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                        {
                            for (int i = 0; i < EditorUtilities.MAX_SUPPORTED_BEND_TYPES; i++)
                            {
                                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                                {
                                    GUILayout.Space(2);
                                    Rect rc = EditorGUILayout.GetControlRect();

                                    if (gCurvedWorldKeywordsShaderInfo.selectedBendTypes[i])
                                    {
                                        bool isBendTypeSupported = true;
                                        for (int j = 0; j < EditorUtilities.MAX_SUPPORTED_BEND_IDS; j++)
                                        {
                                            if (gCurvedWorldKeywordsShaderInfo.selectedBendIDs[j])
                                            {
                                                string bendFileLocation = EditorUtilities.GetBendFileLocation((CurvedWorld.BEND_TYPE)i, j + 1, EditorUtilities.EXTENSTION.cginc);
                                                if (File.Exists(bendFileLocation) == false)
                                                {
                                                    isBendTypeSupported = false;

                                                    break;
                                                }
                                            }
                                        }

                                        EditorGUI.DrawRect(new Rect(rc.xMin - 2, rc.yMin, rc.width + 2, rc.height), isBendTypeSupported ? GUI.skin.settings.selectionColor : Color.red);

                                    }


                                    EditorUtilities.BendTypeNameInfo info = EditorUtilities.GetBendTypeNameInfo((CurvedWorld.BEND_TYPE)i);
                                    gCurvedWorldKeywordsShaderInfo.selectedBendTypes[i] = EditorGUI.ToggleLeft(rc, info.forMenu, gCurvedWorldKeywordsShaderInfo.selectedBendTypes[i]);
                                }
                            }
                        }
                    }
                    #endregion
                }
                else if (gCurvedWorldKeywordsShader != null)
                {
                    EditorGUILayout.HelpBox("Curved World keywords not found.", MessageType.Info);
                }
            }

            void DrawTab_Activator()
            {
                EditorGUILayout.LabelField(preferencesNames[(int)TAB.Activator], guiStyleOptionsHeader, GUILayout.Height(guiStyleOptionsHeaderHeight));
                GUILayout.Space(15);


                bool folderExist = Directory.Exists(gActivatorPath);

                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                {
                    using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(folderExist ? Color.white : Color.red))
                    {
                        EditorGUILayout.LabelField("Asset Folder");

                        using (new AmazingAssets.EditorGUIUtility.GUIEnabled(folderExist))
                        {
                            Rect buttonRect = GUILayoutUtility.GetLastRect();
                            buttonRect.xMin += UnityEditor.EditorGUIUtility.labelWidth;
                            if (GUI.Button(buttonRect, folderExist ? gActivatorPath : "Invalid path", EditorStyles.textField))
                            {
                                if (folderExist)
                                {
                                    UnityEditor.EditorUtility.FocusProjectWindow();

                                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(gActivatorPath);

                                    if (obj != null)
                                        UnityEditor.EditorGUIUtility.PingObject(obj);
                                }
                            }
                        }
                    }


                    if (GUILayout.Button("Select", GUILayout.MaxWidth(100)))
                    {
                        GUIUtility.keyboardControl = 0;

                        string initialPath = gActivatorPath;
                        if (string.IsNullOrEmpty(initialPath) || Directory.Exists(initialPath) == false)
                        {
                            initialPath = Application.dataPath;
                        }

                        string tempPath = gActivatorPath;
                        tempPath = UnityEditor.EditorUtility.OpenFolderPanel("Select folder within project Assets folder", initialPath, string.Empty);


                        if (string.IsNullOrEmpty(tempPath))
                        {

                        }
                        else if (tempPath.Contains(Application.dataPath) == false || Directory.Exists(tempPath) == false)
                        {
                            gActivatorPath = string.Empty;

                            UnityEditor.EditorUtility.DisplayDialog("Invalid path", "Use path relative to the project folder.", "Ok");
                        }
                        else
                        {
                            gActivatorPath = tempPath.Replace(Application.dataPath, "Assets");
                        }
                    }
                }

                GUILayout.Space(15);


                using (new AmazingAssets.EditorGUIUtility.GUIEnabled(Directory.Exists(gActivatorPath)))
                {
                    using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                    {
                        if (GUILayout.Button("Activate", GUILayout.MaxHeight(20)))
                        {
                            Debug.ClearDeveloperConsole();
                            UnityEditor.EditorUtility.DisplayProgressBar("Hold On", string.Empty, 0);

                            LoadActivatorShaders();


                            int doneCount = 0;
                            int skipCount = 0;
                            int problemCount = 0;

                            for (int i = 0; i < gActivatorShaders.Count; i++)
                            {
                                if (gActivatorShaders[i] != null)
                                {
                                    UnityEditor.EditorUtility.DisplayProgressBar("Activating", gActivatorShaders[i].name, 1.0f / gActivatorShaders.Count);

                                    switch (EditorUtilities.ActivateShader(gActivatorShaders[i], true, false))
                                    {
                                        case EditorUtilities.ACTIVATE_STATE.Done: doneCount += 1; break;
                                        case EditorUtilities.ACTIVATE_STATE.Skip: skipCount += 1; break;

                                        case EditorUtilities.ACTIVATE_STATE.Problem:
                                            {
                                                problemCount += 1;
                                            }
                                            break;

                                    }
                                }
                            }

                            UnityEditor.EditorUtility.ClearProgressBar();


                            this.ShowNotification(new GUIContent(string.Format("Activated: {0}" + System.Environment.NewLine + "Skip: {1}" + System.Environment.NewLine + "Problems: {2}", doneCount, skipCount, problemCount)), 5);


                            if (doneCount > 0)
                                AssetDatabase.Refresh();

                        }

                        if (GUILayout.Button("Deactivate", GUILayout.MaxHeight(20)))
                        {
                            Debug.ClearDeveloperConsole();
                            UnityEditor.EditorUtility.DisplayProgressBar("Hold On", string.Empty, 0);

                            LoadActivatorShaders();


                            int doneCount = 0;
                            int skipCount = 0;
                            int problemCount = 0;

                            for (int i = 0; i < gActivatorShaders.Count; i++)
                            {
                                if (gActivatorShaders[i] != null)
                                {
                                    UnityEditor.EditorUtility.DisplayProgressBar("Deactivating", gActivatorShaders[i].name, 1.0f / gActivatorShaders.Count);

                                    switch (EditorUtilities.ActivateShader(gActivatorShaders[i], false, false))
                                    {
                                        case EditorUtilities.ACTIVATE_STATE.Done: doneCount += 1; break;
                                        case EditorUtilities.ACTIVATE_STATE.Skip: skipCount += 1; break;

                                        case EditorUtilities.ACTIVATE_STATE.Problem:
                                            {
                                                problemCount += 1;
                                            }
                                            break;
                                    }
                                }
                            }

                            UnityEditor.EditorUtility.ClearProgressBar();


                            this.ShowNotification(new GUIContent(string.Format("Deactivated: {0}" + System.Environment.NewLine + "Skip: {1}" + System.Environment.NewLine + "Problems: {2}", doneCount, skipCount, problemCount)), 5);


                            if (doneCount > 0)
                                AssetDatabase.Refresh();
                        }
                    }
                }
            }

            void Draw_ShaderPackages()
            {
                EditorGUILayout.LabelField(preferencesNames[(int)TAB.ShaderPackages], guiStyleOptionsHeader, GUILayout.Height(guiStyleOptionsHeaderHeight));
                GUILayout.Space(15);

                if (shaderPackages == null)
                {
                    string path = Path.Combine(EditorUtilities.GetCurvedWorldEditorFolderPath(), "Shaders", "Packages");
                    if (string.IsNullOrEmpty(path) || Directory.Exists(path) == false)
                    {
                        shaderPackages = new string[0];
                        return;
                    }


                    shaderPackages = Directory.GetFiles(path, "*.unitypackage", SearchOption.AllDirectories);
                }

                if (shaderPackages.Length == 0)
                    return;



                string shadersFolderPath = Path.Combine(EditorUtilities.GetCurvedWorldEditorFolderPath(), "Shaders", "Custom");


                bool needRefresh = false;
                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                {
                    for (int i = 0; i < shaderPackages.Length; i++)
                    {
                        if (shaderPackages[i] == null || string.IsNullOrEmpty(shaderPackages[i]))
                            continue;

                        string packageName = Path.GetFileNameWithoutExtension(shaderPackages[i]);
                        string folderName = Path.Combine(shadersFolderPath, packageName);

                        bool folderExist = Directory.Exists(folderName);


                        int selectionID = -1;
                        Rect selectionRect;

                        using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                        {
                            EditorGUILayout.LabelField(packageName);
                            selectionRect = GUILayoutUtility.GetLastRect();


                            //Import
                            using (new AmazingAssets.EditorGUIUtility.GUIEnabled(folderExist ? false : true))
                            {
                                using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(folderExist ?Color.white : Color.green * 0.95f))
                                {
                                    if (GUILayout.Button(" Import "))
                                    {
                                        AssetDatabase.ImportPackage(shaderPackages[i], false);

                                        needRefresh = true;

                                        break;
                                    }
                                }
                            }
                            if (Event.current != null && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                                selectionID = i;


                            //Reimport
                            using (new AmazingAssets.EditorGUIUtility.GUIEnabled(folderExist))
                            {
                                using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(folderExist ? GUI.skin.settings.selectionColor : Color.white))
                                {
                                    if (GUILayout.Button("Reimport"))
                                    {
                                        AssetDatabase.ImportPackage(shaderPackages[i], false);

                                        needRefresh = true;

                                        break;
                                    }
                                }
                            }
                            if (Event.current != null && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                                selectionID = i;


                            //Remove
                            using (new AmazingAssets.EditorGUIUtility.GUIEnabled(folderExist))
                            {
                                using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(folderExist ? Color.red * 0.95f : Color.white))
                                {
                                    if (GUILayout.Button("Remove"))
                                    {
                                        string removeFile = Path.Combine(shadersFolderPath, packageName);

                                        FileUtil.DeleteFileOrDirectory(removeFile);
                                        FileUtil.DeleteFileOrDirectory(removeFile + ".meta");


                                        if (packageName == "TextMesh Pro")
                                        {
                                            removeFile = Path.Combine(EditorUtilities.GetCurvedWorldEditorFolderPath(), "Editor", "Material Editors", "TextMesh Pro");


                                            FileUtil.DeleteFileOrDirectory(removeFile);
                                            FileUtil.DeleteFileOrDirectory(removeFile + ".meta");
                                        }

                                        if (packageName == "SpeedTree")
                                        {
                                            removeFile = Path.Combine(EditorUtilities.GetCurvedWorldEditorFolderPath(), "Editor", "Material Editors", "SpeedTree");


                                            FileUtil.DeleteFileOrDirectory(removeFile);
                                            FileUtil.DeleteFileOrDirectory(removeFile + ".meta");
                                        }

                                        needRefresh = true;
                                    }
                                }
                            }
                            if (Event.current != null && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                                selectionID = i;
                        }


                        //Draw selection rect
                        if (selectionID == i)
                            //EditorGUI.HelpBox(GUILayoutUtility.GetLastRect(), string.Empty, MessageType.None);
                            //EditorGUI.LabelField(GUILayoutUtility.GetLastRect(), "•");
                            EditorGUI.DrawRect(selectionRect, Color.gray * 0.15f);
                    }
                }

                if (needRefresh)
                {
                    AssetDatabase.Refresh();
                }
            }

            void Draw_AboutTab()
            {
                EditorGUILayout.LabelField("Curved World", guiStyleOptionsHeader, GUILayout.Height(guiStyleOptionsHeaderHeight));
                EditorGUILayout.LabelField("Installed Version: " + EditorUtilities.version, EditorStyles.miniLabel);
                GUILayout.Space(15);


                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginHorizontal())
                {
                    if (GUILayout.Button(new GUIContent("Manual", UnityEditor.EditorGUIUtility.IconContent("vcs_document").image), GUILayout.MaxHeight(24)))
                    {
                        string filePath = Path.Combine(EditorUtilities.GetCurvedWorldEditorFolderPath(), "Documentation", "Manual.pdf");
                        if (File.Exists(filePath))
                        {
                            filePath = filePath.Replace('/', Path.DirectorySeparatorChar);

                            System.Diagnostics.Process fileopener = new System.Diagnostics.Process();
                            fileopener.StartInfo.FileName = "explorer";
                            fileopener.StartInfo.Arguments = "\"" + filePath + "\"";
                            fileopener.Start();
                        }
                        else
                        {
                            Debug.LogWarning("File '" + filePath + "' does not exist.");
                        }
                    }

                    if (GUILayout.Button(new GUIContent("Forum", UnityEditor.EditorGUIUtility.IconContent("BuildSettings.Web.Small").image), GUILayout.MaxHeight(24)))
                    {
                        Application.OpenURL(EditorUtilities.assetForumPath);
                    }


                    if (GUILayout.Button(new GUIContent("Support E-Mail", UnityEditor.EditorGUIUtility.IconContent("NetworkLobbyPlayer Icon").image), GUILayout.MaxHeight(24)))
                    {
                        Application.OpenURL(EditorUtilities.assetSupportMail);
                    }


                    if (GUILayout.Button(new GUIContent("Rate Asset", UnityEditor.EditorGUIUtility.IconContent("Favorite").image), GUILayout.MaxHeight(24)))
                    {
                        UnityEditorInternal.AssetStore.Open(EditorUtilities.assetStorePath);
                    }
                }
            }



            void GenerateCoreTransformFile()
            {
                string filePath = EditorUtilities.GetCoreTransformFilePath();


                List<string> file = new List<string>();

                file.Add("#ifndef CURVEDWORLD_TRANSFORM_CGINC");
                file.Add("#define CURVEDWORLD_TRANSFORM_CGINC");
                file.Add(System.Environment.NewLine);

                file.Add("#ifndef CURVEDWORLD_IS_INSTALLED");
                file.Add("#define CURVEDWORLD_IS_INSTALLED");
                file.Add("#endif");
                file.Add(System.Environment.NewLine);

                foreach (CurvedWorld.BEND_TYPE bendType in Enum.GetValues(typeof(CurvedWorld.BEND_TYPE)))
                {
                    if ((int)bendType == 0)
                        file.Add("#if defined (" + EditorUtilities.shaderKeywordPrefix_BendType + bendType.ToString().ToUpperInvariant() + ")");
                    else
                        file.Add("#elif defined (" + EditorUtilities.shaderKeywordPrefix_BendType + bendType.ToString().ToUpperInvariant() + ")");

                    for (int i = 0; i < EditorUtilities.MAX_SUPPORTED_BEND_IDS; i++)
                    {
                        int ID = i + 1;

                        if (ID == 1)
                            file.Add("    #if defined (" + EditorUtilities.shaderKeywordPrefix_BendID + ID + ")");
                        else
                            file.Add("    #elif defined (" + EditorUtilities.shaderKeywordPrefix_BendID + ID + ")");


                        string methodName = "CurvedWorld_" + bendType.ToString() + "_ID" + ID;
                        file.Add("        #include \"../CGINC/" + EditorUtilities.GetBendTypeNameInfo(bendType).nameOnly + "/" + methodName + ".cginc\"");
                        file.Add("        #define CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v, n, t) " + methodName + "(v, n, t);");
                        file.Add("        #define CURVEDWORLD_TRANSFORM_VERTEX(v)                  " + methodName + "(v);	");
                    }

                    file.Add("    #else");
                    file.Add("        #define CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v, n, t) ");
                    file.Add("        #define CURVEDWORLD_TRANSFORM_VERTEX(v)    ");

                    file.Add("    #endif");
                }


                file.Add("#else");
                file.Add("    #define CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v, n, t) ");
                file.Add("    #define CURVEDWORLD_TRANSFORM_VERTEX(v)    ");

                file.Add("#endif");
                file.Add(System.Environment.NewLine);
                file.Add("#endif");


                File.WriteAllLines(filePath, file.ToArray());


                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
            }



            void FindSceneCurvedWorldControllers()
            {
                gSceneControllers = Resources.FindObjectsOfTypeAll<CurvedWorld.CurvedWorldController>();

                if (gSceneControllers == null)
                    gSceneControllers = new CurvedWorld.CurvedWorldController[] { null };
                else
                    gSceneControllers = gSceneControllers.OrderBy(s => (s.bendType.ToString() + s.bendID.ToString())).ToArray();
            }

            public void RebuildSceneShadersOverview()
            {
                if (gRenderersOverviewList == null)
                    gRenderersOverviewList = new List<EditorUtilities.ShaderOverview>();
                else
                {
                    gRenderersOverviewList = gRenderersOverviewList.Where(c => c != null && c.shader != null).ToList();

                    for (int i = 0; i < gRenderersOverviewList.Count; i++)
                    {
                        gRenderersOverviewList[i].materialsInfo = null;
                    }
                }

                GameObject[] gameObjects = gRenderersOverviewMode == RENDERERS_OVERVIEW_MODE.Scene ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects() : Selection.gameObjects;


                for (int i = 0; i < gameObjects.Length; i++)
                {
                    GameObject go = gameObjects[i];
                    if (go == null)
                        continue;


                    Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
                    if (renderers != null && renderers.Length != 0)
                    {
                        for (int r = 0; r < renderers.Length; r++)
                        {
                            if (renderers[r] != null && renderers[r].sharedMaterials != null && renderers[r].sharedMaterials.Length != 0)
                            {
                                for (int m = 0; m < renderers[r].sharedMaterials.Length; m++)
                                {
                                    Material mat = renderers[r].sharedMaterials[m];
                                    if (mat != null)
                                    {
                                        if (gRenderersOverviewList.Count == 0)
                                            gRenderersOverviewList.Add(new EditorUtilities.ShaderOverview(mat));
                                        else
                                        {
                                            int addIndex = -1;
                                            for (int s = 0; s < gRenderersOverviewList.Count; s++)
                                            {
                                                if (gRenderersOverviewList[s].shader == mat.shader && gRenderersOverviewList[s].ContainSameKeywords(mat.shaderKeywords))
                                                {
                                                    addIndex = s;
                                                    break;
                                                }
                                            }


                                            if (addIndex == -1)
                                            {
                                                gRenderersOverviewList.Add(new EditorUtilities.ShaderOverview(mat));
                                            }
                                            else
                                            {
                                                gRenderersOverviewList[addIndex].AddMaterial(mat);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                //Sort
                //gShaderOverview = gShaderOverview.OrderBy(s => (s.shader.name + s.keywordsString)).ToList();

                //for (int i = 0; i < gShaderOverview.Count; i++)
                //{
                //    gShaderOverview[i].materialsInfo = gShaderOverview[i].materialsInfo.OrderBy(m => m.material.name).ToList();
                //}
            }

            List<Renderer> GetShaderOverviewRenderersByMaterial(List<Material> materials)
            {
                List<Renderer> renderers = new List<Renderer>();


                GameObject[] gameObjects = gRenderersOverviewMode == RENDERERS_OVERVIEW_MODE.Scene ? UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects() : Selection.gameObjects;


                for (int i = 0; i < gameObjects.Length; i++)
                {
                    GameObject go = gameObjects[i];
                    if (go == null)
                        continue;


                    Renderer[] objectRenderers = go.GetComponentsInChildren<Renderer>(true);
                    if (objectRenderers != null && objectRenderers.Length > 0)
                    {
                        for (int r = 0; r < objectRenderers.Length; r++)
                        {
                            if (objectRenderers[r] != null && objectRenderers[r].sharedMaterials != null && objectRenderers[r].sharedMaterials.Intersect(materials).Any())
                            {
                                renderers.Add(objectRenderers[r]);
                            }
                        }
                    }
                }


                renderers = renderers.Distinct().ToList();

                return renderers;
            }

            List<Material> GetShaderOverviewMaterials()
            {
                List<Material> materials = new List<Material>();


                for (int i = 0; i < gRenderersOverviewList.Count; i++)
                {
                    if (gRenderersOverviewList[i] == null || gRenderersOverviewList[i].shader == null || gRenderersOverviewList[i].materialsInfo == null || gRenderersOverviewList[i].materialsInfo.Count == 0)
                        continue;

                    if (string.IsNullOrEmpty(gRenderersOverviewFilter) == false)
                    {
                        if (gRenderersOverviewSearchOption == SEARCH_OPTION.ShaderName)
                        {
                            if (gRenderersOverviewList[i].shader.name.Contains(gRenderersOverviewFilter, true) == gRenderersOverviewFilterExclude)
                                continue;
                        }
                        else if (gRenderersOverviewSearchOption == SEARCH_OPTION.MaterialName)
                        {
                            if (gRenderersOverviewList[i].materialsInfo.Where(m => m != null && m.material != null && m.material.name.Contains(gRenderersOverviewFilter, true) != gRenderersOverviewFilterExclude).Count() == 0)
                                continue;
                        }
                        else if (gRenderersOverviewSearchOption == SEARCH_OPTION.Keyword)
                        {
                            if (gRenderersOverviewList[i].keywordsTooltip.Contains(gRenderersOverviewFilter, true) == gRenderersOverviewFilterExclude)
                                continue;
                        }
                    }

                    for (int m = 0; m < gRenderersOverviewList[i].materialsInfo.Count; m++)
                    {
                        if (gRenderersOverviewList[i].materialsInfo[m] == null)
                            continue;

                        if (string.IsNullOrEmpty(gRenderersOverviewFilter) ||
                            gRenderersOverviewSearchOption != SEARCH_OPTION.MaterialName ||
                            (gRenderersOverviewSearchOption == SEARCH_OPTION.MaterialName && gRenderersOverviewList[i].materialsInfo[m].material.name.Contains(gRenderersOverviewFilter, true) != gRenderersOverviewFilterExclude))
                        {
                            materials.Add(gRenderersOverviewList[i].materialsInfo[m].material);
                        }
                    }
                }

                return materials;
            }


            void CallbackBendTypeMenu(object obj)
            {
                CurvedWorld.BEND_TYPE newBendType = (CurvedWorld.BEND_TYPE)obj;

                gBendType = (CurvedWorld.BEND_TYPE)obj;
            }

            void CallBackRenderersOverviewShaderChanged(object obj)
            {
                if (obj == null)
                    return;

                //example:   "12_Amazing Assets/Curved World/Unlit"
                //12 - index in array
                //Everything after '_' is shader name


                string data = obj.ToString();
                if (string.IsNullOrEmpty(data))
                    return;

                int startIndex = data.IndexOf('_');
                if (startIndex == 0)
                    return;

                int ID = -1;
                if (int.TryParse(data.Substring(0, startIndex), out ID) == false)
                    return;

                Shader shader = Shader.Find(data.Substring(startIndex + 1));
                if (shader == null)
                    return;


                CurvedWorld.BEND_TYPE[] bendTypes;
                int[] bendIDs;
                bool hasNormalTransform;

                string bendTypeSkeyword = string.Empty;
                if (EditorUtilities.GetShaderSupportedBendSettings(shader, out bendTypes, out bendIDs, out hasNormalTransform))
                {
                    bendTypeSkeyword = EditorUtilities.GetKeywordName(bendTypes[0]);
                }

                for (int i = 0; i < gRenderersOverviewList[ID].materialsInfo.Count; i++)
                {
                    if (gRenderersOverviewList[ID].materialsInfo[i] != null && gRenderersOverviewList[ID].materialsInfo[i].material != null && gRenderersOverviewList[ID].materialsInfo[i].isBuiltInresource == false)
                    {
                        Undo.RecordObject(gRenderersOverviewList[ID].materialsInfo[i].material, "Change Shader");
                        gRenderersOverviewList[ID].materialsInfo[i].material.shader = shader;

                        if (string.IsNullOrEmpty(bendTypeSkeyword) == false)
                        {
                            if (gRenderersOverviewList[ID].materialsInfo[i].material.shaderKeywords == null || gRenderersOverviewList[ID].materialsInfo[i].material.shaderKeywords.Contains(bendTypeSkeyword) == false)
                            {
                                hasNormalTransform = hasNormalTransform && gRenderersOverviewList[ID].materialsInfo[i].material.IsKeywordEnabled(EditorUtilities.shaderKeywordName_BendTransformNormal);

                                EditorUtilities.SetMaterialBendSettings(gRenderersOverviewList[ID].materialsInfo[i].material, bendTypes[0], bendIDs[0], hasNormalTransform);
                            }
                        }
                    }
                }


                if (gRenderersOverviewList != null)
                    gRenderersOverviewList.Clear();
                gRenderersOverviewList = null;


                AssetDatabase.SaveAssets();
                Repaint();
            }

            void CallbackRenderersOverviewDuplicateMaterials(object obj)
            {
                CurvedWorldMaterialDuplicateEditorWindow.ShowWindow(this.position.center, CallbackRenderersOverviewDuplicateMaterials, obj);
            }

            void CallbackRenderersOverviewDuplicateMaterials(string subFolderName, string prefix, string suffix, object obj)
            {
                List<Material> originalMaterials = (Material)obj == null ? GetShaderOverviewMaterials() : new List<Material>() { (Material)obj };

                List<string> duplicateMaterialPath = new List<string>();
                List<Material> duplicateMaterials = new List<Material>();

                //Create duplicates
                for (int m = 0; m < originalMaterials.Count; m++)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("Creating Material Duplicates", originalMaterials[m].name, (float)m / originalMaterials.Count);

                    string savePath = string.Empty;
                    if (string.IsNullOrEmpty(subFolderName))    //Same folder as material
                    {
                        savePath = AssetDatabase.GetAssetPath(originalMaterials[m].GetInstanceID());
                        if (savePath == "Library/unity default resources" || savePath == "Resources/unity_builtin_extra")
                            savePath = "Assets/unity default resources";
                        else
                        {
                            if (File.Exists(savePath))
                                savePath = Path.GetDirectoryName(savePath);
                        }

                        if (Directory.Exists(savePath) == false)
                            Directory.CreateDirectory(savePath);
                    }
                    else    //SubFolder
                    {
                        savePath = AssetDatabase.GetAssetPath(originalMaterials[m].GetInstanceID());
                        if (savePath == "Library/unity default resources" || savePath == "Resources/unity_builtin_extra")
                            savePath = "Assets/unity default resources";
                        else
                        {
                            if (File.Exists(savePath))
                                savePath = Path.GetDirectoryName(savePath);
                        }

                        savePath = Path.Combine(savePath, subFolderName);
                        if (Directory.Exists(savePath) == false)
                            Directory.CreateDirectory(savePath);
                    }


                    savePath = Path.Combine(savePath, prefix + originalMaterials[m].name + suffix + ".mat");

                    string originalMaterialPath = AssetDatabase.GetAssetPath(originalMaterials[m].GetInstanceID());
                    if (EditorUtilities.IsMaterialBuiltInResource(originalMaterialPath))
                    {
                        File.Copy(originalMaterialPath, savePath);
                    }
                    else
                    {
                        Material newMaterial = new Material(originalMaterials[m].shader);
                        newMaterial.CopyPropertiesFromMaterial(originalMaterials[m]);

                        AssetDatabase.CreateAsset(newMaterial, savePath);
                    }


                    duplicateMaterialPath.Add(savePath);

                }

                UnityEditor.EditorUtility.ClearProgressBar();

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


                for (int i = 0; i < duplicateMaterialPath.Count; i++)
                {
                    duplicateMaterials.Add((Material)AssetDatabase.LoadAssetAtPath(duplicateMaterialPath[i], typeof(Material)));
                }



                //Replace materials
                List<Renderer> renderers = GetShaderOverviewRenderersByMaterial(originalMaterials);

                Undo.RecordObjects(renderers.ToArray(), "Duplicate Materials (" + renderers.Count + " Elements)");

                for (int r = 0; r < renderers.Count; r++)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("Replacing", renderers[r].name, (float)r / renderers.Count);

                    Material[] shaderedMaterials = renderers[r].sharedMaterials;
                    for (int m = 0; m < shaderedMaterials.Length; m++)
                    {
                        for (int d = 0; d < duplicateMaterials.Count; d++)
                        {
                            if (shaderedMaterials[m] == originalMaterials[d])
                                shaderedMaterials[m] = duplicateMaterials[d];
                        }
                    }

                    renderers[r].sharedMaterials = shaderedMaterials;
                }

                UnityEditor.EditorUtility.ClearProgressBar();



                //Refresh
                RebuildSceneShadersOverview();
                Repaint();
            }

            void CallbackGenerateMissingCurvedWorldFiles()
            {
                Dictionary<Shader, EditorUtilities.ShaderCurvedWorldKeywordsInfo> shaderData = new Dictionary<Shader, EditorUtilities.ShaderCurvedWorldKeywordsInfo>();


                string[] guids = AssetDatabase.FindAssets("t:Material");


                //Collecting shader data
                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

                    if (material != null && material.shader != null && EditorUtilities.HasShaderCurvedWorldBendSettingsProperty(material.shader))
                    {
                        if (shaderData.ContainsKey(material.shader) == false)
                            shaderData.Add(material.shader, new EditorUtilities.ShaderCurvedWorldKeywordsInfo(material.shader));


                        if (material.HasProperty(EditorUtilities.shaderProprtyName_BendSettings))
                        {
                            CurvedWorld.BEND_TYPE bendType;
                            int bendID;
                            bool normalTransform;

                            Vector4 materialProp = material.GetVector(EditorUtilities.shaderProprtyName_BendSettings);
                            EditorUtilities.GetBendSettingsFromVector(materialProp, out bendType, out bendID, out normalTransform);


                            if (shaderData[material.shader].supportedBendTypes.Contains(bendType) == false)
                            {
                                List<CurvedWorld.BEND_TYPE> newBendTypes = new List<CurvedWorld.BEND_TYPE>(shaderData[material.shader].supportedBendTypes);
                                newBendTypes.Add(bendType);

                                shaderData[material.shader].supportedBendTypes = newBendTypes.ToArray();
                            }

                            if (shaderData[material.shader].supportedBendIDs.Contains(bendID) == false)
                            {
                                List<int> newBendIDs = new List<int>(shaderData[material.shader].supportedBendIDs);
                                newBendIDs.Add(bendID);

                                shaderData[material.shader].supportedBendIDs = newBendIDs.ToArray();
                            }
                        }
                    }
                }

                

                //Adjust shaders
                foreach (KeyValuePair<Shader, EditorUtilities.ShaderCurvedWorldKeywordsInfo> entry in shaderData)
                {
                    for (int bType = 0; bType < entry.Value.supportedBendTypes.Length; bType++)
                    {
                        for (int bID = 0; bID < entry.Value.supportedBendIDs.Length; bID++)
                        {
                            string cgincFile = EditorUtilities.GetBendFileLocation(entry.Value.supportedBendTypes[bType], entry.Value.supportedBendIDs[bID], EditorUtilities.EXTENSTION.cginc);
                            if (File.Exists(cgincFile) == false)
                            {
                                EditorUtilities.CreateCGINCFile(entry.Value.supportedBendTypes[bType], entry.Value.supportedBendIDs[bID]);
                            }
                        }
                    }

                    EditorUtilities.SetShaderBendSettings(entry.Key, entry.Value.supportedBendTypes, entry.Value.supportedBendIDs, EditorUtilities.KEYWORDS_COMPILE.Default, false);
                }


                guids = AssetDatabase.FindAssets("t:Shader");
                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

                    if (shader != null && EditorUtilities.HasShaderCurvedWorldBendSettingsProperty(shader))
                    {
                        AssetDatabase.ImportAsset(assetPath);
                    }
                }



                UnityEditor.EditorUtility.ClearProgressBar();

                AssetDatabase.Refresh();
            }

            void CallbackRenderersOverviewAdjustCurvedWorld(object obj)
            {
                CurvedWorldBendSettingsEditorWindow.ShowWindow(GUIUtility.GUIToScreenPoint(mousePosition), CallbackRenderersOverviewAdjustCurvedWorld, obj);
            }

            void CallbackRenderersOverviewAdjustCurvedWorld(CurvedWorld.BEND_TYPE bendType, int bendID, int normalTransformState, object obj)
            {

                if (File.Exists(EditorUtilities.GetBendFileLocation(bendType, bendID, EditorUtilities.EXTENSTION.cginc)) == false)
                {
                    EditorUtilities.CreateCGINCFile(bendType, bendID);

                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                }

                List<Material> originalMaterials = (Material)obj == null ? GetShaderOverviewMaterials() : new List<Material>() { (Material)obj };


                Undo.RecordObjects(originalMaterials.ToArray(), "Change Bend Settings (" + originalMaterials.Count + " Elements)");


                for (int i = 0; i < originalMaterials.Count; i++)
                {
                    if (originalMaterials[i] != null && originalMaterials[i].shader != null && EditorUtilities.HasShaderCurvedWorldBendSettingsProperty(originalMaterials[i].shader))
                    {
                        //normalTransformState: 
                        //0 - default
                        //1 - enabled
                        //2 - disabled

                        bool normalTransformKeywordEnabled = EditorUtilities.HasShaderNormalTransform(originalMaterials[i].shader);
                        if (normalTransformKeywordEnabled)
                        {
                            switch (normalTransformState)
                            {
                                case 0: normalTransformKeywordEnabled = originalMaterials[i].IsKeywordEnabled(EditorUtilities.shaderKeywordName_BendTransformNormal); break;
                                case 1: normalTransformKeywordEnabled = true; break;
                                case 2: normalTransformKeywordEnabled = false; break;
                            }
                        }


                        EditorUtilities.SetMaterialBendSettings(originalMaterials[i], bendType, bendID, normalTransformKeywordEnabled);
                    }
                }

                AssetDatabase.Refresh();

                OnFocus();
                Repaint();
            }

            void CallbackCurvedWorldKeywordsUnckechAll()
            {
                gCurvedWorldKeywordsShaderInfo.selectedBendTypes = new bool[EditorUtilities.MAX_SUPPORTED_BEND_TYPES];
                gCurvedWorldKeywordsShaderInfo.selectedBendIDs = new bool[EditorUtilities.MAX_SUPPORTED_BEND_IDS];
            }

            void CallbackCurvedWorldKeywordsMultiCompile(object obj)
            {
                if (obj == null)
                    return;

                gCurvedWorldKeywordsShaderInfo.selecedMultiCompile = ((bool)obj) ? true : false;
            }

            void CallbackCurvedWorldKeywordsRewriteAllProjectShaders(object obj)
            {
                if (obj == null)
                    return;


                CurvedWorld.BEND_TYPE[] bendTypes;
                int[] bendIDs;
                bool hasNormalTransform;

                if (EditorUtilities.StringToBendSettings(obj.ToString(), out bendTypes, out bendIDs, out hasNormalTransform) == false)
                    return;


                string[] guids = AssetDatabase.FindAssets("t:Shader");


                int count = 0;

                for (int i = 0; i < guids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    Shader asset = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);

                    if (asset != null && EditorUtilities.HasShaderCurvedWorldBendSettingsProperty(asset) && EditorUtilities.IsShaderCurvedWorldTerrain(asset) == false)
                    {
                        EditorUtilities.SetShaderBendSettings(asset, bendTypes.ToArray(), bendIDs.ToArray(), EditorUtilities.KEYWORDS_COMPILE.Default, false);

                        count += 1;
                    }
                }

                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

                gCurvedWorldKeywordsShaderInfo = null;
                Repaint();
            }


            void LoadActivatorShaders()
            {
                gActivatorShaders = new List<Shader>();


                if (string.IsNullOrEmpty(gActivatorPath) == false && gActivatorPath.IndexOf("Assets") == 0 && Directory.Exists(gActivatorPath))
                {
                    string[] guids = AssetDatabase.FindAssets("t:Shader", new string[] { gActivatorPath });
                    for (int i = 0; i < guids.Length; i++)
                    {
                        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(AssetDatabase.GUIDToAssetPath(guids[i]));
                        if (shader != null)
                        {
                            gActivatorShaders.Add(shader);
                        }
                    }
                }
            }


            void CallbackUndo()
            {
                OnFocus();
                Repaint();
            }

            void PingObject(string assetPath)
            {
                // Load object
                UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));

                PingObject(obj);
            }

            void PingObject(UnityEngine.Object obj)
            {
                if (obj != null)
                {
                    // Select the object in the project folder
                    Selection.activeObject = obj;

                    // Also flash the folder yellow to highlight it
                    UnityEditor.EditorGUIUtility.PingObject(obj);
                }
            }



            static Rect[] CalcButtonRects(Rect position, GUIContent[] contents, int xCount)
            {
                GUIStyle style = GUI.skin.button;
                GUI.ToolbarButtonSize buttonSize = GUI.ToolbarButtonSize.Fixed;



                int length = contents.Length;
                int num1 = length / xCount;
                if ((uint)(length % xCount) > 0U)
                    ++num1;
                float num2 = (float)CalcTotalHorizSpacing(xCount, style, style, style, style);
                float num3 = (float)(Mathf.Max(style.margin.top, style.margin.bottom) * (num1 - 1));
                float elemWidth = (position.width - num2) / (float)xCount;
                float elemHeight = (position.height - num3) / (float)num1;
                if ((double)style.fixedWidth != 0.0)
                    elemWidth = style.fixedWidth;
                if ((double)style.fixedHeight != 0.0)
                    elemHeight = style.fixedHeight;


                int num = 0;
                float x = position.xMin;
                float yMin = position.yMin;
                GUIStyle guiStyle1 = style;
                Rect[] rectArray = new Rect[length];
                if (length > 1)
                    guiStyle1 = style;
                for (int index = 0; index < length; ++index)
                {
                    float width = 0.0f;
                    switch (buttonSize)
                    {
                        case GUI.ToolbarButtonSize.Fixed:
                            width = elemWidth;
                            break;
                        case GUI.ToolbarButtonSize.FitToContents:
                            width = guiStyle1.CalcSize(contents[index]).x;
                            break;
                    }
                    rectArray[index] = new Rect(x, yMin, width, elemHeight);
                    rectArray[index] = GUIUtility.AlignRectToDevice(rectArray[index]);
                    GUIStyle guiStyle2 = style;
                    if (index == length - 2 || index == xCount - 2)
                        guiStyle2 = style;
                    x = rectArray[index].xMax + (float)Mathf.Max(guiStyle1.margin.right, guiStyle2.margin.left);
                    ++num;
                    if (num >= xCount)
                    {
                        num = 0;
                        yMin += elemHeight + (float)Mathf.Max(style.margin.top, style.margin.bottom);
                        x = position.xMin;
                        guiStyle2 = style;
                    }
                    guiStyle1 = guiStyle2;
                }
                return rectArray;
            }
            static int CalcTotalHorizSpacing(int xCount, GUIStyle style, GUIStyle firstStyle, GUIStyle midStyle, GUIStyle lastStyle)
            {
                if (xCount < 2)
                    return 0;
                if (xCount == 2)
                    return Mathf.Max(firstStyle.margin.right, lastStyle.margin.left);

                int num = Mathf.Max(midStyle.margin.left, midStyle.margin.right);
                return Mathf.Max(firstStyle.margin.right, midStyle.margin.left) + Mathf.Max(midStyle.margin.right, lastStyle.margin.left) + num * (xCount - 3);
            }
        }
    }
}

