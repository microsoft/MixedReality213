using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(ControllerToolTipGroupManager))]
    public class ControllerToolTipGroupManagerInspector : Editor {

        public override void OnInspectorGUI()
        {
            ControllerToolTipGroupManager ttgm = (ControllerToolTipGroupManager)target;

            if (ttgm.NumGroups == 0)
                ttgm.AddGroup("New Group");


            GUI.color = Color.white;
            EditorGUILayout.LabelField("Groups", EditorStyles.whiteLargeLabel);


            // [<-] [ + Group ] [->]
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Group"))
            {
                ttgm.AddGroup("New Group");
            }
            EditorGUILayout.EndHorizontal();

            // [ little group editor ]
            // [ little group editor ]
            // [ little group editor ]
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < ttgm.NumGroups; i++)
            {
                bool delete = false;
                if (DrawLittleGroupEditor(ttgm.GetGroup(i).GroupName, i == ttgm.CurrentGroupIndex, ref delete, ttgm.GetGroup(i)))
                {
                    ttgm.GoToGroup(i);
                }
                if (delete)
                    ttgm.RemoveGroup(i);

            }
            EditorGUILayout.EndVertical();


            DrawBigGroupEditor(ttgm.CurrentGroup, ttgm);

            GUI.color = Color.white;
            if (GUILayout.Button("Add Tool Tip"))
            {
                int index = ttgm.CurrentGroup.visibleToolTipsCount + 1;
                ttgm.AddToolTip(index);
            }

        }

        private bool DrawLittleGroupEditor(string GroupName, bool isCurrentGroup, ref bool delete, ControllerToolTipGroupManager.ToolTipGroup group)
        {
            bool clicked = false;


            // [ Group name ] [X]
            EditorGUILayout.BeginHorizontal();
            GUI.color = isCurrentGroup ? Color.yellow : Color.white;
            EditorGUILayout.LabelField(group.GroupDisplayMode.ToString(), GUILayout.MaxWidth(150));
            if (GUILayout.Button(GroupName))
            {
                clicked = true;
            }
            GUI.color = Color.red;
            if (GUILayout.Button("X", GUILayout.MaxWidth(25)))
            {
                delete = true;
            }
            EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
            return clicked;
        }

        private void DrawBigGroupEditor(ControllerToolTipGroupManager.ToolTipGroup group, ControllerToolTipGroupManager ttgm)
        {
            GUI.color = Color.white;
            EditorGUILayout.LabelField("Current Group: " + group.GroupName, EditorStyles.whiteLargeLabel);
            EditorGUILayout.BeginHorizontal();
            group.GroupName = EditorGUILayout.TextField(group.GroupName);
            EditorGUILayout.EndHorizontal();
            group.groupPrefab = (GameObject)EditorGUILayout.ObjectField("Tool Tip prefab", group.groupPrefab, typeof(GameObject), false);
            if (group.groupPrefab == null)
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Warning: You need to set a prefab");

            }
            //ttgm.ToolTipPrefab = (GameObject)EditorGUILayout.ObjectField("Tool Tip prefab", ttgm.ToolTipPrefab, typeof(GameObject), false);

            //if (ttgm.ToolTipPrefab == null)
            //{
            //    GUI.color = Color.red;
            //    EditorGUILayout.LabelField("Warning: You need to set a prefab");

            //}
            // [ tooltip template ]
            // [ tooltip template ]
            // [ tooltip template ]
            EditorGUILayout.BeginVertical();
            ControllerToolTipGroupManager.DisplayModeEnum X = group.GroupDisplayMode;
            group.GroupDisplayMode = (ControllerToolTipGroupManager.DisplayModeEnum)EditorGUILayout.EnumPopup("Display Mode", group.GroupDisplayMode);
            if (group.GroupDisplayMode != X)
            {
                ttgm.GroupDisplayModeChanged(group, group.GroupDisplayMode, ttgm.CurrentGroupIndex);
            }

            EditorGUILayout.LabelField("Tool tips", EditorStyles.whiteLargeLabel);
            if (group.visibleToolTipsCount < group.ToolTips.Length)
            {
                for (int i = 0; i < group.visibleToolTipsCount; i++)
                {
                    group.ToolTips[i] = DrawTemplateEditor(group.ToolTips[i]);
                    EditorGUILayout.Space();
                }

            }
            else
            {
                // create new group or longer list if longer
            }

            EditorGUILayout.EndVertical();

        }


        private ControllerToolTipGroupManager.ToolTipTemplate DrawTemplateEditor(ControllerToolTipGroupManager.ToolTipTemplate template)
        {
            GUI.color = Color.white;
            /*if (template.IsEmpty)
            {
                GUI.color = Color.gray;
            }*/
            /*else if (!template.IsSetUp)
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField("Warning: There are fields missing");
            }*/

            // [ "ToolTip Text:" ] [ (text input) ] [ (clear button) ]
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ToolTip Text:", GUILayout.MaxWidth(75));
            template.Text = EditorGUILayout.TextField(template.Text);
            if (GUILayout.Button("Clear"))
            {
                template.Text = string.Empty;
            }
            EditorGUILayout.EndHorizontal();

            // if it's empty just show the name, nothing else
            if (!template.IsEmpty)
            {
                template.PositionMode = (ControllerToolTipGroupManager.PositionModeEnum)EditorGUILayout.EnumPopup("Position mode", template.PositionMode);
                switch (template.PositionMode)
                {
                    case ControllerToolTipGroupManager.PositionModeEnum.Automatic:
                    default:
                        template.LineLength = EditorGUILayout.Slider("Line length", template.LineLength, .01f, 10f);
                        break;

                    case ControllerToolTipGroupManager.PositionModeEnum.Manual:
                        template.LocalPosition = EditorGUILayout.Vector3Field("Local position (manual)", template.LocalPosition);
                        break;
                }
                //bool 
                //if(EditorGUILayout.Toggle)
                template.Handedness = (InteractionSourceHandedness)EditorGUILayout.EnumPopup("Handedness",template.Handedness);
                template.ControllerElement = (MotionControllerInfo.ControllerElementEnum)EditorGUILayout.EnumPopup("Element", template.ControllerElement);
                //template.Anchor = (Transform)EditorGUILayout.ObjectField("Anchor object", template.Anchor, typeof(Transform), true);
            }
            return template;

        }

        #region SceneEditorTools
        public enum ObjectModeEnum
        {
            Object,
            Vector
        }

        static ObjectModeEnum ObjectMode = ObjectModeEnum.Object;
        static int selectedVectorIndex = 0;
        static int selectedObjectIndex = 0;

        Color unselectedColor = Color.Lerp(Color.cyan, Color.clear, 0.95f);
        Color selectedColor = Color.Lerp(Color.yellow, Color.clear, 0.95f);
        Color lineColorObject = Color.white;
        Color lineColorVector = Color.red;
        Color inactiveColor = Color.Lerp(Color.gray, Color.clear, 0.99f);

        protected virtual void OnSceneGUI()
        {
            ControllerToolTipGroupManager example = (ControllerToolTipGroupManager)target;

            EditorGUI.BeginChangeCheck();
            // draw handles on objects
            Debug.Log("Length of List " + example.CurrentGroup.ToolTips.Length);
            Debug.Log("Length of Visible " + example.CurrentGroup.visibleToolTipsCount);
            for (int i = 0; i < example.CurrentGroup.visibleToolTipsCount; i++)
            {
                ControllerToolTipGroupManager.ToolTipTemplate connectedObject = example.CurrentGroup.ToolTips[i];

                if (Handles.Button(connectedObject.PivotPoint, Quaternion.identity, 0.05f, 0.05f, Handles.CubeHandleCap))
                {
                    selectedObjectIndex = i;
                }

                if (i == selectedObjectIndex)
                {
                    Handles.color = selectedColor;
                    connectedObject.PivotPoint = Handles.PositionHandle(connectedObject.PivotPoint, Quaternion.identity);
                }
                else
                {
                    Handles.color = unselectedColor;
                }

                Handles.color = Color.Lerp(Handles.color, lineColorVector, 0.5f);
                if (connectedObject.Anchor != null && connectedObject.PivotPoint != null)
                {
                    Handles.DrawLine(connectedObject.Anchor.transform.position, connectedObject.PivotPoint);
                }
                Handles.Label(connectedObject.PivotPoint + Vector3.up * 0.25f, "Object " + i.ToString());

                example.CurrentGroup.ToolTips[i] = connectedObject;
            }
            EditorGUI.EndChangeCheck();
        }
        #endregion
    }
}