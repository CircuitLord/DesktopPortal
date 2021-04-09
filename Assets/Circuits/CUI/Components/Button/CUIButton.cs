using System.Collections;
using System.Collections.Generic;
using CUI.Actions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace CUI.Components {
	[RequireComponent(typeof(Button))]
	public class CUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
		[SerializeField] private Button button;
		[SerializeField] private List<CUIAction> actionsOnHover;

		[SerializeField] private List<CUIAction> actionsOnDown;

		

		[ReadOnly] [SerializeField]
		private bool isHovered = false;
		[ReadOnly] [SerializeField]
		private bool isPressed = false;

		private void Reset() {
			button = GetComponent<Button>();
		}


		[Button]
		private void PreviewHover() {
			if (!Application.isEditor) return;
			if (isHovered) OnPointerExit(null);
			else OnPointerEnter(null);
		}
		
		[Button]
		private void PreviewPress() {
			if (!Application.isEditor) return;
			if (isPressed) OnPointerUp(null);
			else OnPointerDown(null);
		}
		

		public void OnPointerEnter(PointerEventData eventData) {
			if (isHovered) return;
			isHovered = true;

			CUIActionHandler.Activate(actionsOnHover);
		}

		public void OnPointerExit(PointerEventData eventData) {
			if (!isHovered) return;
			isHovered = false;

			CUIActionHandler.Deactivate(actionsOnHover);
		}

		public void OnPointerDown(PointerEventData eventData) {

			isPressed = true;
			
			CUIActionHandler.Activate(actionsOnDown);
		}

		public void OnPointerUp(PointerEventData eventData) {

			isPressed = false;
			
			CUIActionHandler.Deactivate(actionsOnDown);
		}
	}
}