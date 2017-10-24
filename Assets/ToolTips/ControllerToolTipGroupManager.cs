using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MRDL.ToolTips;
using HoloToolkit.Unity.InputModule;
using UnityEngine.XR.WSA.Input;

public class ControllerToolTipGroupManager : MonoBehaviour {

    public const int MaxToolTipsPerGroup = 20;
    public const int MaxToolTipGroups = 10;

    public enum PositionModeEnum
    {
        Automatic,  // 'Boquet' system
        Manual,     // Manually enter a local position
    }

    public enum DisplayModeEnum
    {
        On,
        Off,
        OnFocus,
        None
    }

    #region classes
    [Serializable]
    public struct ToolTipTemplate
    {
        // Has the developer entered a name for this tooltip?
        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Text);
            }
        }

        // Do we have all the components we need?
        public bool IsSetUp
        {
            get
            {
                return true;
            }
        }

        public string Text;
        public PositionModeEnum PositionMode;
        [Tooltip("Object that tooltip will attach to")]
        public Transform Anchor;
        public Transform Pivot;
        public float LineLength;
        public Vector3 PivotPoint;
        public Vector3 LocalPosition;
        public InteractionSourceHandedness Handedness;
        public MotionControllerInfo.ControllerElementEnum ControllerElement;
        //[NonSerialized]
        //[HideInInspector]
        public GameObject AssociatedGameObject;
        // TBD Profiles
    }

    [Serializable]
    public class ToolTipGroup
    {
        public string GroupName;
        public DisplayModeEnum GroupDisplayMode;
        public ToolTipTemplate[] ToolTips = new ToolTipTemplate[MaxToolTipsPerGroup];
        public int visibleToolTipsCount = 0;
        public GameObject groupPrefab;
    }
    #endregion


    public void NextGroup()
    {
        int newGroupIndex = groupIndex;

        newGroupIndex++;
        if (newGroupIndex >= groups.Count)
            newGroupIndex = groups.Count - 1;

        if (newGroupIndex != groupIndex)
        {
            groupIndex = newGroupIndex;

            RefreshCurrentGroup();

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
            RefreshCurrentGroup();

        }
    }

    public ToolTipGroup GetGroup(int index)
    {
        return groups[index];
    }

    public ToolTipGroup CurrentGroup
    {
        get
        {
            if (groupIndex >= groups.Count || groupIndex < 0)
                return null;

            return groups[groupIndex];
        }
    }


    public int NumGroups
    {
        get
        {
            return groups.Count;
        }
    }

    public int CurrentGroupIndex
    {
        get
        {
            return groupIndex;
        }
    }

    public void DeactivateCurrentGroup()
    {
        for (int i = 0; i < currentToolTips.Length; i++)
        {
            currentToolTips[i].gameObject.SetActive(false);
        }
    }

    #region editor tools
    //   #if UNITY_EDITOR

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

            RefreshCurrentGroup();

        }
    }

    public void GoToGroup(string groupName)
    {
        int newGroupIndex = groupIndex;
        for (int i = 0; i < groups.Count; i++)
        {
            // TODO enforce GroupName in editor
            if (groups[i].GroupName.ToLower() == groupName.ToLower())
            {
                newGroupIndex = i;
                break;
            }
        }

        if (newGroupIndex != groupIndex)
        {
            groupIndex = newGroupIndex;

            RefreshCurrentGroup();
        }
    }


    public void RefreshCurrentGroup()
    {
        if (CurrentGroup == null)
            return;

        if (!Application.isPlaying)
            return;

        for (int i = 0; i < CurrentGroup.ToolTips.Length; i++)
        {
            if (!CurrentGroup.ToolTips[i].IsEmpty)
            {
                if (!CurrentGroup.ToolTips[i].IsSetUp)
                {
                    Debug.LogError("Tool tip " + i.ToString() + " is not set up, skipping");
                    currentToolTips[i].gameObject.SetActive(false);
                }
                else
                {
                    currentToolTips[i].ToolTipText = CurrentGroup.ToolTips[i].Text;
                    currentToolTips[i].PivotPosition = CurrentGroup.ToolTips[i].PivotPoint;
                    currentToolTips[i].Anchor = CurrentGroup.ToolTips[i].Anchor.gameObject;

                }
            }
            else
            {
                // Place tooltip according to Position Mode
                // Make Active based on State Mode

            }
        }
    }

    public void AddGroup(string groupName)
    {
        ToolTipGroup group = new ToolTipGroup();
        group.GroupName = groupName;
        // Set the line length to 1 for reach template
        for (int i = 0; i < group.ToolTips.Length; i++)
        {
            group.ToolTips[i].LineLength = 1;
        }
        groups.Add(group);
    }

    public void RemoveGroup(int index)
    {
        if (index < groups.Count)
            groups.RemoveAt(index);
    }



    public void AddToolTip(int index)
    {
        CurrentGroup.visibleToolTipsCount++;
        if (CurrentGroup.visibleToolTipsCount == CurrentGroup.ToolTips.Length)
        {
            // Make array larger and transfer everything over to larger array
        }
        else
        {
            CurrentGroup.ToolTips[index] = new ToolTipTemplate();
        }

    }

    public void ActivateGroup(string groupName)
    {

    }

    public void DeactivateGroup(string groupName)
    {

    }

    public void ActivateGroup(int groupIndex)
    {
        for (int i = 0; i < groups[groupIndex].ToolTips.Length; i++)
        {

        }
    }

    public void DeactivateGroup(int groupIndex)
    {

    }

    //#endif
    #endregion




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


    bool On = true;
    private IEnumerator Start()
    {
        Debug.Log("START - On : " + On);
        while (!MotionControllerVisualizer.Instance.TryGetController(InteractionSourceHandedness.Left, out controller))
        {
            visible = false;
            // Hide all ToolTips
            if (On)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    for (int j = 0; j < groups[i].ToolTips.Length; j++)
                    {
                        if (!(groups[i].ToolTips[j].IsEmpty))
                        {
                            groups[i].ToolTips[j].AssociatedGameObject.SetActive(false);
                        }
                    }
                }
                On = false;
            }
            yield return null;
        }

        if (visible && !On)
        {
            Debug.Log("Is Visible and Off");
            for (int i = 0; i < groups.Count; i++)
            {
                for (int j = 0; j < groups[i].ToolTips.Length; j++)
                {
                    if (!(groups[i].ToolTips[j].IsEmpty))
                    {
                        groups[i].ToolTips[j].AssociatedGameObject.SetActive(true);
                        // Move Pivot under the anchor (is only needed if anchor is not at 0,0,0
                        // groups[i].ToolTips[j].Pivot.SetParent(groups[i].ToolTips[j].Anchor);
                        MotionControllerVisualizer.Instance.TryGetController(groups[i].ToolTips[j].Handedness, out controller);
                        Transform controllerElementTransform = controller.GetElement(groups[i].ToolTips[j].ControllerElement);
                        groups[i].ToolTips[j].AssociatedGameObject.transform.position = controllerElementTransform.position;

                    }
                }
            }
            On = true;
        }
        //// Parent the picker wheel under the element of choice
        //Transform elementTransform = controller.GetElement(element);
        //if (elementTransform == null)
        //{
        //    Debug.LogError("Element " + element.ToString() + " not found in controller, can't proceed.");
        //    gameObject.SetActive(false);
        //    yield break;
        //}

        //transform.parent = elementTransform;
        //transform.localPosition = Vector3.zero;
        //transform.localRotation = Quaternion.identity;

        // Subscribe to input now that we're parented under the controller
        //InteractionManager.InteractionSourceUpdated += InteractionSourceUpdated;
    }

    //private void InteractionSourceUpdated(InteractionSourceUpdatedEventArgs obj)
    //{
    //    if (obj.state.source.handedness == handedness)
    //    {
    //        if (obj.state.touchpadTouched)
    //        {
    //            Visible = true;
    //            SelectorPosition = obj.state.touchpadPosition;
    //        }
    //    }
    //}

    // Use this for initialization
    void Awake()
    {
        Debug.Log("START - On : " + On);

        for (int i = 0; i < groups.Count; i++)
        {
            for (int j = 0; j < groups[i].ToolTips.Length; j++)
            {
                if (!(groups[i].ToolTips[j].IsEmpty))
                {
                    GameObject toolTipGo = GameObject.Instantiate(groups[i].groupPrefab, transform) as GameObject;
                    // Immediately Deactivate if not in active group
                    ToolTip toolTip = toolTipGo.GetComponent<ToolTip>();
                    toolTip.ToolTipText = groups[i].ToolTips[j].Text;
                    toolTip.PivotPosition = groups[i].ToolTips[j].PivotPoint;
                    //toolTip.Anchor.transform.position = groups[i].ToolTips[j].Anchor.position;


                    groups[i].ToolTips[j].AssociatedGameObject = toolTipGo;
                }
            }
            if (groups[i].GroupDisplayMode == DisplayModeEnum.None)
                GroupDisplayNone(groups[i], i);
            //toolTip.GroupTipState = ToolTip.TipDisplayModeEnum.None;

            if (groups[i].GroupDisplayMode == DisplayModeEnum.On)
                GroupDisplayOn(groups[i], i);
            //toolTip.GroupTipState = ToolTip.TipDisplayModeEnum.On;

            if (groups[i].GroupDisplayMode == DisplayModeEnum.Off)
                GroupDisplayOff(groups[i], i);

            //toolTip.GroupTipState = ToolTip.TipDisplayModeEnum.Off;

            if (groups[i].GroupDisplayMode == DisplayModeEnum.OnFocus)
                GroupDisplayOnFocus(groups[i], i);

            //toolTip.GroupTipState = ToolTip.TipDisplayModeEnum.OnFocus;
        }
        this.enabled = true;
    }

    // Update is called once per frame
    void Update()
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

    public void GroupDisplayModeChanged(ToolTipGroup group, DisplayModeEnum X, int index)
    {
        Debug.Log("Got here");
        switch (X)
        {
            case DisplayModeEnum.None:
                GroupDisplayNone(group, index);

                break;
            case DisplayModeEnum.Off:
                GroupDisplayOff(group, index);

                break;
            case DisplayModeEnum.On:
                GroupDisplayOn(group, index);
                break;
            case DisplayModeEnum.OnFocus:
                GroupDisplayOnFocus(group, index);

                break;

        }
    }

    public void GroupDisplayOn(ToolTipGroup group, int index)
    {
        for (int i = 0; i < group.ToolTips.Length; i++)
        {
            if (group.ToolTips[i].AssociatedGameObject != null)
                group.ToolTips[i].AssociatedGameObject.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.On;

        }
    }

    public void GroupDisplayOff(ToolTipGroup group, int index)
    {
        for (int i = 0; i < group.ToolTips.Length; i++)
        {
            if (group.ToolTips[i].AssociatedGameObject != null)
                group.ToolTips[i].AssociatedGameObject.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.Off;

        }
    }

    public void GroupDisplayOnFocus(ToolTipGroup group, int index)
    {
        for (int i = 0; i < group.ToolTips.Length; i++)
        {
            if (group.ToolTips[i].AssociatedGameObject != null)
                group.ToolTips[i].AssociatedGameObject.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.OnFocus;

        }
    }

    public void GroupDisplayNone(ToolTipGroup group, int index)
    {
        for (int i = 0; i < group.ToolTips.Length; i++)
        {
            if (group.ToolTips[i].AssociatedGameObject != null)
                group.ToolTips[i].AssociatedGameObject.GetComponent<ToolTip>().GroupTipState = ToolTip.TipDisplayModeEnum.None;

        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.color = Color.cyan;

        Vector3 anchorPosition = Vector3.zero;
        Vector3 startPosition = Vector3.zero;
        Vector3 direction = Vector3.zero;
        Vector3 endPosition = Vector3.zero;

        if (CurrentGroup != null)
        {
            // Draw crude lines and cubes for each tool tip so we can preview what they'll be like
            foreach (ToolTipTemplate template in CurrentGroup.ToolTips)
            {
                if (!template.IsEmpty && template.IsSetUp)
                {
                    switch (template.PositionMode)
                    {
                        case PositionModeEnum.Automatic:
                            anchorPosition = Vector3.zero;
                            startPosition = Vector3.zero;
                            direction = (startPosition - anchorPosition).normalized;
                            endPosition = anchorPosition + (direction * template.LineLength * lineLength);
                            Gizmos.DrawLine(startPosition, endPosition);
                            // TODO - make the cube the size of the tooltip 
                            Gizmos.DrawCube(endPosition, new Vector3(0.025f, 0.025f, 0.025f));
                            break;

                        case PositionModeEnum.Manual:
                            startPosition = template.Anchor.position;
                            endPosition = transform.TransformPoint(template.LocalPosition);
                            Gizmos.DrawLine(startPosition, endPosition);
                            // TODO - make the cube the size of the tooltip
                            Gizmos.DrawCube(endPosition, Vector3.one);
                            break;
                    }
                }
            }
        }
    }

    [SerializeField]
    private bool visible = false;

    private float lastTimeVisible;
    private bool visibleLastFrame = false;

    [SerializeField]
    private MotionControllerInfo.ControllerElementEnum element = MotionControllerInfo.ControllerElementEnum.Touchpad;
    private MotionControllerInfo controller;

    private Vector2 selectorPosition;

    // The prefab we'll use to create tooltips
    [SerializeField]
    private GameObject toolTipPrefab;

    public GameObject ToolTipPrefab
    {
        set
        {
            toolTipPrefab = value;
        }
        get
        {
            return toolTipPrefab;
        }
    }

    private GameObject[] prefabList;

    // The length of the lines attached to tooltips
    // This will be multiplied by the template's line length
    [SerializeField]
    private float lineLength = 1f;

    // If position mode is set to automatic
    // his will be used as the point from which tooltips will spread outward
    [SerializeField]
    private Transform OriginTransform;

    [SerializeField]
    private int groupIndex = 0;

    // The group - keeping this 
    [SerializeField]
    private List<ToolTipGroup> groups = new List<ToolTipGroup>();

    public List<ToolTipGroup> GetGroups
    {
        get
        {
            return groups;
        }
    }

    // The current set of tooltips
    private ToolTip[] currentToolTips = new ToolTip[MaxToolTipsPerGroup];
}

