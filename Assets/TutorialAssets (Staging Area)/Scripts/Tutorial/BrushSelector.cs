using MRDL.Design;
using System.Collections;
using UnityEngine;

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

        public void AttachToController(GameObject controller) {
            //TODO find relevant transform, parent underneath, size appropriately
        }

        private void Update()
        {
            if (menuOpen)
            {
                if (Time.unscaledTime - menuOpenTime > menuTimeout)
                {
                    menuOpen = false;
                }
            }

            for (int i = 0; i < brushCollection.Objects.Count; i++)
            {
                Brush brush = brushCollection.Objects[i].GetComponent<Brush>();
                if (brush == activeBrush)
                    brush.DisplayMode = Brush.DisplayModeEnum.InHand;
                else
                    brush.DisplayMode = menuOpen ? Brush.DisplayModeEnum.InMenu : Brush.DisplayModeEnum.Hidden;
            }

            // TEMP controller input
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                currentAction = SwipeEnum.Left;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                currentAction = SwipeEnum.Right;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!swiping && activeBrush != null)
                {
                    activeBrush.Draw = !activeBrush.Draw;
                }
            }

            // TEMP touchpad input
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                colorPicker.SelectorPosition += Vector2.left * 0.1f;
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                colorPicker.SelectorPosition += Vector2.right * 0.1f;
            }
        }

        private IEnumerator UpdateMenu()
        {
            while (isActiveAndEnabled)
            {
                while (currentAction == SwipeEnum.None)
                {
                    if (activeBrush != null)
                    {
                        activeBrush.StrokeColor = colorPicker.SelectedColor;
                    }
                    yield return null;
                }

                if (!menuOpen)
                {
                    menuOpenTime = Time.unscaledTime;
                    menuOpen = true;
                }
                swiping = true;

                // Stop the active brush if we have one
                if (activeBrush != null)
                {
                    activeBrush.Draw = false;
                    activeBrush = null;
                }

                // TODO
                // Move the current brush into drawing position

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

                swiping = false;

                yield return null;
            }
        }

        private void OnEnable()
        {
            displayBrushindex = -1;
            currentAction = SwipeEnum.Left;
            StartCoroutine(UpdateMenu());
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

        private float menuOpenTime = 0f;
        private bool menuOpen = false;
        private float startOffset;
        private float targetOffset;
        private bool swiping = false;
        private Brush activeBrush;

    }
}