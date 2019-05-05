using System;
using System.Collections;
using System.IO;
using RiseProject.Tomis.SumoInUnity;
using TMPro;
using Tomis.Utils.Unity;
using UnityEngine;

public class SimulationStateCanvas : SingletonMonoBehaviour<SimulationStateCanvas>
{
    [Header("Start Up State")] 
    [SerializeField] private TextMeshProUGUI sumocfgText;
    [SerializeField] private TextMeshProUGUI subscriptionTypeText;
    [SerializeField] private TextMeshProUGUI redirectionModeText;
    [SerializeField] private TextMeshProUGUI usingMultithreadText;
    [SerializeField] private TextMeshProUGUI stepLengthText;
    
    [Header("Current State")] 
    [SerializeField] private TextMeshProUGUI numberOfActiveVehiclesText;
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private TextMeshProUGUI simStepExecutionTime;
    [SerializeField] private TextMeshProUGUI currentlyContextSubscribedLaneIDText;
    [SerializeField] private TextMeshProUGUI numberOfVehiclesInsideContextRangeText;
    
    private SumoClient _sumoClient;

    private void Awake()
    {
        _sumoClient = SumoClient.Instance;


        StartCoroutine(UpdateStateView());
    }

    private const int LEFT_ALIGN = -30, RIGHT_ALIGN = 30;

    private readonly WaitForSeconds _updateSimulationStateSeconds = new WaitForSeconds(2f);
    private IEnumerator UpdateStateView()
    {
        yield return new WaitForEndOfFrame();
        
        sumocfgText.SetText($"{sumocfgText.text,LEFT_ALIGN}{Path.GetFileName(_sumoClient.SumocfgFile),RIGHT_ALIGN}");
        redirectionModeText.SetText($"{redirectionModeText.text,LEFT_ALIGN}{_sumoClient.RedirectionMode.ToString(),RIGHT_ALIGN}");
        usingMultithreadText.SetText($"{(_sumoClient.UseMultithreading?"No " : "Using ")} Multithreading");
        subscriptionTypeText.SetText($"{subscriptionTypeText.text,LEFT_ALIGN}{(_sumoClient.UseContextSubscription? "Context" : "Variable"),RIGHT_ALIGN}");
        stepLengthText.SetText($"{stepLengthText.text,LEFT_ALIGN}{_sumoClient.StepLength,RIGHT_ALIGN:00.00}");
        enabled = false;
        
        var numberOfActiveVehiclesInitialText = numberOfActiveVehiclesText.text;
        var fpsInitialText = fpsText.text;
        var simStepExecutionTimeInitialText = simStepExecutionTime.text;
        var currentlySubscribedLaneIdInitialText = currentlyContextSubscribedLaneIDText.text;
        var numberOfVehiclesInsideContextRangeInitialText = numberOfVehiclesInsideContextRangeText.text;
        
        if(!_sumoClient.CreateFpsByVehicleCountPlot)
            Destroy(fpsText);
        
        if(!_sumoClient.CreateSimStepExecutionTimeByVehicleCountPlot)
            Destroy(simStepExecutionTime);

        if (!_sumoClient.UseContextSubscription)
        {
            
            Destroy(currentlyContextSubscribedLaneIDText);
            Destroy(numberOfVehiclesInsideContextRangeText);
        }
        
        if (!_sumoClient.CreateFpsByVehicleCountPlot &&
            !_sumoClient.CreateSimStepExecutionTimeByVehicleCountPlot &&
            !_sumoClient.UseContextSubscription)
        {
            Destroy(numberOfActiveVehiclesText);
            yield break;
        }
        
        while (true)
        {
            numberOfActiveVehiclesText.SetText($"{numberOfActiveVehiclesInitialText,LEFT_ALIGN}{_sumoClient.NumberOfActiveVehicles,RIGHT_ALIGN}");
            if(_sumoClient.CreateFpsByVehicleCountPlot)
                fpsText.SetText($"{fpsInitialText,LEFT_ALIGN}{(int)_sumoClient.CurrentFps,RIGHT_ALIGN}");
                
            if(_sumoClient.CreateSimStepExecutionTimeByVehicleCountPlot)
                simStepExecutionTime.SetText($"{simStepExecutionTimeInitialText,LEFT_ALIGN}{_sumoClient.CurrentSimulationStepExecutionTime,RIGHT_ALIGN:00.00}");

            if (_sumoClient.UseContextSubscription)
            {
                currentlyContextSubscribedLaneIDText.SetText($"{currentlySubscribedLaneIdInitialText,LEFT_ALIGN}{_sumoClient.CurrentlyContextSubscribedObjectID,RIGHT_ALIGN}" );
                numberOfVehiclesInsideContextRangeText.SetText($"{numberOfVehiclesInsideContextRangeInitialText,LEFT_ALIGN}{_sumoClient.NumberOfVehiclesInsideContextRange,RIGHT_ALIGN}");
            } 
            yield return _updateSimulationStateSeconds;

        }
    }
}
