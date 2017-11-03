// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using MRDL.Design;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ControllerExamples
{
    public class BrushSelector : MonoBehaviour
    {
        public enum SwipeEnum
        {
            None,
            Left,
            Right,
        }

        [SerializeField]
        private LineObjectCollection brushCollection;
        [SerializeField]
        private SwipeEnum currentAction;
        [SerializeField]
        private AnimationCurve swipeCurve;
        [SerializeField]
        private float swipeDuration = 0.5f;
        [SerializeField]
        private int displayBrushindex = 0;
        [SerializeField]
        private int activeBrushindex = 3;
        [SerializeField]
        private ColorPickerWheel colorPicker;
        [SerializeField]
        private float menuTimeout = 2f;

        [SerializeField]
        private InteractionSourceHandedness handedness = InteractionSourceHandedness.Right;
        [SerializeField]
        private MotionControllerInfo.ControllerElementEnum element = MotionControllerInfo.ControllerElementEnum.PointingPose;
        private MotionControllerInfo controller;

        [SerializeField]
        private Material touchpadMaterial;
        [SerializeField]
        private Gradient touchpadColor;
        [SerializeField]
        private float touchpadGlowLossTime = 0.5f;
        private float touchpadTouchTime;
        private MeshRenderer touchpadRenderer;

        private float menuOpenTime = 0f;
        private bool menuOpen = false;

        private float startOffset;
        private float targetOffset;

        private Brush activeBrush;

        private void Update()
        {
            if (menuOpen)
            {
                if (Time.unscaledTime - menuOpenTime > menuTimeout)
                {
                    menuOpen = false;
                }
            }

            if (controller != null)
            {
                for (int i = 0; i < brushCollection.Objects.Count; i++)
                {
                    Brush brush = brushCollection.Objects[i].GetComponent<Brush>();
                    if (brush == activeBrush)
                        brush.DisplayMode = Brush.DisplayModeEnum.InHand;
                    else
                        brush.DisplayMode = menuOpen ? Brush.DisplayModeEnum.InMenu : Brush.DisplayModeEnum.Hidden;
                }

                // Update our touchpad material
                Color glowColor = touchpadColor.Evaluate((Time.unscaledTime - touchpadTouchTime) / touchpadGlowLossTime);
                touchpadMaterial.SetColor("_EmissionColor", glowColor);
                touchpadMaterial.SetColor("_Color", glowColor);
            }
        }

        private IEnumerator Start()
        {
            while (!MotionControllerVisualizer.Instance.TryGetControllerModel(handedness, out controller))
            {
                menuOpen = false;
                yield return null;
            }

            // Parent the brush tools under the element of choice
            Transform elementTransform = controller.GetElement(element);
            if (elementTransform == null)
            {
                Debug.LogError("Element " + element.ToString() + " not found in controller, can't proceed.");
                gameObject.SetActive(false);
                yield break;
            }

            transform.parent = elementTransform;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Turn off the default controller's renderers
            controller.SetRenderersVisible(false);

            // Get the touchpad and assign our custom material to it
            Transform touchpad = controller.GetElement(MotionControllerInfo.ControllerElementEnum.Touchpad);
            if (touchpad != null)
            {
                touchpadRenderer = touchpad.GetComponentInChildren<MeshRenderer>();
                touchpadRenderer.material = touchpadMaterial;
                touchpadRenderer.enabled = true;
            }

            // Subscribe to input now that we're parented under the controller
            InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionSourceReleased;

            while (isActiveAndEnabled)
            {
                while (currentAction == SwipeEnum.None)
                {
                    foreach (Transform brushObject in brushCollection.Objects)
                    {
                        brushObject.GetComponent<Brush>().StrokeColor = colorPicker.SelectedColor;
                    }
                    yield return null;
                }

                if (!menuOpen)
                {
                    menuOpenTime = Time.unscaledTime;
                    menuOpen = true;
                }

                // Stop the active brush if we have one
                if (activeBrush != null)
                {
                    activeBrush.Draw = false;
                    activeBrush = null;
                }

                // Get the current offset and the target offset from our collection
                startOffset = brushCollection.GetOffsetFromObjectIndex(displayBrushindex);
                targetOffset = startOffset;
                switch (currentAction)
                {
                    case SwipeEnum.Right:
                        displayBrushindex = brushCollection.GetPrevObjectIndex(displayBrushindex);
                        activeBrushindex = brushCollection.GetNextObjectIndex(activeBrushindex);
                        targetOffset -= brushCollection.DistributionOffsetPerObject;
                        break;

                    case SwipeEnum.Left:
                    default:
                        displayBrushindex = brushCollection.GetNextObjectIndex(displayBrushindex);
                        activeBrushindex = brushCollection.GetPrevObjectIndex(activeBrushindex);
                        targetOffset += brushCollection.DistributionOffsetPerObject;
                        break;
                }

                // Get the current brush from the object list
                Transform brushTransform = brushCollection.Objects[activeBrushindex];
                activeBrush = brushTransform.GetComponent<Brush>();

                // Lerp from current to target offset
                float startTime = Time.unscaledTime;
                bool resetInput = false;
                while (Time.unscaledTime < startTime + swipeDuration)
                {
                    float normalizedTime = (Time.unscaledTime - startTime) / swipeDuration;

                    if (!resetInput && normalizedTime > 0.5f)
                    {
                        // If we're past the halfway point, set our swipe action to none
                        // If the user swipes again before we're done switching, we'll move to the next item
                        resetInput = true;
                        currentAction = SwipeEnum.None;
                    }

                    brushCollection.DistributionOffset = Mathf.Lerp(startOffset, targetOffset, swipeCurve.Evaluate(normalizedTime));
                    menuOpenTime = Time.unscaledTime;
                    yield return null;
                }
                brushCollection.DistributionOffset = targetOffset;

                yield return null;
            }
        }

        private void OnEnable()
        {
            displayBrushindex = -1;
            currentAction = SwipeEnum.Left;
        }

        private void OnDestroy()
        {
            Debug.Log("Destroying!");
        }

        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness && obj.pressType == InteractionSourcePressType.Select)
            {
                activeBrush.Draw = true;
            }
        }

        private void InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness)
            {

                if (obj.state.touchpadPressed)
                {
                    // Check which side we clicked
                    if (obj.state.touchpadPosition.x < 0)
                        currentAction = SwipeEnum.Left;
                    else
                        currentAction = SwipeEnum.Right;

                    // Ping the touchpad material so it gets bright
                    touchpadTouchTime = Time.unscaledTime;
                }
            }
        }

        private void InteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness && obj.pressType == InteractionSourcePressType.Select)
            {
                activeBrush.Draw = false;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (activeBrush != null)
            {
                Gizmos.DrawWireSphere(activeBrush.TipPosition, 0.01f);
            }
        }
#endif
    }
}