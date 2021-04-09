using System.Collections;
using System.Collections.Generic;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DPCore;
using DPCore.Apps;
using TMPro;
using UnityEngine;

namespace DesktopPortal.UI {
	public class NotFoundErrorApp : DPApp
	{
		
		//[SerializeField] private string windowSelectAppKey = "dpog-windowselect";
		
		

		[SerializeField] private TextMeshProUGUI searchText;
		[SerializeField] private TextMeshProUGUI exePathText;

		private DPAppSerialized stateToApply;


		private static int spawnIndex = -1;

		public void Init(DPAppSerialized state, string searchParams, string exePath) {

			stateToApply = state;

			appKey = "NotFoundError-" + spawnIndex++;

			gameObject.name = "NotFoundOverlayError: " + searchParams;
			
			searchText.SetText("Search Params: \"" + searchParams + "\"");
			exePathText.SetText("Exe Path: \"" + exePath + "\"");
			
			TheBarManager.I.AddLoadedApp(this);

		}
		

		public void Button_Close() {
		
			TheBarManager.I.CloseApp(this.appKey);
		}

		public void Button_FindManually() {

			//TheBarManager.RemoveAnchoredDP(WindowSelectApp.I.dpMain);

			Vector3 goodPos = dpMain.transform.position + dpMain.transform.forward * -0.05f;

			WindowSelectApp.I.dpMain.SetOverlayTransform(goodPos, dpMain.transform.eulerAngles, true, true, false);
			WindowSelectApp.I.dpMain.overlay.SetWidthInMeters(dpMain.overlay.width);

			
			TheBarManager.I.AddOpenApp(WindowSelectApp.I);
			TheBarManager.I.ToggleDPApp(WindowSelectApp.I, true);


			WindowSelectListElement.onPressedPreHook += OnNewWindowSelected;

		}

		private void OnNewWindowSelected(WindowSelectListElement element) {

			
			TheBarManager.I.MinimizeApp(WindowSelectApp.I.appKey);
			
			TheBarManager.I.CloseApp(appKey);
			
			DPDesktopOverlay dpBase;
			
			if (!element.isDesktop) dpBase = OverlayManager.I.NewDPWindowOverlay(element.window);
			else dpBase = OverlayManager.I.NewDPMonitorOverlay(element.monitor);
			
			DPLayoutManager.ApplyState(dpBase, stateToApply);
			
			TheBarManager.I.AddOpenApp(dpBase.dpAppParent);

			//Show
			DPLayoutManager.SyncNewDPAppVisibility(stateToApply, dpBase, true);


		}

		public void Button_TryLaunch() {
			
		}
		
		
	}
}