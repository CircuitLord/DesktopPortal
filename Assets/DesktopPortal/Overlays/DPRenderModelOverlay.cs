using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Interaction;
using DesktopPortal.Overlays;
using DPCore;
using DPCore.Interaction;
using UnityEngine;
using Valve.VR;

public class DPRenderModelOverlay : DPOverlayBase { 
    
    
    public string pathToRenderModel;

    public Texture2D tempTex;
    
    public Color renderModelColor = Color.white;
    
    

    protected override void InitOVROverlay() {
       // if (overlay.validHandle) return;

        //overlay.SetTextureBounds(new VRTextureBounds_t {uMin = 0, vMin = 0, uMax = 1, vMax = 1});

        overlay.CreateAndApplyOverlay(startOverlayKey);
        
        HmdColor_t color = new HmdColor_t();
        color.a = renderModelColor.a;
        color.r = renderModelColor.r;
        color.g = renderModelColor.g;
        color.b = renderModelColor.b;
        
        overlay.SetVisible(true);

        Debug.Log("setting values");

        //OpenVR.Overlay.SetOverlayRenderModel(overlay.handle, pathToRenderModel, ref color);
        
        SetOverlayTransform(SteamVRManager.I.hmdTrans.position + SteamVRManager.I.hmdTrans.forward, SteamVRManager.I.hmdTrans.eulerAngles, true, true);

        //overlay.SetTexture(tempTex);
        currentTexture = tempTex;
        
        //OpenVR.Overlay.SetOverlayInputMethod(overlay.handle, VROverlayInputMethod.Mouse);
        //OpenVR.Overlay.SetOverlayFlag(overlay.handle, VROverlayFlags.HideLaserIntersection, true);

        //TODO: Look at  event
        //shouldCalculateInteractDesired = true;
        //lookAtEvent.AddListener(delegate(bool arg0) { isFocused = arg0; });

        hasBeenInitialized = true;


        Debug.Log(overlay.handle + " " + overlay.overlayKey);
    }

    public override void RequestRendering(bool force = false) {
        throw new System.NotImplementedException();
    }

    public override void ResizeForRatio(int ratioX, int ratioY) {
        throw new System.NotImplementedException();
    }

    public override List<Vector3> HandleColliderInteracted(DPInteractor interactor, List<Vector2> interactionPoints) {
        throw new System.NotImplementedException();
    }
    
}
