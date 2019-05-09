using System;
using System.Collections.Generic;

using UnityEngine;

using cakeslice; /* For the outline effect */


[RequireComponent(typeof(Collider))] /* Collider for Mouse detection */
public class SelectableObjectEvent : MonoBehaviour
{
    [SerializeField, Tooltip("optional")] private MeshRenderer _onHoverRenderer;
    [SerializeField, Tooltip("optional")] private Outline _onHoverOutline;
    [SerializeField, Tooltip("Update selected targets when this game object is selected")]
    private CurrentlySelectedTargets _currentlySelectedTargets;
    [SerializeField, Tooltip("The transform selected when this gameObject is selected")]
    private Transform m_transform;

    private ISelectableTraciVariable _selectableTraciVariable;
    public Transform Transform { get => m_transform; set => m_transform = value; }
    public MeshRenderer OnHoverRenderer { get => _onHoverRenderer; set => _onHoverRenderer = value; }
    /// <summary>
    /// Used to check if there is already a selected target. When a target is already selected
    /// then this object shouldn't be highlited.
    /// </summary>
    public CurrentlySelectedTargets SelectedTargets { get => _currentlySelectedTargets; set => _currentlySelectedTargets = value; }
    public Outline OnHoverOutline { get => _onHoverOutline; set => _onHoverOutline = value; }
    /// <summary> I can not use GetComponent with generics so I use this to get TraCIVariableData </summary>
    public ISelectableTraciVariable SelectableTraciVariable { get => _selectableTraciVariable; private set => _selectableTraciVariable = value; }

    void Awake()
    {
        if (Transform == null)
            Transform = gameObject.transform;

        /* I can not assign interface through editor so I search it in this object or its parrent */

        if(SelectableTraciVariable == null)
            SelectableTraciVariable = GetComponent<ISelectableTraciVariable>();

        if (SelectableTraciVariable == null)
            SelectableTraciVariable = transform.parent.GetComponent<ISelectableTraciVariable>();

        if(SelectableTraciVariable == null)
            throw new Exception($"Vehicle with name {transform.parent.name} does not have have a selectable Traci Variable assigned");

    }
    /// <summary>
    /// When cursor enters the object then its this OnHoverRenderer is enabled unless there is 
    /// an object already selected.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnMouseEnter()
    {
        /* Object should be highlighted only if there isn't another object selected */
        if(!SelectedTargets.IsATargetAlreadySelected())
        {
            if (OnHoverRenderer)
                OnHoverRenderer.enabled = true;
            if (OnHoverOutline)
                OnHoverOutline.enabled = true;
        }
    }


    public void OnMouseExit()
    {
        if (OnHoverRenderer)
            OnHoverRenderer.enabled = false;
        if (OnHoverOutline)
            OnHoverOutline.enabled = false;
    }

    public void OnMouseDown()
    {
        if (SelectedTargets.IsATargetAlreadySelected())
            return;

        if (OnHoverRenderer)
            OnHoverRenderer.enabled = false;
        if (OnHoverOutline)
            OnHoverOutline.enabled = false;

        /* FIRST configure selected object */
        SelectedTargets.Select(Transform, SelectableTraciVariable);
    }
}
