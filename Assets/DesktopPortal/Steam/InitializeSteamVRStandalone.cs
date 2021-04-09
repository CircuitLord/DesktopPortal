using System;
using System.Collections;
using System.Collections.Generic;
using DPCore;
using UnityEngine;
using Valve.VR;

public class InitializeSteamVRStandalone : MonoBehaviour
{


	public void Init() {
		
		SteamVR.InitializeStandalone(EVRApplicationType.VRApplication_Overlay);
		
		
		
	}


	private void OnApplicationQuit() {
		DisposeSteamVRConnection();
	}

	public void DisposeSteamVRConnection() {
			
		SteamVR_ActionSet_Manager.ChangeSetPriorities(false);
		SteamVR_Input.GetActionSet("/actions/main").Deactivate();
			
		OpenVR.Shutdown();

		SteamVRManager.isConnected = false;
	}
    
}
