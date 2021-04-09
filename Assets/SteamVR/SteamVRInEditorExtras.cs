using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Valve.VR {
	public partial class SteamVRInEditorExtras {
#if UNITY_EDITOR
		public static string GetSteamVRFolderParentPath(bool localToAssetsFolder = false) {
			SteamVR_Settings asset = ScriptableObject.CreateInstance<SteamVR_Settings>();
			UnityEditor.MonoScript scriptAsset = UnityEditor.MonoScript.FromScriptableObject(asset);

			string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(scriptAsset);

			System.IO.FileInfo settingsScriptFileInfo = new System.IO.FileInfo(scriptPath);

			string fullPath = settingsScriptFileInfo.Directory.Parent.Parent.FullName;

			if (localToAssetsFolder == false)
				return fullPath;
			else {
				System.IO.DirectoryInfo assetsDirectoryInfo = new DirectoryInfo(Application.dataPath);
				string localPath = fullPath.Substring(assetsDirectoryInfo.Parent.FullName.Length + 1); //plus separator char
				return localPath;
			}
		}

		public static string GetSteamVRFolderPath(bool localToAssetsFolder = false) {
			SteamVR_Settings asset = ScriptableObject.CreateInstance<SteamVR_Settings>();
			UnityEditor.MonoScript scriptAsset = UnityEditor.MonoScript.FromScriptableObject(asset);

			string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(scriptAsset);

			System.IO.FileInfo settingsScriptFileInfo = new System.IO.FileInfo(scriptPath);
			string fullPath = settingsScriptFileInfo.Directory.Parent.FullName;


			if (localToAssetsFolder == false)
				return fullPath;
			else {
				System.IO.DirectoryInfo assetsDirectoryInfo = new DirectoryInfo(Application.dataPath);
				string localPath = fullPath.Substring(assetsDirectoryInfo.Parent.FullName.Length + 1); //plus separator char
				return localPath;
			}
		}

		public static string GetSteamVRResourcesFolderPath(bool localToAssetsFolder = false) {
			string basePath = GetSteamVRFolderParentPath(localToAssetsFolder);

			string folderPath = Path.Combine(basePath, "SteamVR_Resources");

			if (Directory.Exists(folderPath) == false)
				Directory.CreateDirectory(folderPath);

			string resourcesFolderPath = Path.Combine(folderPath, "Resources");

			if (Directory.Exists(resourcesFolderPath) == false)
				Directory.CreateDirectory(resourcesFolderPath);

			return resourcesFolderPath;
		}
		
		
		
		
		
		public static string GetResourcesFolderPath(bool fromAssetsDirectory = false)
        {
            string inputFolder = string.Format("Assets/{0}", SteamVR_Settings.instance.steamVRInputPath);

            string path = Path.Combine(inputFolder, "Resources");

            bool createdDirectory = false;
            if (Directory.Exists(inputFolder) == false)
            {
                Directory.CreateDirectory(inputFolder);
                createdDirectory = true;
            }


            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
                createdDirectory = true;
            }

            if (createdDirectory)
                UnityEditor.AssetDatabase.Refresh();

            if (fromAssetsDirectory == false)
                return path.Replace("Assets/", "");
            else
                return path;
        }



        private static bool checkingSetup = false;
        private static bool openingSetup = false;
        public static bool IsOpeningSetup() { return openingSetup; }
        private static void CheckSetup()
        {
            if (checkingSetup == false && openingSetup == false && (SteamVR_Input.actions == null || SteamVR_Input.actions.Length == 0))
            {
                checkingSetup = true;
                Debug.Break();

                bool open = UnityEditor.EditorUtility.DisplayDialog("[SteamVR]", "It looks like you haven't generated actions for SteamVR Input yet. Would you like to open the SteamVR Input window?", "Yes", "No");
                if (open)
                {
                    openingSetup = true;
                    UnityEditor.EditorApplication.isPlaying = false;
                    Type editorWindowType = FindType("Valve.VR.SteamVR_Input_EditorWindow");
                    if (editorWindowType != null)
                    {
                        var window = UnityEditor.EditorWindow.GetWindow(editorWindowType, false, "SteamVR Input", true);
                        if (window != null)
                            window.Show();
                    }
                }
                else
                {
                    Debug.LogError("<b>[SteamVR]</b> This version of SteamVR will not work if you do not create and generate actions. Please open the SteamVR Input window or downgrade to version 1.2.3 (on github)");
                }
                checkingSetup = false;
            }
        }

        private static Type FindType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null) return type;
            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null)
                    return type;
            }
            return null;
        }
		
		
		
		
		
		
#endif
	}
}