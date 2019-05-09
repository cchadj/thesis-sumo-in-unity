using System;
using UnityEngine;
using UnityEditor;


public enum LampSpotlightState
{
	On,
	Off
}


public class LampSwitcher : MonoBehaviour
{
	private LampSpotlightState _lampsState;
	
	
	[field: SerializeField] private LampSpotlightState spotlightState;

	public LampSpotlightState SpotlightState
	{
		get { return spotlightState; }
		set
		{
			
			spotlightState = value;
			switch (spotlightState)
			{
				case LampSpotlightState.On:
					foreach (var lamp in transform)
					{
						((Transform) lamp).GetComponent<Light>().enabled = true;
					}		
					break;
				case LampSpotlightState.Off:
					foreach (var lamp in transform)
					{
						((Transform) lamp).GetComponent<Light>().enabled = false;
					}	
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	// Use this for initialization
	public void ToggleLights()
	{
		
	}

	// Update is called once per frame
	void Update()
	{

	}
}

#if UNITY_EDITOR

[CustomEditor(typeof(LampSwitcher))]
public class LampSwitcherEditor : Editor
{
	private LampSwitcher  LampSwitcher => target as LampSwitcher;

	public override void OnInspectorGUI()
	{
		LampSwitcher.SpotlightState =  (LampSpotlightState)EditorGUILayout.EnumPopup( " Lamp Spotlight State ", LampSwitcher.SpotlightState);
		
	}
}

#endif

