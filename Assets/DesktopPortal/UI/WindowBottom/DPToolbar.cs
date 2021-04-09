using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using CUI.Actions;
using DesktopPortal.Overlays;
using DPCore;
using UnityEngine;
using UnityEngine.UI;


namespace DesktopPortal.UI {
	public class DPToolbar : MonoBehaviour {
		public static DPToolbar I;


		//[SerializeField] private WindowSettings windowSettings;
		[SerializeField] private float timeBeforeFadeOut = 4f;
		

		[SerializeField] private DPCameraOverlay toolbarDP;


		[SerializeField] private CUIAction pinVisibleAction;


		[HideInInspector] public DPOverlayBase activeDP;

		//private bool isShowingWindowBottom = false;
		//private bool isWindowBottomAnimating = false;


		[SerializeField] private GameObject pinIcon;
		[SerializeField] private GameObject settingsIcon;


		public bool isActive = false;


		private float timeWithoutInteract = 0f;


		private void Awake() {
			I = this;


			//toolbarDP.onInitialized += delegate {
			//	toolbarDP.overlay.SetSortOrder(1);
			//};
		}


		private void Update() {
			
			HandleVisibility();

			if (!toolbarDP.overlay.shouldRender || !isActive || activeDP == null) return;

			CalculateToolbarPos();

			if (activeDP.isPinned && !pinVisibleAction.isActivated) {
				pinVisibleAction.Activate();
			}
			else if (!activeDP.isPinned && pinVisibleAction.isActivated) {
				pinVisibleAction.Deactivate();
			}

			toolbarDP.alwaysInteract = activeDP.alwaysInteract;
			toolbarDP.alwaysInteractBlockInput = activeDP.alwaysInteractBlockInput;

		}


		private void HandleVisibility() {
			if (activeDP == null) return;

			if (activeDP.isBeingInteracted) SetVisibility(true);

			else timeWithoutInteract += Time.deltaTime;

			if (activeDP.isBeingInteracted || toolbarDP.isBeingInteracted) timeWithoutInteract = 0f;
			

			if (timeWithoutInteract >= timeBeforeFadeOut) {
				SetVisibility(false);
			}

		}

		public void ResetToolbar() {
			activeDP = null;
			toolbarDP.OrphanOverlay();
		}


		public void Target(DPOverlayBase dpBase) {
			if (!dpBase.showToolbar) return;

			if (dpBase == activeDP) return;


			//Unsub from events
			if (activeDP != null) {

			}

			activeDP = dpBase;

			//Sub to events


			toolbarDP.alwaysInteract = activeDP.alwaysInteract;
			toolbarDP.alwaysInteractBlockInput = activeDP.alwaysInteractBlockInput;

			pinIcon.SetActive(activeDP.showToolbarPin);
			settingsIcon.SetActive(activeDP.showToolbarSettings);


			toolbarDP.OrphanOverlay();
			activeDP.AddChildOverlay(toolbarDP);


			toolbarDP.RequestRendering(true);
		}

		private void SetVisibility(bool visible, bool force = false) {
			if (force && visible || visible && !isActive) {
				DPUIManager.Animate(toolbarDP, DPAnimation.FadeIn);
				isActive = true;
			}
			else if (force && !visible || !visible && isActive) {
				DPUIManager.Animate(toolbarDP, DPAnimation.FadeOut);
				isActive = false;
			}
		}
		


		private void CalculateToolbarPos() {
			float amtVertical = (activeDP.overlayHeight / 2f) + 0.03f;

			amtVertical /= activeDP.transform.lossyScale.x;

			toolbarDP.SetOverlayTransform(new Vector3(0f, -amtVertical, 0f), Vector3.zero, true, true, true);
		}

	


		//Window bottom actions:

		public void Button_Close() {
			if (activeDP == null) return;

			if (!activeDP.hasDPAppParent) {
				DPUIManager.Animate(activeDP, DPAnimation.FadeOut);
			}
			else {
				TheBarManager.I.MinimizeApp(activeDP.dpAppParent.appKey);
			}


			if (activeDP.useLookHiding) StartCoroutine(DisableDPDelayed(0.3f));


			//Hide();
		}

		private IEnumerator DisableDPDelayed(float time) {
			yield return new WaitForSeconds(time);
			activeDP.overlay.SetVisible(false);
		}

		/*public void Pin_WindowBottom() {
			if (activeDP == null) return;

			activeDP.isPinned = !activeDP.isPinned;
			
			//Update the pin icon visual:
			if (activeDP.isPinned) {
				pinImage.texture = pinIconFilled;
			}
			else {
				pinImage.texture = pinIconNone;
			}
		}*/


		public void Button_Pin() {
			if (activeDP != null) activeDP.isPinned = !activeDP.isPinned;
		}

		public void Button_Settings() {
			WindowSettings.I.ShowForCurrentMainDP();
		}

		public void Button_Keyboard() {
			KeyboardManager.I.ShowKeyboard(activeDP);
		}
	}
}