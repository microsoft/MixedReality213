using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRDL.ToolTips
{
    // TipState - Set locally
    // GroupState - Set by GroupManager (class available TBD)
    // Global State - Set by MasterManager (class available TBD)
    [Serializable]
    public enum TipDisplayModeEnum
    {
        /// <summary>
        /// No state to have from Manager
        /// </summary>
        None,
        /// <summary>
        /// Tips are always on
        /// </summary>
        On,
        /// <summary>
        /// Looking at Object Activates tip (Object must be interactive)
        /// </summary>
        OnFocus,
        /// <summary>
        /// Tips are always off
        /// </summary>
        Off
    }

    [Serializable]
    public enum TipPresetEnum
    {
        Default,
        ManualDirection,
        ManualPosition,
        Advanced,
    }

    /// <summary>
    /// Base class for a tooltip group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TipGroup<T> where T : TipTemplate, new()
    {
        public string Name = "New Group";
        public GameObject TipPrefab;
        public List<T> Templates = new List<T>();
        public TipDisplayModeEnum DisplayMode = TipDisplayModeEnum.Off;
        public Color EditorColor = Color.cyan;
    }

    [Serializable]
    public class TipTemplate : TipSpawnSettings
    {
        [Header("Presets")]
        public TipPresetEnum Preset = TipPresetEnum.Default;

        /// <summary>
        /// Uses a preset value to update settings
        /// </summary>
        /// <param name="preset"></param>
        /// <param name="settings"></param>
        public virtual void ApplyPreset()
        {
            switch (Preset)
            {
                case TipPresetEnum.Default:
                    ContentBillboardType = TipContentBillboardTypeEnum.ToCameraY;
                    FollowType = TipFollowTypeEnum.PositionAndRotation;
                    PivotDirectionOrient = TipOrientTypeEnum.OrientToObject;
                    PivotMode = TipPivotModeEnum.Automatic;
                    VanishBehavior = TipVanishBehaviorEnum.Manual;
                    AppearBehavior = TipAppearBehaviorEnum.Manual;
                    RemainBehavior = TipRemainBehaviorEnum.Indefinite;
                    break;

                case TipPresetEnum.ManualDirection:
                    ContentBillboardType = TipContentBillboardTypeEnum.ToCameraY;
                    FollowType = TipFollowTypeEnum.PositionAndRotation;
                    PivotDirectionOrient = TipOrientTypeEnum.OrientToObject;
                    PivotMode = TipPivotModeEnum.ManualDirection;
                    VanishBehavior = TipVanishBehaviorEnum.Manual;
                    AppearBehavior = TipAppearBehaviorEnum.Manual;
                    RemainBehavior = TipRemainBehaviorEnum.Indefinite;
                    break;

                case TipPresetEnum.ManualPosition:
                    ContentBillboardType = TipContentBillboardTypeEnum.ToCameraY;
                    FollowType = TipFollowTypeEnum.PositionAndRotation;
                    PivotDirectionOrient = TipOrientTypeEnum.OrientToObject;
                    PivotMode = TipPivotModeEnum.ManualPosition;
                    VanishBehavior = TipVanishBehaviorEnum.Manual;
                    AppearBehavior = TipAppearBehaviorEnum.Manual;
                    RemainBehavior = TipRemainBehaviorEnum.Indefinite;
                    break;

                case TipPresetEnum.Advanced:
                    // Nothing, let dev set things manually
                    break;
            }
        }
    }

    /// <summary>
    /// Base class for a tooltip group manager
    /// Handles the generation and set-up of tooltips as well as visibility for all groups
    /// </summary>
    /// <typeparam name="G"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract class TipGroupManager<G, T> : MonoBehaviour where G : TipGroup<T>, new() where T : TipTemplate, new()
    {
        /*
         * A NOTE ON SERIALIZING GENERICS:
         * To ensure List<G> groups is serialized, you must use a class that inherits from TipGroup<T>
         * 
         * The following will be serialized:
         * public class TipGroupConcrete : TipGroup<T> { }
         * public class TipGroupManager<TipGroupConcrete, TipTemplateBase>
         * {
         *      protected List<TipGroupConcrete> groups = new List<TipGroupConcrete>(); <-- WILL BE SERIALIZED
         * }
         * 
         * The following will NOT be serialized
         * public class TipGroupManager<TipGroup<TipTemplateBase>, TipTemplateBase>
         * {
         *      protected List<TipGroup<TipTemplateBase>> groups = new List<TipGroup<TipTemplateBase>>(); <-- WILL NOT BE SERIALIZED
         * }
         */

        // The groups
        public List<G> Groups
        {
            get
            {
                return groups;
            }
        }

        public int CurrentGroup { get { return currentGroup; } }

        [SerializeField]
        protected List<G> groups = new List<G>();

        // The length of the lines attached to tooltips
        // This will be multiplied by the template's line length
        [SerializeField]
        private float lineLength = 1f;

        [SerializeField]
        private int currentGroup = 0;

        public void NextGroup()
        {
            int newGroupIndex = currentGroup;

            newGroupIndex++;
            if (newGroupIndex >= groups.Count)
                newGroupIndex = groups.Count - 1;

            if (newGroupIndex != currentGroup)
            {
                currentGroup = newGroupIndex;
            }

            GoToGroup(currentGroup);
        }

        public void PrevGroup()
        {
            int newGroupIndex = currentGroup;

            newGroupIndex--;
            if (newGroupIndex < 0)
                newGroupIndex = 0;

            if (newGroupIndex != currentGroup)
            {
                currentGroup = newGroupIndex;
            }

            GoToGroup(currentGroup);
        }

        public void GoToGroup(int newGroupIndex, bool exclusive = true)
        {
            if (newGroupIndex >= groups.Count)
                newGroupIndex = groups.Count - 1;

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
                for (int i = 0; i < groups.Count; i++)
                {
                    if (i == currentGroup)
                    {
                        SetGroupDisplayMode(groups[i], TipDisplayModeEnum.On);
                    }
                    else
                    {
                        SetGroupDisplayMode(groups[i], TipDisplayModeEnum.Off);
                    }
                }
            }
            else
            {
                SetGroupDisplayMode(groups[currentGroup], TipDisplayModeEnum.On);
            }
        }

        public void GoToGroup(string groupName)
        {
            int newGroupIndex = currentGroup;
            for (int i = 0; i < groups.Count; i++)
            {
                // TODO enforce GroupName in editor
                if (groups[i].Name.ToLower() == groupName.ToLower())
                {
                    newGroupIndex = i;
                    break;
                }
            }

            if (newGroupIndex != currentGroup)
            {
                currentGroup = newGroupIndex;
            }
        }

        protected virtual void Awake()
        {
            CreateToolTips();
        }

        /// <summary>
        /// Creates a tooltip from a template and a group
        /// </summary>
        /// <param name="template"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        protected virtual ToolTip CreateToolTip(T template, G group)
        {
            /// You can override this function to make use of additional information provided by your template and group classes
            /// In your override function, call base.CreateToolTip(template, group) to do basic setup

            // Parent the new tooltip under this transform by default
            GameObject toolTipGo = GameObject.Instantiate(group.TipPrefab, transform) as GameObject;
            ToolTip toolTip = toolTipGo.GetComponent<ToolTip>();
            template.InstantiatedToolTip = toolTip;
            toolTip.ToolTipText = template.Text;

            // Get or attach a ToolTipSpawner
            ToolTipSpawner spawner = toolTip.GetComponent<ToolTipSpawner>();
            if (spawner == null)
                spawner = toolTip.gameObject.AddComponent<ToolTipSpawner>();

            // Copy all the connector fields - template inherits from connector settings so this is a single assignment
            spawner.Settings = template;

            if (group.DisplayMode == TipDisplayModeEnum.On)
            {
                // Show tool tip immediately
                spawner.ShowToolTip();
            }
            else
            {
                // Hide tool tip immediately
                spawner.HideToolTip();
            }

            return toolTip;
        }

        /// <summary>
        /// Creates all tool tips
        /// </summary>
        protected void CreateToolTips()
        {
            // Create all of our tool tips in one go
            // They will be automatically hidden
            foreach (G group in groups)
            {
                if (group.TipPrefab == null)
                {
                    Debug.LogError("ToolTip prefab was null in group " + group.Name + ", skipping");
                    continue;
                }

                foreach (T template in group.Templates)
                {
                    if (template.IsEmpty)
                    {
                        Debug.LogWarning("Empty tool tip found in group " + group.Name);
                    }
                    else
                    {   
                        CreateToolTip(template, group);
                    }
                }
            }
        }

        protected void Update()
        {
            //Debug.Log("Updating");
            //if (CurrentGroup.GroupDisplayMode == DisplayModeEnum.On)
            //{
            //    GroupDisplayOn();
            //}
            //if (CurrentGroup.GroupDisplayMode == DisplayModeEnum.Off)
            //{
            //    Debug.Log("OGG");
            //    GroupDisplayOff();
            //}
            //if (CurrentGroup.GroupDisplayMode == DisplayModeEnum.None)
            //{
            //    GroupDisplayNone();
            //}
        }

        private void SetGroupDisplayMode(G group, TipDisplayModeEnum displayMode)
        {
            group.DisplayMode = displayMode;

            if (Application.isPlaying)
            {
                foreach (T template in group.Templates)
                {
                    if (template.IsInstantiated)
                        template.InstantiatedToolTip.GroupTipState = displayMode;
                }
            }
        }
    }
}