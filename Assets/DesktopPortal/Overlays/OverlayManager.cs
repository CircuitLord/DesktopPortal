using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using DesktopPortal.Demo;
using DesktopPortal.IO;
using DesktopPortal.UI;
using DPCore;
using Steamworks;
using TCD.System.TouchInjection;
using uDesktopDuplication;
using UnityEngine;
using uWindowCapture;
using Valve.VR;
using WinStuff;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.Overlays {
	public class OverlayManager : MonoBehaviour {

		public static OverlayManager I;
		
		

		[SerializeField] private Transform _overlaysHolderTransform;

		[SerializeField] private GameObject dpRenderWindowPF;



		[HideInInspector] public List<DPOverlayBase> overlays = new List<DPOverlayBase>();



		private bool isFirstUpdate = true;

		//public int allowPID;

		private float captureRateLookedAt = 1 / 45f;


		//public DPLayoutSerialized currentLayout = null;


		[SerializeField] private Texture2D desktopIcon;



		public static bool isInitialized = false;

		/// <summary>
		/// Called when all the overlays are done being loaded into the OverlayManager
		/// </summary>
		public static Action onInitialized;


		private void Start() {

			I = this;

			DPSettings.LoadConfigJson();
			//StartCoroutine(DPSettings.AutoSave());
			
			DPSettings.OnLoad(HandleMinimizeAtStartup);
 

		//	TemplateWindowItem.selectedEvent += delegate(UwcWindow window) { NewDPWindowOverlay(window); };
			WindowSelectListElement.onPressed += delegate(WindowSelectListElement element) {

				DPDesktopOverlay desktopDP;
				
				if (!element.isDesktop) {
					desktopDP = NewDPWindowOverlay(element.window);
				}

				else {
					desktopDP = NewDPMonitorOverlay(element.monitor);
				}
				
				TheBarManager.I.LaunchAppToMainSnap(desktopDP.dpAppParent.appKey);
				
			};

			
			//SteamVRManager.I.onSteamVRConnected.AddListener(LoadDefaultLayout);

			//StartCoroutine(HandleOverlayRendering());
			
			StartCoroutine(DPOverlayBase.HandleRendering());



			TouchInjector.InitializeTouchInjection();
			
			
		}

		private void OnApplicationQuit() {
			
			DPSettings.SaveSettingsJson();
			
			OpenVR.Shutdown();

			//SteamApps.
		}




		private void Update() {

			HandleNeedToProcess();
			
			//Shader.SetGlobalVector(RefViewDir, SteamVRManager.I.hmdTrans.forward);



		}

		private void HandleMinimizeAtStartup() {
			if (DPSettings.config.minimizeAtStartup) {
					
				int currentPID = System.Diagnostics.Process.GetCurrentProcess().Id;

				WinNative.EnumWindows(delegate(IntPtr wnd, IntPtr param) 					{

					int returnVal = WinNative.GetWindowThreadProcessId(wnd, out int procid);

					//We found it
					if (procid == currentPID) {
						WinNative.ShowWindow(wnd, ShowWindowCommands.Minimize);
						return false;
					}


					return true;
				}, IntPtr.Zero);
				
					
			}
		}
		

		/// <summary>
		/// Handles any new overlays that need to be processed, adds them to the master overlay list, and calls pre-initialize if specified by the overlay.
		/// </summary>
		private void HandleNeedToProcess() {

			if (!SteamVRManager.isConnected) return;

			if (DPOverlayBase.needToProcess.Count <= 0) {
				
				if (!isInitialized) {
					isInitialized = true;
					onInitialized?.Invoke();
				}
				
				return;
			}

			for (int i = 0; i < DPOverlayBase.needToProcess.Count; i++) {

				DPOverlayBase dpBase = DPOverlayBase.needToProcess[i];
				if (dpBase == null) continue;
				
				//Add to the main list of all overlays
				overlays.Add(dpBase);
				
				//Pre-initialize if requested
				if (dpBase.autoPreInitialize) dpBase.PreInitialize();

				//Remove it so we don't process it again
				DPOverlayBase.needToProcess.Remove(dpBase);

			}
			
		}


		
		/// <summary>
		/// Called whenever a new device is connected to refresh any overlays that should be anchored
		/// </summary>
		public void RefreshAnchoredOverlays() {
			foreach (DPOverlayBase dpBase in overlays) {
				if (dpBase.overlay.trackedDevice != DPOverlayTrackedDevice.None) {
					StartCoroutine(AnchorOverlayDelayed(dpBase));
				}
			}
		}
		private IEnumerator AnchorOverlayDelayed(DPOverlayBase dpBase) {
			yield return new WaitForSeconds(2.5f);
			dpBase.SetOverlayTrackedDevice(dpBase.overlay.trackedDevice, 0, false);
		}
		
		
		
		/// <summary>
		/// Used internally for making a new overlay.
		/// </summary>
		/// <param name="window"></param>
		/// <returns>The created overlay</returns>
		public DPDesktopOverlay NewDPWindowOverlay(UwcWindow window) {

			if (window == null) return null;
			
			int index = GetValidOverlayIndex();

			if (DemoManager.isDemo) {
				if (DPDesktopOverlay.overlays.Count >= 3) return null;
			}

			
			DPDesktopOverlay dpWindow = Instantiate(dpRenderWindowPF, _overlaysHolderTransform).GetComponentInChildren<DPDesktopOverlay>();

			dpWindow.PreInitialize();

			string key = "DPWindow-" + index;
			
			dpWindow.dpAppParent.appKey = key;
			
			//dpWindow.InitWindow(window);


			if (WinNative.IsIconic(window.handle)) {
				WinNative.ShowWindow(window.handle, ShowWindowCommands.Restore);
				
			}
			
			//WinNative.SetForegroundWindow(window.handle);

			dpWindow.SetTargetCapture(window);
			

			dpWindow.dpAppParent.iconTex = window.iconTexture;
		
		
			overlays.Add(dpWindow);
			
			TheBarManager.I.AddLoadedApp(dpWindow.dpAppParent);
			
			//OverlayInteractionManager.I.StartPreviewDrag(key);
			//TheBarManager.I.LaunchAppToMainSnap(key);

			Debug.Log(dpWindow.overlay.handle + " : " + dpWindow.overlay.overlayKey + " : " + dpWindow.window.handle);

			return dpWindow;
		}

		public DPDesktopOverlay NewDPMonitorOverlay(UDDMonitor monitor) {
			
			
			int index = GetValidOverlayIndex();

			if (DemoManager.isDemo) {
				if (DPDesktopOverlay.overlays.Count >= 3) return null;
			}

			DPDesktopOverlay monitorDP = Instantiate(dpRenderWindowPF, _overlaysHolderTransform).GetComponentInChildren<DPDesktopOverlay>();

			monitorDP.PreInitialize();

			string key = "DPMonitor-" + index;
			
			monitorDP.dpAppParent.appKey = key;
			
			monitorDP.SetTargetCapture(monitor);
			
			monitorDP.dpAppParent.iconTex = desktopIcon;
			
			overlays.Add(monitorDP);
			
			TheBarManager.I.AddLoadedApp(monitorDP.dpAppParent);
			
			//OverlayInteractionManager.I.StartPreviewDrag(key);
			//TheBarManager.I.LaunchAppToMainSnap(key);

			//Debug.Log(monitorDP.overlay.handle + " : " + monitorDP.overlay.overlayKey + " : " + monitorDP.window.handle);

			return monitorDP;
		}
		
		
		
		
		private static int spawnedOverlaysIndex = -1;
		private static readonly int RefViewDir = Shader.PropertyToID("refViewDir");

		private static int GetValidOverlayIndex() {
			spawnedOverlaysIndex++;
			return spawnedOverlaysIndex;
		}
		
		
	}
}