using UnityEngine;

namespace MRDL.ControllerExamples
{
    [ExecuteInEditMode]
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
            if (Application.isPlaying)
            {
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
            }

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

        [SerializeField]
        private bool visible = false;
        [SerializeField]
        private Transform selectorTransform;
        [SerializeField]
        private float inputScale = 0.4f;
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

        private bool visibleLastFrame = false;
    }
}