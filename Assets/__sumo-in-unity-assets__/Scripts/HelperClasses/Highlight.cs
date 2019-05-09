using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Highlight : MonoBehaviour
{
    private Color startcolor;

    [SerializeField]
    private MeshRenderer _rendererToHighlightOnMouseEnter;
    public MeshRenderer RendererToHighlightOnMouseEnter
    {
        get
        {
            return _rendererToHighlightOnMouseEnter;
        }
        set
        {
            _rendererToHighlightOnMouseEnter = value;
        }
    }

    void OnMouseEnter()
    {
        startcolor = _rendererToHighlightOnMouseEnter.material.color;
        _rendererToHighlightOnMouseEnter.material.color = Color.yellow;
    }
    void OnMouseExit()
    {
        _rendererToHighlightOnMouseEnter.material.color = startcolor;
    }
}
