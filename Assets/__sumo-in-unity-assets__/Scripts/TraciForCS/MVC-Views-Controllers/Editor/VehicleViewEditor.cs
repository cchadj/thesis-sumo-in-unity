using UnityEditor;

using RiseProject.Tomis.SumoInUnity.MVC;
using UnityEngine;


    [CustomEditor(typeof(VehicleView))]
    public class VehicleViewEditor : Editor
    {

        private VehicleView VehicleView { get { return (VehicleView)target; } }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            // TODO FIX VehicleController controller = VehicleView.GetController<VehicleController> ();

            if(GUILayout.Button("Set Speed"))
            {
                //controller.SetSpeed(VehicleView.Speed);
            }
        }

    }