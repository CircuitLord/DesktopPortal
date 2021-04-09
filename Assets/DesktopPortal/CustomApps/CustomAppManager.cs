
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DesktopPortal.Overlays;
using DesktopPortal.UI;
using DPCore.Apps;
using TMPro;
//using UMod;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Graphics = UnityEngine.Graphics;


namespace DesktopPortal.CustomApps {


    public class CustomAppManager : MonoBehaviour {


        private string iconCacheFolder;
        
        
        
        public string modDirectoryPath = @"C:\Users\circu\AppData\LocalLow\CircuitCubed\DesktopPortal\mods";


        public void Start() {
            iconCacheFolder = Path.Combine(Application.persistentDataPath, "cache", "appIcons");
            

            
            SetupCustomAppDetection();
        }


        /*
        public GameObject go;
        public ModHost mod;*/
        public void SetupCustomAppDetection() {


            /*
            modDirectoryPath = Path.Combine(Application.persistentDataPath, "mods");
            
            // Create the mod directory at the path
            ModDirectory modDirectory = new ModDirectory(modDirectoryPath);

            // Setup the mod directory before we can use it
            //ModDirectory.DirectoryLocation = modDirectoryPath;

            // Check if there are any installed mods
            if (modDirectory.HasMods == true)// ModDirectory.HasAnyMods == true)
            {
                // This method will attempt to locate any mods installed in the 'modDirectory' location.
                ModHost[] hosts = Mod.LoadAll(true, modDirectory);

                // Check the load status of all hosts
                foreach (ModHost host in hosts)
                {
                    if (host.IsModLoaded == true)
                    {
                        // The mod is now loaded
                        Debug.Log(string.Format("Mod Loaded: {0}", host.CurrentModPath));
                    }
                    else
                    {
                        // The mod did not load correctly
                        Debug.LogError(string.Format("Failed to load mod: {0}, ({1})", host.CurrentModPath, host.LoadResult.Message));
                    }

                    mod = host;

                }
            }
            else
            {
                // There are no mods installed in 'modDirectory' so just print a message.
                Debug.LogError("There are no mods installed in the mod directory");
            }
            */
            
            
            

        }

        public void Update() {
            /*if (Input.GetKeyDown(KeyCode.Y)) {
                //go.AddComponent<DPApp>();
                //go.GetComponent<AppController>().Init();
                Debug.Log("yay");
                
                // We are now ready to issue a load request for an asset
                // First we will make sure that there is an asset with the specified name
                if (mod.Assets.Exists("MainApp") == true)
                {
                    // Now we can call the instantiate method which will create an instance of the asset with the specified name
                    // This method works in a very similar way to the 'Resources.Load' method returning a prefab object that must be instantiated into the scene
                    go = mod.Assets.Instantiate("MainApp") as GameObject;

                    // go is now an active game object in the scene
                    go.transform.SetParent(transform);
                    
                    go.transform.position = Vector3.zero;
                    
                    
                    
                    TheBarManager.I.AddLoadedApp(go.GetComponent<DPApp>());
                    
                    TheBarManager.I.LaunchAppToMainSnap(go.GetComponent<DPApp>().appKey);
                }
                
                
            }*/
        }


        

        public Texture2D FetchAppIcon(string appKey) {
            string path = Path.Combine(iconCacheFolder, appKey + ".png");

            if (!File.Exists(path)) return null;
            
            byte[] imgData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            tex.LoadImage(imgData);
            return tex;
        }


        public void CacheAppIcon(string appKey, Texture2D tex, bool force = false) {
            
           RenderTexture rt = new RenderTexture(32, 32, 24, RenderTextureFormat.ARGB32);
           Graphics.Blit(tex, rt);

           Directory.CreateDirectory(iconCacheFolder);
            
           string path = Path.Combine(iconCacheFolder, appKey + ".png");
           DumpRenderTexture(rt, path);
           
        }
        
        
        private static void DumpRenderTexture(RenderTexture rt, string pngOutPath)
        {
            var oldRT = RenderTexture.active;

            var tex = new Texture2D(rt.width, rt.height);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();

            File.WriteAllBytes(pngOutPath, tex.EncodeToPNG());
            RenderTexture.active = oldRT;
        }
        
        
        
    }
}