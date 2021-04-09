using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DPCore;
using UnityEngine;

public class DPNightlight : MonoBehaviour
{



    [SerializeField] private DPCameraOverlay nightlightDP;
    
    
    // Start is called before the first frame update
    void Start() {


       // nightlightDP.onInitialized += Setup;


    }


    private void Update() {
        if (nightlightDP.hasBeenInitialized) {

            Vector3 goodPos = SteamVRManager.I.hmdTrans.position + SteamVRManager.I.hmdTrans.forward * 0.11f;
            
            nightlightDP.SetOverlayTransform(goodPos, SteamVRManager.I.hmdTrans.eulerAngles, true);
            

        }
    }





}
