// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HoloToolkit.Unity.Controllers
{
    /// <summary>
    /// Waits for a controller to be instantiated, then attaches itself to a specified element
    /// </summary>
    public class AttachToController : MonoBehaviour
    {
        [Header("AttachToController Elements")]
        protected InteractionSourceHandedness handedness = InteractionSourceHandedness.Left;

        [SerializeField]
        protected MotionControllerInfo.ControllerElementEnum element = MotionControllerInfo.ControllerElementEnum.PointingPose;

        [SerializeField]
        protected Vector3 positionOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 rotationOffset = Vector3.zero;

        [SerializeField]
        protected Vector3 scale = Vector3.one;

        [SerializeField]
        protected bool setScaleOnAttach = false;

        public bool IsAttached { get; private set; }

        public Transform Element { get { return elementTransform; } }

        private MotionControllerInfo controller;
        public MotionControllerInfo Controller { get { return controller; } }

        public event Action<MotionControllerInfo> OnAttachToController;
        public event Action<MotionControllerInfo> OnDetachFromController;

        private Transform elementTransform;

        private void OnEnable()
        {
            // Look if the controller has loaded.
            if (MotionControllerVisualizer.Instance.TryGetControllerModel(handedness, out controller))
            {
                AttachElementToController(controller);
            }

            MotionControllerVisualizer.Instance.OnControllerModelLoaded += AttachElementToController;
            MotionControllerVisualizer.Instance.OnControllerModelUnloaded += DetachElementFromController;
        }

        private void OnDisable()
        {
            if (MotionControllerVisualizer.IsInitialized)
            {
                MotionControllerVisualizer.Instance.OnControllerModelLoaded -= AttachElementToController;
                MotionControllerVisualizer.Instance.OnControllerModelUnloaded -= DetachElementFromController;
            }
        }

        private void OnDestroy()
        {
            if (MotionControllerVisualizer.IsInitialized)
            {
                MotionControllerVisualizer.Instance.OnControllerModelLoaded -= AttachElementToController;
                MotionControllerVisualizer.Instance.OnControllerModelUnloaded -= DetachElementFromController;
            }
        }

        private void AttachElementToController(MotionControllerInfo controller)
        {
            if (!IsAttached)
            {
                if (!controller.TryGetElement(element, out elementTransform))
                {
                    Debug.LogError("Unable to find element of type " + element + " under controller " + controller.ControllerParent.name + "; not attaching.");
                    return;
                }

                // Parent ourselves under the element and set our offsets
                transform.parent = elementTransform;
                transform.localPosition = positionOffset;
                transform.localEulerAngles = rotationOffset;
                if (setScaleOnAttach)
                {
                    transform.localScale = scale;
                }

                // Announce that we're attached
                if (OnAttachToController != null)
                {
                    OnAttachToController(controller);
                }

                IsAttached = true;
            }
            else
            {
                Debug.Log(name + " is already attached.");
            }
        }

        private void DetachElementFromController(MotionControllerInfo e)
        {
            if (OnDetachFromController != null)
            {
                OnDetachFromController(e);
            }

            IsAttached = false;
        }
    }
}