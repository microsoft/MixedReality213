using HoloToolkit.Unity.InputModule;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ControllerExamples
{
    public class ColorPickerWheel : MonoBehaviour
    {
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                visible = value;
                if (value)
                    lastTimeVisible = Time.unscaledTime;
            }
        }

        public Vector2 SelectorPosition
        {
            get { return selectorPosition; }
            set { selectorPosition = value; }
        }

        public Color SelectedColor
        {
            get
            {
                return selectedColor;
            }
        }
        
        private void Update()
        {
            if (controller == null)
                return;

            if (Time.unscaledTime > lastTimeVisible + timeout)
                visible = false;

            if (visible != visibleLastFrame)
            {
                if (visible)
                    animator.SetTrigger("Show");
                else
                    animator.SetTrigger("Hide");
            }
            visibleLastFrame = visible;

            if (!visible)
                return;

            // clamp selector position to a radius of 1
            Vector3 localPosition = new Vector3(selectorPosition.x * inputScale, 0f, selectorPosition.y * inputScale);
            if (localPosition.magnitude > 1)
                localPosition = localPosition.normalized;
            selectorTransform.localPosition = localPosition;
            // Raycast the wheel mesh and get its UV coordinates
            Vector3 raycastStart = selectorTransform.position + selectorTransform.up * 0.15f;
            RaycastHit hit;
            Debug.DrawLine(raycastStart, raycastStart - (selectorTransform.up * 0.25f));
            if (Physics.Raycast(raycastStart, -selectorTransform.up, out hit, 0.25f, 1 << colorWheelObject.layer, QueryTriggerInteraction.Ignore))
            {
                Vector2 uv = hit.textureCoord;
                int pixelX = Mathf.FloorToInt(colorWheelTexture.width * uv.x);
                int pixelY = Mathf.FloorToInt(colorWheelTexture.height * uv.y);
                selectedColor = colorWheelTexture.GetPixel(pixelX, pixelY);
                selectedColor.a = 1f;
            }
        }

        private IEnumerator Start()
        {
            // TODO replace this with a proper singleton
            ControllerVisualizer visualizer = GameObject.FindObjectOfType<ControllerVisualizer>();

            while (!visualizer.GetController(handedness, out controller))
            {
                visible = false;
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

            // Subscribe to input now that we're parented under the controller
            InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;
        }

        private void InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness)
            {
                if (obj.state.touchpadTouched)
                {
                    Visible = true;
                    SelectorPosition = obj.state.touchpadPosition;
                }
            }
        }

        [SerializeField]
        private bool visible = false;
        [SerializeField]
        private Transform selectorTransform;
        [SerializeField]
        private float inputScale = 1.1f;
        [SerializeField]
        private Vector2 selectorPosition;
        [SerializeField]
        private Color selectedColor = Color.white;
        [SerializeField]
        private Texture2D colorWheelTexture;
        [SerializeField]
        private GameObject colorWheelObject;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private float timeout = 2f;

        private float lastTimeVisible;
        private bool visibleLastFrame = false;

        [SerializeField]
        private InteractionSourceHandedness handedness = InteractionSourceHandedness.Left;
        [SerializeField]
        private ControllerInfo.ControllerElementEnum element = ControllerInfo.ControllerElementEnum.Touchpad;
        private ControllerInfo controller;
    }
}