﻿using System.Collections.Generic;
using UnityEngine;

namespace MRDL.Design
{
    public class LineObjectCollection : MonoBehaviour
    {
        public List<Transform> Objects = new List<Transform>();

        [Range(-2f, 2f)]
        public float DistributionOffset = 0f;
        [Range(0f, 2f)]
        public float LengthOffset = 0f;
        [Range(0f, 2f)]
        public float ScaleOffset = 0f;
        [Range(0.001f, 2f)]
        public float ScaleMultiplier = 1f;
        [Range(0.001f, 2f)]
        public float PositionMultiplier = 1f;

        public float DistributionOffsetPerObject
        {
            get
            {
                return 1f / Objects.Count;
            }
        }

        public AnimationCurve ObjectScale = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public AnimationCurve ObjectPosition = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        public bool FlipRotation = false;

        public Vector3 RotationOffset = Vector3.zero;

        public Vector3 PositionOffset = Vector3.zero;

        public LineUtils.RotationTypeEnum RotationTypeOverride = LineUtils.RotationTypeEnum.None;

        public LineUtils.PointDistributionTypeEnum DistributionType = LineUtils.PointDistributionTypeEnum.None;

        [Header("Object Placement")]
        public LineUtils.StepModeEnum StepMode = LineUtils.StepModeEnum.Interpolated;

        // Convenience functions
        public float GetOffsetFromObjectIndex(int index, bool wrap = true)
        {
            if (Objects.Count == 0)
                return 0;

            if (wrap)
                index = WrapIndex(index, Objects.Count);
            else
                index = Mathf.Clamp(index, 0, Objects.Count - 1);

            return (1f / Objects.Count * (index + 1));
        }

        public int GetNextObjectIndex(int index, bool wrap = true)
        {
            if (Objects.Count == 0)
                return 0;

            index++;

            if (wrap)
                return WrapIndex(index, Objects.Count);
            else
                return Mathf.Clamp(index, 0, Objects.Count - 1);
        }

        public int GetPrevObjectIndex(int index, bool wrap = true)
        {
            if (Objects.Count == 0)
                return 0;

            index--;

            if (wrap)
                return WrapIndex(index, Objects.Count);
            else
                return Mathf.Clamp(index, 0, Objects.Count - 1);
        }

        public void Update()
        {
            UpdateCollection();
        }

        public void UpdateCollection()
        {
            if (source == null)
                source = gameObject.GetComponent<LineBase>();
            if (source == null)
                return;

            if (transformHelper == null)
            {
                transformHelper = transform.Find("TransformHelper");
                if (transformHelper == null)
                {
                    transformHelper = new GameObject("TransformHelper").transform;
                    transformHelper.parent = transform;
                }
            }

            switch (StepMode)
            {
                case LineUtils.StepModeEnum.FromSource:
                    break;

                case LineUtils.StepModeEnum.Interpolated:
                    for (int i = 0; i < Objects.Count; i++)
                    {
                        if (Objects[i] == null)
                            continue;

                        float normalizedDistance = Mathf.Repeat(((float)i / Objects.Count) + DistributionOffset, 1f);
                        Objects[i].position = source.GetPoint(normalizedDistance);
                        Objects[i].rotation = source.GetRotation(normalizedDistance, RotationTypeOverride);

                        transformHelper.localScale = Vector3.one;
                        transformHelper.position = Objects[i].position;
                        transformHelper.localRotation = Quaternion.identity;
                        Transform tempParent = Objects[i].parent;
                        Objects[i].parent = transformHelper;
                        transformHelper.localEulerAngles = RotationOffset;
                        Objects[i].parent = tempParent;
                        Objects[i].transform.localScale = Vector3.one * ObjectScale.Evaluate(Mathf.Repeat(ScaleOffset + normalizedDistance, 1f)) * ScaleMultiplier;
                        /*if (FlipRotation) {
                            Objects[i].forward = -Objects[i].forward;
                        }*/
                    }
                    break;
            }
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

            UpdateCollection();
        }
        #endif

        private static int WrapIndex(int index, int numObjects)
        {
            return ((index % numObjects) + numObjects) % numObjects;
        }

        [SerializeField]
        private LineBase source;

        private Transform transformHelper;
    }

    #if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof (LineObjectCollection))]
    public class LineObjectCollectionEditor : UnityEditor.Editor
    {
        public void OnSceneGUI()
        {
            LineObjectCollection loc = (LineObjectCollection)target;

            for (int i = 0; i < loc.Objects.Count; i++)
            {
                if (loc.Objects[i] != null)
                {
                    UnityEditor.Handles.Label(loc.Objects[i].position, "Index: "+ i.ToString("000") + "\nOffset: " + loc.GetOffsetFromObjectIndex(i).ToString("00.00"));
                }
            }
        }
    }
    #endif
}