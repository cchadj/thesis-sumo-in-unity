using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class DataDisplay : MonoBehaviour
{
    [SerializeField]
    private Canvas m_displayCanvas;
    [Tooltip("Used to retrieve selected Vehicle."), SerializeField]
    private CurrentlySelectedTargets _currentlySelectedTargets;

    protected Canvas M_DisplayCanvas
    {
        get
        {
            return m_displayCanvas;
        }
        set
        {
            m_displayCanvas = value;
        }
    }
    public CurrentlySelectedTargets CurrentlySelectedTargets
    {
        get
        {
            return _currentlySelectedTargets;
        }
        set
        {
            _currentlySelectedTargets = value;
        }
    }

    protected virtual void Awake()
    {
        M_DisplayCanvas.enabled = false;
    }

    protected virtual void OnDisable()
    {
        if (M_DisplayCanvas)
            M_DisplayCanvas.enabled = false;
    }

    protected virtual void OnEnable()
    {
        if (M_DisplayCanvas)
            M_DisplayCanvas.enabled = true;
    }

}
