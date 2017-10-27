using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MRDL.ToolTips
{
    public class TipGroupManagerEditor<G,T> : Editor where G : TipGroupBase<T>, new() where T : TipTemplate, new()
    {
        private T currentTemplate;
        
        protected Transform TestingTarget = null;

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();

            TipGroupManagerBase<G, T> manager = (TipGroupManagerBase<G, T>)target;
            
            Undo.RegisterCompleteObjectUndo(target, "TipGroupManagerEditor");

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GROUP EDITOR", EditorStyles.boldLabel);

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

            EditorGUILayout.Space();

            if (GUILayout.Button("+Add Group", EditorStyles.miniButton))
            {
                G newGroup = new G();
                newGroup.Name = "Group " + (manager.Groups.Count + 1);
                manager.Groups.Add(newGroup);
            }
            
            EditorUtility.SetDirty(target);

            if (Application.isPlaying)
                return;

            SceneView.RepaintAll();
        }

        protected void DrawGroupEditor (G group, out bool delete)
        {
            EditorGUILayout.Space();
            GUIStyle toolbarOn = new GUIStyle(EditorStyles.miniButton);
            toolbarOn.normal.background = toolbarOn.active.background;

            delete = false;
            GUI.color = Color.Lerp(group.EditorColor, Color.gray, 0.75f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            GUI.color = (group.DisplayMode == TipDisplayModeEnum.On) ? Color.white : Color.Lerp(Color.white, Color.gray, 0.25f);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(group.Name, EditorStyles.boldLabel, GUILayout.MaxWidth (150));
            bool visible = group.DisplayMode == TipDisplayModeEnum.On;
            if (GUILayout.Button("Visible", visible ? toolbarOn : EditorStyles.miniButton))
            {
                visible = !visible;
            }
            group.DisplayMode = visible ? TipDisplayModeEnum.On : TipDisplayModeEnum.Off;
                        
            if (GUILayout.Button("Delete", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("DELETE GROUP", "Delete Group '" + group.Name + "?'", "OK", "CANCEL"))
                {
                    delete = true;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Draw the group and template editors
            EditorGUILayout.BeginVertical();

            if (visible)
            {
                DrawGroupEditorBase(group);
                DrawGroupEditorExtensions(group);

                EditorGUILayout.LabelField("TEMPLATES", EditorStyles.boldLabel);
                DrawGroupEditorTemplates(group);

                if (GUILayout.Button("+Add Template to " + group.Name, EditorStyles.miniButton))
                {
                    group.Templates.Add(new T());
                }

            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        protected void DrawTipTemplate (T template, out bool delete)
        {
            delete = false;

            GUI.color = template.IsEmpty ? Color.Lerp(Color.red, Color.white, 0.5f) : Color.white;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            DrawTipTemplateBase(template);
            DrawTipTemplateExtensions(template);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Delete", EditorStyles.miniButton, GUILayout.MaxWidth(150)))
            {
                delete = true;
                return;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            GUI.color = Color.white;
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
                List<T> templatesToDelete = new List<T>();
                bool delete = false;

                foreach (T template in group.Templates)
                {
                    DrawTipTemplate(template, out delete);
                    if (delete)
                        templatesToDelete.Add(template);
                }

                foreach (T templateToDelete in templatesToDelete)
                {
                    group.Templates.Remove(templateToDelete);
                }
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawTipTemplateBase (T template)
        {
            switch (template.Preset)
            {
                case TipPresetEnum.Advanced:
                    EditorInspector.Show(template);
                    break;

                case TipPresetEnum.Default:
                default:
                    template.Preset = (TipPresetEnum)EditorGUILayout.EnumPopup("Preset", template.Preset);
                    template.Text = EditorGUILayout.TextField("Text", template.Text);
                    template.PivotDistance = EditorGUILayout.Slider("Distance", template.PivotDistance, 0.01f, 2f);
                    template.PivotDirection = (TipPivotDirectionEnum)EditorGUILayout.EnumPopup("Direction", template.PivotDirection);
                    break;

                case TipPresetEnum.ManualDirection:
                    template.Preset = (TipPresetEnum)EditorGUILayout.EnumPopup("Preset", template.Preset);
                    template.Text = EditorGUILayout.TextField("Text", template.Text);
                    template.PivotDistance = EditorGUILayout.Slider("Distance", template.PivotDistance, 0.01f, 2f);
                    template.ManualPivotDirection = EditorGUILayout.Vector3Field("Manual Pivot Direction", template.ManualPivotDirection).normalized;
                    break;

                case TipPresetEnum.ManualPosition:
                    template.Preset = (TipPresetEnum)EditorGUILayout.EnumPopup("Preset", template.Preset);
                    template.Text = EditorGUILayout.TextField("Text", template.Text);
                    template.ManualPivotLocalPosition = EditorGUILayout.Vector3Field("Manual Pivot Position", template.ManualPivotLocalPosition);
                    break;
            }

            template.ApplyPreset();

        }

        protected virtual void DrawTipTemplateExtensions (T template)
        {
            // Nothing by default - draw extensions to the base TipSpawnSettings class here
        }

        public void OnSceneGUI()
        {
            if (Application.isPlaying)
                return;

            TipGroupManagerBase<G, T> manager = (TipGroupManagerBase<G, T>)target;

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
                    Vector3 contentRot = toolTipRot;

                    Vector3 cubeSize = new Vector3(0.05f, 0.02f, 0.001f);

                    ToolTipConnector.GetTransformationsFromSettings(template, ref toolTipPos, ref toolTipRot, ref anchorPos, ref pivotPos, ref contentRot, Camera.current, TestingTarget);
                    Handles.color = group.EditorColor;
                    Handles.DrawLine(anchorPos, pivotPos);
                    Handles.Label(pivotPos + (Vector3.up * cubeSize.y / 2), template.Text, EditorStyles.centeredGreyMiniLabel);

                    Handles.matrix = Matrix4x4.TRS(pivotPos, Quaternion.Euler(contentRot), Vector3.one);
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
            MixedRealityControllerTips manager = (MixedRealityControllerTips)target;

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