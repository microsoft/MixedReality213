using System;
using System.Collections.Generic;
using UnityEngine;

namespace MRDL.ToolTips
{
    [Serializable]
    public enum TipDisplayModeEnum
    {
        On,
        Off,
        OnFocus,
        None
    }

    /// <summary>
    /// Base class for a tooltip group
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class TipGroup<T> where T : TipSpawnSettings, new()
    {
        public string Name = "New Group";
        public GameObject TipPrefab;
        public List<T> Templates = new List<T>();
        public TipDisplayModeEnum DisplayMode = TipDisplayModeEnum.Off;
        public Color EditorColor = Color.cyan;
    }

    /// <summary>
    /// Base class for a tooltip group manager
    /// Handles the generation and set-up of tooltips as well as visibility for all groups
    /// </summary>
    /// <typeparam name="G"></typeparam>
    /// <typeparam name="T"></typeparam>
    public abstract class TipGroupManager<G, T> : MonoBehaviour where G : TipGroup<T>, new() where T : TipSpawnSettings, new()
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

        [SerializeField]
        protected List<G> groups = new List<G>();

        // The length of the lines attached to tooltips
        // This will be multiplied by the template's line length
        [SerializeField]
        private float lineLength = 1f;

        [SerializeField]
        private int groupIndex = 0;

        public void NextGroup()
        {
            int newGroupIndex = groupIndex;

            newGroupIndex++;
            if (newGroupIndex >= groups.Count)
                newGroupIndex = groups.Count - 1;

            if (newGroupIndex != groupIndex)
            {
                groupIndex = newGroupIndex;
            }
        }

        public void PrevGroup()
        {
            int newGroupIndex = groupIndex;

            newGroupIndex--;
            if (newGroupIndex < 0)
                newGroupIndex = 0;

            if (newGroupIndex != groupIndex)
            {
                groupIndex = newGroupIndex;
            }
        }

        public void GoToGroup(int newGroupIndex)
        {
            if (newGroupIndex >= groups.Count)
                newGroupIndex = groups.Count - 1;

            if (newGroupIndex < 0)
                newGroupIndex = 0;

            if (newGroupIndex != groupIndex)
            {
                // Deactivate Current Group
                // DeactivateCurrentGroup();
                groupIndex = newGroupIndex;
            }
        }

        public void GoToGroup(string groupName)
        {
            int newGroupIndex = groupIndex;
            for (int i = 0; i < groups.Count; i++)
            {
                // TODO enforce GroupName in editor
                if (groups[i].Name.ToLower() == groupName.ToLower())
                {
                    newGroupIndex = i;
                    break;
                }
            }

            if (newGroupIndex != groupIndex)
            {
                groupIndex = newGroupIndex;
            }
        }

        protected G CurrentGroup
        {
            get
            {
                if (groups == null || groups.Count == 0)
                    return null;

                return groups[groupIndex];
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
            toolTip.ToolTipText = template.Text;
            // Get or attach a ToolTipConnector 
            ToolTipConnector connector = toolTip.GetComponent<ToolTipConnector>();
            if (connector == null)
                connector = toolTip.gameObject.AddComponent<ToolTipConnector>();

            // Copy all the connector fields - template inherits from connector settings so this is a single assignment
            connector.Settings = template;
            return toolTip;
        }

        /// <summary>
        /// Creates all tool tips
        /// </summary>
        protected void CreateToolTips()
        {
            // Create all of our tool tips in one go
            // Hide all of them immediately
            foreach (G group in groups)
            {
                foreach (T template in group.Templates)
                {
                    if (!template.IsEmpty)
                    {
                        template.InstantiatedToolTip = CreateToolTip(template, group);
                        template.InstantiatedToolTip.gameObject.SetActive(false);
                    }
                    else
                    {
                        Debug.LogWarning("Empty tool tip found in group " + group.Name);
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

        protected void GroupDisplayModeChanged(G group, TipDisplayModeEnum newDisplayMode, int index)
        {
            Debug.Log("Got here");
            switch (newDisplayMode)
            {
                case TipDisplayModeEnum.None:
                    GroupDisplayNone(group, index);

                    break;
                case TipDisplayModeEnum.Off:
                    GroupDisplayOff(group, index);

                    break;
                case TipDisplayModeEnum.On:
                    GroupDisplayOn(group, index);
                    break;
                case TipDisplayModeEnum.OnFocus:
                    GroupDisplayOnFocus(group, index);

                    break;

            }
        }

        protected void GroupDisplayOn(G group, int index)
        {
            for (int i = 0; i < group.Templates.Count; i++)
            {
                if (group.Templates[i].InstantiatedToolTip != null)
                    group.Templates[i].InstantiatedToolTip.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.On;

            }
        }

        protected void GroupDisplayOff(G group, int index)
        {
            for (int i = 0; i < group.Templates.Count; i++)
            {
                if (group.Templates[i].InstantiatedToolTip != null)
                    group.Templates[i].InstantiatedToolTip.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.Off;

            }
        }

        protected void GroupDisplayOnFocus(G group, int index)
        {
            for (int i = 0; i < group.Templates.Count; i++)
            {
                if (group.Templates[i].InstantiatedToolTip != null)
                    group.Templates[i].InstantiatedToolTip.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.OnFocus;

            }
        }

        protected void GroupDisplayNone(G group, int index)
        {
            for (int i = 0; i < group.Templates.Count; i++)
            {
                if (group.Templates[i].InstantiatedToolTip != null)
                    group.Templates[i].InstantiatedToolTip.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.None;

            }
        }
    }
}