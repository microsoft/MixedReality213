// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ControllerExamples
{
    public class BrushController : MonoBehaviour
    {
        public enum DisplayModeEnum
        {
            InMenu,
            InHand,
            Hidden,
        }

        public bool Draw {
            get {
                return draw;
            }
            set {
                if (colorPicker == null)
                    return;

                if (draw != value) {
                    draw = value;
                    if (draw) {
                        StartCoroutine(DrawOverTime());
                    }
                }
            }
        }

        public Vector3 TipPosition {
            get {
                return tip.position;
            }
        }

        private IEnumerator Start() {
            // Subscribe to press events for drawing
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionSourceReleased;

            // Wait for the color picker
            while (colorPicker == null) {
                colorPicker = GameObject.FindObjectOfType<ColorPickerWheel>();
                yield return null;
            }

            brushRenderer.material.color = colorPicker.SelectedColor;
        }

        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj) {
            if (obj.state.source.handedness == InteractionSourceHandedness.Right && obj.pressType == InteractionSourcePressType.Select) {
                Draw = true;
            }
        }

        private void InteractionSourceReleased(InteractionSourceReleasedEventArgs obj) {
            if (obj.state.source.handedness == InteractionSourceHandedness.Right && obj.pressType == InteractionSourcePressType.Select) {
                Draw = false;
            }
        }

        private IEnumerator DrawOverTime() {
            // Get the position of the tip
            Vector3 lastPointPosition = tip.position;
            // Then wait one frame and get the position again
            yield return null;

            // If we're not still drawing after one frame
            if (!draw) {
                // This was a fluke, abort!
                yield break;
            }

            Vector3 startPosition = tip.position;
            // Create a new brush stroke
            GameObject newStroke = GameObject.Instantiate(strokePrefab) as GameObject;
            LineRenderer line = newStroke.GetComponent<LineRenderer>();
            newStroke.transform.position = startPosition;
            line.SetPosition(0, tip.position);

            while (draw) {
                // Move the last point to the draw point position
                line.SetPosition(line.positionCount - 1, tip.position);
                line.material.color = colorPicker.SelectedColor;
                brushRenderer.material.color = colorPicker.SelectedColor;
                lastPointAddedTime = Time.unscaledTime;

                if (Vector3.Distance(lastPointPosition, tip.position) > minPositionDelta || Time.unscaledTime > lastPointAddedTime + maxTimeDelta) {
                    // Spawn a new point
                    lastPointAddedTime = Time.unscaledTime;
                    lastPointPosition = tip.position;
                    line.positionCount += 1;
                    line.SetPosition(line.positionCount - 1, lastPointPosition);
                }
                yield return null;
            }
        }

        [Header("Drawing settings")]
        [SerializeField]
        private float minColorDelta = 0.01f;
        [SerializeField]
        private float minPositionDelta = 0.01f;
        [SerializeField]
        private float maxTimeDelta = 0.25f;
        [SerializeField]
        private Transform tip;
        [SerializeField]
        private GameObject strokePrefab;
        [SerializeField]
        private Transform brushObjectTransform;
        [SerializeField]
        private Renderer brushRenderer;
        
        private ColorPickerWheel colorPicker;
        private bool draw = false;
        private Color currentStrokeColor = Color.white;
        private float lastPointAddedTime = 0f;
    }
}