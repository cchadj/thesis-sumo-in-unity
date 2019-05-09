
using RiseProject.Tomis.DataContainers;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SearchElement : MonoBehaviour
{
    [SerializeField]
    private InputField _inputField;
    [SerializeField]
    private Dropdown _dropdown;
    public Dropdown Dropdown { get => _dropdown; set => _dropdown = value; }

    [SerializeField]
    private SumoNetworkData _networkData;
    public SumoNetworkData NetworkData { get => _networkData; set => _networkData = value; }
    public InputField InputField { get => _inputField; set => _inputField = value; }

    public List<TraCIVariable> elements;

    TraCIVariable traciElement;

    public bool ShowVehicles = true;
    private List<Dropdown.OptionData> dropdownOptions;

    public void Start()
    {
        InputField.onValueChanged.AddListener(delegate { ShowListOfVehicleIDs(); });
    }

    public void ShowListOfVehicleIDs()
    {
        string input = InputField.text;
        List<string> listOfStrings = new List<string>(_networkData.VehiclesLoadedShared.Keys);

        var queryResult = from p in listOfStrings
                          where p.Contains(input)
                          select p;

        foreach (string result in queryResult)
        {
            Debug.Log(result);
        }
        Debug.Log(queryResult.ToList().ToString());

        _dropdown.ClearOptions();
        _dropdown.AddOptions(queryResult.ToList());
        _dropdown.Show();

        InputField.ActivateInputField();
    }

    public void GiveSearchFieldFocus(bool searchInputField = true)
    {
        InputField.Select();
        InputField.ActivateInputField();
    }

}
