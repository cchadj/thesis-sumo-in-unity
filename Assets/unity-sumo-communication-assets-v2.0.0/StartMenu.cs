using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using RiseProject.Tomis.DataHolders;
using UnityEngine.SceneManagement;
using Crosstales.FB;
using RiseProject.Tomis.SumoInUnity;
using TMPro;
using UnityEngine.Serialization;
using Tomis.UnityEditor.Utilities;
using UnityQuery;

public class StartMenu : MonoBehaviour
{
    [SerializeField]
    [FileSelect(
        OpenAtPath=@"/StreamingAssets/sumo-scenarios",
        AssetRelativePath = true,
        SelectMode = SelectionMode.File,
        ButtonName = "Select sumocfg",
        FileExtensions = "sumocfg,",
        DisplayWarningWhenNotSelected = true,
        Tooltip = "Select default sumo cfg file")]
    private string defaultSumocfg;
    
    [Header("Simulation Configurations")]
    [SerializeField] private Button selectConfigurationFileButton;
    [SerializeField] private Button startSimulationButton;
    [SerializeField] private TextMeshProUGUI selectedSumoCfgText;
    
    private string _initialSelectedSumoCfgText;
    private string _initialStepLengthText;
    [Header("Performance")]
    [SerializeField] private Toggle performanceTestToggle;
    [SerializeField] private Slider stepLengthSlider;
    [SerializeField] private TextMeshProUGUI stepLengthText;
    [SerializeField] private Toggle useMultithreadingToggle;
    [SerializeField] private Toggle useContextSubscriptionsToggle;
    //[SerializeField] private Toggle useVariableSubscriptionsToggle;
    [SerializeField] private TMP_Dropdown redirectionDropDown;
        
    [Header("Components")]
    [SerializeField] private Toggle dontUseSumoToggle;
    [SerializeField] private Toggle dontUseVehicleSimulatorToggle;
    
    [FormerlySerializedAs("makeFpsByVehicleCountLogToggle")]
    [Header("Logs and plots")]
    [SerializeField] private Toggle makeFpsByVehicleCountPlotToggle;
    [SerializeField] private Toggle makeSimStepDelayByVehicleLogToggle;
    [SerializeField] private Toggle useReporterToggle;



    [Space(4), Header("Simulation Related")]
    [SerializeField, Tooltip("Singleton that is assigned at awake."),
        ReadOnly]
    private SimulationStartupData startupData;

    private string _file;

    private void Awake()
    {
        startupData = SimulationStartupData.Instance;
        _initialSelectedSumoCfgText = selectedSumoCfgText.text;

        if (!defaultSumocfg.IsNullOrEmpty())
        {
            startupData.SumoConfigFilename = defaultSumocfg;
            startupData.UseStartupData = true;
            
            selectedSumoCfgText.SetText($"{_initialSelectedSumoCfgText} \n{defaultSumocfg}");
        }
        else
        {
            SetTextToNoSumoCfgSelected();
            startupData.UseStartupData = false;
        }
        
        // START SIMULATION button
        selectConfigurationFileButton.onClick.AddListener(SelectConfigurationFile);
        startSimulationButton.onClick.AddListener(StartSimulation);

        // Set up toggles

        //Component toggles
        startupData.dontUseSumo = dontUseSumoToggle.isOn;
        dontUseSumoToggle.onValueChanged.AddListener(isToggleOn => startupData.dontUseSumo = isToggleOn);

        startupData.dontUSeVehicleSimulator = dontUseVehicleSimulatorToggle.isOn;
        dontUseVehicleSimulatorToggle.onValueChanged.AddListener(isToggleOn =>
            startupData.dontUSeVehicleSimulator = isToggleOn);

        // Performance toggles
        startupData.useMultithreading = useMultithreadingToggle.isOn;
        useMultithreadingToggle.onValueChanged.AddListener(isToggleOn =>
            startupData.useMultithreading = isToggleOn);
        
        startupData.useContextSubscriptions = useContextSubscriptionsToggle.isOn;
        useContextSubscriptionsToggle.onValueChanged.AddListener( isToggleOn =>
            startupData.useContextSubscriptions = isToggleOn
        );
        
        // Logs and plots toggles
        startupData.createFpsByVehicleCountPlot = makeFpsByVehicleCountPlotToggle.isOn;
        makeFpsByVehicleCountPlotToggle.onValueChanged.AddListener(isToggleOn =>
            startupData.createFpsByVehicleCountPlot = isToggleOn);

        startupData.createSimStepDelayByVehicleCount = makeSimStepDelayByVehicleLogToggle.isOn;
        makeSimStepDelayByVehicleLogToggle.onValueChanged.AddListener(isToggleOn => startupData.createSimStepDelayByVehicleCount = isToggleOn);
        
        startupData.useReporter = useReporterToggle.isOn;
        useReporterToggle.onValueChanged.AddListener(isToggleOn => startupData.useReporter = isToggleOn);
        
        // StepLength slider
        var sliderStep = 0.005f;
        _initialStepLengthText = stepLengthText.text;
        startupData.stepLength = stepLengthSlider.value * sliderStep;
        stepLengthText.SetText($"{_initialStepLengthText} {startupData.stepLength}");
        stepLengthSlider.onValueChanged.AddListener(
             value => {
                 startupData.stepLength = value * sliderStep;
                 stepLengthText.SetText($"{_initialStepLengthText} {startupData.stepLength}");
             }
        );

        PopulateDropdownWithEnum<SumoProcessRedirectionMode>(redirectionDropDown);
        startupData.redirectionMode = (SumoProcessRedirectionMode)redirectionDropDown.value;
        redirectionDropDown.onValueChanged.AddListener(selectedOptionIndex =>
            {
                startupData.redirectionMode = (SumoProcessRedirectionMode) selectedOptionIndex;
            });
    }

    private static List<string> PopulateDropdownWithEnum< T> ( TMP_Dropdown tmpDropdown )  where T : Enum
    {
        var enumType = typeof(T);
        var enumNames = Enum.GetNames(enumType);
        var names = new List<string>(enumNames);

        tmpDropdown.ClearOptions();
        tmpDropdown.AddOptions(names);
        
        return names;
    }

    private void SetTextToNoSumoCfgSelected()
    {
        selectedSumoCfgText.color = Color.red;
        selectedSumoCfgText.SetText($"{_initialSelectedSumoCfgText} \nNo sumocfg Selected");
    }

    void SelectConfigurationFile()
    {
        var defaultDirectory = Path.Combine(Application.streamingAssetsPath + "/sumo-scenarios");

        var path = FileBrowser.OpenSingleFile("Select Sumo Configuration File", defaultDirectory, "sumocfg", "sumo.cfg");

        if (!string.IsNullOrEmpty(path))
        {
            selectedSumoCfgText.color = Color.black;
            selectedSumoCfgText.SetText(
                $"{_initialSelectedSumoCfgText} \n{path}"
                );
            startupData.SumoConfigFilename = path;
            startupData.UseStartupData = true;
        }
    }

    private void StartSimulation()
    {
        if(performanceTestToggle.isOn)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
        
    }
}
