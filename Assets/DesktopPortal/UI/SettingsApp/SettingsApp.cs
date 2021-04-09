using System;
using System.Collections;
using System.Collections.Generic;
using CUI;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DesktopPortal.Windows;
using DesktopPortal.Wristboard;
using DPCore;
using DPCore.Apps;
using TMPro;
using UnityEngine;
using Valve.VR;


namespace DesktopPortal.UI {
	public class SettingsApp : DPApp {



		[SerializeField] private TextMeshProUGUI versionText;



		[SerializeField] private Texture2D icon;

		[SerializeField] private SteamVR_ActionSet mainActionSet;

		[SerializeField] private DPCameraOverlay watchDP;

		[SerializeField] private Texture2D watchIcon;


		[SerializeField] private CUIGroup restartWarningGroup;
		[SerializeField] private TextMeshProUGUI restartWarningChange;
		



		[Header("Settings Components")] [SerializeField]
		private DPSettingUIToggle useLegacyCapture;

		[SerializeField] private DPSettingUIToggle autoFocusGame;

		[SerializeField] private DPSettingUIToggle blockInput;


		[SerializeField] private DPSettingUISlider dimGame;

		[SerializeField] private DPSettingUIToggle lockWatch;
		

		[SerializeField] private DPSettingUIToggle minimizeStartup;






		private Action currentRestartWarningCallback;


		private bool apply = false;


		private void Start() {
			iconTex = icon;


			//Subscribe to events
			useLegacyCapture.toggle.onToggled.AddListener(UseLegacyCaptureChanged);

			autoFocusGame.toggle.onToggled.AddListener(AutoFocusGameChanged);

			blockInput.toggle.onToggled.AddListener(BlockInputChanged);

			dimGame.slider.onValueChanged.AddListener(DimGameChanged);
			
			lockWatch.toggle.onToggled.AddListener(LockWatchChanged);

			minimizeStartup.toggle.onToggled.AddListener(UseLegacyCaptureChanged);



		}

		public override void OnInit() {
			base.OnInit();

			/*versionText.SetText("v" + Application.version);
			
			CUIManager.Animate(settingsGroup, true);*/


		}

		public override void OnVisibilityChange(bool visible) {
			base.OnVisibilityChange(visible);

			if (visible) InitUI();
		}




		private void ShowRestartWarning(string change, Action callback) {
			
			restartWarningChange.SetText("Change: " + change);
			
			CUIManager.Animate(restartWarningGroup, true);
			currentRestartWarningCallback = callback;
		}

		public void RestartWarningYes() {
			currentRestartWarningCallback?.Invoke();
		}

		public void Button_Save() {
			DPSettings.SaveSettingsJson();
		}
		
		private void InitUI() {


			apply = false;
			
			//Init any of the UI values, don't notify if possible
			
			useLegacyCapture.toggle.SetValueWithoutNotify(!DPSettings.config.useDDA, true);
			
			autoFocusGame.toggle.SetValueWithoutNotify(DPSettings.config.focusGameWhenNotInteracting, true);

			EVRSettingsError error = new EVRSettingsError();
			bool globalOverlayInput = OpenVR.Settings.GetBool(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_AllowGlobalActionSetPriority, ref error);
			blockInput.toggle.SetValueWithoutNotify(globalOverlayInput, true);
			
			dimGame.slider.value = Mathf.RoundToInt(Maths.Linear(DPSettings.config.dimGameAmount, 0, 1, dimGame.min, dimGame.max));
			
			
			lockWatch.toggle.SetValue(DPSettings.config.lockWatch, true);
			
			minimizeStartup.toggle.SetValueWithoutNotify(DPSettings.config.minimizeAtStartup, true);


			apply = true;

		}
		
		//Functions to apply the settings when buttons pressed

		private void UseLegacyCaptureChanged(bool value) {
			if (apply) DPSettings.config.useDDA = !value;
		}
		
		private void AutoFocusGameChanged(bool value) {
			if (apply) DPSettings.config.focusGameWhenNotInteracting = value;
		}

		
		//Needs to be while the bar is opened
		private void BlockInputChanged(bool value) {
			if (apply) {
				ShowRestartWarning("Block Input -> " + value.ToString(), BlockInputToggleCallback);
			}
		}

		private void BlockInputToggleCallback() {
			
			EVRSettingsError error = new EVRSettingsError();
			bool globalOverlayInput = OpenVR.Settings.GetBool(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_AllowGlobalActionSetPriority, ref error);

			OpenVR.Settings.SetBool(OpenVR.k_pch_SteamVR_Section, OpenVR.k_pch_SteamVR_AllowGlobalActionSetPriority, !globalOverlayInput, ref error);
			if (error != EVRSettingsError.None) {
				Debug.LogError("Error setting GlobalActionSetPriority! " + error.ToString());
			}
			
			DPRestartManager.Restart();
			
		}
		
		
		private void DimGameChanged(float value) {
			float amt = Maths.Linear(value, dimGame.min, dimGame.max, 0, 1);
			
			if (apply) {
				DPSettings.config.dimGameAmount = amt;
				TheBarManager.I.SetBlackoutOpacity(amt);
			}
			
			dimGame.subtitle.SetText(Mathf.RoundToInt(amt * 100f).ToString() + "%");
		}

		private void LockWatchChanged(bool value) {

			if (apply) {
				DPSettings.config.lockWatch = value;
				watchDP.isDraggable = !value;
			}

		}

		public void EditWatchSettings() {
			Vector3 goodPos = dpMain.transform.position + watchDP.transform.position;
			goodPos /= 2;

			WindowSettings.I.Show(watchDP, goodPos, "Watch", watchIcon);
		}

		public void EditSteamVRBindings() {
			OpenVR.Overlay.ShowDashboard(null);
			
			SteamVR_Input.OpenBindingUI(mainActionSet, SteamVR_Input_Sources.Any);
		}

		private void MinimizeAtStartup(bool value) {
			if (apply) DPSettings.config.minimizeAtStartup = value;
		}
		
	}
}