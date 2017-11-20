// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using HoloToolkit.Unity.Controllers;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace HoloToolkit.Unity.ControllerExamples
{
    public class ObjectSpawner : AttachToController
    {
        public enum StateEnum
        {
            Uninitialized,
            Idle,
            Switching,
            Spawning
        }

        public int MeshIndex
        {
            get { return meshIndex; }
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

        private int meshIndex = 0;
        private StateEnum state = StateEnum.Uninitialized;
        private Material instantiatedMaterial;

        private void Awake()
        {
            instantiatedMaterial = new Material(objectMaterial);
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

        protected override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(instantiatedMaterial);
        }

        private void SpawnObject()
        {
            // Instantiate the spawned object
            GameObject newObject = Instantiate(displayObject.gameObject, spawnParent);
            // Detatch the newly spawned object
            newObject.transform.parent = null;
            // Reset the scale transform to 1
            scaleParent.localScale = Vector3.one;
            // Set its material color so its material gets instantiated
            newObject.GetComponent<Renderer>().material.color = colorSource.SelectedColor;
        }

        protected override void OnAttachToController()
        {
            displayObject.sharedMesh = availableMeshes[meshIndex];
            displayObject.GetComponent<Renderer>().sharedMaterial = instantiatedMaterial;

            // Subscribe to input now that we're parented under the controller
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased += InteractionSourceReleased;

            state = StateEnum.Idle;
        }

        protected override void OnDetachFromController()
        {
            // Unsubscribe from input now that we've detached from the controller
            InteractionManager.InteractionSourcePressed -= InteractionSourcePressed;
            InteractionManager.InteractionSourceReleased -= InteractionSourceReleased;

            state = StateEnum.Uninitialized;
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
        
        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness)
            {
                switch (obj.pressType)
                {
                    case InteractionSourcePressType.Grasp:
                        if (state == StateEnum.Idle)
                        {
                            // We've pressed the grasp - enter spawning state
                            state = StateEnum.Spawning;
                            SpawnObject();
                        }
                        break;

                    case InteractionSourcePressType.Select:
                        meshIndex++;
                        if (meshIndex >= NumAvailableMeshes)
                        {
                            meshIndex = 0;
                        }
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
                        if (state == StateEnum.Spawning)
                        {
                            // We've released the grasp - return to idle state
                            state = StateEnum.Idle;
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
