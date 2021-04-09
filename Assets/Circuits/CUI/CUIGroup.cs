using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace CUI {
	
	/// <summary>
	/// CUIViews are groups of different UI components, and can include UIPanels for "sub-menus"
	/// </summary>
	[RequireComponent(typeof(CanvasGroup))]
	public class CUIGroup : MonoBehaviour {

		[Header("CUI Configuration")]
		
		[SerializeField] private bool hideAtStart = false;

		[SerializeField] public bool disableCanvasWhenHidden = true;
		

		[Header("Animation Settings")] 
		
		[SerializeField] private bool childGroupsFollowState = false;
		
		
		[SerializeField] public CUIAnimation showingAnimation = CUIAnimation.FadeIn;
		
		
		
		[SerializeField] public CUIAnimation hidingAnimation = CUIAnimation.FadeOut;
		
		
		
		

		private bool isFirstOpen = true;

		private List<CUIGroup> childGroups = new List<CUIGroup>();
		
		
		private RectTransform _rectTransform;
		public RectTransform rectTransform {
			get {
				if (_rectTransform == null) _rectTransform = GetComponent<RectTransform>();
				return _rectTransform;
			}
		}

		[HideInInspector] public bool isVisible = false;
		
		
		private CanvasGroup _canvasGroup;
		public CanvasGroup canvasGroup {
			get {
				if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
				return _canvasGroup;
			}
		}


		protected virtual void Start() {

			RefreshChildGroups();

			if (hideAtStart) StartCoroutine(HideGOAtStartDelayed());
			else OnShowing();


		}

		private IEnumerator HideGOAtStartDelayed() {
			yield return null;
			gameObject.SetActive(false);
		}


		public void RefreshChildGroups() {
			childGroups.Clear();
			childGroups = GetComponentsInChildren<CUIGroup>().ToList();
			if (childGroups.Contains(this)) childGroups.Remove(this);
		}


		public virtual void OnInit() {
			isFirstOpen = false;
		}

		public virtual void OnShowing() {
			if (isFirstOpen) OnInit();

			if (isVisible) return;
			isVisible = true;

			if (childGroupsFollowState) {
				//Trigger events in children
				foreach (CUIGroup group in childGroups) {
					group.OnShowing();
				}
			}

		}

		public virtual void OnHiding() {
			
			if (!isVisible) return;
			isVisible = false;


			if (childGroupsFollowState) {
				//Trigger events in children
				foreach (CUIGroup group in childGroups) {
					group.OnHiding();
				}
			}

		}


		public void Hide() {
			CUIManager.Animate(this, false);
		}

		public void Show() {
			CUIManager.Animate(this, true);
		}






	}


}