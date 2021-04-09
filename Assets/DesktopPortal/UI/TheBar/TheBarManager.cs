using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CUI;
using DesktopPortal.Interaction;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using UnityEngine;
using uWindowCapture;
using DesktopPortal.UI;
using DesktopPortal.UI.TheBar;
using DPCore;
using DPCore.Apps;
using DPCore.Interaction;
using TMPro;
using UnityEngine.UI;
using Valve.VR;
using WinStuff;

namespace DesktopPortal.UI {
	public class TheBarManager : MonoBehaviour {
		public static TheBarManager I;
		


		public static Vector3 activatedOrigin;
		public static Quaternion activatedRotation;

		public DPCameraOverlay theBarDP;
		public DPCameraOverlay blackoutDP;
		public DPCameraOverlay closeDP;

		public DPCameraOverlay windowListDP;

		public DPCameraOverlay gameArtDP;


		[SerializeField] private Image blackoutImage;
		
		[SerializeField] private RectTransform windowListTrans;
		
		[SerializeField] private GameObject windowListButtonPF;


		public static bool isOpened = false;
		public static bool hasBeenOpened = false;

		[SerializeField] private Transform snapPointsTrans;

		public static List<DPSnapPoint> snapPoints = new List<DPSnapPoint>();
		public DPSnapPoint mainSnapPoint;
		private bool snapPointsVisible = false;

		public List<DPApp> builtInApps = new List<DPApp>();

		[SerializeField] private CUIGroup gameHoverGroup;
		
		

		[Header("Configuration")] [SerializeField]
		private float originYOffset = -0.2f;

		[SerializeField] private float originSpawnDistance = 0.85f;


		//PRIVATE VARIABLES

		/// <summary>
		/// Any app that is loaded in memory
		/// </summary>
		public static Dictionary<string, DPApp> loadedApps = new Dictionary<string, DPApp>();

		/// <summary>
		/// Any app the the user has opened and not closed. (not if the window is actually visible)
		/// </summary>
		public static Dictionary<string, DPApp> openApps = new Dictionary<string, DPApp>();


		private ETrackingUniverseOrigin currentTrackingSpace = ETrackingUniverseOrigin.TrackingUniverseStanding;


		//EVENTS

		public Action<bool> onBarOpened;


		private void Awake() {
			I = this;
		}

		private void Start() {
			OverlayManager.onInitialized += OnOverlaysInitialized;


			StartCoroutine(LoadBuiltInApps());
			

			SteamVRManager.I.onHeadsetStateChanged.AddListener(OnHeadsetStateChanged);

			blackoutDP.onInitialized += delegate { blackoutDP.SetOverlayTransform(new Vector3(0f, 0f, 0.13f), Vector3.zero, true, true); };

			closeDP.onInteractedWith += delegate(bool b) {
				if (b) {
					closeDP.TransitionOverlayWidth(0.07f, 0.3f);
				}
				else {
					closeDP.TransitionOverlayWidth(0.065f, 0.2f);
				}
			};
			
			DPSettings.OnLoad((() => {
				SetBlackoutOpacity(DPSettings.config.dimGameAmount);
			}));

			gameArtDP.onInteractedWith += delegate(bool b) {
				
				CUIManager.Animate(gameHoverGroup, b);
				
				if (b) {
					gameArtDP.TransitionOverlayWidth(0.205f, 0.2f);
				}
				else {
					gameArtDP.TransitionOverlayWidth(0.2f, 0.1f);
				}
			};
			
			WindowListButton.onPress += LaunchAppToMainSnap;
			WindowListButton.onHeldClose += CloseApp;


			LoadAllSnapPoints();
		}


		private void Update() {
			if (currentTrackingSpace != SteamVRManager.trackingSpace) {
				currentTrackingSpace = SteamVRManager.trackingSpace;
				if (isOpened) {
					ToggleTheBar(false);
				}
			}
		}


		private void OnOverlaysInitialized() {
			//They're children so we can set the local position

			gameArtDP.SetOverlayTransform(new Vector3(0f, 0f, -0.01f), Vector3.zero, true);

			closeDP.SetOverlayTransform(new Vector3(0f, -0.1f, 0f), Vector3.zero, true);

			windowListDP.SetOverlayTransform(new Vector3(0f, 0.11f, 0f), new Vector3(-15f, 0, 0f), true);
		}


		private void OnHeadsetStateChanged(bool isWearing) {
			//if (!isWearing && isOpened) ToggleTheBar(false);
		}

		private void OnApplicationQuit() {
			//CloseAllApps();
		}


		IEnumerator LoadBuiltInApps() {
			yield return null;
			yield return null;

			foreach (DPApp dpApp in builtInApps) {
				AddLoadedApp(dpApp);
				yield return null;
			}
		}





		public void ToggleTheBar(bool open) {
			//Close:
			if (!open && isOpened) {
				ToggleVisible(false);
			}

			//Recenter:
			else if (open && isOpened) {
				//CalculateUIOriginPoint(true);
				//AnimShow();
				ToggleVisible(false);
			}

			//Open
			else {
				CalculateUIOriginPoint(false);

				ToggleVisible(true);

				//Refresh the overlays
				//DPRenderWindowOverlay.RefreshAllWindows();

				
				isOpened = true;
				hasBeenOpened = true;
			}
		}

		public void Button_LayoutsPopup() {
			
		}
		

		public void AddLoadedApp(DPApp dpApp) {
			loadedApps[dpApp.appKey] = dpApp;
		}

		public void RemoveLoadedApp(string appKey) {
			openApps.Remove(appKey);

			loadedApps.Remove(appKey);
		}
		

		public void LaunchAppToMainSnap(string appKey) {
			LaunchApp(appKey, mainSnapPoint);
		}

		/// <summary>
		/// Main function to launch and display an app.
		/// </summary>
		/// <param name="appKey">The key of the app to launch</param>
		/// <param name="snapPoint">The snap point to launch the app on, if any</param>
		/// <param name="spawnPos">The spawn pos for the app, if not using snap points</param>
		/// <param name="spawnRot">The spawn rot for the app, if not using snap points</param>
		/// <param name="device">The device it should be anchored to</param>
		public DPApp LaunchApp(string appKey, DPSnapPoint snapPoint = null, Vector3 spawnPos = new Vector3(), Vector3 spawnRot = new Vector3(),
			DPOverlayTrackedDevice device = DPOverlayTrackedDevice.None, bool isAnchoredToTheBar = true) {
			
			DPApp dpApp;

			//Check if the app is already open:
			if (openApps.ContainsKey(appKey)) {
				dpApp = openApps[appKey];

				//Check if the snap point is the same as the one it's on already:
				if (snapPoint != null && snapPoint.dpApp == dpApp) {
					snapPoint.CancelPreviewAndRestore();
					return dpApp;
				}

				//Don't make a new button since it's already open
			}
			else {
				dpApp = loadedApps[appKey];
				
				if (dpApp == null) Debug.LogError("Tried to open not-loaded DPApp :/ " + appKey);
				
				AddOpenApp(dpApp);
			}

			
			dpApp.dpMain.KillTransitions();


			//If the app was previously on a snap point, clear the data of that snap point:
			if (dpApp.snapPoint != null) {
				dpApp.snapPoint.ClearAllSnapData();
			}

			//If the new snap point has an app on it, minimize that app:
			if (snapPoint != null && snapPoint.isOccupied) {
				if (snapPoint.dpApp != null) {
					MinimizeApp(snapPoint.dpApp.appKey);
				}
				else {
					DPUIManager.Animate(snapPoint.dpBase, DPAnimation.FadeOut);
					snapPoint.ClearAllSnapData();
				}
			}
			

			//Disable look hiding
			dpApp.dpMain.useLookHiding = false;
			dpApp.dpMain.useDistanceHiding = false;
			dpApp.dpMain.useSnapAnchoring = false;
			dpApp.dpMain.useSmoothAnchoring = false;
			dpApp.dpMain.isInteractable = true;
			
			//Clear the anchor:
			dpApp.dpMain.SetOverlayTrackedDevice(DPOverlayTrackedDevice.None);

			
			if (isAnchoredToTheBar) {
				AddAnchoredDP(dpApp.dpMain);
			}
			
			//If it's launching onto a snap point:
			if (snapPoint != null) {
				snapPoint.SetSnappedApp(dpApp);

				dpApp.dpMain.overlay.SetWidthInMeters(snapPoint.maxOverlayWidth, false);
				dpApp.dpMain.overlay.SetCurvature(snapPoint.overlayCurvature, false);
				dpApp.dpMain.SetOverlayTransform(snapPoint.transform.localPosition, snapPoint.transform.localEulerAngles, true, false);

				if (dpApp.dpMain.isResponsive && snapPoint.useWindowResizing && DPSettings.config.snapPointsResize)
					dpApp.dpMain.ResizeForRatio(snapPoint.resizeRatioX, snapPoint.resizeRatioY);
				
				dpApp.dpMain.snapPointQueuedToResize = true;
			}
			//Else, we're launching it somewhere in the world:
			else {
				dpApp.dpMain.SetOverlayTransform(spawnPos, spawnRot, true, false);
				dpApp.dpMain.overlay.SetCurvature(0f, false);
			}
			
			ToggleDPApp(dpApp, true);
			dpApp.OnTheBarToggled(true);
			dpApp.OnOpen();


			return dpApp;
		}


		public void AddOpenApp(DPApp dpApp, bool addWindowListButton = true) {

			if (openApps.ContainsKey(dpApp.appKey)) return;
			
			TheBarManager.openApps[dpApp.appKey] = dpApp;
			
			if (addWindowListButton) AddOpenAppWindowListIcon(dpApp);
		}


		/// <summary>
		/// Called when the app isn't in the "openApps" dictionary.
		/// Spawns a button on the window list for this app
		/// </summary>
		/// <param name="appKey"></param>
		public void AddOpenAppWindowListIcon(DPApp dpApp) {

			if (builtInApps.Contains(dpApp)) return;
			
			WindowListButton button = Instantiate(windowListButtonPF, windowListTrans).GetComponent<WindowListButton>();

			button.icon.texture = dpApp.iconTex;
			button.appKey = dpApp.appKey;

			dpApp.windowListButtonGO = button.gameObject;

		}

		/// <summary>
		/// Changes the visibility of a DPApp
		/// </summary>
		/// <param name="dpApp"></param>
		/// <param name="visible"></param>
		public void ToggleDPApp(DPApp dpApp, bool visible, bool instant = false) {
			if (dpApp == null) return;
			
			if (visible && !dpApp.isInitialized) dpApp.OnInit();

			dpApp.OnVisibilityChange(visible);
			
			if (visible) {
				if (instant) dpApp.dpMain.overlay.SetVisible(true);
				else DPUIManager.Animate(dpApp.dpMain, "FadeIn");
			}
			else {
				if (instant) dpApp.dpMain.overlay.SetVisible(false);
				DPUIManager.Animate(dpApp.dpMain, "FadeOut");
			}
		}
		

		/// <summary>
		/// Used to minimize a DPApp. This is for literally minimizing an app, not temporarially hiding it like when the bar closes.
		/// </summary>
		/// <param name="appKey"></param>
		public void MinimizeApp(string appKey) {

			if (!openApps.ContainsKey(appKey)) return;
			
			DPApp dpApp = openApps[appKey];

			if (dpApp == null) return;

			if (dpApp.isUsingSnapPoint) {
				dpApp.snapPoint.ClearAllSnapData();
			}
			
			dpApp.OnMinimize();
			ToggleDPApp(dpApp, false, true);

			


			//dpApp.HandleTheBarToggled(false);

			//dpApp.SetAppState(DPAppState.Minimized);
		}

		/// <summary>
		/// Used to close a DPApp. This is for literally closing an app, not for temporarially hiding it.
		/// </summary>
		/// <param name="appKey"></param>
		public void CloseApp(string appKey) {

			if (!loadedApps.ContainsKey(appKey)) return;
			
			DPApp dpApp = loadedApps[appKey];
			
			if (dpApp == null) return;

			if (dpApp.snapPoint != null) {
				dpApp.snapPoint.ClearAllSnapData();
			}


			if (!builtInApps.Contains(dpApp)) {
				
				//Move the toolbar so we don't destroy it
				//DPToolbar.I.ResetToolbar();
				
				dpApp.OnClose();
				
				Destroy(dpApp.windowListButtonGO);
				
				dpApp.dpMain.OnPreDestroy();
				Destroy(dpApp.dpMain.gameObject);
				Destroy(dpApp.gameObject);

				loadedApps.Remove(appKey);
			}
			else {
				ToggleDPApp(dpApp, false);
			}

			openApps.Remove(appKey);
		}

		public void CloseDP(DPOverlayBase dpBase) {

			dpBase.ClearAllSnapData();
			
			if (DPToolbar.I.activeDP == dpBase) DPToolbar.I.ResetToolbar();
			
			Destroy(dpBase.gameObject);
			
			
			
		}




		/// <summary>
		/// Used internally to toggle the visibility of the bar, it's children, and the DPApps
		/// </summary>
		/// <param name="visible"></param>
		private void ToggleVisible(bool visible) {
			isOpened = visible;


			if (!visible) {
				OverlayInteractionManager.I.EndCurrentDrag();

				TheBarManager.I.ToggleSnapPointsVisible(false);

				//Return focus to the active game if that's on
				if (DPSettings.config.focusGameWhenNotInteracting) GamingManager.FocusActiveGame();


				DPUIManager.Animate(theBarDP, DPAnimation.FadeOut);
			}
			else {
				Vector3 goodRotBar = new Vector3(45, activatedRotation.eulerAngles.y, 0);
				theBarDP.SetOverlayTransform(GetPosFromOriginOffset(new Vector3(0, -0.45f, 0f)), goodRotBar);
				

				DPUIManager.Animate(theBarDP, DPAnimation.FadeInUp);
			}

			OverlayInteractionManager.I.TryEnableInteraction(visible, true);

			ToggleBlackout(visible);
			
			onBarOpened?.Invoke(visible);


			//Handle all the DPApps
			if (visible) UpdateAnchoredAppsTransform();

			ToggleApps(visible);
		}


		/// <summary>
		/// Updates the position and rotation of any apps that are "anchored" to the bar
		/// </summary>
		private void UpdateAnchoredAppsTransform() {
			foreach (DPApp dpApp in openApps.Values) {
				if (dpApp.dpMain.isAnchoredToTheBar) dpApp.dpMain.SyncTransform(true);
			}
		}
		
		private void ToggleApps(bool visible) {
			foreach (DPApp dpApp in openApps.Values) {
				if (dpApp == null) continue;

				dpApp.OnTheBarToggled(visible);

				if (!dpApp.dpMain.isPinned && !dpApp.isMinimized) {
					ToggleDPApp(dpApp, visible);
				}
			}
		}


		public void SetBlackoutOpacity(float opacity) {
			blackoutImage.color = new Color(0f, 0f, 0f, opacity);
			
			blackoutDP.RequestRendering(true);
		}

		public void ToggleBlackout(bool on) {
			if (on) DPUIManager.Animate(blackoutDP, DPAnimation.FadeIn);
			else DPUIManager.Animate(blackoutDP, DPAnimation.FadeOut);
		}


		/// <summary>
		/// Used to calculate/refresh where the origin of open UI is (somewhere in front of the user).
		/// Other elements are based around this.
		/// </summary>
		public void CalculateUIOriginPoint(bool animate = false) {
			float goodY = SteamVRManager.I.hmdTrans.position.y + originYOffset;

			Vector3 fakeForward = SteamVRManager.I.hmdTrans.forward;
			fakeForward.y = 0f;
			fakeForward.Normalize();

			//Vector3 fixedHMDForward = new Vector3(SteamVRManager.I.hmdGO.transform.forward.x, 0,
			//	SteamVRManager.I.hmdGO.transform.forward.z);

			Vector3 forward = SteamVRManager.I.hmdTrans.position + (fakeForward * originSpawnDistance);
			activatedOrigin = new Vector3(forward.x, goodY, forward.z);

			activatedRotation = Quaternion.Euler(new Vector3(0, SteamVRManager.I.hmdTrans.eulerAngles.y, 0));

			//UpdateRelativeSnapPoints(animate);
		}

		/*private void UpdateRelativeSnapPoints(bool animate = false) {
			foreach (DPSnapPoint snapPoint in snapPoints) {
				snapPoint.iconDP.overlay.SetWidthInMeters(0.06f);

				//If it's relative to the bar position:
				if (snapPoint.usesRelativePos) {
					Vector3 goodPos = GetPosFromOriginOffset(snapPoint.customRelativePos);
					Vector3 goodRot = GetRotFromOriginOffset(snapPoint.customRelativeRot);

					snapPoint.transform.position = goodPos;
					snapPoint.transform.eulerAngles = goodRot;


					/*if (snapPoint.app != null) {
	
						snapPoint.app.dpApp.dpMain.SetOverlayTransform(snapPoint.transform.position, snapPoint.transform.eulerAngles, true, false);
						snapPoint.app.dpApp.UpdateSidebarStats(snapPoint.overlayWidth, snapPoint.sidebarSpawnDirection);

						if (animate) {
							DPUIAnimator.CalculatePlayAnimationNewAppState(snapPoint.app.dpApp, DPAppState.Opened);
						}
						
						

					}#1#
				}
			}
		}*/

		private void LoadAllSnapPoints() {
			
			snapPoints.Clear();
			
			DPSnapPoint[] found = snapPointsTrans.GetComponentsInChildren<DPSnapPoint>();

			foreach (DPSnapPoint snapPoint in found) {
				snapPoint.Init();
				snapPoints.Add(snapPoint);

				if (snapPoint.usesRelativePos) {
					snapPoint.transform.SetParent(theBarDP.transform);
					snapPoint.transform.localPosition = snapPoint.customRelativePos;
					snapPoint.transform.localEulerAngles = snapPoint.customRelativeRot;
				}
				

			}
		}

		public void ToggleSnapPointsVisible(bool visible) {
			if (snapPointsVisible == visible) return;

			if (visible && !DPSettings.config.snapPointsEnabled) return;

			foreach (DPSnapPoint snapPoint in snapPoints) {
				if (visible) {
					snapPoint.iconDP.overlay.SetWidthInMeters(0.05f, true);

					//if (snapPoint.isOccupied) snapPoint.iconDP.overlay.SetOpacity(0.5f);
					snapPoint.iconDP.SetOverlayOpacity(0.9f, true);

					snapPoint.iconDP.transform.SetParent(snapPoint.transform);
					snapPoint.iconDP.transform.localEulerAngles = Vector3.zero;
					snapPoint.iconDP.transform.localPosition = new Vector3(0, 0, -0.03f);
					snapPoint.iconDP.SyncTransform();
					DPUIManager.Animate(snapPoint.iconDP, DPAnimation.FadeIn);
					//snapPoint.iconDP.overlay.SetVisible(true);
				}
				else {
					DPUIManager.Animate(snapPoint.iconDP, DPAnimation.FadeOut);
					//snapPoint.iconDP.overlay.SetVisible(false);
				}
			}

			snapPointsVisible = visible;
		}

		public DPSnapPoint FindSnapPointFromID(string id) {
			foreach (DPSnapPoint snapPoint in snapPoints) {
				if (snapPoint.identifier == id) {
					return snapPoint;
				}
			}

			return null;
		}

		public static Vector3 GetPosFromOriginOffset(Vector3 posOffset) {
			Vector3 up = activatedRotation * Vector3.up;
			Vector3 forward = activatedRotation * Vector3.forward;
			Vector3 right = activatedRotation * Vector3.right;

			return activatedOrigin + (up * posOffset.y) + (forward * posOffset.z) + (right * posOffset.x);
		}


		public static Vector3 GetRotFromOriginOffset(Vector3 rotOffset) {
			return new Vector3(activatedRotation.eulerAngles.x + rotOffset.x, activatedRotation.eulerAngles.y + rotOffset.y,
				activatedRotation.eulerAngles.z + rotOffset.z);
		}

		public Texture2D GetAppIconForAppKey(string appKey) {
			DPApp dpApp = loadedApps[appKey];

			if (dpApp != null) return dpApp.iconTex;
			else return null;
		}

		public static void AddAnchoredDP(DPOverlayBase dpBase) {
			dpBase.isAnchoredToTheBar = true;
			//dpBase.SetOverlayTrackedDevice(DPOverlayTrackedDevice.None);
			dpBase.followParentOpacity = false;
			I.theBarDP.AddChildOverlay(dpBase, true);
			//dpBase.transform.SetParent(I.theBarDP.transform, true);
			//dpBase.SyncTransform(true);
		}
		
		public static void RemoveAnchoredDP(DPOverlayBase dpBase) {
			dpBase.isAnchoredToTheBar = false;
			//dpBase.SetOverlayTrackedDevice(DPOverlayTrackedDevice.None);
			I.theBarDP.RemoveChildOverlay(dpBase);
			//dpBase.transform.SetParent(SteamVRManager.I.noAnchorTrans);
			//dpBase.SyncTransform(true);
		}
	}
}