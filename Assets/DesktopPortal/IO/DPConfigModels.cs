using System;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DesktopPortal.UI;
using DPCore;
using UnityEngine;

namespace DesktopPortal.IO {
    
    
    
    
    [Serializable]
    public class DPConfigJson {

        public bool wristHandLeft = true;
        public Vector3 wristPos = new Vector3(-0.05f, 0.09f, -0.21f);
        public Vector3 wristRot = new Vector3(165.7f, -63.7f, 29.5f);
        
        public DPStartMenuData startMenu = new DPStartMenuData();

        public DPDragMode dragMode = DPDragMode.BlendHMDHand;

        public bool snapPointsEnabled = true;
        public bool snapPointsResize = false;

        public bool focusGameWhenNotInteracting = false;

        public float dimGameAmount = 0.2f;

        public bool autoCurveDragging = true;
        public float autoCurveYThreshhold = 0.1f;
        public float autoCurveAmount = 0.11f;

        public bool useDDA = true;

        public bool overrideOverlayRenderQuality = false;
        public DPRenderQuality renderQuality = DPRenderQuality.Maximum;
        
        public float joystickDeadzone = 0.1f;

        public bool hasJoinedDiscord = false;
        public bool isFirstTime = true;
        public bool minimizeAtStartup = false;
        public bool lockWatch = false;
        public bool hideBarHeadsetOff = true;
        public string lastSeenVersion = "";
        public int configVersion = 4;
    }

    
}