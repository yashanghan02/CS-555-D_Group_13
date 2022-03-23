using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace AmazingAssets
{
    namespace CurvedWorldEditor
    {
        [CanEditMultipleObjects]
        public class CurvedWorldSeparatorDrawer : MaterialPropertyDrawer
        {
            public override void OnGUI(Rect position, MaterialProperty prop, String label, MaterialEditor editor)
            {

            }

            public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
            {
                float fValue;
                if (float.TryParse(label, out fValue))
                    return fValue;
                else
                    return 0;
                
            }
        }
    }
}