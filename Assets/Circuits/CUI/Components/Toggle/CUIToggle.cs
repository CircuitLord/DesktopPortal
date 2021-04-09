using System;
using System.Collections;
using System.Collections.Generic;
using CUI;
using CUI.Actions;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class UnityBoolEvent : UnityEvent<bool> {

};


namespace CUI.Components {
	[RequireComponent(typeof(Button))]
	public class CUIToggle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

		[SerializeField] private bool startSelected = false;

		[Tooltip("If true, the toggle will look for a parent CUIToggleGroup to handle the toggling.")]
		[SerializeField] private bool isInGroup = false;
		
		
		[HideInInspector] public bool isSelected = false;
	
		[SceneObjectsOnly]
		[ChildGameObjectsOnly(IncludeSelf = true)]
		[SerializeField] private List<CUIAction> actionsOnActivated = new List<CUIAction>();

		[SceneObjectsOnly]
		[ChildGameObjectsOnly(IncludeSelf = true)]
		[SerializeField] private List<CUIAction> actionsOnHover;
	
		[SceneObjectsOnly]
		[ChildGameObjectsOnly(IncludeSelf = true)]
		[SerializeField] private List<CUIAction> actionsOnClick;


		private CUIToggleGroup parent;

		private Button button;

		[ReadOnly]
		bool isHovered = false;
		
		[ReadOnly]
		private bool isClicked = false;
		
		[Button]
		private void PreviewHover() {
			if (!Application.isEditor) return;
			if (isHovered) OnPointerExit(null);
			else OnPointerEnter(null);
		}
		
		[Button]
		private void PreviewPress() {
			if (!Application.isEditor) return;
			/*if (isClicked) OnPointerUp(null);
			else OnPointerDown(null);*/
			Toggle();
		}
		
		
		

		private void Awake() {
			button = GetComponent<Button>();

			if (isInGroup) {
				parent = transform.GetComponentInParent<CUIToggleGroup>();
			}
		}

		private void Start() {

			if (startSelected) {
				Deselect(true);
				StartCoroutine(ToggleDelayed());
			}
		}

		private IEnumerator ToggleDelayed() {
			yield return null;
			Toggle();
		}


		//[Tooltip("Should be hooked up to whatever script you want to handle the activated state.")]
		//public UnityEvent onShouldActivate;
	
		//[Tooltip("Should be hooked up to whatever script you want to handle the deactivated state.")]
		//public UnityEvent onShouldDisable;

		public UnityBoolEvent onToggled = new UnityBoolEvent();



		public void Select(bool instant = false, bool silent = false) {
			isSelected = true;

			CUIActionHandler.Activate(actionsOnActivated, instant);
		
			if (!silent) onToggled?.Invoke(true);
		}


		public void Deselect(bool instant = false, bool silent = false) {
			isSelected = false;
		
			CUIActionHandler.Deactivate(actionsOnActivated, instant);

			if (!silent) onToggled?.Invoke(false);
		}
	
	
		public void Toggle() {

			if (isInGroup) {
				if (parent == null) parent = transform.parent.GetComponent<CUIToggleGroup>();
				if (parent == null) parent = transform.parent.parent.GetComponent<CUIToggleGroup>();
				parent.OnChildToggled(this, true, false, true);
			}
			else {
				if (isSelected) Deselect();
				else Select();
			}
		}
	






		public void SetParentToggleGroup(CUIToggleGroup group) {
			parent = group;
		}


		private void Reset() {
			CUIToggleGroup group = transform.parent.GetComponent<CUIToggleGroup>();
			if (group != null) {
				group.FindChildrenToggles();
			}
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
			if (isClicked) return;
			isClicked = true;
				
			
			Toggle();
			//CUIActionHandler.Activate(actionsOnClick);
		}

		public void OnPointerUp(PointerEventData eventData) {
			if (!isClicked) return;
			isClicked = false;
			
			//CUIActionHandler.Deactivate(actionsOnClick);
		}


		public void SetValue(bool state, bool instant = false) {
			if (state) Select(instant, false);
			else Deselect(instant, false);
		}
		
		public void SetValueWithoutNotify(bool state, bool instant = false) {
			if (state) Select(instant, true);
			else Deselect(instant, true);
		}
		
	}
}