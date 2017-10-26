//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System;
using UnityEngine;

namespace MRDL.ToolTips
{
    public enum TipFollowTypeEnum
    {
        AnchorOnly,             // The anchor will follow the target - pivot remains unaffected
        PositionOnly,           // Anchor and pivot will follow target position, but not rotation
        PositionAndYRotation,   // Anchor and pivot will follow target like it's parented, but only on Y axis
        PositionAndRotation,    // Anchor and pivot will follow target like it's parented
    }

    public enum TipOrientTypeEnum
    {
        OrientToObject,     // Tooltip will maintain anchor-pivot relationship relative to target object
        OrientToCamera,     // Tooltip will maintain anchor-pivot relationship relative to camera
    }

    public enum TipPivotModeEnum
    {
        ManualPosition,     // Tooltip pivot will be set manually
        ManualDirection,    // Tooltip pivot direction will be set manually
        Automatic,          // Tooltip pivot will be set relative to object/camera based on specified direction and line length
    }

    public enum TipPivotDirectionEnum
    {
        Manual,         // Direction will be specified manually
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest,
        InFront,
    }

    public enum TipContentBillboardTypeEnum
    {
        None,
        ToCameraY,
        ToCameraFull,
    }

    [Serializable]
    public class TipConnectionSettings
    {
        [Header("Automatic placement settings")]
        public TipFollowTypeEnum FollowType = TipFollowTypeEnum.AnchorOnly;
        public TipPivotModeEnum PivotMode = TipPivotModeEnum.ManualPosition;
        public TipPivotDirectionEnum PivotDirection = TipPivotDirectionEnum.North;
        public TipOrientTypeEnum PivotDirectionOrient = TipOrientTypeEnum.OrientToObject;
        public TipContentBillboardTypeEnum ContentBillboardType = TipContentBillboardTypeEnum.None;
        [Header("Manual placement settings")]
        public Vector3 ManualPivotDirection = Vector3.up;
        public Vector3 ManualPivotLocalPosition = Vector3.up;
        [Header("Shared settings")]
        public float PivotDistance = 0.25f;
        public GameObject Target;
    }

    /// <summary>
    /// Connects a ToolTip to a target
    /// Maintains that connection even if the target moves
    /// </summary>
    public class ToolTipConnector : MonoBehaviour
    {
        public TipConnectionSettings Settings;

        private Vector3 toolTipPos;
        private Vector3 toolTipRot;
        private Vector3 anchorPos;
        private Vector3 pivotPos;
        private Vector3 contentRot;

        private void OnEnable()
        {
            if (!FindToolTip())
                return;

            Settings.ManualPivotLocalPosition = transform.InverseTransformPoint (toolTip.PivotPosition);
        }

        private bool FindToolTip()
        {
            if (toolTip == null)
            {
                toolTip = GetComponent<ToolTip>();
            }
            if (toolTip == null)
            {
                return false;
            }

            return true;
        }

        private void UpdatePosition() {

            if (!FindToolTip())
                return;

            toolTipPos = toolTip.transform.position;
            toolTipRot = toolTip.transform.eulerAngles;
            anchorPos = toolTip.AnchorPosition;
            pivotPos = toolTip.PivotPosition;
            contentRot = toolTip.ContentParentTransform.eulerAngles;

            GetTransformationsFromSettings(Settings, ref toolTipPos, ref toolTipRot, ref anchorPos, ref pivotPos, ref contentRot);

            toolTip.transform.position = toolTipPos;
            toolTip.transform.eulerAngles = toolTipRot;
            toolTip.Anchor.transform.position = anchorPos;
            toolTip.PivotPosition = pivotPos;
            toolTip.ContentParentTransform.eulerAngles = contentRot;
        }

        private void Update()
        {
            UpdatePosition();
        }

        [SerializeField]
        private ToolTip toolTip;

        private void OnDrawGizmos ()
        {
            if (Application.isPlaying)
                return;
            
            UpdatePosition();
        }

        /// <summary>
        /// Uses pivot direction enum to determine pivot direction in world space
        /// </summary>
        /// <param name="pivotDirection"></param>
        /// <param name="manualPivotDirection"></param>
        /// <param name="relativeTo"></param>
        /// <returns></returns>
        public static Vector3 GetDirectionFromPivotDirection (TipPivotDirectionEnum pivotDirection, Vector3 manualPivotDirection, Transform relativeTo)
        {
            Vector3 dir = Vector3.zero;
            switch (pivotDirection)
            {
                case TipPivotDirectionEnum.North:
                    dir = Vector3.up;
                    break;

                case TipPivotDirectionEnum.NorthEast:
                    dir = Vector3.Lerp(Vector3.up, Vector3.right, 0.5f).normalized;
                    break;

                case TipPivotDirectionEnum.East:
                    dir = Vector3.right;
                    break;

                case TipPivotDirectionEnum.SouthEast:
                    dir = Vector3.Lerp(Vector3.down, Vector3.right, 0.5f).normalized;
                    break;

                case TipPivotDirectionEnum.South:
                    dir = Vector3.down;
                    break;

                case TipPivotDirectionEnum.SouthWest:
                    dir = Vector3.Lerp(Vector3.down, Vector3.left, 0.5f).normalized;
                    break;

                case TipPivotDirectionEnum.West:
                    dir = Vector3.left;
                    break;

                case TipPivotDirectionEnum.NorthWest:
                    dir = Vector3.Lerp(Vector3.up, Vector3.left, 0.5f).normalized;
                    break;

                case TipPivotDirectionEnum.InFront:
                    dir = Vector3.forward;
                    break;

                case TipPivotDirectionEnum.Manual:
                    dir = manualPivotDirection.normalized;
                    break;
            }

            return relativeTo.TransformDirection(dir);
        }
        
        /// <summary>
        /// Uses tip connection settings to calculate the transformations of ToolTip elements
        /// target is a dummy value that is overridden if the Target in settings is not null
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="toolTipPos"></param>
        /// <param name="toolTipRot"></param>
        /// <param name="anchorPos"></param>
        /// <param name="pivotPos"></param>
        /// <param name="contentRot"></param>
        /// <param name="cam"></param>
        /// <param name="target"></param>
        public static void GetTransformationsFromSettings (
            TipConnectionSettings settings, 
            ref Vector3 toolTipPos, 
            ref Vector3 toolTipRot, 
            ref Vector3 anchorPos, 
            ref Vector3 pivotPos, 
            ref Vector3 contentRot, 
            Camera cam = null, 
            Transform target = null)
        {
            if (settings == null)
            {
                Debug.LogError("Settings cannot be null.");
                return;
            }

            if (target == null && settings.Target == null)
            {
                // One or the other must be set to get transformations
                return;
            }
            
            // Settings target overrides specified target by default
            target = (settings.Target != null) ? settings.Target.transform : target;
            Vector3 targetPos = target.transform.position;
            Vector3 targetRot = target.transform.eulerAngles;
            anchorPos = targetPos;

            switch (settings.ContentBillboardType)
            {
                case TipContentBillboardTypeEnum.None:
                    break;

                case TipContentBillboardTypeEnum.ToCameraY:
                    Vector3 cameraEulerAngles = (cam != null) ? cam.transform.eulerAngles : Camera.current.transform.eulerAngles;
                    contentRot.y = cameraEulerAngles.y;
                    break;

                case TipContentBillboardTypeEnum.ToCameraFull:
                    contentRot = (cam != null) ? cam.transform.eulerAngles : Camera.current.transform.eulerAngles;
                    break;
            }

            switch (settings.FollowType)
            {
                case TipFollowTypeEnum.AnchorOnly:
                default:
                    // Set the position of the anchor to the target's position
                    // And do nothing else
                    anchorPos = targetPos;
                    break;

                case TipFollowTypeEnum.PositionOnly:
                    // Move the entire tooltip transform while maintaining the anchor position offset
                    toolTipPos = targetPos;
                    switch (settings.PivotMode)
                    {
                        case TipPivotModeEnum.Automatic:
                            Transform relativeTo = null;
                            switch (settings.PivotDirectionOrient)
                            {
                                case TipOrientTypeEnum.OrientToCamera:
                                    relativeTo = (cam != null) ? cam.transform : Camera.current.transform;
                                    break;

                                case TipOrientTypeEnum.OrientToObject:
                                    relativeTo = target;
                                    break;
                            }
                            pivotPos = targetPos + GetDirectionFromPivotDirection(
                                settings.PivotDirection,
                                settings.ManualPivotDirection,
                                relativeTo) * settings.PivotDistance;
                            break;

                        case TipPivotModeEnum.ManualPosition:
                            pivotPos = target.TransformPoint(settings.ManualPivotLocalPosition);
                            break;

                        case TipPivotModeEnum.ManualDirection:
                            pivotPos = toolTipPos + (target.TransformDirection(settings.ManualPivotDirection) * settings.PivotDistance);
                            break;
                    }
                    break;

                case TipFollowTypeEnum.PositionAndYRotation:
                    // Set the transform of the entire tool tip
                    // Set the pivot relative to target/camera
                    toolTipPos = targetPos;
                    Vector3 eulerAngles = targetRot;
                    eulerAngles.x = 0f;
                    eulerAngles.z = 0f;
                    toolTipRot = eulerAngles;
                    switch (settings.PivotMode)
                    {
                        case TipPivotModeEnum.Automatic:
                            Transform relativeTo = null;
                            switch (settings.PivotDirectionOrient)
                            {
                                case TipOrientTypeEnum.OrientToCamera:
                                    relativeTo = (cam != null) ? cam.transform : Camera.current.transform;
                                    break;

                                case TipOrientTypeEnum.OrientToObject:
                                    relativeTo = target;
                                    break;
                            }
                            Vector3 localPosition = GetDirectionFromPivotDirection(settings.PivotDirection, settings.ManualPivotDirection, relativeTo) * settings.PivotDistance;
                            pivotPos = targetPos + localPosition;
                            break;

                        case TipPivotModeEnum.ManualPosition:
                            pivotPos = target.TransformPoint(settings.ManualPivotLocalPosition);
                            break;

                        case TipPivotModeEnum.ManualDirection:
                            pivotPos = toolTipPos + (target.TransformDirection(settings.ManualPivotDirection) * settings.PivotDistance);
                            break;
                    }
                    break;

                case TipFollowTypeEnum.PositionAndRotation:
                    // Set the transform of the entire tool tip
                    // Set the pivot relative to target/camera
                    toolTipPos = targetPos;
                    toolTipRot = targetRot;
                    switch (settings.PivotMode)
                    {
                        case TipPivotModeEnum.Automatic:
                            Transform relativeTo = null;
                            switch (settings.PivotDirectionOrient)
                            {
                                case TipOrientTypeEnum.OrientToCamera:
                                    relativeTo = (cam != null) ? cam.transform : Camera.current.transform;
                                    break;

                                case TipOrientTypeEnum.OrientToObject:
                                    relativeTo = target;
                                    break;
                            }
                            pivotPos = targetPos + GetDirectionFromPivotDirection(
                                settings.PivotDirection,
                                settings.ManualPivotDirection,
                                relativeTo) * settings.PivotDistance;
                            break;

                        case TipPivotModeEnum.ManualPosition:
                            pivotPos = target.TransformPoint (settings.ManualPivotLocalPosition);
                            break;

                        case TipPivotModeEnum.ManualDirection:
                            pivotPos = toolTipPos + (target.TransformDirection(settings.ManualPivotDirection) * settings.PivotDistance);
                            break;
                    }
                    break;
            }
        }
    }
}