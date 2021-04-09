using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.CustomApps;
using DesktopPortal.IO;
using UnityEngine;
using uWindowCapture;
using WinStuff;


namespace DesktopPortal.UI {
    public class StartMenuManager : MonoBehaviour {


        public static StartMenuManager I;


        [SerializeField] private GameObject _appIconPF;
        [SerializeField] Transform _favoriteAppsTrans;
        [SerializeField] Transform _appsTrans;
        
        
        [HideInInspector] public List<AppIcon> favoriteApps = new List<AppIcon>();
        
        [HideInInspector] public List<AppIcon> apps = new List<AppIcon>();



        [SerializeField] private CustomAppManager _customAppManager;

        private bool loaded = false;


        private void Start() {

            I = this;
            
            
            DPSettings.OnLoad(() => {
                //TODO: Renable
               // LoadApps(true);
            });



            AppIcon.appSelectEvent += LaunchAppIcon;

        }




        public void SaveLoaded() {


            if (!loaded) return;
            
            DPSettings.config.startMenu.apps.Clear();

            foreach (AppIcon appIcon in apps) {
                StartMenuAppSer appSer = new StartMenuAppSer() {
                    appKey = appIcon.appKey,
                    appTitle = appIcon.text.text,
                    filePath = appIcon.filePath,
                    isCustomApp = appIcon.isCustomApp,
                    isFavorite = appIcon.isFavorite
                };
                DPSettings.config.startMenu.apps.Add(appSer);
            }
            
            DPSettings.SaveSettingsJson();
            
        }



        public void LaunchAppIcon(AppIcon appIcon) {
            
        }
        
        
        
        private void LoadApps(bool force) {

            if (!DPSettings.isLoaded) return;
            
            if (force) {
                foreach (Transform t in _favoriteAppsTrans) {
                    Destroy(t.gameObject);
                }
                
                foreach (Transform t in _appsTrans) {
                    Destroy(t.gameObject);
                }
                
                favoriteApps.Clear();
                apps.Clear();
            }
            
            foreach (StartMenuAppSer appSer in DPSettings.config.startMenu.apps) {

                //See if the app is already added:
                AppIcon app = null;
                AppIcon favApp = null;

                bool found = false;
                foreach (AppIcon temp in apps) {
                    if (temp.appKey == appSer.appKey) {
                        app = temp;
                        found = true;
                        break;
                    }
                }
                
                //If we found the main icon exists, we need to find the identical favorite app icon (if needed)
                if (found && appSer.isFavorite) {
                    foreach (AppIcon temp in favoriteApps) {
                        if (temp.appKey == appSer.appKey) {
                            favApp = temp;
                            break;
                        }
                    }
                }

                if (!found) {
                    app = Instantiate(_appIconPF, _appsTrans).GetComponent<AppIcon>();
                    apps.Add(app);

                    if (appSer.isFavorite) {
                        favApp = Instantiate(_appIconPF, _favoriteAppsTrans).GetComponent<AppIcon>();
                        favoriteApps.Add(favApp);
                    }
                }

                app.appKey = appSer.appKey;
                app.SetIsFavorite(appSer.isFavorite);
                app.SetIsCustomApp(appSer.isCustomApp);
                app.icon.texture = _customAppManager.FetchAppIcon(app.appKey);
                app.filePath = appSer.filePath;
                app.text.SetText(appSer.appTitle);

                if (favApp != null) {
                    favApp.appKey = appSer.appKey;
                    favApp.SetIsFavorite(true);
                    favApp.SetIsCustomApp(appSer.isCustomApp);
                    favApp.icon.texture = _customAppManager.FetchAppIcon(app.appKey);
                    favApp.filePath = appSer.filePath;
                    favApp.text.SetText(appSer.appTitle);
                }
                
            }
            
            loaded = true;
        }



        public void AddCustomAppIcon(string appKey) {
            throw new NotImplementedException();
        }


        public IEnumerator AddWindowCaptureAppIcon(UwcWindow window) {
            window.PopulateFriendlyTitle();

            string key = window.GetAppKey();
            
            //Check if the app already exists
            foreach (AppIcon temp in apps) {
                if (temp.appKey == key) {
                    yield break;
                }
            }
            

   
            window.RequestCaptureIcon();
            yield return new WaitForSeconds(0.3f);
            

            StartMenuAppSer appSer = new StartMenuAppSer {
                appKey = key, 
                appTitle = window.friendlyTitle,
                filePath = WinNative.GetFilePath(window.handle),
                isCustomApp = false,
                isFavorite = false,
            };
            
            DPSettings.config.startMenu.apps.Add(appSer);
            
            _customAppManager.CacheAppIcon(appSer.appKey, window.iconTexture);
            
            yield return new WaitForSeconds(0.2f);


            LoadApps(false);
            SaveLoaded();

        }


        
        
        






    }


    /// <summary>
    /// Class for storing data about the start menu.
    /// </summary>
    [Serializable]
    public class DPStartMenuData {
        //public List<DPAppSer> favoriteApps;
        public List<StartMenuAppSer> apps = new List<StartMenuAppSer>();

    }

    /// <summary>
    /// Data friendly representation of an app icon with a reference to what app it's for.
    /// </summary>
    [Serializable]
    public class StartMenuAppSer {
        public string appKey;
        public string appTitle;
        public string filePath;
        public bool isCustomApp = false;
        public bool isFavorite = false;

    }
    
    
}