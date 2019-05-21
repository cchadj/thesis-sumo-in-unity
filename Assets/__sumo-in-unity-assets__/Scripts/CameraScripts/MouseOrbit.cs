using UnityEngine;
using System.Collections;
using CodingConnected.TraCI.NET.Types;
using Zenject;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbit : MonoBehaviour
{
    [SerializeField, ReadOnly] private Transform _target;

    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    private Rigidbody _rigidBody;

    float x = 0.0f;
    float y = 0.0f;

    private CurrentlySelectedTargets _selectedTargets;

    [Inject]
    private void Construct(CurrentlySelectedTargets selectedTargets)
    {
        _selectedTargets = selectedTargets;
    }
    
    // Use this for initialization
    private void Awake()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        
        _rigidBody =  GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (_rigidBody != null)
        {
            _rigidBody.freezeRotation = true;
        }
        
        _selectedTargets.VehicleSelected += SelectedTargetsVehicleSelected;
    }

    private void SelectedTargetsVehicleSelected(object sender, SelectedVehicleEventArgs e)
    {
        _target = e.SelectedTransform;
        enabled = true;
    }
    
    

    void LateUpdate()
    {
        if (_target)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
                
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }


            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

           distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

            //RaycastHit hit;
            //if (Physics.Linecast(Target.position, transform.position, out hit))
            //{
            //    distance -= hit.distance;
            //}
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + _target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}