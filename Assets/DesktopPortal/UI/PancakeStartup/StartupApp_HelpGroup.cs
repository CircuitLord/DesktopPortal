using System.Diagnostics;
using System.IO;
using CUI;
using DesktopPortal.IO;
using DesktopPortal.Wristboard;
using UnityEngine;

namespace DesktopPortal.UI.Welcome {
	public class StartupApp_HelpGroup : CUIGroup {



		public void Button_ResetUILocations() {
			DPSettings.config.wristPos = new Vector3(-0.05f, 0.09f, -0.21f);
			DPSettings.config.wristRot = new Vector3(165.7f, -63.7f, 29.5f);
			DPSettings.config.wristHandLeft = true;
			
			DPSettings.SaveSettingsJson();
			
			WristboardManager.I.ReloadTransform();
		}

		public void Button_OpenConfig() {
			Process.Start(DPSettings.configFilePath);
		}
		
		public void Button_ModifyAdminTask() {

			string taskInstallerPath = Path.Combine(Application.dataPath, @"..\", "DPTaskInstaller.exe");
			
			ProcessStartInfo info = new ProcessStartInfo() {
				FileName = taskInstallerPath
			};

			Process.Start(info);


		}
		
		
	}
}