// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.Controllers
{
    /// <summary>
    /// Routes controller input to a physics pointer
    /// </summary>
    [RequireComponent(typeof(AttachToController))]
    public class PointerInput : MonoBehaviour
    {
        [SerializeField]
        private PhysicsPointer pointer = null;
        [SerializeField]
        private InteractionSourcePressType activePressType = InteractionSourcePressType.Select;
        [SerializeField]
        private InteractionSourceHandedness handedness = InteractionSourceHandedness.Left;

        private AttachToController attachToController;

        private void Awake()
        {
            attachToController = GetComponent<AttachToController>();
            attachToController.Handedness = handedness;
            attachToController.OnAttach += OnAttach;

            if (pointer == null)
            {
                pointer = GetComponent<PhysicsPointer>();
            }

            pointer.Active = false;
        }

        private void OnAttach()
        {
            // Subscribe to interaction events
            InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionSourceReleased;
        }

        /// <summary>
        /// Presses active
        /// </summary>
        /// <param name="obj"></param>
        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness && obj.pressType == activePressType)
            {
                pointer.Active = true;
            }
        }

        /// <summary>
        /// Updates target point orientation via thumbstick
        /// </summary>
        /// <param name="obj"></param>
        private void InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness && obj.state.thumbstickPressed)
            {
                float angle = 0f;
                Vector2 thumbstickPosition = obj.state.thumbstickPosition;
                if (thumbstickPosition.y != 0 && thumbstickPosition.x != 0)
                {
                    angle = Mathf.Atan2(thumbstickPosition.y, thumbstickPosition.x) * Mathf.Rad2Deg;
                }
                pointer.TargetPointOrientation = angle;
            }
        }

        /// <summary>
        /// Releases active
        /// </summary>
        /// <param name="obj"></param>
        private void InteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness && obj.pressType == activePressType)
            {
                pointer.Active = false;
            }
        }
    }
}