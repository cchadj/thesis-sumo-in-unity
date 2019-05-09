using RiseProject.Tomis.DataHolders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MockTester : MonoBehaviour {

    public static MockTester instance;

    [SerializeField]
    private SumoNetworkData _sumoNetworkData;
    [SerializeField]
    private SharedVehicleData _sharedVehicleData;

    public SumoNetworkData SumoNetworkData { get => _sumoNetworkData; set => _sumoNetworkData = value; }
    public SharedVehicleData SharedVehicleData { get => _sharedVehicleData; set => _sharedVehicleData = value; }

    void Start()
    {
        MockTester.instance = this;
    }
}
