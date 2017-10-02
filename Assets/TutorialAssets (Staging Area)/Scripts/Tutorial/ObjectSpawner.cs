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
            get
            {
                return meshIndex;
            }
            set
            {
                if (state != StateEnum.Idle)
                    return;

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

        public void SpawnObject()
        {
            if (state != StateEnum.Idle)
                return;

            StartCoroutine(SpawnOverTime());
        }

        private void OnEnable()
        {
            instantiatedMaterial = new Material(objectMaterial);
            displayObject.sharedMesh = availableMeshes[meshIndex];
            displayObject.GetComponent<Renderer>().sharedMaterial = instantiatedMaterial;
        }

        private IEnumerator Start()
        {
            // TODO replace this with a proper singleton
            ControllerVisualizer visualizer = GameObject.FindObjectOfType<ControllerVisualizer>();

            while (!visualizer.GetController(handedness, out controller))
            {
                yield return null;
            }

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

            // Turn off the ring
            Transform ringElement = controller.GetElement(ControllerInfo.ControllerElementEnum.Ring);
            if (ringElement != null)
                ringElement.gameObject.SetActive(false);

            // Subscribe to input now that we're parented under the controller
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;

            state = StateEnum.Idle;
        }

        private void Update()
        {
            if (state != StateEnum.Idle)
                return;

            if (meshIndex < 0 || meshIndex >= availableMeshes.Length)
                throw new IndexOutOfRangeException();
            
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
            //AudioSource.PlayClipAtPoint(soundOnSwitch, transform.position);
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
            //AudioSource.PlayClipAtPoint(soundOnSpawn, transform.position);
            GameObject newObject = GameObject.Instantiate(displayObject.gameObject, spawnParent) as GameObject;
            animator.SetTrigger("Spawn");
            yield return null;
            // Wait for the animation to play out
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("SpawnStart"))
            {
                yield return null;
            }
            //AudioSource.PlayClipAtPoint(soundOnSpawn, transform.position);
            while (animator.GetCurrentAnimatorStateInfo(0).IsName("SpawnStart"))
            {
                yield return null;
            }
            // Detatch the newly spawned object
            newObject.transform.parent = null;
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

        [Header("Objects and materials")]
        [SerializeField]
        private Transform displayParent;
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

        [Header("Sounds and animation")]
        [SerializeField]
        private AudioClip soundOnSpawn;
        [SerializeField]
        private AudioClip soundOnSwitch;
        [SerializeField]
        private Animator animator;

        [SerializeField]
        private InteractionSourceHandedness handedness = InteractionSourceHandedness.Left;
        [SerializeField]
        private ControllerInfo.ControllerElementEnum element = ControllerInfo.ControllerElementEnum.PointingPose;
        private ControllerInfo controller;

        private int meshIndex = 0;
        private StateEnum state = StateEnum.Uninitialized;
        private Material instantiatedMaterial;
    }

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(ObjectSpawner))]
    public class ObjectSpawnerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ObjectSpawner objectSpawner = (ObjectSpawner)target;
            objectSpawner.MeshIndex = UnityEditor.EditorGUILayout.IntSlider("Mesh index", objectSpawner.MeshIndex, 0, objectSpawner.NumAvailableMeshes - 1);
            if (GUILayout.Button ("Spawn Object"))
            {
                objectSpawner.SpawnObject();
            }
        }
    }
    #endif
}
