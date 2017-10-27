using UnityEditor;
using UnityEngine;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(MixedRealityControllerTips))]
    public class MixedRealityControllerTipsEditor : TipGroupManagerEditor<ControllerTipGroup, ControllerTipTemplate>
    {
        protected override void PrepareToDisplayTipHandle(ControllerTipTemplate template)
        {
            base.PrepareToDisplayTipHandle(template);

            MixedRealityControllerTips manager = (MixedRealityControllerTips)target;
            if (manager.TestingPrefab != null)
            {
                // Set the testing matrix - this will ensure that our preview 
                //GroupMatrix = manager.transform.localToWorldMatrix;
                // Set our testing target to the position of the element we want to test
                Transform elementTransformFromPrefab = null;
                if (FindTransform(manager.TestingPrefab.transform, template.ControllerElement.ToString().ToLower(), ref elementTransformFromPrefab))
                {
                    TestingTarget.position = manager.transform.TransformPoint(elementTransformFromPrefab.position);
                    TestingTarget.localRotation = elementTransformFromPrefab.rotation;
                } else
                {
                    Debug.Log("Couldn't find " + template.ControllerElement.ToString());
                }
            }
        }

        protected override void DrawTipTemplateExtensions(ControllerTipTemplate template)
        {
            template.ControllerElement = (HoloToolkit.Unity.InputModule.MotionControllerInfo.ControllerElementEnum)EditorGUILayout.EnumPopup("Controller Element", template.ControllerElement);
        }

        private bool FindTransform (Transform start, string name, ref Transform result)
        {
            if (start.name.ToLower() == name)
            {
                result = start;
                return true;
            }
            else
            {
                foreach (Transform child in start)
                {
                    if (child.name.ToLower() == name) {
                        result = child;
                        return true;
                    }
                    else if (FindTransform(child, name, ref result)) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}