using System;
using System.Collections;
using DG.Tweening;
using DPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace DesktopPortal.UI {
	public class BatteryWristBubble : WristBubble {



		[SerializeField] private int secondsUpdateLoop = 10;
		

		[SerializeField] private Image gradientImage;

		[SerializeField] private Image darkHover;

		[SerializeField] private TextMeshProUGUI batteryText;
		
		

		public ETrackedControllerRole role;
		
		public bool useBodyTracker = false;
		public DPGenericTracker bodyTracker;

		private ETrackedPropertyError error;


		private float batteryValue = 0f;

		private Tweener darkHoverTween;
		private Tweener textHoverTween;
		


		protected override void Start() {
			base.Start();
			StartCoroutine(AutoUpdateVisuals());
			
		}


		public override void OnHover(bool hovering) {
			
			darkHoverTween?.Kill();
			textHoverTween?.Kill();

			if (hovering) {
				darkHoverTween = darkHover.DOFade(0.9f, 0.3f);
				textHoverTween = batteryText.DOFade(1f, 0.3f);
			}
			else {
				darkHoverTween = darkHover.DOFade(0f, 0.3f);
				textHoverTween = batteryText.DOFade(0f, 0.3f);
			}

		}

		public override void OnShortClick() {
			
		}


		private IEnumerator AutoUpdateVisuals() {

			while (true) {
				if (!wristVisible) yield return null;
			
				UpdateVisuals();

				yield return new WaitForSeconds(secondsUpdateLoop);
			}
			

		}


		public override void UpdateVisuals() {

			if (!SteamVRManager.isConnected) return;
			
			if (useBodyTracker) {
				batteryValue = OpenVR.System.GetFloatTrackedDeviceProperty(bodyTracker.index, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, ref error);
			}
			else {
				uint index = OpenVR.System.GetTrackedDeviceIndexForControllerRole(role);
				batteryValue = OpenVR.System.GetFloatTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, ref error);
			}

			if (error != ETrackedPropertyError.TrackedProp_Success) return;
			
			//batteryValue = batteryValue / 100f;

			//Debug.Log(batteryValue);

			gradientImage.fillAmount = batteryValue;
			
			string text = Mathf.RoundToInt(batteryValue * 100f) + "%";
			batteryText.SetText(text);


		}
		
		



		
		
	}
}