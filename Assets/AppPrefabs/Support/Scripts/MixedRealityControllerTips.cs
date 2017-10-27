using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MRDL.ToolTips;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ToolTips
{
    /// <summary>
    /// Tip template class with an extra field for controller element
    /// </summary>
    [Serializable]
    public class ControllerTipTemplate : TipTemplate
    {
        [Header("Controller Settings")]
        public MotionControllerInfo.ControllerElementEnum ControllerElement;
    }

    /// <summary>
    /// Tip group class with a list of ControllerTipTemplates
    /// </summary>
    [Serializable]
    public class ControllerTipGroup : TipGroupBase<ControllerTipTemplate> { }

    /// <summary>
    /// Tip group manager that uses ControllerTipGroup
    /// </summary>
    public class MixedRealityControllerTips : TipGroupManagerBase<ControllerTipGroup, ControllerTipTemplate>
    {
        /// <summary>
        /// A prefab of the controller for testing out the positions of tooltips
        /// This prefab should be based on the GLTF models provided by Windows Mixed Reality
        /// </summary>
        public GameObject TestingPrefab;
        
        private IEnumerator Start()
        {
            // Wait for our motion controller to appear
            while (!MotionControllerVisualizer.Instance.TryGetController(handedness, out controller))
            {
                yield return null;
            }

            // Create our tool tips!
            CreateToolTips();
        }

        protected override ToolTip CreateToolTip(ControllerTipTemplate template, ControllerTipGroup group)
        {
            // Get the target element from the template, set it before we create the tool tip
            Transform element = controller.GetElement(template.ControllerElement);
            if (element == null)
            {
                Debug.LogError("Couldn't find controller element " + template.ControllerElement + " in runtime model");
            }
            else
            {
                template.Target = element.gameObject;
            }

            // Now create the base tool tip - this will have pivot, text, etc set up
            ToolTip toolTip = base.CreateToolTip(template, group);

            return toolTip;
        }

        /// <summary>
        /// Draw the test prefab
        /// </summary>
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

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
        [NonSerialized]
        private MotionControllerInfo controller;
    }
}