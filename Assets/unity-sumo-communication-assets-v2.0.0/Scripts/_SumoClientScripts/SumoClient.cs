using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using CodingConnected.TraCI.NET;
using CodingConnected.TraCI.NET.Commands;
using CodingConnected.TraCI.NET.Types;
using RiseProject.Tomis.DataHolders;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using RiseProject.Tomis.Util.Serializable;
using RiseProject.Tomis.Util.TraciAuxilliary;
using Tomis.Utils;
using Tomis.Utils.Unity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using Zenject;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;

// ReSharper disable Unity.RedundantAttributeOnTarget

namespace RiseProject.Tomis.SumoInUnity
{
    [Serializable]
    public enum SumoProcessRedirectionMode
    {
        NoRedirection,
        CaptureStderr,
        CaptureStdout,
        CaptureNone
    }


    
    public class SumoClient : SingletonMonoBehaviour<SumoClient>
    {
        
        [SerializeField] public SumoProcessRedirectionMode RedirectionMode { get; set; }
        
        private TaskManager MTaskManager { get; set; }
        private SimulationStartupData StartupData { get; set; }
        private ApplicationManager AppManager { get; set; }

        [field: SerializeField, Rename("Use Multithreading")]
        public bool UseMultithreading { get; set; }

        #region Defaults

        private const string DEFAULT_IP = "127.0.0.1";
        private const int DEFAULT_REMOTE_PORT = 25000, DEFAULT_NUMBER_OF_CONNECTIONS = 1, DEFAULT_ORDER = 1;
        private const bool DEFAULT_USE_SUMO_GUI = false, DEFAULT_SHOW_TERMINAL = true;

        #endregion Defaults

        #region Connection Configuration Settings

        private string _netXmlFile;

        #endregion Connection Configuration Settings

        #region Serve Sumo

        /// <summary>
        /// Relative to StreamingAssets path.
        /// Files that are in StreamingAssets folder can be found in the same folder in the build.
        /// </summary>
        private const string RELATIVE_BUILD_PATH_TO_SUMO_EXECUTABLE_FOLDER = "sumo-executables";

        [field: SerializeField] public bool ShouldServeSumo { get; set; }

        [field: SerializeField] public bool CaptureSumoProcessErrors { get; set; }
        /// <summary> How many connections will this server support. </summary>
        [field: SerializeField] public int NumberOfConnections { get; set; } = DEFAULT_NUMBER_OF_CONNECTIONS;

        #endregion Serve Sumo 

        #region Connection to SUMO

        /// <summary> In case of multiple clients there must be an order of connection be specified </summary>
        public int Order { private get; set; } = DEFAULT_ORDER;

        public event  EventHandler ConnectionInisialized;
        
        private TcpClient _tcpClient; /* The socket to write and read from */
        private TraCIClient TraCIClient { get; set; }

        private Process _sumoProcess;
        //private Thread clientReceiveThread;

        #endregion Connection to SUMO

        #region Command Declarations && Subsciptions and Subscription Lists

        private ControlCommands _controlCommands;
        private VehicleCommands _vehicleCommands;
        private SimulationCommands _simulationCommands;
        private RouteCommands _routeCommands;
        private JunctionCommands _junctionCommands;
        private LaneCommands _laneCommands;
        private VehicleTypeCommands _vehicleTypeCommands;

        [field: SerializeField] public bool UseContextSubscription { get; set; }
        
        /// <summary> A number of traci variables each new car will subscribe to. Make polling easier for each vehicle. </summary>
        private readonly List<byte> _vehicleSubscriptionList = new List<byte>
        {
//            TraCIConstants.VAR_SPEED,
//            TraCIConstants.VAR_ACCELERATION,
            TraCIConstants.VAR_ANGLE,
            TraCIConstants.VAR_POSITION,
//            TraCIConstants.VAR_LANE_ID,
//            TraCIConstants.VAR_LANE_INDEX,
//            TraCIConstants.VAR_ROAD_ID,
//            TraCIConstants.VAR_ROUTE_ID,
//            TraCIConstants.VAR_ROUTE_INDEX,
//            TraCIConstants.VAR_ROUTE_VALID,
//            TraCIConstants.VAR_SIGNALS
        };

        /// <summary>
        /// A subscriptions list not targeted for a Single Vehicle but for global.
        /// </summary>
        private static readonly List<byte> VehicleGlobalVariablesSubscriptionList =
            new List<byte> {TraCIConstants.ID_COUNT};

        private List<byte> SimulationSubscriptionList { get; set; }
        private ControlCommands ControlCommands
        {
            get
            {
                if (_controlCommands == null)
                    throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                                                                "and the commands have no effect.");
                return _controlCommands;
            }

            set => _controlCommands = value;
        }

        private VehicleCommands VehicleCommands
        {
            get
            {
                if (_vehicleCommands == null)
                    throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                                                                "and the commands have no effect.");
                return _vehicleCommands;
            }

            set => _vehicleCommands = value;
        }

        private SimulationCommands SimulationCommands
        {
            get
            {
                if (_simulationCommands == null)
                    throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                                                                "and the commands have no effect.");
                return _simulationCommands;
            }

            set => _simulationCommands = value;
        }

        private RouteCommands RouteCommands
        {
            get
            {
                if (_routeCommands == null)
                    throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                                                                "and the commands have no effect.");
                return _routeCommands;
            }

            set => _routeCommands = value;
        }

        private JunctionCommands JunctionCommands
        {
            get
            {
                if (_junctionCommands == null)
                    throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                                                                "and the commands have no effect.");
                return _junctionCommands;
            }

            set => _junctionCommands = value;
        }

        private LaneCommands LaneCommands
        {
            get
            {
                if (_laneCommands == null)
                    throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                                                                "and the commands have no effect.");
                return _laneCommands;
            }

            set => _laneCommands = value;
        }

        private VehicleTypeCommands VehicleTypeCommands
        {
            get
            {
                if (_vehicleTypeCommands == null)
                    throw new ConnectionNotInitializedException("The connection was not initialized yet " +
                                                                "and the commands have no effect.");
                return _vehicleTypeCommands;
            }

            set => _vehicleTypeCommands = value;
        }

        private SumoCommands SumoCommands { get; set; }

        #endregion Command Declarations 

        #region ScriptableObject Shared Values  (Accessed by SumoNetworkData ScriptableObject)

        private VehicleDictionary VehiclesActiveShared { get; } = new VehicleDictionary();
        private VehicleDictionary VehiclesDepartedShared { get; } = new VehicleDictionary();
        private VehicleDictionary VehiclesArrivedShared { get; } = new VehicleDictionary();

        /// <summary>
        /// vehicles that entered context range the last sim step
        /// </summary>
        private VehicleDictionary VehiclesEnteredContextRange { get; } = new VehicleDictionary();
        /// <summary>
        /// Vehicles that exited the context range the last sim step
        /// </summary>
        private VehicleDictionary VehiclesExitedContextRange { get; } = new VehicleDictionary();
        /// <summary>
        /// The vehicles that are inside the context range  the last sim step
        /// </summary>
        private VehicleDictionary VehiclesInContextRange { get; }= new VehicleDictionary();
        
        private Dictionary<string, Lane> Lanes { get; set; }

        /// <summary> The amount of positions that should be cached in each vehicle. </summary>
        private const int NUMBER_OF_POSITIONS_TO_SAVE = 4;

        /// <summary>
        /// To be changed when there is a change in routes
        /// </summary>
        private bool _isRouteIDsChangedSinceLastGet = true;

        private List<string> _routeIDs = new List<string>();

        private List<string> RouteIds
        {
            get
            {
                if (_isRouteIDsChangedSinceLastGet)
                {
                    var routeTraciResponse = RouteCommands.GetIdList();

                    try
                    {
                        RouteIds = routeTraciResponse.Content;
                    }
                    catch (NullReferenceException e)
                    {
                        throw new NullReferenceException($"{routeTraciResponse.ErrorMessage}\n{e.StackTrace}");
                    }

                    _isRouteIDsChangedSinceLastGet = false;
                }

                return _routeIDs;
            }
            set
            {
                _isRouteIDsChangedSinceLastGet = true;
                _routeIDs = value;
            }
        }

        private void PopulateVehicleQueue(int number=200)
        {
            for (var i = 0; i < number; i++)
            {
                var v = ScriptableObject.CreateInstance<Vehicle>();
                v.Instantiate(NUMBER_OF_POSITIONS_TO_SAVE);
                VehiclePool.Enqueue(v);
                v.Disable();
            }
        }

        /// <summary>
        /// To be changed whenever there is a change in routes
        /// </summary>
        private bool _isRoutesChangedSinceLastGet = true;

        private Dictionary<string, Route> _routes = new Dictionary<string, Route>();

        private Dictionary<string, Route> Routes
        {
            get
            {
                if (_isRoutesChangedSinceLastGet)
                {
                    var routeIds = RouteIds;
                    foreach (var id in routeIds)
                    {
                        var getEdgeTraciResponse = RouteCommands.GetEdges(id);
                        var newRoute = ScriptableObject.CreateInstance<Route>();
                        newRoute.Instantiate(id);
                        newRoute.EdgeIDs = getEdgeTraciResponse.Content;

                        if (_routes.TryGetValue(id, out _))
                        {
                        }
                        else
                        {
                            _routes.Add(id, newRoute);
                        }
                    }

                    _isRoutesChangedSinceLastGet = false;
                }

                return _routes;
            }
            set
            {
                _routes = value;
                _isRoutesChangedSinceLastGet = true;
            }
        }
        
        private Queue<Vehicle> VehiclePool { get; } = new Queue<Vehicle>(1500);
        [field: SerializeField] public SumoNetworkData SumoNetworkData { private get; set; }

        public int PoolSize { private get; set; } = 1000;
        [field: SerializeField] public float StepLength { get; set; }

        [field: SerializeField] public string SumocfgFile { get; set; }

        /// <summary> Check to use sumo gui. Drops efficiency. </summary>
        [field: SerializeField]
        public bool UseSumoGui { get; set; } = DEFAULT_USE_SUMO_GUI;

        /// <summary> <para> The IP to connect to sumo server. Used to serve sumo to this IP as well. </para> </summary>
        [field: SerializeField]
        public string Ip { get; set; } = DEFAULT_IP;

        /// <summary> The beginning for the simulation to start at </summary>
        [field: SerializeField]
        public int BeginStep { get; set; }

        public string PathToSumoExecutables { private get; set; }

        public int NumberOfActiveVehicles { get; private set; }
        public int NumberOfVehiclesInsideContextRange { get; private set; }
        public string CurrentlyContextSubscribedObjectID { get; private set; }
    
        /// <summary> <para> The port to connect to sumo server. Used to make the served sumo to this port as well. </para> </summary>
        [field: SerializeField]
        public int RemotePort { get; set; } = DEFAULT_REMOTE_PORT;

        /// <summary> Check to display the terminal when game starts. </summary>
        [field: SerializeField]
        public bool ShowTerminal { get; set; } = DEFAULT_SHOW_TERMINAL;

        [field: SerializeField] public SharedVehicleData SharedVehicleData { private get; set; }

        private bool IsSumoPathSet { get; set; }

        #endregion Accessible Values

        #region Main Logic
        /// <summary>
        /// Expands environment variables and, if unqualified, locates the exe in the working directory
        /// or the evironment's path.
        /// </summary>
        /// <param name="exe">The name of the executable file</param>
        /// <returns>The fully-qualified path to the file</returns>
        /// <exception cref="System.IO.FileNotFoundException">Raised when the exe was not found</exception>
        public static string FindExePath(string exe)
        {
            exe = Environment.ExpandEnvironmentVariables(exe);
            if (!File.Exists(exe))
            {
                if (Path.GetDirectoryName(exe) == String.Empty)
                {
                    var ok = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User | EnvironmentVariableTarget.Process);
                    var directoriesInPath =  Regex.Split(ok, ";|:");
                     
                    foreach (var test in directoriesInPath)
                    {
                        string path = test.Trim();
                        if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                            return Path.GetFullPath(path);
                    }
                }
                return "";
            }
            return Path.GetFullPath(exe);
        }

        [Inject]
        private void Construct(SumoCommands sumoCommands)
        {
            SumoCommands = sumoCommands;
        }

        private void Awake()
        {
            AppManager = ApplicationManager.Instance;
            if (AppManager.DontUseSumo)
            {
                enabled = false;
                return;
            }

            StartupData = SimulationStartupData.Instance;
            if (StartupData && StartupData.UseStartupData)
            {
                if (StartupData.dontUseSumo)
                {
                    enabled = false;
                    return;
                }
                
                StepLength = StartupData.stepLength;
                SumocfgFile = StartupData.SumoConfigFilename;
                RedirectionMode = StartupData.redirectionMode;
                
                UseMultithreading = StartupData.useMultithreading;
                UseContextSubscription = StartupData.useContextSubscriptions;
                
                CreateFpsByVehicleCountPlot = StartupData.createFpsByVehicleCountPlot;
                CreateSimStepExecutionTimeByVehicleCountPlot = StartupData.createSimStepDelayByVehicleCount;
            }
            
            if(SumoCommands == null)
                SumoCommands = SumoCommands.Instance;

            SumoNetworkData = SumoNetworkData.Instance;
            SharedVehicleData = SharedVehicleData.Instance;

            if(UseMultithreading)
                MTaskManager = TaskManager.Instance;
            
            const string sumoPathName = "SUMO_HOME";
            var sumoHomeEnvironmentalVariable = Environment.GetEnvironmentVariable(sumoPathName);

            Debug.Log("PathToExecutable: " + PathToSumoExecutables);
            var value = sumoHomeEnvironmentalVariable + PathToSumoExecutables;

            try
            {
                const EnvironmentVariableTarget target = EnvironmentVariableTarget.Machine;
                if (sumoHomeEnvironmentalVariable != null)
                {

                }
                else
                {
                    PathToSumoExecutables = Path.Combine(Application.streamingAssetsPath,
                        RELATIVE_BUILD_PATH_TO_SUMO_EXECUTABLE_FOLDER);
                    Environment.SetEnvironmentVariable(sumoPathName,
                        Path.Combine(PathToSumoExecutables),
                        target);                    

                }

                IsSumoPathSet = true;
                Debug.Log($"Environmental Variable <{sumoPathName}> set to: {value}");
            }
            catch (SecurityException e)
            {
                IsSumoPathSet = false;
                Debug.LogError("SUMO_HOME is not set and can not be set. \n" + e.StackTrace);
            }

            // Assign Step length to shared vehicle data so each Vehicle can use it (for VehicleMover).
            SharedVehicleData.SimulationStepLength = StepLength;

            if (SumocfgFile == null) throw new Exception("No sumo cfg file selected");

            Debug.Log("Cfg used is             :" + SumocfgFile);
            GetUsedFilesFromConfigurationXml();

            PopulateVehicleQueue(PoolSize);
        }

        private string _sumoPath;

        private ProcessExecutor _sumoProcessExecutor;


        private Task<bool> RunSumoExecutable()
        {
            var tcs = new TaskCompletionSource<bool>(); 
            var argsStr = " -c " + "\"" + SumocfgFile + "\"" + " --remote-port " + RemotePort +
                          " --step-length " + StepLength +
                          " --start  " + "  --num-clients " + NumberOfConnections;
            var sumoExecutable = "/usr/local/bin/sumo";

            //   UseSumoGui ? 
            //      Path.Combine(PathToSumoExecutables, "sumo-gui"):
            //     Path.Combine(PathToSumoExecutables, "sumo");
            Debug.Log("SumoExecutaable: " + sumoExecutable);

            var mode = ProcessExecutor.RedirectionMode.None;
            var captureStdout = false;
            var captureStderr = false;
                        
            switch (RedirectionMode)
            {
                case SumoProcessRedirectionMode.NoRedirection:
                    mode = ProcessExecutor.RedirectionMode.None;
                    break;
                case SumoProcessRedirectionMode.CaptureStderr:
                    mode = ProcessExecutor.RedirectionMode.UseHandlers;
                    captureStderr = true;
                    break;
                case SumoProcessRedirectionMode.CaptureStdout:
                    mode = ProcessExecutor.RedirectionMode.UseHandlers;
                    captureStdout = true;
                    break;
                case SumoProcessRedirectionMode.CaptureNone:
                    mode = ProcessExecutor.RedirectionMode.UseHandlers;
                    captureStderr = false;
                    captureStdout = false;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            DataReceivedEventHandler stdoutHandler;
            DataReceivedEventHandler stderrHandler;
            
            if (captureStdout)
                stdoutHandler = (o, e) => { Debug.Log(e.Data);};
            else
                stdoutHandler = (o, e) => { };

            if (captureStderr)
                stderrHandler = SumoProcess_ErrorDataReceived;
            else
                stderrHandler = (o, e) => { };
            
            _sumoProcessExecutor = new ProcessExecutor(sumoExecutable)
            {
                Args = new[] {argsStr},
                Mode = mode,
                StderrHandler = stderrHandler,
                StdoutHandler = stdoutHandler,
                ExitHandler = ( sender,  e) =>
                {
                    tcs.SetResult(true);
                },
                WaitForExit = false
            };
            _sumoProcess = _sumoProcessExecutor.Execute();

            return tcs.Task;
        }
       
        /// <summary>
        /// Serves Sumo at this instance IP and this instance RemotePort
        /// </summary>
        public async void ServeSumo()
        {
            await RunSumoExecutable();
        }

        private void SumoProcess_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!CaptureSumoProcessErrors)
                return;

            
            var message = $"SUMO error output:\n {e.Data}\n\n Do you wish to terminate simulation?";
            Debug.LogError(message);
//            const string caption = "Error Detected in Sumo";
//            const MessageBoxButtons buttons = MessageBoxButtons.YesNo;
//
//            // Displays the MessageBox.
//            var result = MessageBox.Show(message, caption, buttons);
//            if (result == DialogResult.Yes)
//                Terminate();
        }

        private async Task<bool> ConnectToSumoAsync()
        {
            bool result;
            TraCIClient = new TraCIClient();
            try
            {
                await TraCIClient.ConnectAsync(Ip, RemotePort);
                result = true;
            }
            catch (Exception e)
            {
                Debug.LogError("On client connect exception " + e);
                result = false;
            }

            return result;
        }

        public bool IsConnectionInitialized { get; private set; }

        /// <summary>
        /// Connects to this instance IP and Remote Port.
        /// </summary>
        public void ConnectToSumo()
        {
            if (!IsSumoPathSet)
            {
                Debug.LogError("ERROR::SumoClient::ConnectToSumo() SUMO_PATH is not set. No use connecting to sumo.");
                return;
            }

            var hasConnectionInitialised = InitialiseConnection();

            // Add subscription handlers
            if (hasConnectionInitialised)
            {
                VehicleCommands.Subscribe("ignored", 0, 10000, VehicleGlobalVariablesSubscriptionList);
                
                // When using context subscription we care about vehicles that arrive to their destination and are inside
                // the context range so we can add them to vehicles that exited the context range.
                if(UseContextSubscription)
                    SimulationSubscriptionList = new List<byte>
                    {
                        TraCIConstants.VAR_ARRIVED_VEHICLES_IDS
                    };
                else
                    SimulationSubscriptionList = new List<byte>
                    {
                        TraCIConstants.VAR_DEPARTED_VEHICLES_IDS, 
                        TraCIConstants.VAR_ARRIVED_VEHICLES_IDS
                    };

                // By subscribing to simulation commands now (I believe) that arrived vehicles will be updated before
                // using them to add them to vehicles that exited context range.
                SimulationCommands.Subscribe("ignored", 0, 10000, SimulationSubscriptionList);
                
                if (UseContextSubscription) // Either use context or variable subscriptions for polling the vehicle data.
                    TraCIClient.LaneContextSubscription += TraCIClient_LaneContextSubscription;

                // When doing context subscription we do not need to subscribe to this subscription unless
                // we want to collect information about the amount of active vehicles
                if(!UseContextSubscription || (CreateFpsByVehicleCountPlot || CreateSimStepExecutionTimeByVehicleCountPlot))
                    TraCIClient.VehicleSubscription += TraCIClient_VehicleSubscription;
                
                TraCIClient.SimulationSubscription += TraCIClient_SimulationSubscription;

                IsConnectionInitialized = true;
                ConnectionInisialized?.Invoke(this, EventArgs.Empty);
            }
            
            // No use setting order of connections if the number of connections is only one
            if (NumberOfConnections > 1)
            {
                lock (SumoCommands.ClientLock)
                {
                    ControlCommands.SetOrder(Order);
                }
            }

            lock (SumoCommands.ClientLock)
            {
                ControlCommands.SimStep(BeginStep);
            }
        }

        // Use this for initialization
        private bool InitialiseConnection()
        {
            Debug.Log("Initialising Connection at address " + Ip + ":" + RemotePort);
            var connectToSumoTask = Task.Run(async () => await ConnectToSumoAsync());
            connectToSumoTask.Wait();

            IsConnectionInitialized = connectToSumoTask.Result;

            // Initialise command objects
            ControlCommands = new ControlCommands(TraCIClient);
            VehicleCommands = new VehicleCommands(TraCIClient);
            SimulationCommands = new SimulationCommands(TraCIClient);
            JunctionCommands = new JunctionCommands(TraCIClient);
            RouteCommands = new RouteCommands(TraCIClient);
            LaneCommands = new LaneCommands(TraCIClient);
            VehicleTypeCommands = new VehicleTypeCommands(TraCIClient);

            // Associate SumoCommands
            if (SumoCommands != null)
            {
                SumoCommands.ControlCommands = ControlCommands;
                SumoCommands.VehicleCommands = VehicleCommands;
                SumoCommands.SimulationCommands = SimulationCommands;
                SumoCommands.JunctionCommands = JunctionCommands;
                SumoCommands.RouteCommands = RouteCommands;
                SumoCommands.LaneCommands = LaneCommands;
                SumoCommands.VehicleTypeCommands = VehicleTypeCommands;
            }

            // Associate  Dictionaries with ScriptableObject NetworkData dictionaries
            if (SumoNetworkData != null)
            {
                SumoNetworkData.VehiclesDepartedShared = VehiclesDepartedShared;
                SumoNetworkData.VehiclesArrivedShared = VehiclesArrivedShared;
                SumoNetworkData.VehiclesLoadedShared = VehiclesActiveShared;

                SumoNetworkData.VehiclesEnteredContextRange = VehiclesEnteredContextRange;
                SumoNetworkData.VehiclesExitedContextRange = VehiclesExitedContextRange;
                SumoNetworkData.VehiclesInContextRange = VehiclesInContextRange;
            }

            Routes = new Dictionary<string, Route>();
            Junctions = new Dictionary<string, Junction>();
            Lanes = new Dictionary<string, Lane>();
            /* Execute the get to cache routes on init */
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Routes.ContainsKey("1");
            Debug.Log("RouteIDs in the current doc");
            RouteIds.ForEach(Debug.Log);

            return IsConnectionInitialized;
        }

        public Dictionary<string, Junction> Junctions { private get; set; }

        private void StartSimulationStepTask()
        {
            TaskManager.StartAsRepeatingFunction(() => ControlCommands.SimStep(), (int) (StepLength * 1000f),
                SumoCommands.ClientLock);
        }

        private bool IsFirstStep { get; set; } = true;


        public float CurrentSimulationStepExecutionTime { get; private set; }
        private readonly Stopwatch _sw = new Stopwatch();
        public void MakeSimulationStep()
        {
            if (!IsSumoPathSet) return;

            //var simStepThread = new Thread(SimulationStepCaller);
            if (IsFirstStep)
            {
                IsFirstStep = false;

                if (!UseContextSubscription)
                {                
                    var loadedVehicles = SimulationCommands.GetLoadedIDList("ignored").Content;
                    loadedVehicles.ForEach(AddVehicleToDeparted);
                }

                if (UseMultithreading) 
                    StartSimulationStepTask();
            }

           
            _sw.Restart();
            if (!UseMultithreading) ControlCommands.SimStep();
            _sw.Stop();

            if (CreateSimStepExecutionTimeByVehicleCountPlot)
            {
                CurrentSimulationStepExecutionTime = _sw.ElapsedMilliseconds;
                _simStepExecutionTimes.Add(CurrentSimulationStepExecutionTime);
                _vehicleCountForSimStepPlotList.Add(NumberOfActiveVehicles);
            }
        }

        private void AddVehicleToDeparted(string id)
        {
            // if using context subscriptions then code execution should never get in this method
            // because vehicles make variable subscriptions here. We do not care about departed vehicles when
            // using context subscription.
            Assert.IsFalse( UseContextSubscription );
            
            if (VehiclesDepartedShared.ContainsKey(id)) 
                return;

            Position2D pos;
            float angle;

            lock (SumoCommands.ClientLock)
            {
                // No additional subscriptions should happen when using context subscription
                Assert.IsFalse( UseContextSubscription );
                VehicleCommands.Subscribe(id, 0, 10000000, _vehicleSubscriptionList);
                pos = VehicleCommands.GetPosition(id).Content;
                angle = (float) VehicleCommands.GetAngle(id).Content;
            }

            
//            var speed = (float) VehicleCommands.GetSpeed(id).Content;
//            var acceleration = (float) VehicleCommands.GetAcceleration(id).Content;
//            var length = (float) VehicleCommands.GetLength(id).Content;
//            var width = (float) VehicleCommands.GetWidth(id).Content;
//            var height = (float) VehicleCommands.GetHeight(id).Content;
//            var maxAcceleration = (float) VehicleCommands.GetAccel(id).Content;
//            var signal = VehicleCommands.GetSignals(id).Content;
//            var edgeId = VehicleCommands.GetRoadID(id).Content;
//            var routeId = VehicleCommands.GetRouteID(id).Content;
//            var laneId = VehicleCommands.GetRouteID(id).Content;
//            var laneIndex = VehicleCommands.GetLaneIndex(id).Content;
//            var edgeIds = VehicleCommands.GetRoute(id).Content;

            var v = RetrieveVehicleInstance();
            v.Instantiate(id);
            
            v.VehicleState = Vehicle.SimulationState.Departed;
            v.SetPositionFromRawPosition2D(pos);
            v.Angle = angle;
            
//            v.Speed = speed;
//            v.Acceleration = acceleration;
//            v.Length = length;
//            v.Width = width;
//            v.Height = height;
//            v.MaxAcceleration = maxAcceleration;
//            v.Signal = signal;
//            v.EdgeID = edgeId;
//            v.RouteID = routeId;
//            v.LaneID = laneId;
//            v.LaneIndex = laneIndex;
//            v.EdgeIDs = edgeIds;
//            v.StepsInSimulation = 1;

            // Add it to vehicles loaded as-well in order to update the same reference

            VehiclesDepartedShared.Add(id, v);
            
            if (!VehiclesActiveShared.ContainsKey(id))
                VehiclesActiveShared.Add(id, v);
        }

        /// <summary>
        /// Retrieve a vehicle instance.
        /// </summary>
        /// <returns></returns>
        private Vehicle RetrieveVehicleInstance()
        {
            if(VehiclePool.Any())
                return VehiclePool.Dequeue();
            
            /* If the queue is empty then we must repopulate it to support more vehicles */
            PopulateVehicleQueue(PoolSize);
            return VehiclePool.Dequeue();
        }

        #endregion Main Logic 

        #region  Subscription Callbacks

        private List<string> _departedVehicleIDs;
        private List<string> _arrivedVehicleIDs;
        private List<string> _loadedVehicleIDs;

        private List<string> _vehiclesThatEnteredContextRangeIDs;
        private List<string> _vehiclesThatExitedContextRangeIDs;
        
        private void TraCIClient_SimulationSubscription(object sender, SubscriptionEventArgs e)
        {
            if (UseMultithreading)
                MTaskManager.QueueAction(() => HandleSimulationSubscription(e));
            else
                HandleSimulationSubscription(e);
        }


        private void TraCIClient_VehicleSubscription(object sender, SubscriptionEventArgs e)
        {
            var varSubEvents = (VariableSubscriptionEventArgs) e;
            // We queue the action since we want it to be called in main thread. 
            // This is because we can't change transform or anything game related in another thread
            if (UseMultithreading)
                MTaskManager.QueueAction(() => HandleVehicleSubscription(varSubEvents));
            else
                HandleVehicleSubscription(varSubEvents);
        }


        private void TraCIClient_LaneContextSubscription(object sender, ContextSubscriptionEventArgs e)
        {
            if (UseMultithreading)
                MTaskManager.QueueAction(() => HandleLaneContextSubscription(e));
            else
                HandleLaneContextSubscription(e);
        }


        private void HandleSimulationSubscription(SubscriptionEventArgs e)
        {
            
            // Assert to check the bare minimum responses are being subscribed to
            Assert.IsTrue(
                UseContextSubscription && e.Responses.Count() == 1 ||
                         !UseContextSubscription && e.Responses.Count() == 2
                );
            
            foreach (var responseObj in e.Responses)
            {
                var respInfo = responseObj as IResponseInfo;
                switch (respInfo?.Variable)
                {
                    case TraCIConstants.VAR_DEPARTED_VEHICLES_IDS:
                        // We don't care about departed vehicles in Context subscription
                        Assert.IsFalse( UseContextSubscription );
                        
                        _departedVehicleIDs = ((TraCIResponse<List<string>>) responseObj).Content;
                        break;
                    case TraCIConstants.VAR_ARRIVED_VEHICLES_IDS:
                        _arrivedVehicleIDs = ((TraCIResponse<List<string>>) responseObj).Content;
                        break;
                    case TraCIConstants.ID_COUNT:
                        Debug.Log("Active Vehicles: " + ((TraCIResponse<int>) responseObj).Content);
                        break;
                    default:
                        Debug.Log("ERROR::SumoClient::TraCIClient_SimulationSubscription No such VAR: " +
                                  respInfo?.Variable);
                        break;
                }
            }
                       
            VehiclesArrivedShared.Clear();

            if (UseContextSubscription) 
                return;
            
            Assert.IsFalse(UseContextSubscription);
            
            VehiclesDepartedShared.Clear();
            
            // The rest are only useful if we have vehicle variable subscription
            _departedVehicleIDs?.ForEach(AddVehicleToDeparted);
            
            _arrivedVehicleIDs?.ForEach(id =>
            {
                try
                {
                    var retrievedVehicle = VehiclesActiveShared[id];
                    VehiclesArrivedShared.Add(id, retrievedVehicle);
                    VehiclesActiveShared.Remove(id);
                    VehiclePool.Enqueue(retrievedVehicle);
                    
                    // I change this state in VehicleSimulator
                    //retrievedVehicle.VehicleState = Vehicle.SimulationState.Arrived;
                }
                catch (KeyNotFoundException exception)
                {
                    Debug.LogError(
                        $"Vehicle with id {id} was never loaded. Better check why that happens ;) \n{exception.StackTrace}");
                }
            });
        }



        private void HandleLaneContextSubscription(ContextSubscriptionEventArgs e)
        {
            NumberOfVehiclesInsideContextRange = 0;
            
            Assert.IsTrue(UseContextSubscription);
            
            var variableSubscriptionByObjectId = e.VariableSubscriptionByObjectId;
            var vehiclesFoundThisSimStep = new HashSet<string>();
            
            VehiclesExitedContextRange.Clear();
            
            // Update VehiclesEnteredContextRange, VehiclesInContextRange, VehiclesExitedContextRange
            foreach (var vehicleId in variableSubscriptionByObjectId.Keys)
            {
                NumberOfVehiclesInsideContextRange++;

                // -------------- UPDATE DICTIONARIES and GET VEHICLE INSTANCE (or create instance) -------------- //
                Vehicle vehicle = null;
                var vehicleJustEntered = false;

                
                if (VehiclesEnteredContextRange.ContainsKey(vehicleId))
                {
                    VehiclesEnteredContextRange.Remove(vehicleId);
                }
                else if (!VehiclesInContextRange.ContainsKey(vehicleId))
                {
                    vehicleJustEntered = true;
                    vehicle = RetrieveVehicleInstance();
                    
                    vehicle.Instantiate(vehicleId);
                    vehicle.SubscriptionState = Vehicle.ContextSubscriptionState.JustEnteredContextRange;
                    VehiclesEnteredContextRange[vehicleId] = vehicle;
                    VehiclesInContextRange[vehicleId] = vehicle;
                }
                
                if (!vehicleJustEntered)
                {
                    vehicle = VehiclesInContextRange[vehicleId];
                    vehicle.SubscriptionState = Vehicle.ContextSubscriptionState.InsideContextRange;
                }

                // Add retrieved vehicle to vehicles found 
                vehiclesFoundThisSimStep.Add(vehicleId);
                
                // ---------------------------    SET SUBSCRIPTION RESULTS  ---------------------------- //
                // Get subscription variable results
                var subscriptionResultsResponses = variableSubscriptionByObjectId[vehicleId].ResponseByVariableCode;
                var pos = subscriptionResultsResponses[TraCIConstants.VAR_POSITION].GetContentAs<Position2D>();
                var angle = subscriptionResultsResponses[TraCIConstants.VAR_ANGLE].GetContentAs<float>();
                
                // Assert that subscription results are only 2 (the bare minimum we care about)
                Assert.AreEqual(subscriptionResultsResponses.Count, 2);
                
                
                vehicle.SetPositionFromRawPosition2D(pos);
                vehicle.Angle = angle;
                
            }

            foreach (var id in VehiclesInContextRange.Keys)
            {
                if (!vehiclesFoundThisSimStep.Contains(id))
                {
                    var v = VehiclesInContextRange[id];
                    VehiclesExitedContextRange[id] = v;
                    v.SubscriptionState = Vehicle.ContextSubscriptionState.ExitedContextRange;
                }                
            }

            foreach (var v in VehiclesExitedContextRange.Keys) 
                VehiclesInContextRange.Remove(v);

            foreach (var id in _arrivedVehicleIDs)
            {
                if (VehiclesInContextRange.TryGetValue(id, out var vehicle))
                {
                    VehiclesExitedContextRange[id] = vehicle;
                    VehiclesInContextRange.Remove(id);
                }                
            }

            CurrentlyContextSubscribedObjectID = e.ObjectId;
        }


        private void HandleVehicleSubscription(VariableSubscriptionEventArgs e)
        {
            // This handler should not execute for Context subscriptions because no data received from here are important
            // unless we want to create logs. In that case we care about the amount 
            Assert.IsFalse(
                UseContextSubscription && !( CreateFpsByVehicleCountPlot || CreateSimStepExecutionTimeByVehicleCountPlot)
                );
            
            Assert.IsFalse(e.ObjectId != "ignored" && UseContextSubscription );
                        
            var vehicleId = e.ObjectId;

            // Global VAR subscription list
            if (vehicleId == "ignored")
            {
                
                Assert.AreEqual(e.Responses.Count(), 1);
                
                foreach (var responseObj in e.Responses)
                {
                   
                    var responseInfo = (responseObj as IResponseInfo);
                    switch (responseInfo?.Variable)
                    {
                        case TraCIConstants.ID_COUNT:
                        {
                            NumberOfActiveVehicles = responseInfo.GetContentAs<int>();
                            break;
                        }
                        default:
                        {
                            Debug.LogError("Command " + responseInfo?.Variable +
                                           " Not handled in TraCIClient_VehicleSubscription");
                            break;
                        }
                    }
                }

                return;
            }


            // Below this line everything should not be executed when dealing with context subscription, no matter the case
            Assert.IsFalse( UseContextSubscription );
            
            var vehicleRetrieved = VehiclesActiveShared[vehicleId];

            vehicleRetrieved.StepsInSimulation++;

            if (vehicleRetrieved == null)
            {
                try
                {
                    throw new Exception(
                        $"SumoClient::TraCIClient_VehicleSubscription. No vehicle with {vehicleId} retrieved");
                }
                catch
                {
                    return;
                }
            }

            foreach (var responseObj in e.Responses)
            {
                var responseInfo = responseObj as IResponseInfo;

                Assert.IsNotNull(responseInfo);   
                
                switch (responseInfo.Variable)
                {
                    case TraCIConstants.VAR_SPEED:
                        // Returns the speed of the named vehicle within the last step [m/s]; error value: -2^30
                        vehicleRetrieved.Speed = responseInfo.GetContentAs<float>();
                        break;
                    case TraCIConstants.VAR_ACCELERATION: // Returns the acceleration in the previous time step [m/s^2]	
                        vehicleRetrieved.Acceleration = responseInfo.GetContentAs<float>();
                        break;
                    case TraCIConstants.VAR_ANGLE: 
                        // Returns the angle of the named vehicle within the last step [°]; error value: -2^30
                        vehicleRetrieved.Angle = responseInfo.GetContentAs<float>();
                        break;
                    case TraCIConstants.VAR_POSITION:
                        var pos = responseInfo.GetContentAs<Position2D>();
                        vehicleRetrieved.SetPositionFromRawPosition2D(pos);
                        break;
                    case TraCIConstants.VAR_SIGNALS:
                        vehicleRetrieved.Signal = responseInfo.GetContentAs<int>();
                        break;
                    case TraCIConstants.VAR_LANE_ID: 
                        // Returns the id of the lane the named vehicle was at within the last step; error value: ""
                        vehicleRetrieved.LaneId = responseInfo.GetContentAs<string>();
                        break;
                    case TraCIConstants.VAR_LANE_INDEX: 
                        // Returns the index of the lane the named vehicle was at within the last step; error value: -2^30
                        vehicleRetrieved.LaneIndex = responseInfo.GetContentAs<int>();
                        break;
                    case TraCIConstants.VAR_EDGES: // Returns the ids of the edges the vehicle's route is made of
                        vehicleRetrieved.EdgeIDs = responseInfo.GetContentAs<List<string>>();
                        break;
                    case TraCIConstants.VAR_ROAD_ID:
                        // Returns the id of the edge the named vehicle was at within the last step; error value: ""
                        vehicleRetrieved.EdgeId = responseInfo.GetContentAs<string>();
                        break;
                    case TraCIConstants.VAR_ROUTE:
                        //vehicleRetrieved.RouteID = ((TraCIResponse<List<string>>)responseObj).Content;
                        break;
                    case TraCIConstants.VAR_ROUTE_ID: // Returns the id of the route of the named vehicle
                        vehicleRetrieved.RouteId = responseInfo.GetContentAs<string>();
                        break;
                    case TraCIConstants.VAR_ROUTE_INDEX:
                        // Returns the index of the current edge within the vehicles route or -1 if the vehicle has not yet departed
                        throw new NotImplementedException("VAR_ROUTE_INDEX not supported");
                    case TraCIConstants.VAR_ROUTE_VALID:
                        throw new NotImplementedException("VAR_ROUTE_VALID not supported");              
                    default:
                        Debug.LogError("Command " + responseInfo.Variable +
                                       " Not handled in TraCIClient_VehicleSubscription");
                        break;
                }
            }
        }

        public void Terminate()
        {
            ControlCommands.Close();
            if (_sumoProcess != null && !_sumoProcess.HasExited)
            {
                _sumoProcess.Kill();
                _sumoProcess.Close();
            }
            Application.Quit();
        }

        #endregion  Subscription Callbacks

        #region Performance, logs and plots

        private readonly List<int> _vehicleCountFpsTestList = new List<int>();
        private readonly List<float> _fpsCountList = new List<float>();
        private readonly List<int> _vehicleCountForSimStepPlotList = new List<int>();
        private readonly List<float> _simStepExecutionTimes = new List<float>();
        [field: SerializeField] public bool CreateFpsByVehicleCountPlot { get; set; }
        [field: SerializeField] public bool CreateSimStepExecutionTimeByVehicleCountPlot { get; set; }
        private void Start()
        {
            /* The update in this class is only to measure the fps count by the
             vehicle count. By disabling this Monobehaviour we make sure that Update()
             and OnDestroy() doesn't get called. */
            enabled = CreateFpsByVehicleCountPlot;
            _fpsNextPeriod = Time.realtimeSinceStartup + FPS_MEASURE_PERIOD;
        }

        private const float FPS_MEASURE_PERIOD = 0.5f;
        private int _fpsAccumulator;
        private float _fpsNextPeriod;
        public float CurrentFps { get; private set; }
        private float _updateStartUpTime;
        private bool _isFirstUpdate = true;

        /// <summary>
        ///  Used to create Graph with fps and vehicle count
        /// </summary>
        private void Update()
        {
            //Debug.Log(_sumoProcessExecutor?.StdoutReader?.ReadToEnd());

            if (_isFirstUpdate)
            {
                _updateStartUpTime = Time.realtimeSinceStartup;
                _isFirstUpdate = false;
            }

            // measure average frames per second
            _fpsAccumulator++;
            if (Time.realtimeSinceStartup - _updateStartUpTime > _fpsNextPeriod)
            {
                CurrentFps = _fpsAccumulator / FPS_MEASURE_PERIOD;
                _fpsAccumulator = 0;
                _fpsNextPeriod += FPS_MEASURE_PERIOD;

                _vehicleCountFpsTestList.Add(NumberOfActiveVehicles);
                _fpsCountList.Add(CurrentFps);
            }
        }

        /// <summary>
        /// Used to create the log files and their plots
        /// </summary>
        private void OnApplicationQuit()
        {
            if (!CreateFpsByVehicleCountPlot && !CreateSimStepExecutionTimeByVehicleCountPlot)
                return;
            
            
            var directory = Application.dataPath + @"\logs\";
            
            // Create python context to execute the python script that creates the plots
            var scriptPath = Application.streamingAssetsPath.Replace("/", @"\") +
                             @"\python-scripts\csv2plot2D.py";

            var pythonScriptExecutor = new PythonScriptExecutor(scriptPath)
            {
                WaitForExit = true,
                Mode = ProcessExecutor.RedirectionMode.RedirectStreams,
                PythonExecutablePath = @"C:\Users\VR_Lab\.conda\envs\tomis-pycharm\python.exe"
            };

            const string textFilePostfix = ".txt";
            if (CreateFpsByVehicleCountPlot)
            {
                const string filename = "fps_count-by-vehicle_number";
                var cols = new[] {"vehicle_count", "fps"};
                
                var filenameFpsByVehCount = FileHelper.GetAvailableFileName(directory, filename,
                    textFilePostfix, out _ );
                
                TomisUtils.Logger.Logger.CreateCsvWithColumns(filenameFpsByVehCount.Replace("\"",  ""),
                    new Tuple<string, IEnumerable<int>>  (cols[0], _vehicleCountFpsTestList),
                    new Tuple<string, IEnumerable<float>>(cols[1], _fpsCountList));
                
                
                var argsStr =$"--filename {filenameFpsByVehCount} " +
                             "--delimiter , " +
                             $"--columns {cols[0]} {cols[1]} " +
                             "--scatterplot " +
                             "--line-plot-mean " +
                             "--bar-plot-mean " +
                             "--store-dataframe-description";
                
                pythonScriptExecutor.Args = new [] {argsStr};
                pythonScriptExecutor.Execute();

                var stdoutStr = pythonScriptExecutor.StdoutReader.ReadToEnd();
                var stderrStr = pythonScriptExecutor.StderrReader.ReadToEnd();
                
                Debug.Log("Executed: " + Path.GetFileName(scriptPath) + " with args " + argsStr);
                Debug.Log("Python stdout: " + stdoutStr);
                Debug.Log("Python stdout: " + stderrStr );
            }

            if (CreateSimStepExecutionTimeByVehicleCountPlot)
            {
                const string filename = "sim_step_time-by-vehicle_number";
                var cols = new[] {"vehicle_count", "sim_step_execution_time"};
                
                var filenameSimStepDelayByVehCount = FileHelper.GetAvailableFileName(directory, filename, 
                    textFilePostfix, out _);
                
                
                TomisUtils.Logger.Logger.CreateCsvWithColumns(filenameSimStepDelayByVehCount,
                    new Tuple<string, IEnumerable<int>>  (cols[0], _vehicleCountForSimStepPlotList),
                    new Tuple<string, IEnumerable<float>>(cols[1], _simStepExecutionTimes));
                
                var argsStr = $"--filename {filenameSimStepDelayByVehCount} " +
                              "--delimiter , " +
                              $"--columns {cols[0]} {cols[1]} " +
                              "--scatterplot " +
                              "--line-plot-mean " +
                              "--bar-plot-mean " +
                              "--store-dataframe-description";
                
                pythonScriptExecutor.Args = new [] {argsStr};
                pythonScriptExecutor.Execute();     
                
                var stdoutStr = pythonScriptExecutor.StdoutReader.ReadToEnd();
                var stderrStr = pythonScriptExecutor.StderrReader.ReadToEnd();
                
                Debug.Log("Executed: " + Path.GetFileName(scriptPath) + " with args " + argsStr);
                Debug.Log("Python stdout: " + stdoutStr);
                Debug.Log("Python stdout: " + stderrStr);             
            }
        }

        #endregion

        #region Road Network Related

        /*                 MESSAGE FORMAT FROM CLIENT TO SUMO 
        //			0                 7 8               15
        //			+--------------------------------------+
        //			| Message Length including this header |     -> So we must add whatever the length +2
        //			+--------------------------------------+
        //			|      (Message Length, continued)     |     -> +2 more if the length is bigger than 2 bytes
        //			+--------------------------------------+  \
        //			|     Length        |    Identifier    |  |  -> We can send a bunch of commands each simulation step
        //			+--------------------------------------+   > Command_0
        //			|           Command_0 content          |  |
        //			+--------------------------------------+  /
        //				...
        //			+--------------------------------------+  \
        //			|     Length        |    Identifier    |  |
        //			+--------------------------------------+   > Command_n-1
        //			|          Command_n-1 content         |  |
        //			+--------------------------------------+  /
        */
        /// <summary>
        /// Extract info from the traci response to get all the edges in a dictionary.
        /// </summary>
        /// <param name="getEdgeTraciResponse"> The TraciResponse to extract info from </param>
        /// <returns> A Dictionary with keys Edge id and value Edge </returns>
        // ReSharper disable once UnusedMember.Local
        private Dictionary<string, Edge> GetEdgesFromTraciResponse(TraCIResponse<List<string>> getEdgeTraciResponse)
        {
            var edgeIDs = getEdgeTraciResponse.Content;
            var edgeDict = new Dictionary<string, Edge>();
            foreach (var e in edgeIDs)
            {
                var edge = ScriptableObject.CreateInstance<Edge>();
                edge.Instantiate(e);
                edgeDict.Add(e, edge);
            }

            return edgeDict;
        }

        private void GetUsedFilesFromConfigurationXml()
        {
            var doc = XDocument.Load(SumocfgFile);
            var configurationElement = doc.Element("configuration");
            var inputElement = configurationElement?.Element("input");

            var netFileElement = inputElement?.Element("net-file");
            var additionalFilesElement = inputElement?.Element("additional-files");
            var routeFiles = inputElement?.Element("route-files");

            _netXmlFile = (string) netFileElement?.Attribute("value");
            Debug.Log("Files used in configuration file");
            Debug.Log("================================");
            Debug.Log("NetXmlFile       : " + _netXmlFile);
            Debug.Log("Route files      : " + (string) routeFiles?.Attribute("value"));
            Debug.Log("Additional files : " + (string) additionalFilesElement?.Attribute("value"));
        }

        private void CollectLaneDataFromNetXml()
        {
            var doc = XDocument.Load(_netXmlFile);
            // ReSharper disable once PossibleNullReferenceException
            var edgesElements = doc.Root.Elements("edge");
            foreach (var edgeElement in edgesElements)
            {
                /* http://sumo.sourceforge.net/userdoc/Networks/SUMO_Road_Networks.html */
                /* <lane id="<ID>_1" index="1" speed="<SPEED>" length="<LENGTH>" shape="0.00,498.35,2.00 248.50,498.35,3.00"/> */
                var laneElements = edgeElement.Elements("lane");
                foreach (var laneElement in laneElements)
                {
                    var newLane = ScriptableObject.CreateInstance<Lane>();
                    newLane.Instantiate((string) laneElement.Attribute("id"));
                    newLane.Index = (uint) laneElement.Attribute("index");
                    newLane.Length = (float) laneElement.Attribute("length");
                    var vectorPoints = new List<Vector2>();
                    var shapePositions = ((string) laneElement.Attribute("shape")).Split(null);

                    foreach (var t in shapePositions)
                    {
                        var point = t.Split(',');
                        vectorPoints.Add(new Vector2(float.Parse(point[0], CultureInfo.InvariantCulture.NumberFormat),
                            float.Parse(point[1], CultureInfo.InvariantCulture.NumberFormat)));
                        newLane.ShapeVertexPoints = vectorPoints;
                        /* UseCultureInfo.InvariantCulture.NumberFormat for . decimal mark */
                    }

                    Lanes.Add(newLane.ID, newLane);
                }
            }
        }

        // ReSharper disable once UnusedMember.Local
        private void CollectDataForRoadNetwork()
        {
            /* JUNCTIONS */
            var junctionTraciResponse = JunctionCommands.GetIdList();
            var junctionIDs = junctionTraciResponse.Content;

            CollectLaneDataFromNetXml();
            junctionIDs.ForEach(junctionId =>
            {
                var position = (JunctionCommands.GetPosition(junctionId)).Content;
                var vectorPosition = TraCIAuxiliaryMethods.Raw2DPositionToVector3(position);
                var junctionShapeVectorPoints =
                    TraCIAuxiliaryMethods.GetVectorListFromPolygon((JunctionCommands.GetShape(junctionId)).Content);

                var junction = ScriptableObject.CreateInstance<Junction>();
                junction.Instantiate(junctionId);
                
                junction.Raw2DPosition = vectorPosition;
                junction.ShapeVertexPoints = junctionShapeVectorPoints;

                Junctions.Add(junctionId, junction);
            });

            /* NOTE */
            foreach (var lane in Lanes)
            {
                Debug.Log(lane.ToString());
            }

            /* ROUTES */
            var routeTraciResponce = RouteCommands.GetIdList();
            var routeIDs = routeTraciResponce.Content;
            // ReSharper disable once CollectionNeverQueried.Local
            var edges = new List<Edge>();
            routeIDs?.ForEach(id =>
            {
                var edgeIDs = new List<string>();

                var e = ScriptableObject.CreateInstance<Edge>();
                e.Instantiate(id);
                edgeIDs.ForEach(edgeId => edges.Add(e));
                edgeIDs = (RouteCommands.GetEdges(id)).Content;

                var route = ScriptableObject.CreateInstance<Route>();
                route.Instantiate(id);
                route.EdgeIDs = edgeIDs;
            });
            if (routeIDs != null) Debug.Log("Route IDs found : " + string.Join(",", routeIDs.ToArray()));
        }

        #endregion  Network Related
    }
    /* TIPS */
    /* to print a list string text = string.Join(",", list); */
    /* byte[] to hexadecimal string https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa */
    /* Read a number of bites from byte[] BinaryReader.ReadBytes Method (Int32) */
}