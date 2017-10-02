using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MRDL.ControllerExamples
{
    public class Eraser : Brush
    {
        // Instead of drawing, the eraser will remove existing strokes
        protected override IEnumerator DrawOverTime()
        {
            // Get all the brush strokes that currently exist
            List<GameObject> brushStrokes = new List<GameObject>(GameObject.FindGameObjectsWithTag("BrushStroke"));

            while (draw)
            {
                // Move backwards through the brush strokes, removing any we intersect with
                for (int i = brushStrokes.Count - 1; i >= 0; i--)
                {
                    // Do a crude check for proximity with the brush stroke's render bounds
                    Bounds strokeBounds = brushStrokes[i].GetComponent<Renderer>().bounds;
                    if (strokeBounds.Contains (TipPosition))
                    {
                        // If we're in bounds, check whether any point of the stroke is within range
                        LineRenderer lineRenderer = brushStrokes[i].GetComponent<LineRenderer>();
                        Vector3[] positions = new Vector3[lineRenderer.positionCount];
                        lineRenderer.GetPositions(positions);
                        for (int j = 0; j < positions.Length; j++)
                        {
                            if (Vector3.Distance (positions[j], TipPosition) < eraseRange)
                            {
                                // Erase the brush stroke and remove it from our array
                                GameObject.Destroy(brushStrokes[i]);
                                brushStrokes.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
                yield return null;
            }
            yield break;
        }

        [SerializeField]
        private float eraseRange = 0.1f;
    }
}