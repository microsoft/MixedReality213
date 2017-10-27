using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(TipGroupController))]
    public class TipGroupControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TipGroupController controller = (TipGroupController)target;

            base.DrawDefaultInspector();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Prev Group"))
            {
                controller.PrevGroup();
            }
            
            if (GUILayout.Button("Next Group"))
            {
                controller.NextGroup();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}