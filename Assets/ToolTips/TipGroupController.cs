using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

namespace MRDL.ToolTips
{
    /// <summary>
    /// Controls tool tip manager display with controller input
    /// </summary>
    public class TipGroupController : MonoBehaviour
    {
        public int CurrentGroup { get { return currentGroup; } }

        [SerializeField]
        private int currentGroup = 0;

        /// <summary>
        /// Interface for the group manager
        /// We use an interface to ensure that our tip group controller will work with any implementation of the manger
        /// </summary>
        private ITipGroupSource groupManager;

        [SerializeField]
        private InteractionSourceHandedness handedness;
        [SerializeField]
        private InteractionSourcePressType toggleTipsPress = InteractionSourcePressType.Menu;

        [NonSerialized]
        private MotionControllerInfo controller;

        private bool groupsVisible = true;
        private bool changedGroup = false;
        
        private IEnumerator Start()
        {
            // Wait for our motion controller to appear
            while (!MotionControllerVisualizer.Instance.TryGetController(handedness, out controller))
            {
                yield return null;
            }

            // Subscribe to input now that we have the controller
            // InteractionSourcePressed for toggling
            InteractionManager.InteractionSourcePressed += InteractionSourcePressed;
            // InteractionSourceUpdated for next / prev tooltips
            InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;

            yield break;
        }

        private void InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness)
            {
                if (obj.state.thumbstickPosition.x < -0.5f)
                {
                    if (!changedGroup)
                    {
                        PrevGroup();
                        changedGroup = true;
                    }
                }
                else if (obj.state.thumbstickPosition.x > 0.5f)
                {
                    if (!changedGroup)
                    {
                        NextGroup();
                        changedGroup = true;
                    }
                }
                else
                {
                    changedGroup = false;
                }
            }
        }

        private void InteractionSourcePressed(InteractionSourcePressedEventArgs obj)
        {
            if (obj.state.source.handedness == handedness && obj.pressType == toggleTipsPress)
            {
                groupsVisible = !groupsVisible;
            }

            if (groupsVisible)
                ShowAllGroups();
            else
                HideAllGroups();
        }

        public void GoToGroup(int newGroupIndex, bool exclusive = true)
        {
            FindGroupManager();

            if (newGroupIndex >= groupManager.NumGroups)
                newGroupIndex = groupManager.NumGroups - 1;

            if (newGroupIndex < 0)
                newGroupIndex = 0;

            if (newGroupIndex != currentGroup)
            {
                // Deactivate Current Group
                // DeactivateCurrentGroup();
                currentGroup = newGroupIndex;
            }

            if (exclusive)
            {
                for (int i = 0; i < groupManager.NumGroups; i++)
                {
                    if (i == currentGroup)
                    {
                        groupManager.SetGroupDisplayMode(i, TipDisplayModeEnum.On);
                    }
                    else
                    {
                        groupManager.SetGroupDisplayMode(i, TipDisplayModeEnum.Off);
                    }
                }
            }
            else
            {
                groupManager.SetGroupDisplayMode(currentGroup, TipDisplayModeEnum.On);
            }
        }

        public void NextGroup()
        {
            FindGroupManager();

            // If the current group is hidden
            // turn that on instead of incrementing
            if (currentGroup == 0)
            {
                if (groupManager.GetGroupDisplayMode(currentGroup) != TipDisplayModeEnum.On)
                {
                    groupManager.SetGroupDisplayMode(currentGroup, TipDisplayModeEnum.On);
                    return;
                }
            }

            int newGroupIndex = currentGroup;

            newGroupIndex++;
            if (newGroupIndex >= groupManager.NumGroups)
            {
                // We've reached the end
                // Turn off all groups
                newGroupIndex = groupManager.NumGroups - 1;
                HideAllGroups();
                return;
            }

            if (newGroupIndex != currentGroup)
            {
                currentGroup = newGroupIndex;
            }

            GoToGroup(currentGroup);
        }

        public void PrevGroup()
        {
            FindGroupManager();

            // If the current group is hidden
            // turn that on instead of incrementing
            if (currentGroup == groupManager.NumGroups - 1)
            {
                if (groupManager.GetGroupDisplayMode(currentGroup) != TipDisplayModeEnum.On)
                {
                    groupManager.SetGroupDisplayMode(currentGroup, TipDisplayModeEnum.On);
                    return;
                }
            }

            int newGroupIndex = currentGroup;

            newGroupIndex--;
            if (newGroupIndex < 0)
            {
                // We've reached the beginning
                // Turn off all groups
                HideAllGroups();
                newGroupIndex = 0;
                return;
            }

            if (newGroupIndex != currentGroup)
            {
                currentGroup = newGroupIndex;
            }

            GoToGroup(currentGroup);
        }

        public void HideAllGroups()
        {
            for (int i = 0; i < groupManager.NumGroups; i++)
            {
                groupManager.SetGroupDisplayMode(i, TipDisplayModeEnum.Off);
            }
        }

        public void ShowAllGroups()
        {
            for (int i = 0; i < groupManager.NumGroups; i++)
            {
                groupManager.SetGroupDisplayMode(i, TipDisplayModeEnum.On);
            }
        }

        private void FindGroupManager()
        {
            if (groupManager == null)
                groupManager = (ITipGroupSource)gameObject.GetComponent(typeof(ITipGroupSource));
        }

    }
}