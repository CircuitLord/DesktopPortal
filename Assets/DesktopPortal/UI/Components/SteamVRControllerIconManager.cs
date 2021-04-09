using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DesktopPortal.Overlays;
using DesktopPortal.Steam;
using DPCore;
using UnityEngine;
using Valve.VR;

namespace DesktopPortal.UI.Components {
    public class SteamVRControllerIconManager : MonoBehaviour {

        //public static SteamVRControllerIconManager I;
        
        public static Texture2D leftIcon;
        public static Texture2D rightIcon;
        public static Texture2D hmdIcon;


        public static Action onControllerIconsUpdated;
        
        private void Start() {
            SteamVRManager.I.onSteamVRConnected.AddListener(UpdateIcons);
            SteamVRManager.I.onTrackedDeviceConnected.AddListener(UpdateIcons);
        }

        
        private static void UpdateIcons() {

            OverlayManager.I.StartCoroutine(UpdateIconsDelayed());
        }

        private static IEnumerator UpdateIconsDelayed() {
            yield return new WaitForSeconds(3f);
            UpdateIcon(ref leftIcon, SteamVRManager.leftHandIndex);
            UpdateIcon(ref rightIcon, SteamVRManager.rightHandIndex);
            UpdateIcon(ref hmdIcon, SteamVRManager.hmdIndex);
            
            onControllerIconsUpdated?.Invoke();
        }

        private static bool UpdateIcon(ref Texture2D icon, uint deviceIndex) {
            //yield return new WaitForSeconds(1f);
           // Debug.Log("updating icons");

            StringBuilder iconPathLeft = new StringBuilder(500);
            ETrackedPropertyError pError = new ETrackedPropertyError();
            OpenVR.System.GetStringTrackedDeviceProperty(deviceIndex,
                ETrackedDeviceProperty.Prop_NamedIconPathDeviceReady_String, iconPathLeft, 500, ref pError);

            if (pError != ETrackedPropertyError.TrackedProp_Success) return false;

            if (iconPathLeft.Length <= 0) return false;

            string controllerType = "";
            string restPath = "";

            for (int i = 1; i < 50; i++) {

                if (iconPathLeft[i] == '}') {
                    restPath = iconPathLeft.ToString().Replace("{" + controllerType + "}/", "");
                    break;
                }

                controllerType += iconPathLeft[i];
            }

            
            string steamPath = SteamFinder.FindGameFolder(250820);
            icon = LoadPNG(Path.Combine(steamPath, "drivers", controllerType, "resources", restPath));

            return true;
        }
        
                
        public static Texture2D LoadPNG(string filePath) {
            Texture2D tex = null;
            byte[] fileData;
 
            if (File.Exists(filePath))     {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, TextureFormat.BGRA32, true);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
            return tex;
        }

    }
}