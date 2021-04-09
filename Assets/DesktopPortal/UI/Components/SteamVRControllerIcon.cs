using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DesktopPortal.Steam;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;


namespace DesktopPortal.UI.Components {
    
    
    public class SteamVRControllerIcon : MonoBehaviour
    {

        [SerializeField] private RawImage rawImage;

        //[SerializeField] private SteamVRControllerIconsConfig iconsConfig;


        [SerializeField] private bool isLeft = true;

        [SerializeField] private bool isHMD = false;


        private void Start() {
            SteamVRControllerIconManager.onControllerIconsUpdated += UpdateIcon;
        }


        public void UpdateIcon() {

            if (isHMD) {
                rawImage.texture = SteamVRControllerIconManager.hmdIcon;
            } 
            
            else if (isLeft) {
                rawImage.texture = SteamVRControllerIconManager.leftIcon;
            }
            else {
                rawImage.texture = SteamVRControllerIconManager.rightIcon;
            }

        }


        
    }

    /*[CreateAssetMenu(fileName = "SteamVRControllerIconsConfig", menuName = "DP/SteamVRControllerIconsConfig", order = 0)]
    public class SteamVRControllerIconsConfig : ScriptableObject {

        [ShowInInspector]
        public Dictionary<string, SteamVRControllerIconCollection> icons = new Dictionary<string, SteamVRControllerIconCollection>();


        
        
        
    }

    [System.Serializable]
    public class SteamVRControllerIconCollection {
        public Texture2D activatedIcon;
        public Texture2D deactivatedIcon;
    }*/
    
}