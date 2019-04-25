

using RiseProject.Tomis.Util.TraciAuxilliary;
using RiseProject.Tomis.SumoInUnity.SumoTypes;
using UnityEditor;
using UnityEngine;

namespace RiseProject.Tomis.Editors
{
    [CustomEditor(typeof(Vehicle))]
    public class VehicleEditor : Editor
    {
        private Vehicle Vehicle => target as Vehicle;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Vehicle.SetPositionFromVector2Position(EditorGUILayout.Vector2Field(
                new GUIContent()
                {
                    text = "Raw 2D Position",
                    tooltip = "Represents the value as is it polled from Sumo"
                }, Vehicle.Raw2DPosition));

            if (GUILayout.Button("Depart a vehicle at Position2D for this Vehicle SO"))
            {
                MockTester.instance.SumoNetworkData.AddMockVehicleToDeparted(Vehicle);
                // Do not use PlaceVehicleAtPosition since the transformed is attached after it is
                // departed. The vehicle placement happens during departure in VehicleSimulator.
            }
            else if (GUILayout.Button("Apply Mock Data"))
            {
                // Sync the step length with the mock step length
                MockTester.instance.SharedVehicleData.SimulationStepLength = Vehicle.MockStep;
                Vehicle.RunningCoroutine = MockTester.instance.StartCoroutine(Vehicle.ApplyMockData());
            }
            else if (GUILayout.Button("STOP"))
            {
                // Sync the step length with the mock step length
                MockTester.instance.StopCoroutine(Vehicle.RunningCoroutine);
                PlaceVehicleAtPosition(Vehicle.MockPositions[0], Vehicle.MockAngles[0]);
            }
            else if (GUILayout.Button("Clear Mock Data"))
            {
                Vehicle.ClearMockData();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.PrefixLabel(
                new GUIContent(){
                    text = "How use editor for mock positions",
                    tooltip = "Create, edit and delete new mock positions to be used for testing vehicle movement"
                }, EditorStyles.boldLabel);
            EditorGUILayout.PrefixLabel("SO = ScriptableObject", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField(
                new GUIContent()
                {
                    tooltip = "Instructions to use editor",
                    text =
                    " • ADD    : shift + click to add mock positions and angles\n" +
                    " • EDIT   : Edit existing positions and angles by using the handles\n" +
                    " • DELETE : Select existing handle and then press delete to delete\n" +
                    " • UNDO   : ctrl + z to undo last action\n" +
                    " • REDO   : ctrl + y to redo last undo\n" +
                    " • STEP   : set mock step for the time between each mock position\n" +
                    " • START POSITION :Press P to Place the Vehicle at the first position of mock positions\n" +
                    " • TEST   : Click PLAY and click Apply Mock positions to test attached VehicleMover\n" +
                    " • STOP   : click STOP to stop the test and place vehicle back to the first mock position"
                }, EditorStyles.helpBox);
        }

        private bool _alreadyEnabled = false;

        void OnEnable()
        {
            if(!_alreadyEnabled)
                SceneView.onSceneGUIDelegate += OnSceneGUI;
            _alreadyEnabled = true;
            Vehicle.SelectedIndex = -1;
        }

       
        private bool _showGismos = true;
        // Make the contents of the window
        void ControlWindow(int windowID)
        {
            _showGismos = (GUILayout.Toggle(
                _showGismos,
                new GUIContent()
                {
                    text = " Show Mock position Gismos "
                })) ;
            if (GUILayout.Button("Exit Vehicle Mock Editor"))
            {
                SceneView.onSceneGUIDelegate -= OnSceneGUI;
                Vehicle.SelectedIndex = -1;
                _alreadyEnabled = false;
            }
            /* Order Matters. Leave DragWindow() Last */
            GUI.DragWindow();

        }

        private Rect controlWindowRect = new Rect(20, 20, 120, 50);
        private void  ShowHide()
        {
            controlWindowRect = GUILayout.Window(0, controlWindowRect, ControlWindow, "Vehicle Mock Positions");
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            ShowHide();

            if (!_showGismos)
                return;

            DrawHandlesOnMockPositions();
            DrawTargetPositionHandle();

            HandleUtility.Repaint();
            Event e = Event.current;

            if (e == null)
                return;
            else if(e.shift)
            {
                HandleAddingMockPoints();
            }
            else if (e.isKey && e.type.Equals(EventType.KeyDown))
            {
                switch (e.keyCode)
                {
                    case KeyCode.Delete:
                        {
                            GUIUtility.hotControl = 0;
                            DeleteSelected();
                            e.Use();
                            break;
                        }
                    case KeyCode.P:
                        {
                            PlaceVehicleAtPosition(Vehicle.MockPositions[0], Vehicle.MockAngles[0]);
                            e.Use();
                            break;
                        }
                }
            }
        }

        [SerializeField, HideInInspector]
        private void DeleteSelected()
        {
            if (Vehicle.SelectedIndex == -1)
                return;
            else
            {
                Undo.RecordObject(Vehicle, "Change Mock new position");
                Vehicle.MockPositions.RemoveAt(Vehicle.SelectedIndex);
                Vehicle.MockAngles.RemoveAt(Vehicle.SelectedIndex);
                Vehicle.SelectedIndex = -1;
            }

        }

        private void PlaceVehicleAtPosition(Vector3 pos, float angle)
        {
            Undo.RecordObject(Vehicle, "Update vehicle position");
            Vehicle.SetPositionFromVector2Position(TraCIAuxiliaryMethods.Vector3toVector2(pos));
            Vehicle.Angle = angle;

            if (Vehicle.AttachedVehicleTransform)
            {
                Vehicle.AttachedVehicleTransform.position = Vehicle.Position;
                Vehicle.AttachedVehicleTransform.rotation = Quaternion.Euler(new Vector3(0f, Vehicle.Angle, 0f));
            }
            else
            {
                Debug.LogError("No Vehicle Attacked to this Vehicle ScriptableObject.\n" +
                    "Make sure that the Vehicle Transform has Departed and that the transform was attached to this SO");
            }
        }

        private void HandleAddingMockPoints()
        {
            if (EditorWindow.mouseOverWindow is SceneView)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

                Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                RaycastHit hit;
                Vector3 hoveredPosition = Vector3.zero;
                if (Physics.Raycast(ray, out hit, 1000.0f))
                    hoveredPosition = hit.point;

                Handles.color = UnityEngine.Color.white;
                Handles.DrawWireCube(hoveredPosition, new Vector3(1.5f, 1.5f, 1.5f));
                HandleUtility.Repaint();
                Event e1 = Event.current;
                if (e1.type == EventType.MouseDown && e1.button == 0)
                {
                    Undo.RecordObject(Vehicle, "Add Mock  position");
                    Vehicle.MockPositions.Add(hoveredPosition);
                    Vehicle.MockAngles.Add(90f);
                    Vehicle.MockAccelerations.Add(0f);
                    Vehicle.MockSpeeds.Add(0f);
                    e1.Use();
                }
            }
        }

        private void DrawTargetPositionHandle()
        {
            Handles.color = UnityEngine.Color.red;
            Handles.DrawWireCube(Vehicle.Position, Vector3.one * 2);

            EditorGUI.BeginChangeCheck();

            Vector3 newPosition2D = Handles.PositionHandle(Vehicle.Position, Quaternion.Euler(Vector3.up * Vehicle.Angle));
            Quaternion newAngle = Handles.RotationHandle(Quaternion.Euler(Vector3.up * Vehicle.Angle), newPosition2D);

            Handles.color = UnityEngine.Color.red;
            Handles.DrawWireCube(newPosition2D, Vector3.one * 2f); // (newMockPosition, Vector3.one);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(Vehicle, "Change Mock new position");
                
                Vehicle.SetPositionFromVector2Position(TraCIAuxiliaryMethods.Vector3toVector2(newPosition2D));
                Vehicle.Angle = newAngle.eulerAngles.y;
            }
        }
        private void DrawHandlesOnMockPositions()
        {
            // Don't use foreach. We need the reference to the real MockPosition
            for (int i = 0; i < Vehicle.MockPositions.Count; i++)
            {
                EditorGUI.BeginChangeCheck();

                Vector3 newMockPosition = Handles.PositionHandle(Vehicle.MockPositions[i], Quaternion.Euler(Vector3.up * Vehicle.MockAngles[i]));
                Quaternion newMockAngle = Handles.RotationHandle(Quaternion.Euler(Vector3.up * Vehicle.MockAngles[i]), newMockPosition);


                Handles.color = i == Vehicle.SelectedIndex ? UnityEngine.Color.green : UnityEngine.Color.yellow;

                Handles.DrawSolidArc(newMockPosition, Vector3.up, Vector3.right, newMockAngle.eulerAngles.y, 1f); // (newMockPosition, Vector3.one);

                if (Handles.Button(newMockPosition, Quaternion.identity, 1f, 2f, Handles.CubeHandleCap))
                {
                    Vehicle.SelectedIndex = i;
                }

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(Vehicle, "Change Mock new position");
                    Vehicle.MockPositions[i] = newMockPosition;
                    Vehicle.MockAngles[i] = newMockAngle.eulerAngles.y;
                }
            }
        }
    }
}


