using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;

namespace DesktopPortal.UI {
    

    public class TheBarStatus : MonoBehaviour {


        
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI perLeft;
        [SerializeField] private TextMeshProUGUI perRight;

        [SerializeField] private DPCameraOverlay timeDP;


        [SerializeField] private Image leftFill;
        [SerializeField] private Image rightFill;
        
        
        [Header("Configuration")] 
        
        [SerializeField] private float fastUpdateRate = 3f;
        
        // Start is called before the first frame update
        void Start() {

            StartCoroutine(UpdateInfo());

        }


        
        private IEnumerator UpdateInfo() {
            while (true) {


                while (!SteamVRManager.isConnected || !TheBarManager.isOpened) {
                    yield return null;
                }

                while (!timeDP.overlay.shouldRender) {
                    yield return new WaitForSeconds(fastUpdateRate);
                }
                
                
                //Current time:
                timeText.SetText(DateTime.Now.ToShortTimeString());



                ETrackedPropertyError error = ETrackedPropertyError.TrackedProp_Success;
                
                //Get battery lives
                float left = OpenVR.System.GetFloatTrackedDeviceProperty(SteamVRManager.leftHandIndex, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, ref error);
                float right = OpenVR.System.GetFloatTrackedDeviceProperty(SteamVRManager.rightHandIndex, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float, ref error);


                leftFill.fillAmount = left;
                rightFill.fillAmount = right;
                
                string leftPercent;
                string rightPercent;
                
                if (left >= 99.9f) {
                    leftPercent = "Full";
                }
                else {
                    leftPercent = Mathf.RoundToInt(left * 100) + "%";
                }

                if (right >= 99.9f) {
                    rightPercent = "Full";
                }
                else {
                    rightPercent = Mathf.RoundToInt(right * 100) + "%";
                }
                
                
                perLeft.SetText(leftPercent);
                perRight.SetText(rightPercent);

                
                timeDP.RequestRendering();


                yield return new WaitForSeconds(fastUpdateRate);
            }
        }
    }
}
