﻿using System.Collections.Generic;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.Util.Serializable;
using UnityEngine;
using UnityEngine.Serialization;

namespace RiseProject.Tomis.DataHolders
{
    [CreateAssetMenu(menuName = "DataHolders/SumoNetworkData")]
    public class SumoNetworkData : NetworkData
    {
        [SerializeField] private EdgeDictionary edges;
        [SerializeField] private LaneDictionary lanes;        
        [SerializeField] private RouteDictionary routes;
        [SerializeField] private JunctionDictionary junctions;

        [SerializeField, Header("Debug")] 
        public bool showLanesInsideFrustum = true;
        
        public readonly LaneDictionary LanesInsideFrustum = new LaneDictionary();
        
        public EdgeDictionary Edges { get => edges; set => edges = value; }
        public LaneDictionary Lanes { get => lanes; set => lanes = value; }
        public RouteDictionary Routes { get => routes; set => routes = value; }
        public JunctionDictionary Junctions { get => junctions; set => junctions = value; }

        public VehicleDictionary VehiclesArrivedShared { get; set; }
        public VehicleDictionary VehiclesDepartedShared { get; set; }
        public VehicleDictionary VehiclesLoadedShared { get; set; }
        
        public VehicleDictionary VehiclesEnteredContextRange { get; set; }
        public VehicleDictionary VehiclesExitedContextRange { get; set; }
        public VehicleDictionary VehiclesInContextRange { get; set; }
        
        [SerializeField] private Vehicle mockVehicle;
        public Vehicle MockVehicle { get => mockVehicle; set => mockVehicle = value; }

        public void AddMockVehicleToDeparted()
        {
            VehiclesDepartedShared.Add(MockVehicle.ID, MockVehicle);
        }

        public void AddMockVehicleToDeparted(Vehicle v)
        {
            VehiclesDepartedShared.Add(v.ID, v);
        }

        public void ClearDictionaries()
        {
            Edges.Clear(); Lanes.Clear(); Routes.Clear(); VehiclesArrivedShared.Clear(); VehiclesDepartedShared.Clear(); VehiclesLoadedShared.Clear();
        }

        public void ClearVehicleDictionaries()
        {
            VehiclesArrivedShared.Clear(); VehiclesDepartedShared.Clear(); VehiclesLoadedShared.Clear();
        }
    }

}