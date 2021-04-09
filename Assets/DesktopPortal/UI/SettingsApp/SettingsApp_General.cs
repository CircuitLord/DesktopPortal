using System.Collections;
using System.Collections.Generic;
using CUI;
using DesktopPortal.IO;
using DesktopPortal.Wristboard;
using DG.Tweening;
using DPCore;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace DesktopPortal.UI {
	public class SettingsApp_General : CUIGroup
	{

		[SerializeField] private Image wristLeftHand;
		[SerializeField] private Image wristRightHand;
		
		[SerializeField] private SteamVR_ActionSet mainActionSet;

		[SerializeField] private Toggle enableSnapPoints;
		[SerializeField] private Toggle snapPointsResize;

		[SerializeField] private Toggle returnFocusBarClosed;
		
		[SerializeField] private Toggle dimActiveGame;
		
		[SerializeField] private Toggle useDDA;
		
		[SerializeField] private ToggleGroup dragGroup;
		[SerializeField] private Toggle dragNormal;
		[SerializeField] private Toggle dragHMD;
		[SerializeField] private Toggle dragHand;
		[SerializeField] private Toggle dragBlend;
		
		[SerializeField] private Toggle autoCurveDrag;
		
		
		
		
		private bool isSilent = false;
		
		

		public void SetWristHand(bool useLeft) {
			
			if (!isSilent) DPSettings.config.wristHandLeft = useLeft;

			if (useLeft) {
				if (!isSilent) WristboardManager.I._wristOverlay.SetOverlayTrackedDevice(DPOverlayTrackedDevice.LeftHand);
				
				wristLeftHand.DOFade(1f, 0.3f);
				wristRightHand.DOFade(0f, 0.3f);
			}
			else {
				if (!isSilent) WristboardManager.I._wristOverlay.SetOverlayTrackedDevice(DPOverlayTrackedDevice.RightHand);
				wristRightHand.DOFade(1f, 0.3f);
				wristLeftHand.DOFade(0f, 0.3f);
			}
			
			if (!isSilent) WristboardManager.I.SetToDefaultTransform();
			
			DPSettings.SaveSettingsJson();
			

		}

		public void SetSnapPointsEnabled(bool enable) {
			if (!isSilent) {
				DPSettings.config.snapPointsEnabled = enable;
				DPSettings.SaveSettingsJson();
			}
			
			if (isSilent) enableSnapPoints.SetIsOnWithoutNotify(enable);
			
		}
		
		public void SetSnapPointsResizeWindows(bool enable) {
			if (!isSilent) {
				DPSettings.config.snapPointsResize = enable;
				DPSettings.SaveSettingsJson();
			}

			if (isSilent) snapPointsResize.SetIsOnWithoutNotify(enable);
			
			
			
		}

		public void SetReturnFocusBarClosed(bool enable) {

			if (!isSilent) {
				DPSettings.config.focusGameWhenNotInteracting = enable;
				//TODO: Fix return focus
				//DPRenderWindowOverlay.RefreshAllWindows();
				DPSettings.SaveSettingsJson();
			}
			
			if (isSilent) returnFocusBarClosed.SetIsOnWithoutNotify(enable);
		}
		
		public void SetDimActiveGameWhenBarOpened(bool enable) {

			/*if (!isSilent) {
				DPSettings.config.dimActiveGameWhenBarOpened = enable;

				if (TheBarManager.isOpened) {
					TheBarManager.I.ToggleBlackout(enable);
				}
				
				DPSettings.SaveSettingsJson();
				
			}
			
			if (isSilent) dimActiveGame.SetIsOnWithoutNotify(enable);*/
		}

		public void SetUseDDA(bool enable) {
			if (!isSilent) {
				DPSettings.config.useDDA = enable;
				//TODO: Refresh windows
				//DPRenderWindowOverlay.RefreshAllWindows();
				
				DPSettings.SaveSettingsJson();
			}
			
			if (isSilent) useDDA.SetIsOnWithoutNotify(enable);
		}
		

		public void SetDragModeNormal(bool yes) {
			if (!yes) return;

			if (!isSilent) {
				DPSettings.config.dragMode = DPDragMode.Normal;
			}
			
			dragGroup.SetAllTogglesOff();
			
			dragNormal.SetIsOnWithoutNotify(true);

		}
		public void SetDragModeHMD(bool yes) {
			if (!yes) return;

			if (!isSilent) {
				DPSettings.config.dragMode = DPDragMode.FaceHMD;
				DPSettings.SaveSettingsJson();
			}
			
			dragGroup.SetAllTogglesOff();
			
			dragHMD.SetIsOnWithoutNotify(true);

		}
		public void SetDragModeHand(bool yes) {
			if (!yes) return;

			if (!isSilent) {
				DPSettings.config.dragMode = DPDragMode.FaceHand;
				DPSettings.SaveSettingsJson();
			}
			
			dragGroup.SetAllTogglesOff();
			
			dragHand.SetIsOnWithoutNotify(true);

		}
		public void SetDragModeBlend(bool yes) {
			if (!yes) return;

			if (!isSilent) {
				DPSettings.config.dragMode = DPDragMode.BlendHMDHand;
				DPSettings.SaveSettingsJson();
			}
			
			dragGroup.SetAllTogglesOff();
			
			dragBlend.SetIsOnWithoutNotify(true);

		}

		public void SetAutoCurveDrag(bool yes) {

			if (!isSilent) {
				DPSettings.config.autoCurveDragging = yes;
				DPSettings.SaveSettingsJson();
			}
			
			if (isSilent)autoCurveDrag.SetIsOnWithoutNotify(yes);

		}
		
		public void Button_OpenBindingsUI() {
			
			OpenVR.Overlay.ShowDashboard(null);
			
			SteamVR_Input.OpenBindingUI(mainActionSet, SteamVR_Input_Sources.Any);
			
			
			
		}


		public override void OnInit() {
			base.OnInit();
			
			PopulateValues();
			
		}
		

		private void PopulateValues() {

			if (!DPSettings.isLoaded) return;

			isSilent = true;
			
			SetWristHand(DPSettings.config.wristHandLeft);
			
			SetSnapPointsEnabled(DPSettings.config.snapPointsEnabled);
			SetSnapPointsResizeWindows(DPSettings.config.snapPointsResize);
			
			SetReturnFocusBarClosed(DPSettings.config.focusGameWhenNotInteracting);
			
			//SetDimActiveGameWhenBarOpened(DPSettings.config.dimActiveGameWhenBarOpened);
			
			SetUseDDA(DPSettings.config.useDDA);

			switch (DPSettings.config.dragMode) {
			
				case DPDragMode.Normal:
					SetDragModeNormal(true);
					break;
				case DPDragMode.FaceHMD:
					SetDragModeHMD(true);
					break;
				case DPDragMode.FaceHand:
					SetDragModeHand(true);
					break;
				case DPDragMode.BlendHMDHand:
					SetDragModeBlend(true);
					break;
				
			}
			
			SetAutoCurveDrag(DPSettings.config.autoCurveDragging);


			isSilent = false;

		}
	}
}