using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DesktopPortal.Overlays;
using DesktopPortal.UI;
using UnityEngine;

namespace DesktopPortal.IO {
	public static class DPSettings {
		
		
		public static DPConfigJson config;

		public static bool isLoaded = false;
		
		

		public static readonly string configFilePath =
			Path.Combine(Application.persistentDataPath, "dpconfig.json");
		
		
		private static List<Action> pendingActions = new List<Action>();



		public static IEnumerator AutoSave() {
			while (true) {
				yield return new WaitForSeconds(60f);
				
				SaveSettingsJson();
			}
		}
		
		public static void LoadConfigJson(bool regenConfig = false) {
			
			//If it doesn't exist, we need to gen a new one.
			if (regenConfig || !File.Exists(configFilePath)) {
				
				//Gen new config will autoload the new config.
				GenNewConfig();
				return;
			}
			
			try {
				config = JsonUtility.FromJson<DPConfigJson>(File.ReadAllText(configFilePath));
			} catch (Exception e) {
				Debug.LogError(e);
			}

			isLoaded = true;


			foreach (var pendingAction in pendingActions) {
				pendingAction();
			}
			//pendingActions.Clear();
		}
		
		public static void OnLoad(Action action) {
			if (isLoaded) action();
			else pendingActions.Add(action);
		}
		
		public static void SaveSettingsJson() {
			File.WriteAllText(configFilePath, JsonUtility.ToJson(config, true));
			
			/*System.IO.FileStream fs = System.IO.File.OpenWrite(configFilePath);
			byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(config, true));
			fs.Write(bytes, 0, bytes.Length);
			fs.Flush();
			fs.Close();
			
			fs.Dispose();*/
			
		}


		private static void GenNewConfig() {
			Debug.Log("Generating new configuration file...");

			config = new DPConfigJson();
			isLoaded = true;

			if (File.Exists(configFilePath)) File.Delete(configFilePath);

			SaveSettingsJson();

		}
	}





	
}