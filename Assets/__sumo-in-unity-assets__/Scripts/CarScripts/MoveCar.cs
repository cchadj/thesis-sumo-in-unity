//using CodingConnected.TraCI.NET.Helpers;
//using CodingConnected.TraCI.NET.Types;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;

//public class MoveCar : MonoBehaviour
//{
//    public static readonly float GROUND_OFFSET = 0.4f;

//    /// <summary>
//    /// The Vehicle that the gameObject will get the position and orientation from.
//    /// </summary>
//    public Vehicle Vehicle;

//    /// <summary>
//    /// Event will fire whenever a car reaches a target position.
//    /// The gameobject will be destroied if it's elligible for destruction.
//    /// </summary>
//    private CarReachedDestinationEvent reachedDestination;

//    public VehicleSimulator VehicleSimulator;
//    public ApplicationManager ApplicationManager;

//    /// <summary>
//    /// startPosition for each lerp.
//    /// </summary>
//    private Vector3 startingPosition;
//    /// <summary>
//    /// endPosition for each lerp.
//    /// </summary>
//    private Vector3 targetPosition;
//    /// <summary>
//    /// True when car reached targetPosition.
//    /// </summary>
//    private bool isPositionChanged;

//    /// <summary>
//    /// The starting rotation for the slerp.
//    /// </summary>
//    private Quaternion startingRotation;
//    /// <summary>
//    /// The target rotation for the slerp.
//    /// </summary>
//    private Quaternion targetRotation;
//    /// <summary>
//    /// True when the rotation is changed.
//    /// </summary>
//    private bool isRotationChanging;

//    /// <summary>
//    /// To be used when rotating the car.
//    /// </summary>
//    private readonly float rotationSpeed;

//    public Transform wheels;
//    private float wheelRadius;
//    private float wheelCircumference;
//    private float rotationsPerSecond = 100.0f;

//    public Transform leftBlinkers;
//    public Transform rightBlinkers;
//    public bool isLeftBlinkersTurnedOn;
//    public bool isRightBlinkersTurnedOn;
//    private IEnumerator leftBlinkerCoroutine;
//    private IEnumerator rightBlinkerCoroutine;
//    private bool isLeftBlinkerCoroutineStarted = false;
//    private bool isRightBlinkerCoroutineStarted = false;

//    /* Cache blinkers renderer for faster access to change material */
//    Renderer[] leftBlinkersRenderers;
//    Renderer[] rightBlinkersRenderers;


//    class CarReachedDestinationEvent : UnityEvent<string>
//    {
//    }

//    // Use this for initialization
//    void Start()
//    {

//        reachedDestination = new CarReachedDestinationEvent();
//        reachedDestination.AddListener(DestroyCar);
//        isPositionChanged = true;

//        transform.localPosition = Vehicle.PrevPosition2D;
//        transform.localRotation = Quaternion.Euler(0f, Vehicle.Angle, 0f);

//        #region Cache Blinkers Renderer
//        leftBlinkers = transform.Find("leftBlinkers");
//        rightBlinkers = transform.Find("rightBlinkers");
//        leftBlinkersRenderers = new Renderer[leftBlinkers.childCount];
//        int i = 0;
//        foreach(Transform blinker in leftBlinkers)
//        {
//            leftBlinkersRenderers[i++] = blinker.gameObject.GetComponent<Renderer>();
//        }

//        i = 0;
//        rightBlinkersRenderers = new Renderer[rightBlinkers.childCount];
//        foreach (Transform blinker in rightBlinkers)
//        {
//            rightBlinkersRenderers[i++] = blinker.gameObject.GetComponent<Renderer>();
//        }
//        #endregion

//        wheels = transform.Find("wheels");
//        wheelRadius = 0.3f;
//        wheelCircumference = 2 * Mathf.PI * wheelRadius;

//        //StartCoroutine(LeftBlinkersCoroutine());
//    }

//    private void FixedUpdate()
//    {
//        rotationsPerSecond = Vehicle.Speed / wheelCircumference;
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        //if(transform.localRotation != Quaternion.Euler(0f,Vehicle.Angle, 0f))
//        //{
//        //    if(!isRotationChanging)
//        //        StartCoroutine(ChangeCarsRotationSlerp());
//        //}
//        transform.localRotation = Quaternion.Euler(0f, Vehicle.Angle, 0f);
//        //transform.localPosition = Vehicle.Position2D;
//        //Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0f,Vehicle.Angle,0f),  Time.deltaTime * rotationSpeed);
//        if (isPositionChanged)
//        {
//            reachedDestination.Invoke(Vehicle.ID);
//            startingPosition = transform.localPosition;
//            targetPosition = Vehicle.Position2D;
//            targetPosition += Vector3.up * GROUND_OFFSET;
//            StartCoroutine(ChangeCarsPositionLerp());
//        }

//        /* Rotate the wheels */
//        foreach (Transform wheel in wheels)
//        {
//            wheel.Rotate(Vector3.right * 360 * Time.deltaTime * rotationsPerSecond);

//#if DEEBUG
//            Debug.Log("I got rotated? " + i++ + " rotations per second " + rotationsPerSecond);
//            Debug.Log("Vehicle speed " + Vehicle.Speed + " Wheel Circumference " + wheelCircumference);
//#endif

//        }


//        /* Blink lights */
//        if (TraCIAuxiliaryMethods.IsLeftBlinkerOn(Vehicle.Signal))
//        {
//            if(!isLeftBlinkerCoroutineStarted)
//            {
//                leftBlinkerCoroutine = LeftBlinkersCoroutine();
//                StartCoroutine(leftBlinkerCoroutine);
//                isLeftBlinkerCoroutineStarted = true;
//            }
//        }
//        else
//        {
//            if (isLeftBlinkerCoroutineStarted)
//            {
//                StopCoroutine(leftBlinkerCoroutine);
//                if(isLeftBlinkersTurnedOn)
//                {
//                    foreach(Renderer blinkerRenderer in leftBlinkersRenderers)
//                    {
//                        blinkerRenderer.material = VehicleSimulator.blinkersOffMaterial;
//                    }
//                }
//            }

//        }


//        if (TraCIAuxiliaryMethods.IsRightBlinkerOn(Vehicle.Signal))
//        {
//            Debug.Log("Right signal is blinking for car with id " + Vehicle.ID);
//            if (!isRightBlinkerCoroutineStarted)
//            {
//                rightBlinkerCoroutine = RightBlinkersCoroutine();
//                StartCoroutine(rightBlinkerCoroutine);
//                isRightBlinkerCoroutineStarted = true;
//            }
//        }
//        else
//        {
//            if (isRightBlinkerCoroutineStarted)
//            {
//                StopCoroutine(rightBlinkerCoroutine);
//                if(isRightBlinkersTurnedOn)
//                {
//                    foreach (Renderer blinkerRenderer in rightBlinkersRenderers)
//                    {
//                        blinkerRenderer.material = VehicleSimulator.blinkersOffMaterial;
//                    }
//                }
//            }

//        }
//    }

//    /// <summary>
//    /// Attempt to destroy car whenever it reaches a target position.
//    /// If a a car is eligable for destruction (arrived it's destination
//    /// in sumo) then it's destroyed.
//    /// </summary>
//    /// <param name="id"> The id of the car to attempt to destroy </param>
//    void DestroyCar(string id)
//    {
//        VehicleSimulator.DestroyCar(id);
//    }

//    /// <summary>
//    /// Change a cars position from starting to target.
//    /// </summary>
//    /// <returns></returns>
//    IEnumerator ChangeCarsPositionLerp()
//    {
//        float lerpRate = 0f;
//        isPositionChanged = false;
//        for (lerpRate = 0f; lerpRate <= 1.0f; lerpRate += 0.1f)
//        {
//            transform.localPosition = Vector3.Lerp(startingPosition, targetPosition, lerpRate);
//            yield return new WaitForSeconds(0.001f);
//        }
//        transform.localPosition = targetPosition;
//        isPositionChanged = true;
//    }

//    IEnumerator ChangeCarsRotationSlerp()
//    {
//        float lerpRate = 0f;
//        isRotationChanging = true;
//        for (lerpRate = 0f; lerpRate <= 1.0f; lerpRate += 0.1f)
//        {
//            transform.localRotation = Quaternion.Slerp(startingRotation, targetRotation, lerpRate);
//            yield return new WaitForSeconds(0.001f);
//        }
//        transform.localRotation = targetRotation;
//        isRotationChanging = false;
//    }


//    /// <summary>
//    /// Flash the left blinkers each second
//    /// </summary>
//    /// <returns></returns>
//    IEnumerator LeftBlinkersCoroutine()
//    {
//        while (true)
//        {
//            foreach (Renderer blinkerRendered in leftBlinkersRenderers)
//            {
//                /* Change blinkers state */
//                blinkerRendered.material = isLeftBlinkersTurnedOn ?
//                    VehicleSimulator.blinkersOffMaterial : VehicleSimulator.blinkersOnMaterial;
//            }
//            isLeftBlinkersTurnedOn = !isLeftBlinkersTurnedOn;
//            yield return new WaitForSeconds(1f);
//        }
//    }

//    IEnumerator RightBlinkersCoroutine()
//    {
//        while (true)
//        {
//            foreach (Renderer blinkerRendered in rightBlinkersRenderers)
//            {
//                /* Change blinkers state */
//                blinkerRendered.material = isRightBlinkersTurnedOn ?
//                    VehicleSimulator.blinkersOffMaterial : VehicleSimulator.blinkersOnMaterial;
//            }
//            isRightBlinkersTurnedOn = !isRightBlinkersTurnedOn;
//            yield return new WaitForSeconds(1f);
//        }
//    }
//}
