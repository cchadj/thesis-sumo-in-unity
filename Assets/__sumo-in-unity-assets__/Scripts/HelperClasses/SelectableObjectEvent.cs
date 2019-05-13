using System;
using System.Collections.Generic;

using UnityEngine;

using cakeslice;
using Zenject;

/* For the outline effect */


[RequireComponent(typeof(Collider))] /* Collider for Mouse detection */
public class SelectableObjectEvent : MonoBehaviour
{
    [SerializeField, Tooltip("optional")] private MeshRenderer _onHoverRenderer;


    public Transform _transform;
    public MeshRenderer OnHoverRenderer { get => _onHoverRenderer; set => _onHoverRenderer = value; }

    /// <summary>
    /// Used to check if there is already a selected target. When a target is already selected
    /// then this object shouldn't be highlited.
    /// </summary>
    private CurrentlySelectedTargets _selectedTargets;

    [field: SerializeField] [field: Tooltip("optional")] public Outline OnHoverOutline { get; set; }

    /// <summary> I can not use GetComponent with generics so I use this to get TraCIVariableData </summary>
    public ISelectableTraciVariable SelectableTraciVariable { get; private set; }


    [Inject]
    private void Construct(CurrentlySelectedTargets selectedTargets )
    {
        _selectedTargets = selectedTargets;
    }
    
    void Awake()
    {
        if (_transform == null)
            _transform = gameObject.transform;

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
        if(!_selectedTargets.IsATargetAlreadySelected())
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
        if (_selectedTargets.IsATargetAlreadySelected())
            return;

        if (OnHoverRenderer)
            OnHoverRenderer.enabled = false;
        if (OnHoverOutline)
            OnHoverOutline.enabled = false;

        /* FIRST configure selected object */
        _selectedTargets.Select(_transform, SelectableTraciVariable);
    }
}
