using System.Diagnostics;
using DPCore;
using DPCore.Apps;
using UnityEngine;
using WinStuff;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.UI.Welcome {
	public class PancakeWindowControl : MonoBehaviour {
		
		
		[SerializeField] private DPApp startupScreenDPApp;
		
		
		public void Minimize() {
			
			if (SteamVRManager.isWearingHeadset && startupScreenDPApp.dpMain.overlay.shouldRender) {
				
				TheBarManager.I.MinimizeApp(startupScreenDPApp.appKey);
				
			}
			else {
				WinNative.ShowWindow(WinNative.GetForegroundWindow(), ShowWindowCommands.Minimize);
			}

		}

		public void Close() {


			if (SteamVRManager.isWearingHeadset && startupScreenDPApp.dpMain.overlay.shouldRender) {
				
				TheBarManager.I.MinimizeApp(startupScreenDPApp.appKey);
				
			}

			else {
				Application.Quit();
			}
			
		}
		
		
	}
}