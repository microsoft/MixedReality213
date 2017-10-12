//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System;
using System.Collections;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.Controllers
{
    /// <summary>
    /// Waits for a controller to be instantiated, then attaches itself to a specified element
    /// </summary>
    public class AttachToController : MonoBehaviour
    {
        public Action OnAttach;

        public Transform Element { get { return elementTransform; } }

        public MotionControllerInfo Controller { get { return controller; } }

        public InteractionSourceHandedness Handedness { set { handedness = value; } }

        private IEnumerator Start()
        {
            // Wait for our controller to appear
            while (!MotionControllerVisualizer.Instance.TryGetController (handedness, out controller))
            {
                yield return null;
            }

            elementTransform = controller.GetElement(element);

            if (elementTransform == null)
            {
                Debug.LogError("Unable to find element of type " + element + " under controller " + controller.ControllerParent.name + ", not attaching.");
                yield break;
            }

            // Parent ourselves under the element and set our offsets
            transform.parent = elementTransform;
            transform.localPosition = positionOffset;
            transform.localEulerAngles = rotationOffset;
            if (setScaleOnAttach)
                transform.localScale = scale;

            // Announce that we're attached
            if (OnAttach != null)
                OnAttach();

            yield break;
        }
        
        [SerializeField]
        private Vector3 positionOffset = Vector3.zero;
        [SerializeField]
        private Vector3 rotationOffset = Vector3.zero;
        [SerializeField]
        private bool setScaleOnAttach = false;
        [SerializeField]
        private Vector3 scale = Vector3.one;
        [SerializeField]
        private InteractionSourceHandedness handedness = InteractionSourceHandedness.Left;
        [SerializeField]
        private MotionControllerInfo.ControllerElementEnum element = MotionControllerInfo.ControllerElementEnum.PointingPose;

        private MotionControllerInfo controller;
        private Transform elementTransform;
    }
}