using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DesktopPortal.Interaction;
using DesktopPortal.Overlays;
using DPCore.Apps;
using DPCore.Interaction;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.UI;
using uWindowCapture;


namespace DesktopPortal.UI {
	public class WindowSelectAppOld : DPApp {

		[SerializeField] private GameObject _windowItemPF;
		
		[SerializeField] private GridLayoutGroup _windowsLayoutGroup;

		[SerializeField] private RectTransform _windowsRectTransform;
		
		[SerializeField] private float scrollSpeedMultiplier = 200f;


		private bool windowsLoaded = false;



		public List<TemplateWindowItem> windowItems = new List<TemplateWindowItem>();
		
		public override void OnInit() {
			base.OnInit();

			dpMain.onScrolled += HandleScrollEvent;


			//var q =  Steamworks.Ugc.Query.All.InLanguage("eng");


			foreach (UwcWindow window in UwcManager.windows.Values.ToList()) {


				//if (string.IsNullOrEmpty(window.title)) continue;

				//if (kvp.Value.texture == null) {
					//Win32Stuff.ShowWindow(window.handle, ShowWindowCommands.ShowNoActivate);
					//window.RequestCapture();
					//window.RequestCaptureIcon();
				//}
				

			}
			
			StartCoroutine(BuildWindowList(true));
			
			
			
			
			
			
		}


		/*
		public override void OnOpen() {
			base.OnOpen();


			if (!windowsLoaded) return;
			StartCoroutine(BuildWindowList(false));

		}
		*/
		
		
		
		
		
		
		
		
		
		public void HandleScrollEvent(DPInteractor interactor, float delta) {
			//if (!dpApp.dpMain) return;


			if (!windowsLoaded) return;
			

			float curX = _windowsLayoutGroup.padding.top;
			
			

			if (delta < -0.1f) {
				_windowsLayoutGroup.padding.top -= (int)(scrollSpeedMultiplier * Time.deltaTime);
				
				//Figure out how far down user is allowed to scroll:
				//int maxScroll = (gamesList.Count / 7) * 400;

				//if (_gamesListGroup.padding.top <= -maxScroll) _gamesListGroup.padding.top = -maxScroll;
	
				LayoutRebuilder.MarkLayoutForRebuild(_windowsRectTransform);

			}

			else if (delta > 0.1f) {
				_windowsLayoutGroup.padding.top += (int)(scrollSpeedMultiplier * Time.deltaTime);
				//if (_gamesListGroup.padding.top >= gamesGridInitialYOffset) _gamesListGroup.padding.top = (int)gamesGridInitialYOffset;
				if (_windowsLayoutGroup.padding.top >= 0) _windowsLayoutGroup.padding.top = 0;
				
				LayoutRebuilder.MarkLayoutForRebuild(_windowsRectTransform);
			}
		}


		private IEnumerator BuildWindowList(bool forceRefresh = false) {



			windowsLoaded = false;

			if (forceRefresh) {
				foreach (Transform child in _windowsRectTransform) {
					Destroy(child.gameObject);
				}
				
				windowItems.Clear();
			}
			
			
			foreach (TemplateWindowItem item in windowItems) {
				item.exists = false;
			}


			List<UwcWindow> allWindows = UwcManager.windows.Values.ToList();


			//foreach (KeyValuePair<int, UwcWindow> kvp in UwcManager.windows) {

			for (int i = 0; i < allWindows.Count; i++) {



				UwcWindow window = allWindows[i];

				//See if we should display it or not:
				if (window.title == "") continue;

				bool skip = false;
				foreach (TemplateWindowItem temp in windowItems) {
					if (temp.window.handle == window.handle) {
						temp.exists = true;
						skip = true;
						break;
					}
				}

				if (skip) continue;
				
				
				
				
				
				TemplateWindowItem item = Instantiate(_windowItemPF, _windowsRectTransform).GetComponent<TemplateWindowItem>();

				item.exists = true;
				item.window = window;

				//window.PopulateFriendlyTitle(true);
				item.title.SetText(window.title);
				
				StartCoroutine(LoadItemTex(item));
				//item.pic.texture = item.window.texture;
				
				windowItems.Add(item);






				yield return null;

			}
			
			
			//Destroy the windows that don't exist anymore:
			for (int i = windowItems.Count - 1; i >= 0 ; i--) {
				if (windowItems[i].exists) continue;
				Destroy(windowItems[i].gameObject);
				windowItems.RemoveAt(i);
			}


			windowsLoaded = true;
			
			yield break;
			
			
			
		}
		
		
		private IEnumerator LoadItemTex(TemplateWindowItem item) {
			 
			item.window.RequestCapture();


			yield return new WaitForSeconds(1f);

			item.pic.texture = item.window.texture;
		}
		
		
		
	}
}