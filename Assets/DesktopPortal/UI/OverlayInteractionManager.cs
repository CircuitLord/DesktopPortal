using System.Data.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using DesktopPortal.Interaction;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DesktopPortal.Windows;
using DG.Tweening;
using DPCore;
using DPCore.Apps;
using DPCore.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR;
using Valve.VR;
using WinStuff;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.UI {
	public class OverlayInteractionManager : MonoBehaviour {
		public static OverlayInteractionManager I;

		//In scene stuff

		[SerializeField] private LaserPointerInteractor rLaserPointer;
		[SerializeField] private LaserPointerInteractor lLaserPointer;


		[SerializeField] private Transform dummyDragChild;
		[SerializeField] private DPCameraOverlay windowBottomDP;
		[SerializeField] private WindowSettings _windowSettings;


		[SerializeField] private GameObject cursorPF;

		[SerializeField] private RawImage pinImage;
		[SerializeField] private Texture2D pinIconNone;
		[SerializeField] private Texture2D pinIconFilled;


		[SerializeField] private SteamVR_ActionSet mainActionSet;


		[SerializeField] private SteamVR_Action_Single primaryInteractAction;
		[SerializeField] private SteamVR_Action_Boolean secondaryInteractAction;

		[SerializeField] private SteamVR_Action_Skeleton leftSkeletonAction;
		[SerializeField] private SteamVR_Action_Skeleton rightSkeletonAction;
		

		public static bool interactionEnabled = false;
		public static bool inputBlocked = false;

		/// <summary>
		/// Used when dragging an overlay
		/// </summary>
		[HideInInspector] public DPOverlayBase draggingDP;


		[Header("Configuration")] [SerializeField]
		private float extendDragMultiplier = 1.0f;

		[SerializeField] private float scaleOverlayMultiplier = 0.5f;

		[SerializeField] private float smoothFollowStrength = 1.0f;

		[SerializeField] private float snapTestStrength = 0.85f;

		[SerializeField] private float transitionSnapPointSpeed = 0.25f;


		//[SerializeField] private float pointerRaycastDistance = 10f;
		[SerializeField] private float cursorHoverOverOverlayDistance = 0.05f;

		[SerializeField] private LayerMask overlaysLayerMask;


		// --- Interaction Variables ---

		[HideInInspector] public LaserPointerInteractor primaryLaser;

		private List<DPCursor> _cursorsDPPool = new List<DPCursor>();
		private int _cursorPoolIndex = 0;


		// --- Dragging variables

		public bool isDragging { get; private set; } = false;

		private bool draggingDPIsAutoCurving = false;

		public bool isDraggingAnchored {
			get {
				if (!isDragging) return false;

				if (draggingDP.overlay.trackedDevice != DPOverlayTrackedDevice.None) return true;
				if (draggingDP.useSmoothAnchoring || draggingDP.useSnapAnchoring) return true;

				else return false;
			}
		}
		

		public bool isUsingSnapPoint => activeSnapPoint != null;
		public DPSnapPoint activeSnapPoint { get; private set; } = null;


		//private Transform activeDraggerTrans;


		//private bool _isHoveringDP = false;
		//private DPApp _focusedDPApp;
		//private DPOverlayBase _focusedDP;


		//Window bottom hover thingy

		private bool _isShowingWindowBottom = false;

		private bool _isWindowBottomAnimating = false;

		/// <summary>
		/// The current overlay the window bottom is being shown on.
		/// </summary>
		private DPOverlayBase _activeWindowBottomDP;


		private void Start() {
			I = this;

			SteamVRManager.dashboardToggledEvent += delegate(bool b) {
				//Debug.Log(b);

				if (b == false) {
					//TryEnableInteraction(true);
				}
				else {
					if (TheBarManager.isOpened) {
						TheBarManager.I.ToggleTheBar(false);
						//Debug.Log("closing");
					}

					if (interactionEnabled) {
						TryEnableInteraction(false, true);
					}
				}
			};
		}


		private void Update() {
			HandleInteractorsAndCursors();

			if (!interactionEnabled) return;

			SetInteractorsActive();


			//UpdateWindowBottomPos();


			if (!isDragging) return;


			HandleSnapPoints();

			HandleDrag(draggingDP);
		}


		public void TempEnableInteraction(SteamVR_Input_Sources source) {
			if (interactionEnabled) return;

			//mainActionSet.Deactivate(SteamVR_Input_Sources.Any);
			mainActionSet.Activate(SteamVR_Input_Sources.Any, 0x01000001);
		}

		public void TryEnableInteraction(bool enable, bool force = false) {
			if (!enable && TheBarManager.isOpened && !force) return;

			else if (enable && SteamVRManager.dashboardOpened) return;

			interactionEnabled = enable;

			BlockInput(enable);

			if (enable) {
				//mainActionSet.Deactivate(SteamVR_Input_Sources.Any);
				//mainActionSet.Activate(SteamVR_Input_Sources.Any, 0x01000001);

				if (primaryLaser != null) primaryLaser.Activate(true);
				else rLaserPointer.Activate(true);
			}
			else {
				//mainActionSet.Deactivate(SteamVR_Input_Sources.Any);
				//mainActionSet.Activate(SteamVR_Input_Sources.Any, 0);

				lLaserPointer.Disable();
				rLaserPointer.Disable();
			}
		}

		public static void BlockInput(bool block = true) {
			inputBlocked = block;

			if (block) {
				I.mainActionSet.Deactivate(SteamVR_Input_Sources.Any);
				I.mainActionSet.Activate(SteamVR_Input_Sources.Any, 0x01000001);
			}
			else {
				I.mainActionSet.Deactivate(SteamVR_Input_Sources.Any);
				I.mainActionSet.Activate(SteamVR_Input_Sources.Any, 0);
			}
		}


		private void SetInteractorsActive() {
			if (rLaserPointer.multiInteractActive || lLaserPointer.multiInteractActive) return;

			if (isDragging) return;

			//Toggle between the two lasers
			if (primaryInteractAction[SteamVR_Input_Sources.Any].axis > 0.1f) {
				switch (primaryInteractAction.activeDevice) {
					case SteamVR_Input_Sources.LeftHand:


						lLaserPointer.Activate();
						rLaserPointer.Disable();

						primaryLaser = lLaserPointer;

						break;

					case SteamVR_Input_Sources.RightHand:


						rLaserPointer.Activate();
						lLaserPointer.Disable();

						primaryLaser = rLaserPointer;

						break;
				}
			}
		}


		private void HandleInteractorsAndCursors() {
			if (!SteamVRManager.isWearingHeadset) return;

			foreach (DPInteractor interactor in DPInteractor.all) {
				//If it's interacting with an overlay:
				if (interactor.HandleInteractionDetection(out List<Vector3> interactionPoints)) {
					//if (!interactor.isActivated) continue;

					interactor.ProcessInput();

					ShowCursorsAtInteractionPoints(interactor.targetDP, interactionPoints);
				}
			}

			DPDesktopOverlay.mouseInteractor = null;


			DisableInactiveCursors();
		}


		// --- Cursor management ---

		private void ShowCursorsAtInteractionPoints(DPOverlayBase dpBase, List<Vector3> interactionPoints) {
			foreach (Vector3 point in interactionPoints) {
				//Un-offset to be centered again
				//Vector2 fixedPoint = new Vector2(point.x - dpBase.overlay.width / 2f, point.y - dpBase.overlayHeight / 2f);
				//Debug.Log(fixedPoint.x);

				if (_cursorsDPPool.Count <= _cursorPoolIndex) {
					_cursorsDPPool.Add(Instantiate(cursorPF, SteamVRManager.I.noAnchorTrans).GetComponent<DPCursor>());
					_cursorsDPPool[_cursorPoolIndex].cursorDP.PreInitialize();
					_cursorsDPPool[_cursorPoolIndex].cursorDP.overlay.SetSortOrder(100);
					_cursorsDPPool[_cursorPoolIndex].cursorDP.RequestRendering();
					//Debug.Log("added cursor");
				}

				DPCameraOverlay cursorDP = _cursorsDPPool[_cursorPoolIndex].cursorDP;
				DPCursor cursor = _cursorsDPPool[_cursorPoolIndex];


				//Get the distance to the overlay
				float distance = Vector3.Distance(SteamVRManager.I.hmdTrans.position, point);

				distance /= 100f;
				cursorDP.overlay.SetWidthInMeters(0.001f + distance);

				//Set position, if new overlay, jump instantly

				if (cursor.targetDP != dpBase) {
					cursorDP.transform.position = point;

					cursor.targetDP = dpBase;
				}
				else {
					cursorDP.transform.position = Vector3.Lerp(cursorDP.transform.position, point, Time.deltaTime * 30f);
				}


				cursorDP.transform.LookAt(2 * cursorDP.transform.position - SteamVRManager.I.hmdTrans.position);

				//cursor.transform.LookAt(SteamVRManager.I.hmdTrans);


				cursorDP.SetOverlayTransform(cursorDP.transform.position, cursorDP.transform.eulerAngles, false);

				if (!cursorDP.overlay.isVisible) cursorDP.overlay.SetVisible(true);

				_cursorPoolIndex++;
			}
		}

		private void DisableInactiveCursors() {
			if (_cursorsDPPool.Count <= 0) return;

			for (int i = 0; i < _cursorsDPPool.Count; i++) {
				//We need to find what index of _cursorDPPool we stopped at, and disable any cursors after that point
				if (i < _cursorPoolIndex) continue;
				_cursorsDPPool[_cursorPoolIndex].cursorDP.overlay.SetVisible(false);
			}

			//Reset the cursor pool index for the next frame.
			_cursorPoolIndex = 0;
		}


		// --- Drag management ---

		/// <summary>
		/// Used for dragging a specific overlay. This can be pretty much anything.
		/// </summary>
		/// <param name="dpToDrag">The DPBase you want to drag</param>
		public void HandleDrag(DPOverlayBase dpToDrag) {
			if (isUsingSnapPoint) return;

			float final = 0.1f * smoothFollowStrength * Time.deltaTime;

			dpToDrag.transform.position = Vector3.Lerp(dpToDrag.transform.position, dummyDragChild.position, final);

			dpToDrag.transform.rotation = Quaternion.Lerp(dpToDrag.transform.rotation, dummyDragChild.rotation, final);

			//We allow more precise rotation if dragging anchored
			if (isDraggingAnchored) {
			}
			else {
				//dpToDrag.transform.LookAt(SteamVRManager.I.hmdTrans);
				//dpToDrag.transform.LookAt(2 * dpToDrag.transform.position - primaryLaser.pointer.position);

				switch (DPSettings.config.dragMode) {
					case DPDragMode.Normal:
						//Do nothing extra:
						break;

					case DPDragMode.FaceHMD:
						dpToDrag.transform.LookAt(2 * dpToDrag.transform.position - SteamVRManager.I.hmdTrans.position);
						break;

					case DPDragMode.FaceHand:
						dpToDrag.transform.LookAt(2 * dpToDrag.transform.position - primaryLaser.pointer.position);
						break;

					case DPDragMode.BlendHMDHand:
						Vector3 blendPos = (SteamVRManager.I.hmdTrans.position + primaryLaser.pointer.position) / 2f;
						dpToDrag.transform.LookAt(2 * dpToDrag.transform.position - blendPos);
						break;
				}

				dpToDrag.transform.eulerAngles = new Vector3(dpToDrag.transform.eulerAngles.x, dpToDrag.transform.eulerAngles.y, 0);


				//Auto-curve it

				if (DPSettings.config.autoCurveDragging && draggingDP.canBeCurvedWhenDragged) {
					if (draggingDPIsAutoCurving) dpToDrag.transform.eulerAngles = new Vector3(0f, dpToDrag.transform.eulerAngles.y, 0);

					float yPos = draggingDP.transform.position.y;

					float hmdY = SteamVRManager.I.hmdTrans.position.y;

					if ((yPos > hmdY - DPSettings.config.autoCurveYThreshhold * 3f) && (yPos < hmdY + DPSettings.config.autoCurveYThreshhold)) {
						if (!draggingDPIsAutoCurving) {
							dpToDrag.TransitionOverlayCurvature(DPSettings.config.autoCurveAmount, 0.15f, false);
							//dpToDrag.TransitionOverlayPosition(dpToDrag.transform.position, new Vector3(0f, dpToDrag.transform.eulerAngles.y, 0f), 0.15f);

							draggingDPIsAutoCurving = true;
						}
					}

					else if (draggingDPIsAutoCurving) {
						dpToDrag.TransitionOverlayCurvature(0f, 0.15f, false);

						draggingDPIsAutoCurving = false;
					}
				}
			}


			dpToDrag.SyncTransform();
		}

		/// <summary>
		/// Handles checking and animating the currently dragged overlay to the snap points
		/// </summary>
		private void HandleSnapPoints() {
			//If the overlay we're dragging is anchored, ignore snap points:
			if (isDraggingAnchored) return;

			if (!draggingDP.canUseSnapPoints) return;

			if (!DPSettings.config.snapPointsEnabled) return;

			//Disable snapping when button held:
			if (secondaryInteractAction[primaryLaser.inputSource].state == true) {
				if (isUsingSnapPoint) ClearActiveSnapPoint(draggingDP);
				return;
			}


			bool foundSnapPoint = primaryLaser.DetectSnapPoints(out DPSnapPoint foundPoint);

			//If we found a new snap point
			if (foundSnapPoint && foundPoint != activeSnapPoint) {
				activeSnapPoint = foundPoint;

				draggingDP.KillTransitions();

				//draggingDP.TransitionOverlayWidth(foundPoint.maxOverlayWidth, transitionSnapPointSpeed, false);
				draggingDP.TransitionOverlayCurvature(foundPoint.overlayCurvature, transitionSnapPointSpeed, false);

				activeSnapPoint.PreviewSnappedDP(draggingDP);

				foundPoint.iconDP.TransitionOverlayWidth(0.07f, transitionSnapPointSpeed, false);

				Vector3 inFrontPos = foundPoint.transform.localPosition + foundPoint.transform.up * -0.01f;

				draggingDP.TransitionOverlayPosition(inFrontPos, foundPoint.transform.localEulerAngles, transitionSnapPointSpeed,
					Ease.InOutCubic, false);


				//Size change:
				if (draggingDP.isResponsive && activeSnapPoint.useWindowResizing && DPSettings.config.snapPointsResize) {
					draggingDP.ResizeForRatio(activeSnapPoint.resizeRatioX, activeSnapPoint.resizeRatioY);
				}

				draggingDP.snapPointQueuedToResize = true;
			}

			//If we were using a snap point, but no longer are, reset state
			else if (isUsingSnapPoint && !foundSnapPoint) {
				ClearActiveSnapPoint(draggingDP);
			}
		}

		private void ClearActiveSnapPoint(DPOverlayBase dpBase) {
			if (!isUsingSnapPoint) return;

			activeSnapPoint.CancelPreviewAndRestore();

			dpBase.KillTransitions();

			dpBase.TransitionOverlayWidth(dpBase.overlay.targetWidth, transitionSnapPointSpeed, false);
			dpBase.TransitionOverlayOpacity(dpBase.overlay.targetOpacity, transitionSnapPointSpeed, Ease.InOutCubic, false);
			//dpBase.TransitionOverlayCurvature(dpBase.overlay.targetCurvature, transitionSnapPointSpeed, false);

			activeSnapPoint.iconDP.TransitionOverlayWidth(activeSnapPoint.iconDP.overlay.targetWidth, transitionSnapPointSpeed, false);
			activeSnapPoint = null;
		}


		/// <summary>
		/// Moves the current dragging overlay forwards or backwards
		/// </summary>
		/// <param name="strength">-1.0 to 1.0, moves backwards or forwards</param>
		public void ExtendCurrentDrag(float strength) {
			if (!isDragging) return;

			float final = strength * extendDragMultiplier * Time.deltaTime;

			//We move the dummy and let the HandleDrag() function handle lerping the overlay to that new pos.
			Vector3 newPos = dummyDragChild.transform.position + dummyDragChild.transform.forward * final;

			dummyDragChild.transform.position = newPos;
		}

		public void ScaleCurrentDrag(float strength) {
			if (!isDragging) return;

			float newWidth;

			newWidth = draggingDP.overlay.width + strength * Time.deltaTime * scaleOverlayMultiplier;

			newWidth = Mathf.Clamp(newWidth, 0.07f, 4f);

			draggingDP.overlay.SetWidthInMeters(newWidth, true);
		}


		public void StartDragDP(DPOverlayBase dpToDrag) {
			if (dpToDrag == null) return;

			if (!interactionEnabled) return;

			if (isDragging) EndCurrentDrag();

			dummyDragChild.SetParent(primaryLaser.pointer);
			dummyDragChild.position = dpToDrag.transform.position;
			dummyDragChild.eulerAngles = dpToDrag.transform.eulerAngles;

			draggingDP = dpToDrag;

			dpToDrag.onOverlayDragged?.Invoke(true);

			if (draggingDP.canUseSnapPoints) TheBarManager.I.ToggleSnapPointsVisible(true);

			if (!DPSettings.config.autoCurveDragging) draggingDP.overlay.SetCurvature(0f);

			isDragging = true;
		}
		

		public void EndCurrentDrag() {
			if (!isDragging) return;

			//Debug.Log("ending drag");


			if (draggingDP == null) {
				Debug.LogError("Dragging DP null");
				return;
			}

			draggingDP.onOverlayDragged?.Invoke(false);

			//If it was using a snap point in the past, clear it:
			draggingDP.ClearAllSnapData();

			//If we ended the drag on a new snap point
			if (isUsingSnapPoint) {
				draggingDP.TransitionOverlayPosition(activeSnapPoint.transform.localPosition, activeSnapPoint.transform.localEulerAngles, transitionSnapPointSpeed,
					Ease.InOutCubic, false);

				//Remove any previous thing
				if (activeSnapPoint.dpApp != null) {
					TheBarManager.I.MinimizeApp(activeSnapPoint.dpApp.appKey);
				}
				else if (activeSnapPoint.dpBase != null) {
					DPUIManager.Animate(activeSnapPoint.dpBase, DPAnimation.FadeOut);
					activeSnapPoint.ClearAllSnapData();
				}

				if (draggingDP.hasDPAppParent && draggingDP.dpAppParent.dpMain == draggingDP) {
					activeSnapPoint.SetSnappedApp(draggingDP.dpAppParent);
				}
				else {
					activeSnapPoint.SetSnappedDP(draggingDP);
				}
			}


			TheBarManager.I.ToggleSnapPointsVisible(false);

			isDragging = false;

			dummyDragChild.parent = null;

			draggingDP = null;
			activeSnapPoint = null;

			draggingDPIsAutoCurving = false;
		}
	}


	public enum DPDragMode {
		Normal = 0,
		FaceHMD = 1,
		FaceHand = 2,
		BlendHMDHand = 3
	}
}