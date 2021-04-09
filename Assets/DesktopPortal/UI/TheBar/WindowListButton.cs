using System;
using System.Collections;
using System.Collections.Generic;
using CUI.Actions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesktopPortal.UI.TheBar {
	public class WindowListButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler {

		public RawImage icon;

		[HideInInspector] public string appKey;

		[SerializeField] private List<CUIAction> closeActions;


		public static Action<string> onHeldClose;
		public static Action<string> onPress;
		


		private float holdTimer = 0f;

		private static float holdToCloseTime = 0.6f;

		private bool closeIconShowing = false;

		private bool pointerDown = false;

		private bool isHovered = false;

		private void Update() {
			if (pointerDown) {
				
				if (!closeIconShowing && holdTimer >= holdToCloseTime) {
					CUIActionHandler.Activate(closeActions);

					closeIconShowing = true;
				}
				
				holdTimer += Time.deltaTime;
			}
		}


		public void OnPointerDown(PointerEventData eventData) {


			pointerDown = true;

		}

		public void OnPointerUp(PointerEventData eventData) {


			if (closeIconShowing) {
				//TODO: Window list button actions
				CUIActionHandler.Deactivate(closeActions);
				
				if (isHovered) onHeldClose?.Invoke(appKey);
			}
			else {
				onPress?.Invoke(appKey);
			}
			


			closeIconShowing = false;
			pointerDown = false;
			holdTimer = 0f;

		}

		public void OnPointerEnter(PointerEventData eventData) {
			isHovered = true;
		}

		public void OnPointerExit(PointerEventData eventData) {
			isHovered = false;
		}
	}
}