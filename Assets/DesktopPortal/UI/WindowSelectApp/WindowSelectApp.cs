using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DPCore;
using DPCore.Apps;
using uDesktopDuplication;
using UnityEngine;
using UnityEngine.UI;
using uWindowCapture;

namespace DesktopPortal.UI {
	public class WindowSelectApp : DPApp {
		public static WindowSelectApp I;

		//[SerializeField] private DPCameraOverlay selectDP;

		//[SerializeField] private DPCameraOverlay theBarDP;


		//[SerializeField] private DPCameraOverlay mainDP;


		[SerializeField] private RectTransform windowsRectTransform;


		[SerializeField] private GameObject windowListElementPF;

		//[SerializeField] private GameObject _spacerPF;


		[SerializeField] private Texture2D desktopIcon;


		private List<WindowSelectListElement> windowElements = new List<WindowSelectListElement>();

		//public Button theBarButton;


		//public static Action<>

		[HideInInspector] public bool isShowing = false;

		private bool windowsLoaded = false;


		protected override void Awake() {
			I = this;
		}

		private void Start() {
			UwcManager.onInitialized += LoadIcons;

			WindowSelectListElement.onPressed += delegate(WindowSelectListElement item) { Hide(); };
		}


		private void LoadIcons() {
			foreach (UwcWindow window in UwcManager.windows.Values) {
				window.RequestCaptureIcon();
			}
		}


		private IEnumerator BuildWindowList(bool forceRefresh = false) {
			windowsLoaded = false;

			yield return new WaitForSeconds(0.1f);

			foreach (Transform child in windowsRectTransform) {
				Destroy(child.gameObject);
			}

			windowElements.Clear();


			int i = 1;

			//TODO: Proper detection of no DDA
			if (UDDManager.monitors.Count <= 0) DPSettings.config.useDDA = false;


			if (DPSettings.config.useDDA) {
				//Spawn desktops:
				foreach (UDDMonitor monitor in UDDManager.monitors) {
					WindowSelectListElement element = Instantiate(windowListElementPF, windowsRectTransform).GetComponent<WindowSelectListElement>();

					element.icon.texture = desktopIcon;
					element.isDesktop = true;
					element.desktopIndex = i - 1;
					element.monitor = monitor;

					element.title.SetText("Display " + i);

					i++;
				}


				//Spawn the spacer
				//Instantiate(_spacerPF, _windowsRectTransform);
			}


			//Spawn the windows

			//.Log("Spawning windows");

			List<UwcWindow> sortedWindows = UwcManager.windows.Values.OrderBy(x => x.title).ToList();

			foreach (UwcWindow window in sortedWindows) {
				if (window.title == "") continue;
				if (window.title.Contains("DISPLAY")) continue;

				if (window.title == "Microsoft Text Input Application") continue;


				bool shouldSkip = false;

				//Don't spawn existing windows:
				/*foreach (DPOverlayBase dpBase in OverlayManager.I.overlays) {
					if (dpBase is DPDesktopOverlay renderDP) {
						if (renderDP.isTargetingWindow && renderDP.window == window) {
							shouldSkip = true;
							break;
						}
					}
				}*/

				if (shouldSkip) continue;

				WindowSelectListElement element = Instantiate(windowListElementPF, windowsRectTransform).GetComponent<WindowSelectListElement>();

				//element.exists = true;
				element.window = window;

				element.title.SetText(window.title);

				element.icon.texture = window.iconTexture;

				windowElements.Add(element);
			}

			windowsLoaded = true;

			yield break;
		}


		public override void OnVisibilityChange(bool visible) {
			base.OnVisibilityChange(visible);
			
			//Clear all the pre-hook events that other stuff may have added
			WindowSelectListElement.onPressedPreHook = null;

			if (!visible) return;
			

			foreach (UwcWindow window in UwcManager.windows.Values) {
				window.RequestUpdateTitle();
			}
			
			
			LoadIcons();
			
			StartCoroutine(BuildWindowList(true));

			isShowing = visible;

		}


		protected void Showi() {
			if (isShowing) {
				Hide();
				return;
			}

			foreach (UwcWindow window in UwcManager.windows.Values) {
				window.RequestUpdateTitle();
			}

			LoadIcons();

			StartCoroutine(BuildWindowList());


			//theBarDP.SetOtherTransformRelativeToElement(selectDP.transform, theBarButton.GetComponent<RectTransform>(), new Vector3(0f, 0.25f, 0.05f));

			//theBarDP.GetWorldPositionOverlayElement()

			//DPUIManager.Animate(selectDP, DPAnimation.FadeInUp);

			isShowing = true;
		}

		public void Hide() {
			//DPUIManager.Animate(selectDP, DPAnimation.FadeOut);
			isShowing = false;
		}
	}
}