using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MRDL.ToolTips;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ToolTips
{
    [Serializable]
    public class ControllerTipTemplate : TipTemplate
    {
        [Header("Controller Settings")]
        public MotionControllerInfo.ControllerElementEnum ControllerElement;
    }

    [Serializable]
    public class ControllerTipGroup : TipGroup<ControllerTipTemplate> { }

    public class ControllerTips : TipGroupManager<ControllerTipGroup, ControllerTipTemplate>
    {
        public GameObject TestingPrefab;

        // Don't instantiate tips on awake
        protected override void Awake()
        {
            // Do nothing
        }

        // Instantiate tips here instead
        private IEnumerator Start()
        {
            // Wait for our motion controller to appear
            while (!MotionControllerVisualizer.Instance.TryGetController(handedness, out controller))
            {
                yield return null;
            }

            // Subscribe to input now that we have the controller
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            // Create our tool tips!
            CreateToolTips();
        }

        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.pressType == toggleTipsPress && obj.state.source.handedness == handedness)
            {

            }
        }

        protected override ToolTip CreateToolTip(ControllerTipTemplate template, ControllerTipGroup group)
        {
            // Get the target element from the template, set it before we create the tool tip
            Transform element = controller.GetElement(template.ControllerElement);
            template.Target = element.gameObject;

            // Now create the base tool tip - this will have pivot, text, etc set up
            ToolTip toolTip = base.CreateToolTip(template, group);

            return toolTip;
        }

        private void OnDrawGizmos()
        {
            if (TestingPrefab != null)
            {
                Gizmos.color = Color.Lerp(Color.blue, Color.clear, 0.5f);
                MeshFilter[] meshes = TestingPrefab.GetComponentsInChildren<MeshFilter>();
                foreach (MeshFilter mf in meshes)
                {
                    Gizmos.matrix = transform.localToWorldMatrix * mf.transform.localToWorldMatrix;
                    Gizmos.DrawMesh(mf.sharedMesh);
                }
                Gizmos.matrix = Matrix4x4.identity;
            }
        }

        [SerializeField]
        private InteractionSourceHandedness handedness;
        [SerializeField]
        private InteractionSourcePressType toggleTipsPress = InteractionSourcePressType.Menu;

        [NonSerialized]
        private MotionControllerInfo controller;
    }
}