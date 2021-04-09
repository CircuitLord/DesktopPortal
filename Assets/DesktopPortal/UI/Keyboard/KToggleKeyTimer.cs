using System;
using DPCore;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DesktopPortal.UI.Keyboard {
	public class KToggleKeyTimer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {


		private float timer = 0f;

		private bool isDown = false;

		[SerializeField] private UnityEvent onToggled;
		[SerializeField] private UnityEvent onUntoggled;
		

		public static float holdForNoToggle = 0.3f;

		private bool isToggled = false;


		private bool queuedToToggleOff = false;

		private void Update() {
			if (isDown) timer += Time.deltaTime;
			
			
			
		}

		public void OnPointerDown(PointerEventData eventData) {

			if (isDown) return;

			if (isToggled) {
				queuedToToggleOff = true;
				return;
			}

			isDown = true;
			isToggled = true;
			
			onToggled?.Invoke();
		}

		public void OnPointerUp(PointerEventData eventData) {
			isDown = false;

			//If they held it down, we should untoggle it when they let go
			if (queuedToToggleOff || timer > holdForNoToggle) {
				onUntoggled?.Invoke();
				isToggled = false;
				queuedToToggleOff = false;
			}
			
			timer = 0f;
		}
		
		
	}
}