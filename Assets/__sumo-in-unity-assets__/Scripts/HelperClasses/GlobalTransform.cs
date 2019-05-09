#if UNITY_EDITOR
using UnityEngine;

// Shows the global transform instead of the local
public class GlobalTransform : MonoBehaviour
{

    [SerializeField] private Vector3 position;
    [SerializeField] private Vector3 rotation;
    [SerializeField] private Vector3 scale;

    private Transform _transform;

    private void OnEnable()
    {
        _transform = transform;
    }

    // Update is called once per frame
    private void OnDrawGizmosSelected()
    {
        var t = transform;
        position = t.position;
        rotation = t.rotation.eulerAngles;
        scale = t.lossyScale;
    }

}
#endif
