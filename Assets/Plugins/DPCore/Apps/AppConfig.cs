using System.IO;
using UnityEngine;

namespace DPCore.Apps {


	
	public class AppConfig<SettingsClass> where SettingsClass : new() {

		
		private string appKey;
		private string appFolder;
		private string configFile;

		public SettingsClass opts;

		
		
		

		public AppConfig(string newKey) {
			
			
			appKey = newKey;

			//Debug.Log("AppKey: " + appKey);

			appFolder = Path.Combine(Application.persistentDataPath, "cache", appKey);
			configFile = Path.Combine(appFolder, "config.json");
			
			LoadSettings();
			
		}

		
		
		
		/// <summary>
		/// Loads the app settings from the <see cref="configFile"/> path
		/// </summary>
		public void LoadSettings() {

			if (!File.Exists(configFile) || !Directory.Exists(appFolder)) {
				Directory.CreateDirectory(appFolder);
				
				opts = new SettingsClass();
				SaveSettings();
			}
			else {
				opts = JsonUtility.FromJson<SettingsClass>(File.ReadAllText(configFile));
			}
			
		}

		/// <summary>
		/// Saves <see cref="opts"/> to <see cref="configFile"/> path
		/// </summary>
		public void SaveSettings() {
			File.WriteAllText(configFile, JsonUtility.ToJson(opts, true));
		}
		
		
	}
}