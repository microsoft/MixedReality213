using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MRDL.ControllerExamples
{
    public class Brush : MonoBehaviour
    {
        const int MaxGradientKeys = 8; // Unity restriction

        public enum DisplayModeEnum
        {
            InMenu,
            InHand,
            Hidden,
        }

        public DisplayModeEnum DisplayMode
        {
            set
            {
                displayMode = value;
            }
        }

        public bool Draw
        {
            get
            {
                return draw;
            }
            set
            {
                if (displayMode != DisplayModeEnum.InHand)
                    return;

                if (draw != value)
                {
                    draw = value;
                    if (draw)
                    {
                        StartCoroutine(DrawOverTime());
                    }
                }
            }
        }

        public Vector3 TipPosition
        {
            get
            {
                return tip.position;
            }
        }

        public Color StrokeColor
        {
            set
            {
                Color32 newColor = value;
                Color32 oldColor = currentStrokeColor;

                float delta = (float)(Mathf.Abs(oldColor.r - currentStrokeColor.r)
                    + Mathf.Abs(oldColor.g - currentStrokeColor.g)
                    + Mathf.Abs(oldColor.b - currentStrokeColor.b)
                    + Mathf.Abs(oldColor.a - currentStrokeColor.a)) / 4;

                if (delta < minColorDelta)
                    return;

                currentStrokeColor = value;
                brushRenderer.material.color = currentStrokeColor;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(UpdateDisplayMode());
        }

        protected virtual IEnumerator DrawOverTime()
        {
            // Get the position of the tip
            Vector3 lastPointPosition = tip.position;
            // Then wait one frame and get the position again
            yield return null;
            Vector3 startPosition = tip.position;
            // Create a new brush stroke
            GameObject newStroke = GameObject.Instantiate(strokePrefab) as GameObject;
            LineRenderer line = newStroke.GetComponent<LineRenderer>();
            newStroke.transform.position = startPosition;
            line.SetPosition(0, tip.position);

            while (draw)
            {
                // Move the last point to the draw point position
                line.SetPosition(line.positionCount - 1, tip.position);
                line.material.color = currentStrokeColor;

                if (Vector3.Distance(lastPointPosition, tip.position) > minPositionDelta)
                {
                    // Spawn a new point
                    lastPointPosition = tip.position;
                    line.positionCount += 1;
                    line.SetPosition(line.positionCount - 1, lastPointPosition);
                }
                yield return null;
            }
        }

        private IEnumerator UpdateDisplayMode()
        {
            // Variables we'll be using
            Vector3 targetPosition = inMenuPosition;
            Vector3 targetScale = Vector3.one;
            Quaternion targetRotation = Quaternion.Euler(inMenuRotation);
            Vector3 startPosition = targetPosition;
            Vector3 startScale = targetScale;
            Quaternion startRotation = targetRotation;

            // Reset before starting
            displayMode = DisplayModeEnum.InMenu;
            DisplayModeEnum lastDisplayMode = displayMode;
            brushObjectTransform.localPosition = targetPosition;
            brushObjectTransform.localRotation = targetRotation;

            while (isActiveAndEnabled)
            {
                // Wait for displayMode to change
                while (displayMode == lastDisplayMode)
                {
                    yield return null;
                }

                startPosition = brushObjectTransform.localPosition;
                startRotation = brushObjectTransform.localRotation;
                startScale = brushObjectTransform.localScale;
                lastDisplayMode = displayMode;
                switch (displayMode)
                {
                    case DisplayModeEnum.InHand:
                        targetPosition = inHandPosition;
                        targetScale = Vector3.one;
                        targetRotation = Quaternion.Euler(inHandRotation);
                        brushObjectTransform.gameObject.SetActive(true);
                        break;

                    case DisplayModeEnum.InMenu:
                        targetPosition = inMenuPosition;
                        targetScale = Vector3.one;
                        targetRotation = Quaternion.Euler(inMenuRotation);
                        brushObjectTransform.gameObject.SetActive(true);
                        break;

                    case DisplayModeEnum.Hidden:
                        targetPosition = inMenuPosition;
                        targetRotation = Quaternion.Euler(inMenuRotation);
                        targetScale = startScale * 0.01f;
                        break;
                }

                // Keep going until we're done transitioning, or until the mode changes, whichever comes first
                float startTime = Time.unscaledTime;
                while ((Time.unscaledTime < startTime + transitionDuration) && lastDisplayMode == displayMode)
                {
                    float normalizedTime = (Time.unscaledTime - startTime) / transitionDuration;
                    brushObjectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, transitionCurve.Evaluate (normalizedTime));
                    brushObjectTransform.localScale = Vector3.Lerp(startScale, targetScale, transitionCurve.Evaluate(normalizedTime));
                    brushObjectTransform.localRotation = Quaternion.Lerp(startRotation, targetRotation, transitionCurve.Evaluate(normalizedTime));                  
                    yield return null;
                }
                brushObjectTransform.localPosition = targetPosition;
                brushObjectTransform.localScale = targetScale;
                brushObjectTransform.localRotation = targetRotation;

                if (displayMode == DisplayModeEnum.Hidden)
                {
                    brushObjectTransform.gameObject.SetActive(false);
                }

                yield return null;
            }
            yield break;
        }

        [Header("Drawing settings")]
        [SerializeField]
        private float maxStrokeLength = 10f;
        [SerializeField]
        private float minColorDelta = 0.01f;
        [SerializeField]
        private float minPositionDelta = 0.01f;
        [SerializeField]
        private Transform tip;
        [SerializeField]
        private GameObject strokePrefab;
        [SerializeField]
        private Transform brushObjectTransform;
        [SerializeField]
        private Renderer brushRenderer;

        protected bool draw = false;

        [Header("Mode settings")]
        [SerializeField]
        private Vector3 inMenuPosition;
        [SerializeField]
        private Vector3 inMenuRotation;
        [SerializeField]
        private Vector3 inHandPosition;
        [SerializeField]
        private Vector3 inHandRotation;
        [SerializeField]
        private DisplayModeEnum displayMode = DisplayModeEnum.InMenu;
        [SerializeField]
        private float transitionDuration = 0.5f;
        [SerializeField]
        private AnimationCurve transitionCurve;
        
        private Color currentStrokeColor = Color.white;

        #if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(Brush))]
        public class BrushEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                Brush brush = (Brush)target;

                base.OnInspectorGUI();
                if (GUILayout.Button("Start/Stop"))
                {
                    brush.Draw = !brush.Draw;
                }
            }
        }
        #endif
    }
}