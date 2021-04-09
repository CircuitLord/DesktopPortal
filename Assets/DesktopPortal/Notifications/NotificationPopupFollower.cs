using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DG.Tweening;
using DPCore;
using UnityEngine;

namespace DesktopPortal.Notifications {
	public class NotificationPopupFollower : MonoBehaviour {
		[Header("Other Components")] [SerializeField]
		private DPCameraOverlay _notifPopupOverlay;

		[SerializeField] private DPCameraOverlay _wristboardOverlay;

		[SerializeField] private Transform _hmdTransform;


		[Header("Configuration")] [SerializeField]
		private float offsetForward = 1.2f;

		[SerializeField] float offsetDown = 0.2f;


		[HideInInspector] public bool popupIsActive = false;
		[HideInInspector] public bool flyToWatch = false;

		[HideInInspector] public bool isFirstMove = false;


		private void Update() {

			
			//Initalize the popup to be in front of the user.
			if (isFirstMove) {
				isFirstMove = false;
				
				Vector3 finalPos = _hmdTransform.position + _hmdTransform.forward * offsetForward;
				finalPos = new Vector3(finalPos.x, finalPos.y - offsetDown, finalPos.z);

				_notifPopupOverlay.transform.position = finalPos;


				_notifPopupOverlay.transform.rotation = _hmdTransform.rotation;
				
				_notifPopupOverlay.SetOverlayPositionWithCurrent(false);


			}
			
			if (popupIsActive) {
				Vector3 velocity = Vector3.zero;

				Vector3 finalPos = _hmdTransform.position + _hmdTransform.forward * offsetForward;
				finalPos = new Vector3(finalPos.x, finalPos.y - offsetDown, finalPos.z);

				_notifPopupOverlay.transform.position = Vector3.SmoothDamp(_notifPopupOverlay.transform.position, finalPos, ref velocity, 0.3f);

				//_notifPopupOverlay.SetOverlayTransform(
					//Vector3.SmoothDamp(_notifPopupOverlay.transform.position, finalPos, ref velocity, 0.3f),
					//.transform.localEulerAngles, true, true);


				Quaternion curRot = _notifPopupOverlay.transform.rotation;
				Quaternion desRot = _hmdTransform.rotation;


				_notifPopupOverlay.transform.rotation = Quaternion.Lerp(curRot, desRot, Time.deltaTime);
				
				_notifPopupOverlay.SetOverlayTransform(_notifPopupOverlay.transform.position, _notifPopupOverlay.transform.eulerAngles, false, false);
			}

			if (flyToWatch) {
				Vector3 finalPos = Vector3.Lerp(_notifPopupOverlay.transform.position,
					_wristboardOverlay.transform.position, Time.deltaTime * 4);

				_notifPopupOverlay.transform.position = finalPos;
				
				_notifPopupOverlay.SetOverlayPositionWithCurrent();
			}
			
			
		}
		
		
	}
}