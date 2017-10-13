// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HoloToolkit.Unity.InputModule
{
    /// <summary>
    /// This script keeps track of the GameObjects for each button on the controller.
    /// It also keeps track of the animation Transforms in order to properly animate according to user input.
    /// </summary>
    [Serializable]
    public class MotionControllerInfo
    {
        public GameObject ControllerParent;
        public InteractionSourceHandedness Handedness;

        private GameObject home;
        private Transform homePressed;
        private Transform homeUnpressed;
        private GameObject menu;
        private Transform menuPressed;
        private Transform menuUnpressed;
        private GameObject grasp;
        private Transform graspPressed;
        private Transform graspUnpressed;
        private GameObject thumbstickPress;
        private Transform thumbstickPressed;
        private Transform thumbstickUnpressed;
        private GameObject thumbstickX;
        private Transform thumbstickXMin;
        private Transform thumbstickXMax;
        private GameObject thumbstickY;
        private Transform thumbstickYMin;
        private Transform thumbstickYMax;
        private GameObject select;
        private Transform selectPressed;
        private Transform selectUnpressed;
        private GameObject touchpadPress;
        private Transform touchpadPressed;
        private Transform touchpadUnpressed;
        private GameObject touchpadTouchX;
        private Transform touchpadTouchXMin;
        private Transform touchpadTouchXMax;
        private GameObject touchpadTouchY;
        private Transform touchpadTouchYMin;
        private Transform touchpadTouchYMax;
        private GameObject touchpadTouchVisualizer;

        private GameObject pointingPose;
        /* These elements will be available in future versions
        private GameObject handlePose;
        private GameObject caseElement;
        private GameObject ringElement;
        private GameObject inputPose;
        */

        // These values are used to determine if a button's state has changed.
        private bool wasGrasped;
        private bool wasMenuPressed;
        private bool wasHomePressed;
        private bool wasThumbstickPressed;
        private bool wasTouchpadPressed;
        private bool wasTouchpadTouched;
        private Vector2 lastThumbstickPosition;
        private Vector2 lastTouchpadPosition;
        private double lastSelectPressedAmount;

        public enum ControllerElementEnum
        {
            // control elements
            Home,
            Menu,
            Grasp,
            Thumbstick,
            Select,
            Touchpad,
            TouchpadTouchX,
            TouchpadTouchY,
            // control pressed / unpressed
            HomePressed,
            HomeUnpressed,
            MenuPressed,
            MenuUnpressed,
            GraspPressed,
            GraspUnpressed,
            ThumbstickPressed,
            ThumbstickUnpressed,
            TouchpadPressed,
            TouchpadUnpressed,
            // control min max
            ThumbstickXMin,
            ThumbstickXMax,
            ThumbstickYMin,
            ThumbstickYMax,
            TouchpadTouchXMin,
            TouchpadTouchXMax,
            TouchpadTouchYMin,
            TouchpadTouchYMax,
            // body elements & poses
            PointingPose,
            /* These elements will be available in future versions
            HandlePose,
            InputPose,
            Ring,
            Case,
            */
        }

        public Transform GetElement(ControllerElementEnum element)
        {
            switch (element)
            {
                // control elements
                case ControllerElementEnum.Home: return home.transform;
                case ControllerElementEnum.Menu: return menu.transform;
                case ControllerElementEnum.Grasp: return grasp.transform;
                case ControllerElementEnum.Thumbstick: return thumbstickPress.transform;
                case ControllerElementEnum.Touchpad: return touchpadPress.transform;
                case ControllerElementEnum.TouchpadTouchX: return touchpadTouchX.transform;
                case ControllerElementEnum.TouchpadTouchY: return touchpadTouchY.transform;
                // control pressed / unpressed
                case ControllerElementEnum.HomePressed: return homePressed;
                case ControllerElementEnum.HomeUnpressed: return homeUnpressed;
                case ControllerElementEnum.MenuPressed: return menuPressed;
                case ControllerElementEnum.MenuUnpressed: return menuUnpressed;
                case ControllerElementEnum.GraspPressed: return graspPressed;
                case ControllerElementEnum.GraspUnpressed: return graspUnpressed;
                case ControllerElementEnum.ThumbstickPressed: return thumbstickPressed;
                case ControllerElementEnum.ThumbstickUnpressed: return thumbstickUnpressed;
                case ControllerElementEnum.TouchpadPressed: return touchpadPressed;
                case ControllerElementEnum.TouchpadUnpressed: return touchpadUnpressed;
                // control min max
                case ControllerElementEnum.ThumbstickXMin: return thumbstickXMin;
                case ControllerElementEnum.ThumbstickXMax: return thumbstickXMax;
                case ControllerElementEnum.ThumbstickYMin: return thumbstickYMin;
                case ControllerElementEnum.ThumbstickYMax: return thumbstickYMax;
                case ControllerElementEnum.TouchpadTouchXMin: return touchpadTouchXMin;
                case ControllerElementEnum.TouchpadTouchXMax: return touchpadTouchXMax;
                case ControllerElementEnum.TouchpadTouchYMin: return touchpadTouchYMin;
                case ControllerElementEnum.TouchpadTouchYMax: return touchpadTouchYMax;
                // body elements & poses
                case ControllerElementEnum.PointingPose: return pointingPose.transform;
                /* These elements will be available in future versions
                case ControllerElementEnum.HandlePose: return handlePose.transform;
                case ControllerElementEnum.InputPose: return inputPose.transform;
                case ControllerElementEnum.Case: return caseElement.transform;
                case ControllerElementEnum.Ring: return ringElement.transform;
                */
                default:
                    return null;
            }
        }

        /// <summary>
        /// Iterates through the Transform array to find specifically named GameObjects.
        /// These GameObjects specify the animation bounds and the GameObject to modify for button,
        /// thumbstick, and touchpad animation.
        /// </summary>
        /// <param name="childTransforms">The transforms of the glTF model.</param>
        /// <param name="visualizerScript">The script containing references to any objects to spawn.</param>
        public void LoadInfo(Transform[] childTransforms, MotionControllerVisualizer visualizerScript)
        {
            // Regex to remove numbers from end of child name
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\d+$");

            foreach (Transform child in childTransforms)
            {
                // Animation bounds are named in two pairs:
                // pressed/unpressed and min/max. There is also a value
                // transform, which is the transform to modify to
                // animate the interactions. We also look for the
                // touch transform, in order to spawn the touchpadTouched
                // visualizer.
                string childName = regex.Replace(child.name.ToLower(), "").Trim();

                switch (childName)
                {
                    case "home":
                        home = child.gameObject;
                        break;
                    case "menu":
                        menu = child.gameObject;
                        break;
                    case "grasp":
                        grasp = child.gameObject;
                        break;
                    case "select":
                        select = child.gameObject;
                        break;
                    case "touch":
                        touchpadTouchVisualizer = visualizerScript.SpawnTouchpadVisualizer(child);
                        break;
                    case "pointing_pose":
                        pointingPose = child.gameObject;
                        break;

                    case "pressed":
                        switch (child.parent.name.ToLower())
                        {
                            case "home":
                                homePressed = child;
                                break;
                            case "menu":
                                menuPressed = child;
                                break;
                            case "grasp":
                                graspPressed = child;
                                break;
                            case "select":
                                selectPressed = child;
                                break;
                            case "thumbstick_press":
                                thumbstickPressed = child;
                                break;
                            case "touchpad_press":
                                touchpadPress = child.gameObject;
                                break;
                            default:
                                Debug.LogWarning("Unknown parent " + child.parent.name + " for pressed transform");
                                break;
                        }
                        break;
                    case "unpressed":
                        switch (child.parent.name.ToLower())
                        {
                            case "home":
                                homeUnpressed = child;
                                break;
                            case "menu":
                                menuUnpressed = child;
                                break;
                            case "grasp":
                                graspUnpressed = child;
                                break;
                            case "select":
                                selectUnpressed = child;
                                break;
                            case "thumbstick_press":
                                thumbstickUnpressed = child;
                                break;
                            case "touchpad_press":
                                touchpadUnpressed = child;
                                break;
                            default:
                                Debug.LogWarning("Unknown parent " + child.parent.name + " for unpressed transform");
                                break;
                        }
                        break;
                    case "min":
                        switch (child.parent.name.ToLower())
                        {
                            case "thumbstick_x":
                                thumbstickXMin = child;
                                break;
                            case "thumbstick_y":
                                thumbstickYMin = child;
                                break;
                            case "touchpad_touch_x":
                                touchpadTouchXMin = child;
                                break;
                            case "touchpad_touch_y":
                                touchpadTouchYMin = child;
                                break;
                            default:
                                Debug.LogWarning("Unknown parent " + child.parent.name + " for min transform");
                                break;
                        }
                        break;
                    case "max":
                        switch (child.parent.name.ToLower())
                        {
                            case "thumbstick_x":
                                thumbstickXMax = child;
                                break;
                            case "thumbstick_y":
                                thumbstickYMax = child;
                                break;
                            case "touchpad_touch_x":
                                touchpadTouchXMax = child;
                                break;
                            case "touchpad_touch_y":
                                touchpadTouchYMax = child;
                                break;
                            default:
                                Debug.LogWarning("Unknown parent " + child.parent.name + " for max transform");
                                break;
                        }
                        break;
                    case "value":
                        switch (child.parent.name.ToLower())
                        {
                            case "home":
                                home = child.gameObject;
                                break;
                            case "menu":
                                menu = child.gameObject;
                                break;
                            case "grasp":
                                grasp = child.gameObject;
                                break;
                            case "select":
                                select = child.gameObject;
                                break;
                            case "thumbstick_press":
                                thumbstickPress = child.gameObject;
                                break;
                            case "thumbstick_x":
                                thumbstickX = child.gameObject;
                                break;
                            case "thumbstick_y":
                                thumbstickY = child.gameObject;
                                break;
                            case "touchpad_touch_x":
                                touchpadTouchX = child.gameObject;
                                break;
                            case "touchpad_touch_y":
                                touchpadTouchY = child.gameObject;
                                break;
                            default:
                                Debug.LogWarning("Unknown parent " + child.parent.name + " for value transform");
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void AnimateGrasp(bool isGrasped)
        {
            if (grasp != null && graspPressed != null && graspUnpressed != null && isGrasped != wasGrasped)
            {
                SetLocalPositionAndRotation(grasp, isGrasped ? graspPressed : graspUnpressed);
                wasGrasped = isGrasped;
            }
        }

        public void AnimateMenu(bool isMenuPressed)
        {
            if (menu != null && menuPressed != null && menuUnpressed != null && isMenuPressed != wasMenuPressed)
            {
                SetLocalPositionAndRotation(menu, isMenuPressed ? menuPressed : menuUnpressed);
                wasMenuPressed = isMenuPressed;
            }
        }

        public void AnimateHome(bool isHomePressed)
        {
            if (home != null && homePressed != null && homeUnpressed != null && isHomePressed != wasHomePressed)
            {
                SetLocalPositionAndRotation(home, isHomePressed ? homePressed : homeUnpressed);
                wasHomePressed = isHomePressed;
            }
        }

        public void AnimateSelect(float newSelectPressedAmount)
        {
            if (select != null && selectPressed != null && selectUnpressed != null && newSelectPressedAmount != lastSelectPressedAmount)
            {
                select.transform.localPosition = Vector3.Lerp(selectUnpressed.localPosition, selectPressed.localPosition, newSelectPressedAmount);
                select.transform.localRotation = Quaternion.Lerp(selectUnpressed.localRotation, selectPressed.localRotation, newSelectPressedAmount);
                lastSelectPressedAmount = newSelectPressedAmount;
            }
        }

        public void AnimateThumbstick(bool isThumbstickPressed, Vector2 newThumbstickPosition)
        {
            if (thumbstickPress != null && thumbstickPressed != null && thumbstickUnpressed != null && isThumbstickPressed != wasThumbstickPressed)
            {
                SetLocalPositionAndRotation(thumbstickPress, isThumbstickPressed ? thumbstickPressed : thumbstickUnpressed);
                wasThumbstickPressed = isThumbstickPressed;
            }

            if (thumbstickX != null && thumbstickY != null && thumbstickXMin != null && thumbstickXMax != null && thumbstickYMin != null && thumbstickYMax != null && newThumbstickPosition != lastThumbstickPosition)
            {
                Vector2 thumbstickNormalized = (newThumbstickPosition + Vector2.one) * 0.5f;

                thumbstickX.transform.localPosition = Vector3.Lerp(thumbstickXMin.localPosition, thumbstickXMax.localPosition, thumbstickNormalized.x);
                thumbstickX.transform.localRotation = Quaternion.Lerp(thumbstickXMin.localRotation, thumbstickXMax.localRotation, thumbstickNormalized.x);

                thumbstickY.transform.localPosition = Vector3.Lerp(thumbstickYMax.localPosition, thumbstickYMin.localPosition, thumbstickNormalized.y);
                thumbstickY.transform.localRotation = Quaternion.Lerp(thumbstickYMax.localRotation, thumbstickYMin.localRotation, thumbstickNormalized.y);

                lastThumbstickPosition = newThumbstickPosition;
            }
        }

        public void AnimateTouchpad(bool isTouchpadPressed, bool isTouchpadTouched, Vector2 newTouchpadPosition)
        {
            if (touchpadPress != null && touchpadPressed != null && touchpadUnpressed != null && isTouchpadPressed != wasTouchpadPressed)
            {
                SetLocalPositionAndRotation(touchpadPress, isTouchpadPressed ? touchpadPressed : touchpadUnpressed);
                wasTouchpadPressed = isTouchpadPressed;
            }

            if (touchpadTouchVisualizer != null && isTouchpadTouched != wasTouchpadTouched)
            {
                touchpadTouchVisualizer.SetActive(isTouchpadTouched);
                wasTouchpadTouched = isTouchpadTouched;
            }

            if (touchpadTouchX != null && touchpadTouchY != null && touchpadTouchXMin != null && touchpadTouchXMax != null && touchpadTouchYMin != null && touchpadTouchYMax != null && newTouchpadPosition != lastTouchpadPosition)
            {
                Vector2 touchpadNormalized = (newTouchpadPosition + Vector2.one) * 0.5f;

                touchpadTouchX.transform.localPosition = Vector3.Lerp(touchpadTouchXMin.localPosition, touchpadTouchXMax.localPosition, touchpadNormalized.x);
                touchpadTouchX.transform.localRotation = Quaternion.Lerp(touchpadTouchXMin.localRotation, touchpadTouchXMax.localRotation, touchpadNormalized.x);

                touchpadTouchY.transform.localPosition = Vector3.Lerp(touchpadTouchYMax.localPosition, touchpadTouchYMin.localPosition, touchpadNormalized.y);
                touchpadTouchY.transform.localRotation = Quaternion.Lerp(touchpadTouchYMax.localRotation, touchpadTouchYMin.localRotation, touchpadNormalized.y);

                lastTouchpadPosition = newTouchpadPosition;
            }
        }

        private void SetLocalPositionAndRotation(GameObject buttonGameObject, Transform newTransform)
        {
            buttonGameObject.transform.localPosition = newTransform.localPosition;
            buttonGameObject.transform.localRotation = newTransform.localRotation;
        }

        public void SetRenderersVisible(bool visible)
        {
            MeshRenderer[] renderers = ControllerParent.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = visible;
            }
        }
    }
}