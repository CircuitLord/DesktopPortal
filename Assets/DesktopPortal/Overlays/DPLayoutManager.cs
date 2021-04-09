using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DesktopPortal.UI;
using DPCore;
using DPCore.Apps;
using Similarity;
using uDesktopDuplication;
using UnityEngine;
using uWindowCapture;
using WinStuff;


namespace DesktopPortal {
	[Serializable]
	public class DPLayoutsFile {
		public List<DPLayoutSerialized> layouts = new List<DPLayoutSerialized>();
	}

	[Serializable]
	public class DPLayoutSerialized {
		public Vector3 theBarPos = Vector3.zero;
		public Vector3 theBarRot = Vector3.zero;
		public List<DPAppSerialized> overlays = new List<DPAppSerialized>();
		public bool isDefault = false;
		public string name;
	}

	[Serializable]
	public class DPAppSerialized {
		public string appKey;

		public bool isCapture = true;

		public string partialWindowTitle = "";
		public string exePath = "";
		public string displayName = "";

		/// <summary>
		/// Is the overlay actually visible? (still needs to follow pinned state)
		/// </summary>
		public bool isVisible = true;

		public bool isMinimized = false;

		public float width = 0.5f;
		public float curvature = 0f;

		public DPOverlayTrackedDevice trackedDevice = DPOverlayTrackedDevice.None;
		public bool isAnchoredToTheBar = false;

		public bool useSmoothAnchoring = false;
		public float smoothAnchoringStrength;
		public DPOverlayTrackedDevice smoothAnchoringTrackedDevice;

		public bool useSnapAnchoring = false;
		public float snapAnchoringDistance = 0.5f;
		public DPOverlayTrackedDevice snapAnchoringTrackedDevice;

		public bool isPinned = false;

		public int captureFPS = 30;
		public bool forceCaptureRate = false;

		public float opacity = 1.0f;

		public bool useLookHiding = false;
		public float lookHidingStrength;
		public float lookHidingHideOpacity = 0.1f;

		public bool useDistanceHiding = false;
		public float distanceHidingDistance = 0.5f;
		public float distanceHidingOpacity = 0.1f;

		public bool useWindowCropping = false;
		public Vector4 cropAmount = Vector4.zero;

		public bool useSBS = false;
		public bool sbsCrossedMode = false;

		public bool useTouchInput = true;

		public bool alwaysInteract = false;
		public bool alwaysInteractBlockInput = false;

		public bool disableInteraction = false;
		public bool disableDragging = false;


		public Vector3 pos;
		//public Vector3 theBarWorldPos;
		//public Vector3 theBarWorldRot;
		public Vector3 rot;
	}


	public class DPLayoutManager : MonoBehaviour {
		public static DPLayoutManager I;


		private static string configFilePath;
		private static DPLayoutsFile layoutsFile;
		private static bool layoutsFileLoaded = false;

		public static DPLayoutSerialized currentLayout = new DPLayoutSerialized();

		[SerializeField] private int partialWindowTitleCharacters = 8;

		[SerializeField] private NotFoundErrorApp notFoundError;


		private static readonly decimal allowedAccuracyTitleMatch = 0.85m;


		private void Awake() {
			I = this;
		}


		private void Start() {
			configFilePath = Path.Combine(Application.persistentDataPath, "layouts.json");

			LoadLayoutsJson();

			OverlayManager.onInitialized += LoadDefaultLayout;


			UwcManager.onInitialized += LoadIcons;

		}

		
		private void LoadIcons() {
			foreach (UwcWindow window in UwcManager.windows.Values) {
				window.RequestCaptureIcon();
			}
		}

		public static void LoadLayoutsJson(bool regenConfig = false) {
			//If it doesn't exist, we need to gen a new one.
			if (regenConfig || !File.Exists(configFilePath)) {
				//Gen new config will autoload the new config.
				GenLayoutsFile();
				return;
			}

			try {
				layoutsFile = JsonUtility.FromJson<DPLayoutsFile>(File.ReadAllText(configFilePath));
				layoutsFileLoaded = true;
			}
			catch (Exception e) {
				Debug.LogError(e);
				//layoutsFileLoaded = false;
			}
			
		}

		private static void GenLayoutsFile() {
			Debug.Log("Generating new layouts file...");

			layoutsFile = new DPLayoutsFile();
			layoutsFileLoaded = true;

			if (File.Exists(configFilePath)) File.Delete(configFilePath);

			SaveLayoutsFile();
		}

		public static void SaveLayoutsFile() {
			File.WriteAllText(configFilePath, JsonUtility.ToJson(layoutsFile, true));

			/*System.IO.FileStream fs = System.IO.File.OpenWrite(configFilePath);
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(layoutsFile, true));
			fs.Write(bytes, 0, bytes.Length);
			fs.Flush();
			fs.Close();

			fs.Dispose();*/
		}


		IEnumerator WaitToLoadLayout() {
			while (!DPSettings.isLoaded) yield return null;

			while (UwcManager.windows.Values.Count <= 0) yield return null;

			yield return new WaitForSeconds(0.5f);

			LoadDefaultLayout();
		}


		public static void LoadDefaultLayout() {
			LoadLayout(-1);
		}

		public static bool LoadLayout(int index, bool force = false) {
			if (!layoutsFileLoaded) return false;

			if (layoutsFile.layouts.Count <= 0) return false;

			DPLayoutSerialized layoutToLoad = null;

			//Load the default layout:
			if (index == -1) {
				foreach (DPLayoutSerialized layout in layoutsFile.layouts) {
					if (!layout.isDefault) continue;

					layoutToLoad = layout;

					break;
				}

				//If there's no default, load the first layout
				if (layoutToLoad == null) layoutToLoad = layoutsFile.layouts[0];
			}

			//Load a specific index:
			else {
				if (layoutsFile.layouts[index] != null) {
					layoutToLoad = layoutsFile.layouts[index];
				}
			}

			if (layoutToLoad == null) return false;

			//If it's the same layout return
			if (layoutToLoad == currentLayout && !force) return false;


			//Load it
			
			//Put the bar in the right pos if needed
			if (!TheBarManager.hasBeenOpened) {
				TheBarManager.I.theBarDP.SetOverlayTransform(layoutToLoad.theBarPos, layoutToLoad.theBarRot);
			}

			

			foreach (DPAppSerialized state in layoutToLoad.overlays) {
				if (!state.isCapture) {
					//Check to see if it's a built in app or a custom app
					foreach (DPApp dpApp in TheBarManager.I.builtInApps) {
						if (dpApp.appKey == state.appKey) {
							ApplyState(dpApp.dpMain, state);
						}
					}

					//TODO: Custom app check like the one above ^
				}

				//It's a desktop window:
				else {
					bool isNotFoundError = false;
					DPOverlayBase dpBase;

					//WINDOW
					if (!string.IsNullOrEmpty(state.partialWindowTitle)) {
						UwcWindow window = FindMatchingWindow(state.partialWindowTitle);
						if (window == null) {
							//Adds to loaded in TheBarManager
							dpBase = I.ShowNotFoundError(state, true);
							dpBase.PreInitialize();
							isNotFoundError = true;
						}
						else {
							//Adds to loaded in TheBarManager
							dpBase = OverlayManager.I.NewDPWindowOverlay(window);
						}
					}

					//MONITOR
					else {
						UDDMonitor monitor = FindMatchingMonitor(state.displayName);
						if (monitor == null) {
							//Adds to loaded in TheBarManager
							dpBase = I.ShowNotFoundError(state, false);
							dpBase.PreInitialize();
							isNotFoundError = true;
						}
						else {
							//Adds to loaded in TheBarManager
							dpBase = OverlayManager.I.NewDPMonitorOverlay(monitor);
						}
					}

					//Don't spawn a window list icon for the not found error
					TheBarManager.I.AddOpenApp(dpBase.dpAppParent, !isNotFoundError);

					ApplyState(dpBase, state);

					SyncNewDPAppVisibility(state, dpBase);
				}


				//DPSnapPoint snapPoint = null;

				/*if (!String.IsNullOrEmpty(dpAppSer.snapPointID)) {
					snapPoint = TheBarManager.I.FindSnapPointFromID(dpAppSer.snapPointID);
				}*/

				//TheBarManager.I.LaunchApp(dpAppSer.appKey, snapPoint, dpAppSer.pos, dpAppSer.rot, dpAppSer.trackedDevice);
			}

			currentLayout = layoutToLoad;

			return true;
		}


		/// <summary>
		/// Handles if a manually spawned DPApp should actually be visible or not
		/// </summary>
		/// <param name="state"></param>
		/// <param name="dpBase"></param>
		public static void SyncNewDPAppVisibility(DPAppSerialized state, DPOverlayBase dpBase, bool forceVisible = false) {
			//Show
			if (forceVisible || !state.isMinimized && (TheBarManager.isOpened || (!TheBarManager.isOpened && state.isPinned))) {
				TheBarManager.I.ToggleDPApp(dpBase.dpAppParent, true, true);
				dpBase.dpAppParent.OnTheBarToggled(true);
				dpBase.dpAppParent.OnOpen();
			}
			//Hide
			else {
				TheBarManager.I.ToggleDPApp(dpBase.dpAppParent, false, true);
				dpBase.dpAppParent.OnTheBarToggled(false);
				//dpBase.dpAppParent.OnMinimize();
			}
		}

		private DPOverlayBase ShowNotFoundError(DPAppSerialized state, bool isTargetingWindow) {
			NotFoundErrorApp error = Instantiate(notFoundError);

			if (isTargetingWindow) {
				error.Init(state, state.partialWindowTitle, state.exePath);
			}
			else {
				error.Init(state, state.displayName, "");
			}

			return error.dpMain;
		}


		public void InstanceSaveCurrentLayout() {
			SaveCurrentLayout();
		}

		public static void SaveCurrentLayout() {
			currentLayout.overlays.Clear();


			currentLayout.theBarPos = TheBarManager.I.theBarDP.transform.localPosition;
			currentLayout.theBarRot = TheBarManager.I.theBarDP.transform.localEulerAngles;
			
			int savedAmt = 0;

			foreach (DPApp dpApp in TheBarManager.loadedApps.Values) {
				//Don't save built-in apps
				if (TheBarManager.I.builtInApps.Contains(dpApp)) continue;

				currentLayout.overlays.Add(GetState(dpApp));
				savedAmt++;
			}


			//TODO: VERY TEMP DON'T CLEAR ALL
			layoutsFile.layouts.Clear();
			layoutsFile.layouts.Add(currentLayout);

			SaveLayoutsFile();

			Debug.Log("Saved " + savedAmt + " overlay states!");
		}


		public static void ApplyState(DPOverlayBase dpBase, DPAppSerialized state) {
			
			dpBase.overlay.SetWidthInMeters(state.width);

			dpBase.overlay.SetCurvature(state.curvature);

			dpBase.useSmoothAnchoring = state.useSmoothAnchoring;
			dpBase.smoothAnchoringStrength = state.smoothAnchoringStrength;

			dpBase.useSnapAnchoring = state.useSnapAnchoring;
			dpBase.snapAnchoringDistance = state.snapAnchoringDistance;

			dpBase.SetOverlayTransform(state.pos, state.rot);

			if (state.isAnchoredToTheBar) {
				TheBarManager.AddAnchoredDP(dpBase);
				//Apply the position in case the bar hasn't been opened
				//if (!TheBarManager.hasBeenOpened) dpBase.SetOverlayTransform(state., state.theBarWorldRot, true, true, false);
				dpBase.SetOverlayTransform(state.pos, state.rot);
			}
			else if (state.useSmoothAnchoring) dpBase.SetOverlayTrackedDevice(state.smoothAnchoringTrackedDevice);
			else if (state.useSnapAnchoring) dpBase.SetOverlayTrackedDevice(state.snapAnchoringTrackedDevice);
			else dpBase.SetOverlayTrackedDevice(state.trackedDevice);

			dpBase.isPinned = state.isPinned;

			dpBase.fpsToCaptureAt = state.captureFPS;
			dpBase.forceHighCaptureFramerate = state.forceCaptureRate;

			dpBase.SetOverlayOpacity(state.opacity);


			dpBase.useLookHiding = state.useLookHiding;
			dpBase.lookHidingStrength = state.lookHidingStrength;
			dpBase.lookHidingOpacity = state.lookHidingHideOpacity;

			dpBase.useDistanceHiding = state.useDistanceHiding;
			dpBase.distanceHidingDistance = state.distanceHidingDistance;
			dpBase.distanceHidingOpacity = state.distanceHidingOpacity;

			dpBase.useWindowCropping = state.useWindowCropping;

			dpBase.cropAmount = state.cropAmount;

			dpBase.overlay.SetSBS(state.useSBS, state.sbsCrossedMode);

			dpBase.useTouchInput = state.useTouchInput;

			dpBase.alwaysInteract = state.alwaysInteract;
			dpBase.alwaysInteractBlockInput = state.alwaysInteractBlockInput;

			dpBase.isInteractable = state.disableInteraction;

			dpBase.isDraggable = state.disableDragging;


			dpBase.dpAppParent.isMinimized = state.isMinimized;
		}

		/// <summary>
		/// Gets the current state of a DPApp and puts it in a serializable form
		/// </summary>
		/// <param name="dpApp">The DPApp to get the state of</param>
		/// <returns></returns>
		public static DPAppSerialized GetState(DPApp dpApp) {
			var dpBase = dpApp.dpMain;

			DPAppSerialized state = new DPAppSerialized() {
				isVisible = dpApp.isVisible,

				isMinimized = dpApp.isMinimized,

				pos = dpBase.transform.localPosition,
				rot = dpBase.transform.localEulerAngles,

				width = dpBase.overlay.width,

				curvature = dpBase.overlay.curvature,

				trackedDevice = dpBase.overlay.trackedDevice,
				smoothAnchoringTrackedDevice = dpBase.smoothAnchoringTrackedDevice,
				snapAnchoringTrackedDevice = dpBase.snapAnchoringTrackedDevice,
				isAnchoredToTheBar = dpBase.isAnchoredToTheBar,

				useSmoothAnchoring = dpBase.useSmoothAnchoring,
				smoothAnchoringStrength = dpBase.smoothAnchoringStrength,

				useSnapAnchoring = dpBase.useSnapAnchoring,
				snapAnchoringDistance = dpBase.snapAnchoringDistance,

				isPinned = dpBase.isPinned,

				captureFPS = dpBase.fpsToCaptureAt,

				forceCaptureRate = dpBase.forceHighCaptureFramerate,

				opacity = dpBase.overlay.targetOpacity,

				useLookHiding = dpBase.useLookHiding,
				lookHidingStrength = dpBase.lookHidingStrength,
				lookHidingHideOpacity = dpBase.lookHidingOpacity,

				useDistanceHiding = dpBase.useDistanceHiding,
				distanceHidingDistance = dpBase.distanceHidingDistance,
				distanceHidingOpacity = dpBase.distanceHidingOpacity,

				useWindowCropping = dpBase.useWindowCropping,
				cropAmount = dpBase.cropAmount,

				useSBS = dpBase.overlay.useSBS,
				sbsCrossedMode = dpBase.overlay.sbsCrossedMode,

				useTouchInput = dpBase.useTouchInput,

				alwaysInteract = dpBase.alwaysInteract,
				alwaysInteractBlockInput = dpBase.alwaysInteractBlockInput,

				disableInteraction = dpBase.isInteractable,

				disableDragging = dpBase.isDraggable
			};


			//If it's smooth/snap anchored, get the position of the target object instead
			if (state.useSmoothAnchoring) {
				state.pos = dpApp.dpMain.smoothAnchoringDummyObject.transform.localPosition;
				state.rot = dpApp.dpMain.smoothAnchoringDummyObject.transform.localEulerAngles;
			}
			else if (state.useSnapAnchoring) {
				state.pos = dpApp.dpMain.snapAnchoringDummyObject.transform.localPosition;
				state.rot = dpApp.dpMain.snapAnchoringDummyObject.transform.localEulerAngles;
			}


			//Special code if it's a desktop capture
			if (dpApp.isCapture && dpBase is DPDesktopOverlay desktopDP) {
				state.isCapture = true;

				if (desktopDP.isTargetingWindow) {
					//If the title is long enough, store the last characters
					if (desktopDP.window.title.Length > I.partialWindowTitleCharacters) {
						state.partialWindowTitle = desktopDP.window.title.Substring(desktopDP.window.title.Length - I.partialWindowTitleCharacters);
					}
					else {
						state.partialWindowTitle = desktopDP.window.title;
					}

					state.exePath = WinNative.GetFilePath(desktopDP.window.handle);
				}
				else {
					//Store the name of the display
					state.displayName = desktopDP.monitor.name;
				}
			}

			else {
				state.isCapture = false;
			}


			return state;
		}


		public static UwcWindow FindMatchingWindow(string title) {
			if (UwcManager.windows.Count <= 0) {
				Debug.LogError("No windows!");
				return null;
			}

			//TODO: option to prioritize starting charcters and use start of window title
			//We reverse the string so priority is on the last characters, most likely part of the app name
			//string reverseTitle = new string(title.ToCharArray().Reverse().ToArray());


			foreach (UwcWindow window in UwcManager.windows.Values) {
				if (window.title == "") continue;
				if (window.title.Contains("DISPLAY")) continue;

				string testTitle;

				//If the title is long enough, store the last characters
				if (window.title.Length > I.partialWindowTitleCharacters) {
					testTitle = window.title.Substring(window.title.Length - I.partialWindowTitleCharacters);
				}
				else {
					testTitle = window.title;
				}

				//string reverseTestTitle = new string(testTitle.ToCharArray().Reverse().ToArray());


				decimal accuracy = StringSimilarity.Calculate(title, testTitle);

				//It's a match!
				if (accuracy >= allowedAccuracyTitleMatch) {
					return window;
				}
			}

			return null;
		}


		public static UDDMonitor FindMatchingMonitor(string title) {
			foreach (UDDMonitor monitor in UDDManager.monitors) {
				if (monitor.name == title) return monitor;
			}


			return null;
		}
	}
}