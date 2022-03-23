using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AmazingAssets
{
    namespace CurvedWorldEditor
    {
        class CurvedWorldMaterialDuplicateEditorWindow : EditorWindow
        {            
            static public CurvedWorldMaterialDuplicateEditorWindow window;

            public delegate void DataChanged(string subFolderName, string prefix, string suffix, object obj);
            static DataChanged callback;

            static char[] unsupported = new char[] { '\\', '|', '/', ':', '*', '?', '\'', '<', '>'  };
            
            string subFolderName;
            string prefix;
            string suffix;

            static object objMaterial;

            static Vector2 windowResolution = new Vector2(340, 158);


            static public void ShowWindow(Vector2 position, DataChanged method, object obj)
            {
                if (window != null)
                {
                    window.Close();
                    window = null;
                }
                

                window = (CurvedWorldMaterialDuplicateEditorWindow)CurvedWorldMaterialDuplicateEditorWindow.CreateInstance(typeof(CurvedWorldMaterialDuplicateEditorWindow));
                window.titleContent = new GUIContent("Duplicate Material");
                
                callback = method;
                objMaterial = obj;


                window.minSize = windowResolution;
                window.maxSize = windowResolution;

                window.ShowUtility();
                window.position = new Rect(position.x, position.y, windowResolution.x, windowResolution.y);
            }


            void OnLostFocus()
            {
                if (window != null)
                {
                    window.Close();
                    window = null;
                }
            }

            void OnGUI()
            {
                if (callback == null ||
                   (window != null && this != window))
                    this.Close();


                subFolderName = subFolderName == null ? string.Empty : subFolderName;
                prefix = prefix == null ? string.Empty : prefix;
                suffix = suffix == null ? string.Empty : suffix;


                using (new AmazingAssets.EditorGUIUtility.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                {
                    using (new AmazingAssets.EditorGUIUtility.EditorGUIUtilityLabelWidth(110))
                    {
                        using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(subFolderName.Trim().IndexOfAny(unsupported) == -1 ? Color.white : Color.red))
                        {
                            subFolderName = EditorGUILayout.TextField("Subfolder Name", subFolderName);
                        }

                        GUILayout.Space(5);
                        EditorGUILayout.HelpBox("Leave 'Subfolder Name' field empty to create material duplicate in the same folder. In this case file prefix/suffix are required.", MessageType.Info);
                        GUILayout.Space(5);

                        if (prefix == null) prefix = string.Empty;
                        if (suffix == null) suffix = string.Empty;

                        Color backGroundColor = Color.white;
                        if (string.IsNullOrEmpty(subFolderName.Trim()))
                        {
                            if (string.IsNullOrEmpty((prefix + suffix).Trim()) || prefix.IndexOfAny(unsupported) != -1)
                                backGroundColor = Color.red;
                        }
                        else
                        {
                            if (prefix.IndexOfAny(unsupported) != -1)
                                backGroundColor = Color.red;
                        }
                        using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(backGroundColor))
                        {
                            prefix = EditorGUILayout.TextField("File Prefix", prefix);
                        }


                        backGroundColor = Color.white;
                        if (string.IsNullOrEmpty(subFolderName.Trim()))
                        {
                            if (string.IsNullOrEmpty((prefix + suffix).Trim()) || suffix.IndexOfAny(unsupported) != -1)
                                backGroundColor = Color.red;
                        }
                        else 
                        {
                            if (suffix.IndexOfAny(unsupported) != -1)
                                backGroundColor = Color.red;
                        }
                        using (new AmazingAssets.EditorGUIUtility.GUIBackgroundColor(backGroundColor))
                        {
                            suffix = EditorGUILayout.TextField("File Suffix", suffix);
                        }
                    }

                    GUILayout.Space(15);
                    bool saveAvailable = true;
                    string checkString = (prefix + suffix).Trim();
                    if (string.IsNullOrEmpty(subFolderName.Trim()))
                    {
                        if (string.IsNullOrEmpty(checkString) || checkString.IndexOfAny(unsupported) != -1)
                            saveAvailable = false;
                    }
                    else 
                    {
                        if(subFolderName.Trim().IndexOfAny(unsupported) != -1)
                            saveAvailable = false;
                        else if (checkString.IndexOfAny(unsupported) != -1)
                            saveAvailable = false;
                    }


                    using (new AmazingAssets.EditorGUIUtility.GUIEnabled(saveAvailable))
                    {
                        if (GUILayout.Button("Create Duplicate"))
                        {
                            this.Close();

                            callback(subFolderName.Trim(), prefix, suffix, objMaterial);                            
                        }
                    }
                }
            }
        }
    }
}