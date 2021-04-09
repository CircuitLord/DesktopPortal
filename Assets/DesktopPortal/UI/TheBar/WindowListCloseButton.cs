using System;
using System.Collections;
using System.Collections.Generic;
using CUI.Actions;
using DPCore;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesktopPortal.UI.TheBar {
	public class WindowListCloseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
		
		
		[SerializeField] private UnityBoolEvent onHovered;

		private bool isHovering = false;


		public void OnPointerEnter(PointerEventData eventData) {

			if (isHovering) return;

			isHovering = true;
			
			onHovered?.Invoke(true);

		}

		public void OnPointerExit(PointerEventData eventData) {
			isHovering = false;
			
			onHovered?.Invoke(false);
		}
	}
}