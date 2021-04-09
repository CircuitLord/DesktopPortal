using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Circuits.CEvents;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
using Debug = UnityEngine.Debug;

namespace DPCore {


	[Serializable]
	public class UnityBoolEvent : UnityEvent<bool> {
	};

	[DefaultExecutionOrder(-40000)]
	public class SteamVRManager : MonoBehaviour {
		//Singleton
		public static SteamVRManager I;

		[Header("Other Components")] [SerializeField]
		public Transform lPointerTrans;

		[SerializeField] public Transform rPointerTrans;
		[SerializeField] public Transform lHandTrans;
		[SerializeField] public Transform rHandTrans;

		[SerializeField] public Transform hmdTrans;


		[SerializeField] public Transform noAnchorTrans;


		[Header("Configuration")] public float steamVRUpdateRate = 5f;


		//EVENTS
		public static Action<bool> dashboardToggledEvent;


		//PUBLIC STATES

		/// <summary>
		/// Is the manager connected with SteamVR?
		/// </summary>
		public static bool isConnected = false;

		public static uint leftHandIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
		public static uint rightHandIndex = OpenVR.k_unTrackedDeviceIndexInvalid;
		public static uint hmdIndex = OpenVR.k_unTrackedDeviceIndex_Hmd;

		public static ETrackingUniverseOrigin trackingSpace = ETrackingUniverseOrigin.TrackingUniverseStanding;

		public static List<DPGenericTracker> bodyTrackers = new List<DPGenericTracker>();


		public static bool isWearingHeadset = false;

		public static int hmdRefreshRate = 90;

		public static bool dashboardOpened = false;

		public UnityEvent onShouldConnectSteamVR = new UnityEvent();
		public UnityEvent onSteamVRConnected = new UnityEvent();
		
		public UnityEvent onTrackedDeviceConnected = new UnityEvent();
		//public UnityEvent onTrackedDeviceInteractionEnded = new UnityEvent();

		public UnityBoolEvent onHeadsetStateChanged;


		//PRIVATE
		private uint prevPrimaryDeviceIndex;

		private VREvent_t pEvent = new VREvent_t();

		private TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
		private TrackedDevicePose_t[] gamePoses = new TrackedDevicePose_t[0];


		// --------------------- UNITY FUNCTIONS ---------------------


		private void Awake() {
			I = this;
		}

		private void Start() {
			//I = this;

			StartCoroutine(InitSteamVRConnection());
			StartCoroutine(SteamVRUpdateLoop());
		}


		private void Update() {
			if (!isConnected) return;

			UpdateTrackedDevicePositions();

			/*while (PollNextEvent(ref pEvent)) {
				DigestEvent(pEvent);
			}*/

			while (PollNextEvent(ref pEvent)) {
				DigestEvent(pEvent);
			}
		}


		private void OnApplicationQuit() {
			Debug.Log("Quit requested... Shutting down...");


			//OpenVR.Shutdown();
		}


		// --------------------- OTHER ---------------------


		private void UpdateTrackedDevicePositions() {
			if (!isConnected) return;

			if (OpenVR.Compositor == null) return;


			OpenVR.Compositor.GetLastPoses(poses, gamePoses);

			if (leftHandIndex != OpenVR.k_unTrackedDeviceIndexInvalid && poses.Length > leftHandIndex) {
				var lPose = new SteamVR_Utils.RigidTransform(poses[leftHandIndex].mDeviceToAbsoluteTracking);
				lHandTrans.transform.position = lPose.pos;
				lHandTrans.transform.rotation = lPose.rot;
			}

			if (rightHandIndex != OpenVR.k_unTrackedDeviceIndexInvalid && poses.Length > rightHandIndex) {
				var rPose = new SteamVR_Utils.RigidTransform(poses[rightHandIndex].mDeviceToAbsoluteTracking);
				rHandTrans.transform.position = rPose.pos;
				rHandTrans.transform.rotation = rPose.rot;
			}

			if (poses.Length > hmdIndex) {
				var hmdPose = new SteamVR_Utils.RigidTransform(poses[hmdIndex].mDeviceToAbsoluteTracking);
				hmdTrans.position = hmdPose.pos;
				hmdTrans.rotation = hmdPose.rot;
			}
		}


		private IEnumerator InitSteamVRConnection() {
			while (true) {
				if (Process.GetProcessesByName("vrserver").Length <= 0 ||
				    Process.GetProcessesByName("SteamVR").Length <= 0) {
					yield return new WaitForSeconds(0.2f);

					Debug.Log("Starting Up SteamVR Connection...");

					//Valve.VR.SteamVR.InitializeStandalone(EVRApplicationType.VRApplication_Overlay);

					onShouldConnectSteamVR.Invoke();

					//Debug.Log("SteamVR connection successful!");
					//Debug.Log(
					//	$"SteamVR Version Data:\n{OpenVR.IVRSystem_Version}\n{OpenVR.IVRApplications_Version}\n{OpenVR.IVROverlay_Version}");


					//OLDSteamVRInputManager.I.InitializeInput();
					//isConnected = true;
					//onSteamVRConnected.Invoke();


					yield break;
				}

				yield return new WaitForSeconds(1f);
			}
		}


		readonly uint pEventSize = (uint) System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VREvent_t));

		private bool PollNextEvent(ref VREvent_t pEvent) {
			return OpenVR.System.PollNextEvent(ref pEvent, pEventSize);
		}

		private void DigestEvent(VREvent_t pEvent) {
			EVREventType type = (EVREventType) pEvent.eventType;

			switch (type) {
				
				
				case EVREventType.VREvent_ButtonPress:
					
					break;
				
				case EVREventType.VREvent_TrackedDeviceActivated:
					onTrackedDeviceConnected?.Invoke();
					break;

				case EVREventType.VREvent_TrackedDeviceUserInteractionStarted:
					onTrackedDeviceConnected?.Invoke();
					break;

				/*
				case EVREventType.VREvent_TrackedDeviceUserInteractionEnded:
					onTrackedDeviceInteractionEnded?.Invoke();
					//isWearingHeadset = false;
					break;
					*/
					
				


				case EVREventType.VREvent_DashboardActivated:
					dashboardOpened = true;
					dashboardToggledEvent.Invoke(dashboardOpened);
					//Debug.Log("open dashboard");
					break;

				case EVREventType.VREvent_DashboardDeactivated:
					dashboardOpened = false;
					dashboardToggledEvent.Invoke(dashboardOpened);
					break;
			}
		}


		/// <summary>
		/// Update loop that occasionally polls SteamVR components at <see cref="steamVRUpdateRate"/>
		/// </summary>
		/// <returns></returns>
		private IEnumerator SteamVRUpdateLoop() {
			while (!isConnected) yield return null;

			while (true) {
				
				FetchDeviceIndexes();

				yield return null;

				HandleDisplayFrequency();

				yield return null;

				FetchPointerOffset(leftHandIndex, lPointerTrans);

				yield return null;

				FetchPointerOffset(rightHandIndex, rPointerTrans);



				trackingSpace = OpenVR.Compositor.GetTrackingSpace();

				//yield return null;


				yield return new WaitForSeconds(steamVRUpdateRate / 2f);
				
				HandleHMDWearing();
				
				yield return new WaitForSeconds(steamVRUpdateRate / 2f);
				
				HandleHMDWearing();

			}
		}

		private void HandleHMDWearing() {
			EDeviceActivityLevel level = OpenVR.System.GetTrackedDeviceActivityLevel(0);

			switch (level) {
				case EDeviceActivityLevel.k_EDeviceActivityLevel_Unknown:
				case EDeviceActivityLevel.k_EDeviceActivityLevel_Idle:
				case EDeviceActivityLevel.k_EDeviceActivityLevel_Idle_Timeout:
				case EDeviceActivityLevel.k_EDeviceActivityLevel_Standby:

					if (!isWearingHeadset) break;
					isWearingHeadset = false;
					onHeadsetStateChanged.Invoke(false);
					break;
					
					
				case EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction:
					if (isWearingHeadset) break;
					isWearingHeadset = true;
					onHeadsetStateChanged.Invoke(true);
					break;
			}
		}

		private void HandleDisplayFrequency() {
			
			ETrackedPropertyError error = new ETrackedPropertyError();
			float rr = OpenVR.System.GetFloatTrackedDeviceProperty(OpenVR.k_unTrackedDeviceIndex_Hmd, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref error);


			if (error == ETrackedPropertyError.TrackedProp_Success) {
				hmdRefreshRate = (int) rr;
				Application.targetFrameRate = hmdRefreshRate;
			}
			
			//Time.fixedDeltaTime = 1f / hmdRefreshRate;
		}


		private void FetchDeviceIndexes() {
			leftHandIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
			rightHandIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
			
			//Find the body trackers
			for (uint i = 1; i < 9; i++) {
				ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass(i);

				switch (deviceClass) {
					case ETrackedDeviceClass.GenericTracker:
						
						StringBuilder serialNumber = new StringBuilder(50);
						ETrackedPropertyError pError = new ETrackedPropertyError();
						OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String, serialNumber, 50, ref pError);


						if (pError == ETrackedPropertyError.TrackedProp_Success) {
							DPGenericTracker existingTracker = bodyTrackers.Find(x => x.id == serialNumber.ToString());
						
							if (existingTracker == null) bodyTrackers.Add(new DPGenericTracker() { index = i, id = serialNumber.ToString() });

							else {
								existingTracker.index = i;
							}
							
						}
						
						break;
				}
			}
			
		}


		public void FetchPointerOffset(uint index, Transform pointer) {
			StringBuilder renderModelName = new StringBuilder(50);

			ETrackedPropertyError pError = new ETrackedPropertyError();
			OpenVR.System.GetStringTrackedDeviceProperty(index,
				ETrackedDeviceProperty.Prop_RenderModelName_String, renderModelName, 50, ref pError);

			VRControllerState_t state = new VRControllerState_t();
			RenderModel_ControllerMode_State_t rState = new RenderModel_ControllerMode_State_t();
			RenderModel_ComponentState_t compState = new RenderModel_ComponentState_t();
			bool found = OpenVR.RenderModels.GetComponentState(renderModelName.ToString(), "tip", ref state, ref rState,
				ref compState);

			if (!found) return;

			var pose = new SteamVR_Utils.RigidTransform(compState.mTrackingToComponentLocal);
			pointer.localPosition = pose.pos;
			pointer.localRotation = pose.rot;
		}


		/// <summary>
		/// Fetches the active dashboard controller device and returns the <see cref="Transform"/> for the pointer obj of it.
		/// </summary>
		/// <returns></returns>
		public Transform GetActivePointerTransform() {
			//TODO: FIX, always returning hmd
			uint primaryIndex = OpenVR.Overlay.GetPrimaryDashboardDevice();

			if (primaryIndex == leftHandIndex) {
				prevPrimaryDeviceIndex = primaryIndex;
				return lPointerTrans;
			}
			else if (primaryIndex == rightHandIndex) {
				prevPrimaryDeviceIndex = primaryIndex;
				return rPointerTrans;
			}
			else if (primaryIndex == hmdIndex) {
				prevPrimaryDeviceIndex = primaryIndex;
				return rPointerTrans;
			}

			else {
				if (prevPrimaryDeviceIndex == leftHandIndex) {
					return lPointerTrans;
				}
				else if (prevPrimaryDeviceIndex == rightHandIndex) {
					return rPointerTrans;
				}
				else {
					return rPointerTrans;
				}
			}
		}
	}

	public class DPGenericTracker {
		public uint index;
		public string id;
	}
}