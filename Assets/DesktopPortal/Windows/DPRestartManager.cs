using System.Diagnostics;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.Windows {
	public static class DPRestartManager {

		public static void Restart() {

			#if UNITY_EDITOR
			if (Application.isEditor) {
				EditorApplication.isPlaying = false;
				return;
			}
			#endif
			
			string launcherPath = Path.Combine(Application.dataPath, @"..\", "DPTaskLauncher.exe");

			ProcessStartInfo info = new ProcessStartInfo() {
				Arguments = "-delayed",
				FileName = launcherPath
			};

			Process.Start(info);

			Application.Quit();
		}
		
		
	}
}