// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ControllerExamples
{
    public class ObjectSpawner : MonoBehaviour
    {
        public enum StateEnum
        {
            Uninitialized,
            Idle,
            Switching,
            Spawning,
        }

        public int MeshIndex
        {
            get            {                return meshIndex;            }
            set
            {
                if (state != StateEnum.Idle)
                {
                    return;
                }

                if (meshIndex != value)
                {
                    meshIndex = value;
                    state = StateEnum.Switching;
                    StartCoroutine(SwitchOverTime());
                }
            }
        }

        public int NumAvailableMeshes
        {
            get { return availableMeshes.Length; }
        }

        [Header("Objects and materials")]
        [SerializeField]
        private Transform displayParent;
        [SerializeField]
        private Transform scaleParent;
        [SerializeField]
        private Transform spawnParent;
        [SerializeField]
        private MeshFilter displayObject;
        [SerializeField]
        private Material objectMaterial;
        [SerializeField]
        private ColorPickerWheel colorSource;
        [SerializeField]
        private Mesh[] availableMeshes;

        [Header("Animation")]
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private AnimationCurve growCurve;
        [SerializeField]
        private float growTime = 2f;

        [SerializeField]
        private InteractionSourceHandedness handedness = InteractionSourceHandedness.Left;
        [SerializeField]
        private MotionControllerInfo.ControllerElementEnum element = MotionControllerInfo.ControllerElementEnum.PointingPose;
        private MotionControllerInfo controller;

        private int meshIndex = 0;
        private StateEnum state = StateEnum.Uninitialized;
        private Material instantiatedMaterial;
        private bool released;
        private float timePressed;

        private void SpawnObject()
        {
            if (state != StateEnum.Idle)
            {
                return;
            }

            state = StateEnum.Spawning;
            StartCoroutine(SpawnOverTime());
        }

        private IEnumerator Start()
        {
            while (!MotionControllerVisualizer.Instance.TryGetControllerModel(handedness, out controller))
            {
                yield return null;
            }

            instantiatedMaterial = new Material(objectMaterial);
            displayObject.sharedMesh = availableMeshes[meshIndex];
            displayObject.GetComponent<Renderer>().sharedMaterial = instantiatedMaterial;

            // Parent the picker wheel under the element of choice
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

            // Subscribe to input now that we're parented under the controller
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionSourceReleased;

            state = StateEnum.Idle;
        }

        private void Update()
        {
            if (state != StateEnum.Idle)
            {
                return;
            }

            if (meshIndex < 0 || meshIndex >= availableMeshes.Length)
            {
                throw new IndexOutOfRangeException();
            }

            displayObject.sharedMesh = availableMeshes[meshIndex];
            instantiatedMaterial.color = colorSource.SelectedColor;
        }

        private IEnumerator SwitchOverTime()
        {
            animator.SetTrigger("Switch");
            // Wait for the animation to play out
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("SwitchStart"))
            {
                yield return null;
            }

            while (animator.GetCurrentAnimatorStateInfo(0).IsName("SwitchStart"))
            {
                yield return null;
            }
            // Now switch the mesh on the display object
            // Then wait for the reverse to play out
            displayObject.sharedMesh = availableMeshes[meshIndex];
            while (animator.GetCurrentAnimatorStateInfo(0).IsName("SwitchFinish"))
            {
                yield return null;
            }
            state = StateEnum.Idle;
            yield break;
        }

        private IEnumerator SpawnOverTime()
        {
            released = false;
            timePressed = Time.unscaledTime;

            GameObject newObject = GameObject.Instantiate(displayObject.gameObject, spawnParent) as GameObject;
            Vector3 startScale = scaleParent.localScale;
            // Hide the display object while we're scaling up the newly spawned object
            displayObject.gameObject.SetActive(false);

            while (!released)
            {
                // Grow the object while the control is pressed
                float normalizedGrowth = (Time.unscaledTime - timePressed) / growTime;
                scaleParent.localScale = startScale + (Vector3.one * +growCurve.Evaluate(normalizedGrowth));
                yield return null;
            }

            // Once we've released, start our spawn animation
            animator.SetTrigger("Spawn");
            yield return null;
            // Wait for the animation to play out
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("SpawnStart"))
            {
                yield return null;
            }

            while (animator.GetCurrentAnimatorStateInfo(0).IsName("SpawnStart"))
            {
                yield return null;
            }

            // Detatch the newly spawned object
            newObject.transform.parent = null;
            // Reset the scale transform to 1
            scaleParent.localScale = Vector3.one;
            // Set its material color so its material gets instantiated
            newObject.GetComponent<Renderer>().material.color = colorSource.SelectedColor;

            // Show our old display object again
            displayObject.gameObject.SetActive(true);
            // Then wait for the new display object to show
            while (animator.GetCurrentAnimatorStateInfo(0).IsName("SpawnFinish"))
            {
                yield return null;
            }
            // Reset to idle
            state = StateEnum.Idle;
            yield break;
        }

        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness)
            {
                switch (obj.pressType)
                {
                    case InteractionSourcePressType.Grasp:
                        SpawnObject();
                        break;

                    case InteractionSourcePressType.Select:
                        meshIndex++;
                        if (meshIndex >= NumAvailableMeshes)
                            meshIndex = 0;
                        break;

                    default:
                        break;
                }
            }
        }

        private void InteractionSourceReleased(InteractionSourceReleasedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness)
            {
                switch (obj.pressType)
                {
                    case InteractionSourcePressType.Grasp:
                        if (state != StateEnum.Spawning)
                        {
                            return;
                        }
                        // Release object
                        released = true;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
