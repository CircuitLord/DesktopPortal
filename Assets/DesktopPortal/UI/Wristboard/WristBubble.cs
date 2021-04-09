using System;
using System.Collections;
using System.Collections.Generic;
using CUI;
using DPCore;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DesktopPortal.UI {
	public abstract class WristBubble : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {


		public static DPCameraOverlay wristboardDP;
		
		public static List<WristBubble> bubbles = new List<WristBubble>();
		
		public static bool wristVisible = false;



		public string identifier;

		[SerializeField] private GameObject pinnedLine;

		[SerializeField] public CUIGroup cuiGroup;
		
		

		protected bool isBeingHovered = false;


		public bool isPinned { get; private set; } = true;


		private float pointerDownTimer = 0f;
		private bool pointerDown = false;
		

		public abstract void OnHover(bool hovering);

		public abstract void OnShortClick();

		public abstract void UpdateVisuals();


		protected virtual void Start() {
			bubbles.Add(this);
			//SetIsPinned(true);
		}


		protected virtual void Update() {
			if (pointerDown) pointerDownTimer += Time.deltaTime;

			if (pointerDownTimer >= 1f) {
				//SetIsPinned(!isPinned);
				pointerDownTimer = 0f;
				pointerDown = false;
			}
		}

		public void SetIsPinned(bool pinned) {
			isPinned = pinned;
			pinnedLine.gameObject.SetActive(pinned);
		}


		
		public void OnPointerEnter(PointerEventData eventData) {
			if (isBeingHovered) return;

			isBeingHovered = true;
			OnHover(true);
		}

		public void OnPointerExit(PointerEventData eventData) {
			isBeingHovered = false;
			OnHover(false);
		}

		public void OnPointerDown(PointerEventData eventData) {
			pointerDown = true;
			pointerDownTimer = 0f;
		}

		public void OnPointerUp(PointerEventData eventData) {
			pointerDown = false;

			if (pointerDownTimer < 1f) {
				OnShortClick();
			}

		}
	}
}