//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Displays 2d content on a large virtual screen.
//
//=============================================================================

using UnityEngine;
using System.Collections;
using Valve.VR;

namespace Valve.VR
{
    public class SteamVR_Overlay : MonoBehaviour
    {
        public Texture texture;

        [Tooltip("Size of overlay view.")]
        public float scale = 3.0f;

        [Tooltip("Distance from surface.")]
        public float distance = 1.25f;

        [Tooltip("Opacity"), Range(0.0f, 1.0f)]
        public float alpha = 1.0f;

        public Vector4 uvOffset = new Vector4(0, 0, 1, 1);
        public Vector2 mouseScale = new Vector2(1, 1);

        public VROverlayInputMethod inputMethod = VROverlayInputMethod.None;

        static public SteamVR_Overlay instance { get; private set; }

        static public string key { get { return "unity:" + Application.companyName + "." + Application.productName; } }

        private ulong handle = OpenVR.k_ulOverlayHandleInvalid;

        void OnEnable()
        {
            var overlay = OpenVR.Overlay;
            if (overlay != null)
            {
                var error = overlay.CreateOverlay(key, gameObject.name, ref handle);
                if (error != EVROverlayError.None)
                {
                    Debug.Log("<b>[SteamVR]</b> " + overlay.GetOverlayErrorNameFromEnum(error));
                    enabled = false;
                    return;
                }
            }

            SteamVR_Overlay.instance = this;
        }

        void OnDisable()
        {
            if (handle != OpenVR.k_ulOverlayHandleInvalid)
            {
                var overlay = OpenVR.Overlay;
                if (overlay != null)
                {
                    overlay.DestroyOverlay(handle);
                }

                handle = OpenVR.k_ulOverlayHandleInvalid;
            }

            SteamVR_Overlay.instance = null;
        }

        public void UpdateOverlay()
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
                return;

            if (texture != null)
            {
                var error = overlay.ShowOverlay(handle);
                if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
                {
                    if (overlay.FindOverlay(key, ref handle) != EVROverlayError.None)
                        return;
                }

                var tex = new Texture_t();
                tex.handle = texture.GetNativeTexturePtr();
                tex.eType = SteamVR.instance.textureType;
                tex.eColorSpace = EColorSpace.Auto;
                overlay.SetOverlayTexture(handle, ref tex);

                overlay.SetOverlayAlpha(handle, alpha);
                overlay.SetOverlayWidthInMeters(handle, scale);

                var textureBounds = new VRTextureBounds_t();
                textureBounds.uMin = (0 + uvOffset.x) * uvOffset.z;
                textureBounds.vMin = (1 + uvOffset.y) * uvOffset.w;
                textureBounds.uMax = (1 + uvOffset.x) * uvOffset.z;
                textureBounds.vMax = (0 + uvOffset.y) * uvOffset.w;
                overlay.SetOverlayTextureBounds(handle, ref textureBounds);

                var vecMouseScale = new HmdVector2_t();
                vecMouseScale.v0 = mouseScale.x;
                vecMouseScale.v1 = mouseScale.y;
                overlay.SetOverlayMouseScale(handle, ref vecMouseScale);

                var vrcam = SteamVR_Render.Top();
                if (vrcam != null && vrcam.origin != null)
                {
                    var offset = new SteamVR_Utils.RigidTransform(vrcam.origin, transform);
                    offset.pos.x /= vrcam.origin.localScale.x;
                    offset.pos.y /= vrcam.origin.localScale.y;
                    offset.pos.z /= vrcam.origin.localScale.z;

                    offset.pos.z += distance;

                    var t = offset.ToHmdMatrix34();
                    overlay.SetOverlayTransformAbsolute(handle, SteamVR.settings.trackingSpace, ref t);
                }

                overlay.SetOverlayInputMethod(handle, inputMethod);
            }
            else
            {
                overlay.HideOverlay(handle);
            }
        }

        public bool PollNextEvent(ref VREvent_t pEvent)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
                return false;

            var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VREvent_t));
            return overlay.PollNextOverlayEvent(handle, ref pEvent, size);
        }
    }
}