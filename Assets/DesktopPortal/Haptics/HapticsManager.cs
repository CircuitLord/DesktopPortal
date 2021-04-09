using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Text;
using DPCore;
using UnityEngine;
using Valve.VR;

namespace DesktopPortal.Haptics {
	public class HapticsManager : MonoBehaviour {

		public Transform pointer;


		protected SteamVR_Utils.RigidTransform matrixConverter;

		private void Start() {
			matrixConverter = new SteamVR_Utils.RigidTransform(transform);
		}
		
		//TODO main hand
		//virtual vr::TrackedDeviceIndex_t GetPrimaryDashboardDevice() = 0;


		public static void SendHaptics(HapticsPreset preset) {

			if (!SteamVRManager.isConnected) return;

			uint primaryIndex = OpenVR.Overlay.GetPrimaryDashboardDevice();
			


			switch (preset) {
				case HapticsPreset.UIHover:
					OpenVR.System.TriggerHapticPulse(primaryIndex, 0, 800);
					break;
			}
		}
	}


	public enum HapticsPreset {
		UIHover = 0
	}
}