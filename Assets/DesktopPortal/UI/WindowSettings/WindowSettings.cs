using System;
using System.Collections;
using System.Collections.Generic;
using CUI.Components;
using DesktopPortal.Overlays;
using DPCore;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using Valve.VR;

namespace DesktopPortal.UI {
	public class WindowSettings : MonoBehaviour {
		public static WindowSettings I;



		[SerializeField] private float cropIncrement = 0.015f;
		

		[Header("Components")] [SerializeField]
		private DPCameraOverlay windowSettingsDP;

		[SerializeField] private DPCameraOverlay windowSettingsCurrentDP;
		

		[SerializeField] private DPCameraOverlay windowSettingsTopDP;

		[SerializeField] private RawImage iconCurrent;

		[SerializeField] private TextMeshProUGUI textCurrent;

		[SerializeField] private Transform transCalcHMDDir;
		
		


		[Header("Transform")] [SerializeField] private DPSettingUIPlusMinus size;

		[SerializeField] private DPSettingUISlider curvature;

		[SerializeField] private CUIToggleGroup anchorGroup;

		[SerializeField] private DPSettingUIToggle smoothAnchoring;
		[SerializeField] private DPSettingUISlider smoothAnchoringSpeed;

		[SerializeField] private DPSettingUIToggle snapAnchoring;
		[SerializeField] private DPSettingUISlider snapAnchoringDistance;


		[Header("Visual")] [SerializeField] private DPSettingUIToggle pinned;

		[SerializeField] private CUIToggleGroup captureFPSGroup;
		[SerializeField] private DPSettingUIToggle forceCaptureRate;
		

		[SerializeField] private DPSettingUISlider opacity;

		[SerializeField] private DPSettingUIToggle lookHiding;
		[SerializeField] private DPSettingUISlider lookHidingStrength;
		[SerializeField] private DPSettingUISlider lookHidingOpacity;

		[SerializeField] private DPSettingUIToggle distanceHiding;
		[SerializeField] private DPSettingUISlider distanceHidingDistance;
		[SerializeField] private TextMeshProUGUI distanceHidingCurrentDistanceText;
		[SerializeField] private Button distanceHidingSetToCurrent;
		
		
		[SerializeField] private DPSettingUISlider distanceHidingOpacity;

		[SerializeField] private DPSettingUIToggle windowCropping;
		[SerializeField] private DPSettingUIPlusMinus windowCropLeft;
		[SerializeField] private DPSettingUIPlusMinus windowCropTop;
		[SerializeField] private DPSettingUIPlusMinus windowCropRight;
		[SerializeField] private DPSettingUIPlusMinus windowCropBottom;

		[SerializeField] private DPSettingUIToggle sbs;
		[SerializeField] private DPSettingUIToggle sbsCrossedMode;


		[Header("Interaction")]

		[SerializeField]
		private DPSettingUIToggle touchInput;

		[SerializeField] private DPSettingUIToggle alwaysInteract;

		[SerializeField] private DPSettingUIToggle blockInput;

		[SerializeField] private DPSettingUIToggle disableInteraction;

		[SerializeField] private DPSettingUIToggle disableDragging;


		//private DPOverlayBase currentDP;

		private Action<bool> dragAction;


		public DPOverlayBase currentDP { get; private set; }

		private bool isActive = false;


		private bool apply = false;

		private float distanceFromCurrentDP = 0f;


		private void Awake() {
			//windowSettingsDP.dpOverlayEvent += PassthroughOverlayEvents;
			I = this;
		}


		private void Start() {
			
			
			TheBarManager.I.onBarOpened += b => {
				if (!b) Hide();
			};

			
			//Subscribe to events

			//TRANSFORM:
			
			size.plusButton.onClick.AddListener((() => {
				float newSize = currentDP.overlay.width + 0.1f;

				SizeChanged(newSize);
			}));

			size.minusButton.onClick.AddListener((() => {
				float newSize = currentDP.overlay.width - 0.1f;

				SizeChanged(newSize);
			}));

			size.resetButton.onClick.AddListener((() => { SizeChanged(0.5f); }));


			curvature.slider.onValueChanged.AddListener(CurvatureChanged);

			anchorGroup.onIndexSelected.AddListener(AnchorChanged);

			smoothAnchoring.toggle.onToggled.AddListener(SmoothAnchoringChanged);
			smoothAnchoringSpeed.slider.onValueChanged.AddListener(SmoothAnchoringSpeedChanged);

			snapAnchoring.toggle.onToggled.AddListener(SnapAnchoringChanged);
			snapAnchoringDistance.slider.onValueChanged.AddListener(SnapAnchoringDistanceChanged);
			
			
			//VISUALS
			pinned.toggle.onToggled.AddListener(PinnedChanged);
			
			captureFPSGroup.onIndexSelected.AddListener(CaptureFPSChanged);
			forceCaptureRate.toggle.onToggled.AddListener(ForceCaptureRateChanged);
			
			opacity.slider.onValueChanged.AddListener(OpacityChanged);
			
			lookHiding.toggle.onToggled.AddListener(LookHidingChanged);
			lookHidingStrength.slider.onValueChanged.AddListener(LookShowingStrengthChanged);
			lookHidingOpacity.slider.onValueChanged.AddListener(LookShowingOpacityChanged);
			
			distanceHiding.toggle.onToggled.AddListener(DistanceHidingChanged);
			distanceHidingDistance.slider.onValueChanged.AddListener(DistanceHidingDistanceChanged);
			distanceHidingSetToCurrent.onClick.AddListener((() => {

				distanceHidingDistance.slider.value = Maths.Linear(distanceFromCurrentDP, 0.2f, 2f, distanceHidingDistance.min, distanceHidingDistance.max);

				//DistanceHidingDistanceChanged(distanceFromCurrentDP);
			}));
			distanceHidingOpacity.slider.onValueChanged.AddListener(DistanceHidingOpacityChanged);
			
			windowCropping.toggle.onToggled.AddListener(WindowCroppingChanged);
			
			windowCropLeft.plusButton.onClick.AddListener((() => {
				WindowCropLeftChanged(currentDP.cropAmount.x + cropIncrement);
			}));
			windowCropLeft.minusButton.onClick.AddListener((() => {
				WindowCropLeftChanged(currentDP.cropAmount.x - cropIncrement);
			}));
			windowCropLeft.resetButton.onClick.AddListener((() => {
				WindowCropLeftChanged(0f);
			}));
			
			windowCropTop.plusButton.onClick.AddListener((() => {
				WindowCropTopChanged(currentDP.cropAmount.y + cropIncrement);
			}));
			windowCropTop.minusButton.onClick.AddListener((() => {
				WindowCropTopChanged(currentDP.cropAmount.y - cropIncrement);
			}));
			windowCropTop.resetButton.onClick.AddListener((() => {
				WindowCropTopChanged(0f);
			}));
			
			windowCropRight.plusButton.onClick.AddListener((() => {
				WindowCropRightChanged(currentDP.cropAmount.z + cropIncrement);
			}));
			windowCropRight.minusButton.onClick.AddListener((() => {
				WindowCropRightChanged(currentDP.cropAmount.z - cropIncrement);
			}));
			windowCropRight.resetButton.onClick.AddListener((() => {
				WindowCropRightChanged(0f);
			}));
			
			windowCropBottom.plusButton.onClick.AddListener((() => {
				WindowCropBottomChanged(currentDP.cropAmount.w + cropIncrement);
			}));
			windowCropBottom.minusButton.onClick.AddListener((() => {
				WindowCropBottomChanged(currentDP.cropAmount.w - cropIncrement);
			}));
			windowCropBottom.resetButton.onClick.AddListener((() => {
				WindowCropBottomChanged(0f);
			}));

			sbs.toggle.onToggled.AddListener(SBSChanged);
			sbsCrossedMode.toggle.onToggled.AddListener(SBSCrossedModeChanged);
			
			
			
			
			
			//INTERACTION
			touchInput.toggle.onToggled.AddListener(TouchInputChanged);
			
			alwaysInteract.toggle.onToggled.AddListener(AlwaysInteractChanged);
			
			blockInput.toggle.onToggled.AddListener(BlockInputChanged);
			
			disableInteraction.toggle.onToggled.AddListener(DisableInteractionChanged);
			
			disableDragging.toggle.onToggled.AddListener(DisableDraggingChanged);
			
			
			
			
		}


		private void Update() {

			if (isActive && currentDP != null && distanceHiding.toggle.isSelected) {
				distanceFromCurrentDP = Mathf.Abs(Vector3.Distance(currentDP.transform.position, SteamVRManager.I.hmdTrans.position));
				distanceHidingCurrentDistanceText.SetText("Current Distance: " + distanceFromCurrentDP.ToString("F2") + "m");
			}
			
		}


		public void ShowForCurrentMainDP() {
			
			transCalcHMDDir.position = SteamVRManager.I.hmdTrans.position;
			transCalcHMDDir.LookAt(DPToolbar.I.activeDP.transform);
			
			float dist = Mathf.Abs(Vector3.Distance(DPToolbar.I.activeDP.transform.position, transCalcHMDDir.position));
			float mid = dist / 1.3f;
			
			//Clamp the distance:
			mid = Mathf.Clamp(mid, 0.3f, 0.9f);

			Vector3 goodPos = transCalcHMDDir.position + (transCalcHMDDir.forward * mid);

			//Move it down:
			goodPos = goodPos + (transCalcHMDDir.up * -1f * 0.4f * DPToolbar.I.activeDP.overlayHeight);
			
			Show(DPToolbar.I.activeDP, goodPos, DPToolbar.I.activeDP.dpAppParent.title, DPToolbar.I.activeDP.dpAppParent.iconTex);
			
			windowSettingsDP.alwaysInteract = currentDP.alwaysInteract;
			windowSettingsDP.alwaysInteractBlockInput = currentDP.alwaysInteractBlockInput;
			
		}

		public void Show(DPOverlayBase dpBase, Vector3 pos, string name, Texture2D icon) {
			//if (currentDP == dpBase) return;

			//Unsub from events:
			if (currentDP != null) currentDP.onOverlayDragged -= dragAction;


			currentDP = dpBase;

			//Sub to events
			dragAction = currentDP.onOverlayDragged += delegate(bool b) {
				if (!b) InitUI(currentDP);
			};

			InitUI(dpBase);
			
			textCurrent.SetText(name);
			iconCurrent.texture = icon;
			

			/*transCalcHMDDir.position = SteamVRManager.I.hmdTrans.position;
			transCalcHMDDir.LookAt(dpBase.transform);*/
			
		
			//windowSettingsDP.OrphanOverlay();

			windowSettingsTopDP.SetOverlayTransform(new Vector3(0f, 0.36f, 0f), new Vector3(0f, 0f, 15f), true, true, true);

			windowSettingsDP.KillTransitions();

			windowSettingsDP.transform.position = pos;
			windowSettingsDP.transform.LookAt(2 * windowSettingsDP.transform.position - SteamVRManager.I.hmdTrans.position);

			windowSettingsDP.SyncTransform();
			
			windowSettingsCurrentDP.SetOverlayTransform(new Vector3(0f, 0.36f, 0), Vector3.zero);

			//dpBase.AddChildOverlay(windowSettingsDP);
			

			DPUIManager.Animate(windowSettingsDP, DPAnimations.FadeIn);

			isActive = true;
		}


		public void Hide() {
			DPUIManager.Animate(windowSettingsDP, "FadeOut");
			currentDP = null;
			isActive = false;
		}


		/// <summary>
		/// Applies the state of an overlay to the values on the UI without setting any values on the overlay itself.
		/// This is usually handled by calling a set value on the related UI element itself, and letting everything handle itself.
		/// </summary>
		/// <param name="dpBase">The overlay to set the values based on</param>
		private void InitUI(DPOverlayBase dpBase) {
			//Makes the functions not apply their values to the overlay, since that'd be pointless.
			apply = false;

			if (dpBase == null) {
				Debug.LogError("Tried to open settings for null DP");
				return;
			}

			//Size
			SizeChanged(dpBase.overlay.width);

			//Curvature
			curvature.slider.value = Maths.Linear(dpBase.overlay.curvature, curvature.min, curvature.max, 0, 20);

			//Anchor
			switch (dpBase.overlay.trackedDevice) {
				case DPOverlayTrackedDevice.LeftHand:
					anchorGroup.SelectIndex(1);
					break;

				case DPOverlayTrackedDevice.RightHand:
					anchorGroup.SelectIndex(0);
					break;

				case DPOverlayTrackedDevice.HMD:
					anchorGroup.SelectIndex(4);
					break;

				default:

					if (dpBase.useSmoothAnchoring) {
						switch (dpBase.smoothAnchoringTrackedDevice) {
							case DPOverlayTrackedDevice.LeftHand:
								anchorGroup.SelectIndex(1);
								break;
							
							case DPOverlayTrackedDevice.RightHand:
								anchorGroup.SelectIndex(0);
								break;
							
							case DPOverlayTrackedDevice.HMD:
								anchorGroup.SelectIndex(4);
								break;
						}
					}
					
					else if (dpBase.useSnapAnchoring) {
						switch (dpBase.snapAnchoringTrackedDevice) {
							case DPOverlayTrackedDevice.LeftHand:
								anchorGroup.SelectIndex(1);
								break;
							
							case DPOverlayTrackedDevice.RightHand:
								anchorGroup.SelectIndex(0);
								break;
							
							case DPOverlayTrackedDevice.HMD:
								anchorGroup.SelectIndex(4);
								break;
						}
					}


					else {
						anchorGroup.SelectIndex(3);
					}
					break;

			}
			
			//Override for the bar anchor:
			if (dpBase.isAnchoredToTheBar) {
				anchorGroup.SelectIndex(2);
			}
			
			smoothAnchoring.toggle.SetValue(dpBase.useSmoothAnchoring);
			smoothAnchoringSpeed.slider.value = dpBase.smoothAnchoringStrength;

			snapAnchoring.toggle.SetValue(dpBase.useSnapAnchoring, true);
			snapAnchoringDistance.slider.value = dpBase.snapAnchoringDistance;


			
			//VISUALS
			pinned.toggle.SetValue(dpBase.isPinned);

			switch (dpBase.fpsToCaptureAt) {
				case 2:
					captureFPSGroup.SelectIndex(2);
					break;
				
				case 5:
					captureFPSGroup.SelectIndex(1);
					break;
				
				case 15:
					captureFPSGroup.SelectIndex(0);
					break;
				
				case 60:
					captureFPSGroup.SelectIndex(4);
					break;
				
				case 30:
					captureFPSGroup.SelectIndex(5);
					break;
				
				default:
					captureFPSGroup.SelectIndex(3);
					break;
			}
			
			
			forceCaptureRate.toggle.SetValue(dpBase.forceHighCaptureFramerate);

			opacity.slider.value = Maths.Linear(dpBase.overlay.opacity, 0.1f, 1f, opacity.min, opacity.max);
			
			lookHiding.toggle.SetValue(dpBase.useLookHiding);
			lookHidingStrength.slider.value = Maths.Linear(dpBase.lookHidingStrength, 1f - 0.5f, 1f - 0.05f, lookHidingStrength.min, lookHidingStrength.max);
			lookHidingOpacity.slider.value = Maths.Linear(dpBase.lookHidingOpacity, 0f, 0.3f, lookHidingOpacity.min, lookHidingOpacity.max);
			
			distanceHiding.toggle.SetValue(dpBase.useDistanceHiding);
			distanceHidingDistance.slider.value = Maths.Linear(dpBase.distanceHidingDistance, 0.2f, 2f, distanceHidingDistance.min, distanceHidingDistance.max);
			distanceHidingOpacity.slider.value = Maths.Linear(dpBase.distanceHidingOpacity, 0f, 0.3f, distanceHidingOpacity.min, distanceHidingOpacity.max);
			
			//Window cropping:
			windowCropping.toggle.SetValue(currentDP.useWindowCropping);
			WindowCropLeftChanged(currentDP.cropAmount.x);
			WindowCropTopChanged(currentDP.cropAmount.y);
			WindowCropRightChanged(currentDP.cropAmount.z);
			WindowCropBottomChanged(currentDP.cropAmount.w);


			sbs.toggle.SetValue(dpBase.overlay.useSBS);
			sbsCrossedMode.toggle.SetValue(dpBase.overlay.sbsCrossedMode);
			
			
			touchInput.toggle.SetValue(dpBase.useTouchInput);
			alwaysInteract.toggle.SetValue(dpBase.alwaysInteract);
			//blockInput.toggle.SetValue(dpBase.);
			disableInteraction.toggle.SetValue(!dpBase.isInteractable);
			disableDragging.toggle.SetValue(!dpBase.isDraggable);
			
			
			

			apply = true;
		}


		//SET VALUES HERE ----- ABOVE IS FOR SETTING THEM WITHOUT NOTIFYING


		#region TRANSFORM

		private void SizeChanged(float value) {
			currentDP.overlay.SetWidthInMeters(value);
		}

		private void CurvatureChanged(float value) {
			//0-20 on slider -> 0
			float amt = Maths.Linear(value, curvature.min, curvature.max, 0, 1);

			if (apply) currentDP.overlay.SetCurvature(amt);

			curvature.subtitle.SetText((amt * 100f).ToString() + "%");
		}

		private void AnchorChanged(int index) {

			if (currentDP == null) return;
			
			DPOverlayTrackedDevice device;

			bool isAnchoringToBar = false;
			
			
			switch (index) {
				case 0:
					device = DPOverlayTrackedDevice.RightHand;
					AnchorChanged(3);
					PurifyOverlay();
					AnchorToTheBar(false);
					break;

				case 1:
					device = DPOverlayTrackedDevice.LeftHand;
					AnchorChanged(3);
					PurifyOverlay();
					break;
				
				case 2:
					device = DPOverlayTrackedDevice.None;
					AnchorToTheBar(true);
					isAnchoringToBar = true;

					break;
				
				case 4:
					device = DPOverlayTrackedDevice.HMD;
					AnchorChanged(3);
					PurifyOverlay();
					AnchorToTheBar(false);
					break;

				default:
					device = DPOverlayTrackedDevice.None;
					ClearAnchorSettings();
					AnchorToTheBar(false);
					break;
			}

			void AnchorToTheBar(bool yes) {

				if (!apply) return;

				if (yes && !currentDP.isAnchoredToTheBar) {
					TheBarManager.AddAnchoredDP(currentDP);
				}
				else if (!yes && currentDP.isAnchoredToTheBar) {
					TheBarManager.RemoveAnchoredDP(currentDP);
				}
				
			}

			void PurifyOverlay() {
				currentDP.ClearAllSnapData();
				CurvatureChanged(0f);
			}

			void ClearAnchorSettings() {
				if (smoothAnchoring.toggle.isSelected) smoothAnchoring.toggle.SetValue(false);
				if (snapAnchoring.toggle.isSelected) snapAnchoring.toggle.SetValue(false);
			}

			if (apply && !isAnchoringToBar) currentDP.SetOverlayTrackedDevice(device);
		}

		private void SmoothAnchoringChanged(bool state) {
			if (apply) currentDP.useSmoothAnchoring = state;
			if (!state) AnchorChanged(anchorGroup.selectedIndex);
			//if (!state)
		}

		private void SmoothAnchoringSpeedChanged(float value) {
			//float amt = Maths.Linear(value, 1f, )

			if (apply) currentDP.smoothAnchoringStrength = value;

			smoothAnchoringSpeed.subtitle.SetText(Maths.Linear(value, smoothAnchoringSpeed.min, smoothAnchoringSpeed.max, 10f, 100f).ToString() + "%");
		}


		private void SnapAnchoringChanged(bool state) {
			if (apply) currentDP.useSnapAnchoring = state;
			if (!state) AnchorChanged(anchorGroup.selectedIndex);
		}

		private void SnapAnchoringDistanceChanged(float value) {
			float amt = Maths.Linear(value, snapAnchoringDistance.min, snapAnchoringDistance.max, 0.1f, 2f);

			if (apply) currentDP.snapAnchoringDistance = amt;

			snapAnchoringDistance.subtitle.SetText(amt.ToString() + "m");
		}

		#endregion


		#region VISUALS

		private void PinnedChanged(bool state) {
			if (apply) currentDP.isPinned = state;
		}

		private void CaptureFPSChanged(int index) {
			
			if (!apply) return;
			
			switch (index) {
				case 0:
					currentDP.fpsToCaptureAt = 15;
					break;
				
				case 1:
					currentDP.fpsToCaptureAt = 5;
					break;
				
				case 2:
					currentDP.fpsToCaptureAt = 2;
					break;
				
				case 3:
					currentDP.fpsToCaptureAt = SteamVRManager.hmdRefreshRate;
					break;
				
				case 4:
					currentDP.fpsToCaptureAt = 60;
					break;
				
				case 5:
					currentDP.fpsToCaptureAt = 30;
					break;
				
			}
			
			
		}
		
		private void ForceCaptureRateChanged(bool state) {
			if (apply) currentDP.forceHighCaptureFramerate = state;
		}

		private void OpacityChanged(float value) {
			if (apply) currentDP.SetOverlayOpacity(Maths.Linear(value, opacity.min, opacity.max, 0.2f, 1), true);

			opacity.subtitle.SetText(Mathf.RoundToInt(Maths.Linear(value, opacity.min, opacity.max, 5f, 100f)).ToString() + "%");
		}

		private void LookHidingChanged(bool state) {
			if (apply) {
				currentDP.useLookHiding = state;
				
				//if we're disabling it, make sure the overlay is visible
				if (!state) currentDP.SetOverlayOpacity(currentDP.overlay.targetOpacity);
			}
		}

		private void LookShowingStrengthChanged(float value) {

			float sub = Maths.Linear(value, lookHidingStrength.min, lookHidingStrength.max, 0.05f, 0.5f);

			float amt = 1f - sub;

			if (apply) currentDP.lookHidingStrength = amt;
			
			lookHidingStrength.subtitle.SetText((sub * 100f).ToString() + " degrees");
		}

		private void LookShowingOpacityChanged(float value) {
			float amt = Maths.Linear(value, lookHidingOpacity.min, lookHidingOpacity.max, 0f, 0.3f);

			if (apply) currentDP.lookHidingOpacity = amt;

		//	float percent = Maths.Linear(value, lookShowingOpacity.min, lookShowingOpacity.max, 0f, 60f);

			lookHidingOpacity.subtitle.SetText((amt * 100f) + "%");
		}

		private void DistanceHidingChanged(bool state) {
			if (apply) {
				currentDP.useDistanceHiding = state;

				//if we're disabling it, make sure the overlay is visible
				if (!state) currentDP.SetOverlayOpacity(currentDP.overlay.targetOpacity);
				
			}
		}

		private void DistanceHidingDistanceChanged(float value) {
			float amt = Maths.Linear(value, distanceHidingDistance.min, distanceHidingDistance.max, 0.2f, 2f);

			if (apply) currentDP.distanceHidingDistance = amt;

			distanceHidingDistance.subtitle.SetText(amt.ToString("F2") + "m");
		}

		private void DistanceHidingOpacityChanged(float value) {
			float amt = Maths.Linear(value, distanceHidingOpacity.min, distanceHidingOpacity.max, 0f, 0.3f);

			if (apply) currentDP.distanceHidingOpacity = amt;

			//float percent = Maths.Linear(value, distanceHidingOpacity.min, distanceHidingOpacity.max, 10f, 100f);

			distanceHidingOpacity.subtitle.SetText((amt * 100f) + "%");
		}

		private void WindowCroppingChanged(bool state) {
			if (apply) currentDP.useWindowCropping = state;
		}

		private void WindowCropLeftChanged(float value) {
			if (apply) currentDP.cropAmount.x = value;
			windowCropLeft.subtitle.SetText(Mathf.RoundToInt(Maths.Linear(value, 0f, 1f, 0f, 100f)).ToString() + "%");
		}

		private void WindowCropTopChanged(float value) {
			if (apply) currentDP.cropAmount.y = value;
			windowCropTop.subtitle.SetText(Mathf.RoundToInt(Maths.Linear(value, 0f, 1f, 0f, 100f)).ToString() + "%");
		}

		private void WindowCropRightChanged(float value) {
			if (apply) currentDP.cropAmount.z = value;
			windowCropRight.subtitle.SetText(Mathf.RoundToInt(Maths.Linear(value, 0f, 1f, 0f, 100f)).ToString() + "%");
		}

		private void WindowCropBottomChanged(float value) {
			if (apply) currentDP.cropAmount.w = value;
			windowCropBottom.subtitle.SetText(Mathf.RoundToInt(Maths.Linear(value, 0f, 1f, 0f, 100f)).ToString() + "%");
		}

		private void SBSChanged(bool state) {
			if (apply) currentDP.overlay.SetSBS(state, currentDP.overlay.sbsCrossedMode);
		}

		private void SBSCrossedModeChanged(bool state) {
			if (apply) currentDP.overlay.SetSBS(currentDP.overlay.useSBS, state);
		}

		#endregion


		#region Interaction


		private void TouchInputChanged(bool state) {
			if (apply) currentDP.useTouchInput = state;
		}

		private void AlwaysInteractChanged(bool state) {
			if (apply) currentDP.alwaysInteract = state;
		}
		
		private void BlockInputChanged(bool state) {
			//TODO:
		}
		
		private void DisableInteractionChanged(bool state) {
			if (apply) currentDP.isInteractable = !state;
		}
		
		private void DisableDraggingChanged(bool state) {
			if (apply) currentDP.isDraggable = !state;
		}

		#endregion

	
	}
}