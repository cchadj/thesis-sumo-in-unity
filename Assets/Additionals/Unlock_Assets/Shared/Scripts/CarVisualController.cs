using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

//[ExecuteInEditMode]
public class CarVisualController : MonoBehaviour
{
    public static readonly float groundOffset = 0.4f;

    public enum Steer
    {
        None,
        Left,
        Right
    }
    [Header("Car Steer Control")]
    public Steer steerOrientation;
    
    [Range(2.5f, 5f)]
    [Tooltip("How fast will the transition of wheel and steering wheel rotation will happen")]
    public float steerRotationSpeed;
    [Tooltip("How much the wheels will turn in degrees")]
    [Range(30f, 90f)]
    public float frontWheelSteeringAngle;
    [Tooltip("How much does the wheel affect the wheels rotation. See https://en.wikipedia.org/wiki/Steering_ratio")]
    [Range(0.1f, 15f)]
    public float steerWheelToWheelsRotationRatio;
    [Tooltip("")]
    public List<Animator> frontWheelAnimators;
    public List<Animator> rearWheelAnimators;


    [Space(5)]
    [Header("Car Speed Control")]
    public float speed;

    [Space(5)]
    [Header("Car Light Controller")]
    public bool leftBlink;
    public bool rightBlink;
    public bool brakeLights;
    public bool frontLights;
    
    [Tooltip("The interval between on and off for indicators in s. 0.5 is good.")]
    [Range(0.2f, 1f)]
    public float indicatorBlinkInterval;
    [Header("Lights")]
    /*
     * The following booleans are used to change the state of the indicators 
     * and are set true and false at time intervals all the time.
     * They do not represent the real state of the car lights.
     */
    public bool frontLightsOn;
    public bool brakeEffectsOn;
    public bool isLeftIndicatorOn;
    public bool rightIndicatorOn;

    [Space(5)]
    public GameObject brakeEffects;
    public GameObject frontLightEffects;
    public GameObject reverseEffect;
    public GameObject rightIndicator;
    public GameObject leftIndicator;
    [Header("Needles")]
    public Transform speedNeedle;
    public Vector2 speedNeedleRotateRange = Vector3.zero;
    private Vector3 _speedEulers = Vector3.zero;
    public Transform rpmNeedle;
    public Vector2 rpmNeedleRotateRange = Vector3.zero;
    private Vector3 _rpmEulers = Vector3.zero;
    public float needleSmoothing = 1.0f;
    public Transform steeringWheel;

    private float _rotateNeedles = 0.0f;
    
    private Transform[] _wheels;
    [Header("Wheels")]
    public Transform wheelFR;
    public Transform wheelFL;
    public Transform wheelRR;
    public Transform wheelRL;

    private Transform[] _wheelParents;
    private Transform _wheelFRParent;
    private Transform _wheelFLParent;
    private Transform _wheelRRParent;
    private Transform _wheelRLParent;

    [Header("Appearance")]
    [SerializeField] private GameObject CarHull;

    [SerializeField, CallSetter("CarHullColor")]
    private Color _carHullColor;
    public Color CarHullColor 
    { 
    get => _carHullColor; 
    set 
    {

            if (!CarHull)
                return;
            _carHullColor = value;


#if UNITY_EDITOR
            var rend = CarHull.GetComponent<MeshRenderer>();
            if (Application.isEditor)
            {
                var tempMaterial = new Material(rend.GetComponent<MeshRenderer>().sharedMaterial);
                rend.sharedMaterial = tempMaterial;
                tempMaterial.color = _carHullColor;
                rend.sharedMaterial = tempMaterial;
            }
            
#else
            var meshRenderer = CarHull.GetComponent<MeshRenderer>();
            meshRenderer.material.color = _carHullColor;
#endif
    }
    }

    // private float wheelRadius;
    //  private float wheelCircumference;
    //  wheelCircumference = 2 * Mathf.PI * wheelRadius; 
    // wheelRadius = wheel_FL.transform.lossyScale.x;
    [Header("Panel Texts")]
    public Text txtSpeed, txtRPM, txtSpeed2;
    public Slider sliderRPM;

    private IEnumerator _indicatorLightsCoroutine;

    [field: SerializeField, Header("Performance Related"), Rename("Display Texts")]
    private bool DisplayTexts { get; }

    /// <summary>
    /// Should the visual controller handle the wheel rotation?
    /// </summary>
    public bool HandleWheelRotations { private get; set; } = true;

    private void Awake()
    {
        _isSteeringWheelNotNull = steeringWheel != null;
        _wheels = new Transform[4];

        _wheels[0] = wheelFR;
        _wheels[1] = wheelFL;
        _wheels[2] = wheelRR;
        _wheels[3] = wheelRL;

        _wheelParents = new Transform[4];
        // Cache wheel parents
        _wheelParents[0] = _wheelFRParent = wheelFR.transform.parent;
        _wheelParents[1] = _wheelFLParent = wheelFL.transform.parent;
        _wheelParents[2] = _wheelRRParent = wheelRR.transform.parent;
        _wheelParents[3] = _wheelRLParent = wheelRL.transform.parent;

        frontLightsOn = true;
        brakeEffectsOn = true;
        isLeftIndicatorOn = false;
        rightIndicatorOn = false;

        if (speedNeedle) _speedEulers = speedNeedle.localEulerAngles;
        if (rpmNeedle) _rpmEulers = rpmNeedle.localEulerAngles;

        _indicatorLightsCoroutine = WaitIndicatorLights(0.50f);

        StartCoroutine(_indicatorLightsCoroutine);

    }
    
    private float _rpm;
    private float _previousEuler;

    private float _curSteerAngle;
    private float _lerpDuration = 0.5f;
    private void Update()
    {
        LightControl();
        
        _rpm = speed / wheelFR.localScale.x;

        if (HandleWheelRotations)
            RotateWheels();

        if (DisplayTexts)
        {
            if (txtSpeed)
                txtSpeed.text = ((int)(_rotateNeedles * 100.0f)).ToString();// + " km/h";
            txtSpeed2.text = ((int)(_rotateNeedles * 100.0f)).ToString();
            if (txtRPM)
                txtRPM.text = ((int)(_rotateNeedles * 1000.0f)).ToString();
            if (sliderRPM)
                sliderRPM.value = (_rotateNeedles * 1000.0f);
            // Speed needle is not needed to be updated
            if (speedNeedle)
            {
                var temp = new Vector3(_speedEulers.x, _speedEulers.y, Mathf.Lerp(speedNeedleRotateRange.x, speedNeedleRotateRange.y, (_rotateNeedles)));
                speedNeedle.localEulerAngles = Vector3.Lerp(speedNeedle.localEulerAngles, temp, Time.deltaTime * needleSmoothing);
            }

            if (rpmNeedle)
            {
                var temp = new Vector3(_rpmEulers.x, _rpmEulers.y, Mathf.Lerp(rpmNeedleRotateRange.x, rpmNeedleRotateRange.y, (_rotateNeedles)));
                rpmNeedle.localEulerAngles = Vector3.Lerp(rpmNeedle.localEulerAngles, temp, Time.deltaTime * needleSmoothing);
            }
        }
    }

    private void LightControl()
    {
        if (leftBlink)
        {
            TurnOnLeftIndicatorLights();
        }
        else // Ensure that the indicators are closed */
        {
            if (leftIndicator.activeSelf) leftIndicator.SetActive(false);
        }

        if (rightBlink)
        {
            TurnOnRightIndicatorLights();
        }
        else // Ensure that the indicators are closed */
        {
            if (rightIndicator.activeSelf) rightIndicator.SetActive(false);
        }

        if (brakeLights)
        {
            TurnOnBackLights();
        }
        else
        {
            if (brakeEffects.activeSelf) brakeEffects.SetActive(false);
        }

        if (frontLights)
        {
            TurnOnFrontLights();
        }
        else
        {
            if (frontLightEffects.activeSelf) frontLightEffects.SetActive(false);
        }
    }

    private void RotateWheels()
    {
        
        foreach (var wheel in _wheels)    
            wheel.Rotate(Vector3.right, _rpm * 2 * Mathf.PI * Time.deltaTime * Mathf.Rad2Deg,Space.Self);

        if (_isSteeringWheelNotNull)
        {
            var targetRotationEulers = steeringWheel.localRotation.eulerAngles;

            switch (steerOrientation)
            {
                case Steer.Right:
                    targetRotationEulers.z = frontWheelSteeringAngle;
                    StopAllCoroutines();
                    StartCoroutine(RotateWheelTowards(targetRotationEulers.z, _lerpDuration));
                    break;
                case Steer.Left:
                    targetRotationEulers.z = -frontWheelSteeringAngle ;
                    StopAllCoroutines();
                    StartCoroutine(RotateWheelTowards(targetRotationEulers.z, _lerpDuration));
                    break;
                default:
                    targetRotationEulers.z = 0f;
                    StopAllCoroutines();
                    StartCoroutine(RotateWheelTowards(targetRotationEulers.z, _lerpDuration));
                    break;
            }

            steeringWheel.localRotation = Quaternion.Slerp(
                steeringWheel.localRotation, 
                Quaternion.Euler(targetRotationEulers / steerWheelToWheelsRotationRatio),
                Time.deltaTime * steerRotationSpeed);

        }
    }
    
    private bool _isCoroutinePlaying;
    private bool _isSteeringWheelNotNull;

    private IEnumerator RotateWheelTowards(float targetAngle, float duration)
    {
        if(_isCoroutinePlaying)
            yield break;

        _isCoroutinePlaying = true;
        var startAngle = _wheelFRParent.transform.localRotation.y;
        
        var timeElapsed = 0f;
        while(timeElapsed < duration)
        {
            var t = timeElapsed / duration;
            var lerpAngle = Mathf.LerpAngle(startAngle, targetAngle, t);

            _wheelFLParent.localRotation = Quaternion.Euler(new Vector3(0f, lerpAngle, 0f));
            _wheelFRParent.localRotation = Quaternion.Euler(new Vector3(0f, lerpAngle, 0f));
            timeElapsed += Time.deltaTime;

            yield return null;
        }
        _wheelFLParent.localRotation = Quaternion.Euler(new Vector3(0f, targetAngle, 0f));
        _wheelFRParent.localRotation = Quaternion.Euler(new Vector3(0f, targetAngle, 0f));
        _isCoroutinePlaying = false;
    }

    private IEnumerator WaitLights(float waitTime)
    {
        var waitFor = new WaitForSeconds(waitTime);
        while (true)
        {
            yield return waitFor;
            frontLightsOn = !frontLightsOn;
            brakeEffectsOn = !brakeEffectsOn;
        }
    }

   
    /// <summary>
    /// Synchronise 
    /// </summary>
    /// <param name="waitTime"></param>
    /// <returns></returns>
    private IEnumerator WaitIndicatorLights(float waitTime)
    {
        
        var waitFor= new WaitForSeconds(waitTime);
        while (true)
        {
            yield return waitFor;
            isLeftIndicatorOn = !isLeftIndicatorOn;
            rightIndicatorOn = !rightIndicatorOn;
        }
    }

    public void TurnOnBothIndicatorLights()
    {
        if (isLeftIndicatorOn && rightIndicatorOn)
        {
            rightIndicator.SetActive(true);
            leftIndicator.SetActive(true);
        }
        else
        {
            rightIndicator.SetActive(false);
            leftIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Start blinking left indicators
    /// </summary>
    public void TurnOnLeftIndicatorLights()
    {
        leftIndicator.SetActive(isLeftIndicatorOn);
    }

    /// <summary>
    /// Start blinking right indicators
    /// </summary>
    public void TurnOnRightIndicatorLights()
    {
        rightIndicator.SetActive(rightIndicatorOn);
    }

    public void TurnOnFrontLights()
    {
        if (frontLightsOn)
        {
            frontLightEffects.SetActive(frontLightsOn);
        }
    }

    public void TurnOnBackLights()
    {
        brakeEffects.SetActive(brakeEffectsOn);
    }
 }
