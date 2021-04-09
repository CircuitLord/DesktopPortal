using System;
using UnityEngine;
using UnityEngine.UI;

namespace DPCore.Apps {
    
    

    public abstract class AppControllerOld : MonoBehaviour {

        [HideInInspector] public DPApp dpApp;

        [SerializeField] public string appKey;
        
        [SerializeField] public Texture2D iconTex;

        [HideInInspector] public GameObject theBarButtonGO;

        /// <summary>
        /// Can only be used by DesktopPortal apps. Will be set to false no matter what for custom apps.
        /// </summary>
        public bool isIconPersistant = false;
        
        

        public virtual void Start() {
            
            FetchAppComponents();
            
            //VerifyAppKey();
            
        }

        private void VerifyAppKey() {
            appKey = appKey.Replace(" ", "-");
            
            if (appKey == "") appKey = "unnamed";
        }


        public void FetchAppComponents() {
            
            dpApp = GetComponent<DPApp>();
            if (!dpApp) Debug.LogError("Could not find DPApp component. Ensure it is attached to the root object with the App Controller.");
            

        }



        /// <summary>
        /// Called when the app is opened for the first time. Called after <see cref="Start"/>
        /// </summary>
        public virtual void Init() {
            
        }

        /// <summary>
        /// Called when the app is opening/un-minimizing
        /// </summary>
        public virtual void Opening() {
            
        }

        /// <summary>
        /// Called when the app is being minimized
        /// </summary>
        public virtual void Minimizing() {
            
        }

        /// <summary>
        /// Called when the app needs to be completely closed and destroyed
        /// </summary>
        public virtual void Closing() {
            
        }

        /// <summary>
        /// Called when the bar is opening
        /// </summary>
        public virtual void TheBarOpening() {
            
        }
        
        /// <summary>
        /// Called when the bar is closing
        /// </summary>
        public virtual void TheBarClosing() {
            
        }



    }
}