using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleCarController : MonoBehaviour {
    public WheelCollider frontRightWheel, frontLeftWheel;
    public WheelCollider rearRightWheel, rearLeftWheel;
    public Transform frontRightTransform, frontLeftTransform;
    public Transform rearRightTransform, rearLeftTransform;
    public Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }   

    public void GetInput()
    {
        m_horizontalInput = Input.GetAxis("Horizontal");
        m_verticalInput = Input.GetAxis("Vertical");

    }

    private void Steer()
    {
        m_steerAngle = maxSteerAngle * m_horizontalInput;
        frontLeftWheel.steerAngle = m_steerAngle;
        frontRightWheel.steerAngle = m_steerAngle;
        
    }
    private void Update()
    {
        
        Debug.Log(rb.angularVelocity);
        Debug.Log(rb.velocity.magnitude);
    }

    private void Accelerate()
    {
        frontRightWheel.motorTorque = m_verticalInput * motorForce;
        frontLeftWheel.motorTorque = m_verticalInput * motorForce;
    }

    private void UpdateWheelPoses()
    {
        UpdateWheelPose(frontRightWheel, frontRightTransform);
        UpdateWheelPose(frontLeftWheel, frontLeftTransform);
        UpdateWheelPose(rearRightWheel, rearRightTransform);
        UpdateWheelPose(rearLeftWheel, rearLeftTransform);
    }

    private void UpdateWheelPose(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;// = wheelTransform.position;
        Quaternion quat;// = wheelTransform.rotation;

        wheelCollider.GetWorldPose(out pos, out quat);
        wheelTransform.transform.position = pos;
        wheelTransform.transform.rotation = quat;
    }

    private void FixedUpdate()
    {

        GetInput();
        Steer();
        Accelerate();
        UpdateWheelPoses();
    }

    private float m_horizontalInput;
    private float m_verticalInput;
    private float m_steerAngle;
    public float maxSteerAngle = 30f;
    public float motorForce = 50f;

}
