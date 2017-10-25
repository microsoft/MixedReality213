using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MRDL.ToolTips
{
    public class TipGroupManagerEditor<G,T> : Editor where G : TipGroup<T>, new() where T : TipSpawnSettings, new()
    {
        private T currentTemplate;
        
        protected Transform TestingTarget = null;

        public override void OnInspectorGUI()
        {
            TipGroupManager<G, T> manager = (TipGroupManager<G, T>)target;
            
            EditorGUILayout.LabelField("TOOLTIP GROUPS", EditorStyles.boldLabel);

            List<G> groupsToDelete = new List<G>();

            foreach (G g in manager.Groups)
            {
                bool delete = false;
                DrawGroupEditor(g, out delete);
                if (delete)
                {
                    groupsToDelete.Add(g);
                }
            }

            foreach (G groupToDelete in groupsToDelete)
            {
                manager.Groups.Remove(groupToDelete);
            }

            if (GUILayout.Button("+Add Group", EditorStyles.miniButton))
            {
                manager.Groups.Add(new G());
            }
        }

        protected void DrawGroupEditor (G group, out bool delete)
        {
            delete = false;
            GUI.color = (group.DisplayMode == TipDisplayModeEnum.On) ? Color.white : Color.gray;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // Draw the visible button
                EditorGUILayout.BeginHorizontal();

                    if (GUILayout.Button (group.DisplayMode.ToString(), GUILayout.MaxWidth (60), GUILayout.MaxHeight(60)))
                    {
                        if (group.DisplayMode == TipDisplayModeEnum.On)
                        {
                            group.DisplayMode = TipDisplayModeEnum.Off;
                        } else
                        {
                            group.DisplayMode = TipDisplayModeEnum.On;
                        }
                    }

                    // Draw the group and template editors
                    EditorGUILayout.BeginVertical();
                        DrawGroupEditorBase(group);
                        DrawGroupEditorExtensions(group);
                        if (group.DisplayMode == TipDisplayModeEnum.On)
                        {
                            EditorGUILayout.LabelField("TEMPLATES", EditorStyles.boldLabel);
                            DrawGroupEditorTemplates(group);
                        }
                        if (GUILayout.Button("+Add Template to " + group.Name, EditorStyles.miniButton))
                        {
                            group.Templates.Add(new T());
                        }
                    EditorGUILayout.EndVertical();

                    // Draw the delete button
                    EditorGUILayout.BeginVertical();                        
                        if (GUILayout.Button ("Delete", GUILayout.MaxWidth(60), GUILayout.MaxHeight(60)))
                        {
                            delete = true;
                        }
                    EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        protected void DrawTipTemplate (T template)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawTipTemplateBase(template);
            DrawTipTemplateExtensions(template);
            EditorGUILayout.EndVertical();
        }

        private void DrawGroupEditorBase(G group)
        {
            group.Name = EditorGUILayout.TextField("Name", group.Name);
            group.TipPrefab = (GameObject)EditorGUILayout.ObjectField("Tip Prefab", group.TipPrefab, typeof(GameObject), false);
            group.EditorColor = EditorGUILayout.ColorField("Editor Color", group.EditorColor);
        }

        protected virtual void DrawGroupEditorExtensions (G group)
        {
            // Nothing by default - draw extensions to the base TipGroup class here
        }

        private void DrawGroupEditorTemplates(G group)
        {
            EditorGUILayout.BeginVertical();
            if (group.Templates.Count == 0)
            {
                EditorGUILayout.LabelField("(None)");
            }
            else
            {
                foreach (T template in group.Templates)
                {
                    DrawTipTemplate(template);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTipTemplateBase (T template)
        {
            EditorInspector.Show(template);
        }

        protected virtual void DrawTipTemplateExtensions (T template)
        {
            // Nothing by default - draw extensions to the base TipSpawnSettings class here
        }

        public void OnSceneGUI()
        {
            TipGroupManager<G, T> manager = (TipGroupManager<G, T>)target;

            foreach (G group in manager.Groups)
            {
                if (group.DisplayMode == TipDisplayModeEnum.Off)
                    continue;

                foreach (T template in group.Templates)
                {
                    PrepareToDisplayTipHandle(template);

                    Vector3 toolTipPos = TestingTarget.transform.position;
                    Vector3 toolTipRot = TestingTarget.transform.eulerAngles;
                    Vector3 anchorPos = TestingTarget.transform.position;
                    Vector3 pivotPos = TestingTarget.transform.position;

                    Vector3 cubeSize = new Vector3(0.05f, 0.02f, 0.001f);

                    ToolTipConnector.GetTransformationsFromSettings(template, ref toolTipPos, ref toolTipRot, ref anchorPos, ref pivotPos, Camera.current, TestingTarget);
                    Handles.color = group.EditorColor;
                    Handles.DrawLine(anchorPos, pivotPos);
                    Handles.Label(pivotPos + (Vector3.up * 0.01f), template.Text);

                    Handles.matrix = Matrix4x4.TRS(pivotPos, Quaternion.Euler(toolTipRot), Vector3.one);
                    Handles.DrawWireCube((Vector3.up * 0.01f), cubeSize);
                    Handles.matrix = Matrix4x4.identity;
                }
            }
        }

        /// <summary>
        /// Override this function if you want handles from OnSceneGUI to be displayed using a different testing matrix
        /// Or if you want to specify the position of the testing transform for templates that don't have a 'Target' set
        /// </summary>
        protected virtual void PrepareToDisplayTipHandle(T template)
        {
            ControllerTips manager = (ControllerTips)target;

            if (TestingTarget == null)
            {
                TestingTarget = manager.transform.Find("Testing Transform");
                if (TestingTarget == null)
                    TestingTarget = new GameObject("Testing Transform").transform;

                TestingTarget.parent = manager.transform;
            }
        }
    }
}