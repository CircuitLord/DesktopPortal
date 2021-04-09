using System;
using Circuits;
using Circuits.CEvents;
using UnityEngine;
using Valve.VR;

namespace DPCore {
	public class OVROverlay {
		//private CVROverlay Overlay = OpenVR.Overlay;

		public static bool pauseAllTexturePtrs = false;
		
		
		public Texture_t _overlayTexture_t = new Texture_t();
		private ETextureType _overlayTextureType = ETextureType.DirectX;

		private VROverlayTransformType _overlayTransformType = VROverlayTransformType.VROverlayTransform_Absolute;

		private ETrackingUniverseOrigin _overlayTransformAbsoluteTrackingOrigin =
			ETrackingUniverseOrigin.TrackingUniverseStanding;

		private uint _overlayTransformTrackedDeviceRelativeIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

		public HmdVector2_t overlayMouseScale = new HmdVector2_t();

		//private bool _isMinimal = false;
		private Texture _currentTexture;
		private HmdMatrix34_t _overlayTransform;
		private string _overlayName;

		/// <summary>
		/// The key and the name of the OpenVR overlay
		/// </summary>
		public string overlayKey { get; private set; }


		//Overlay current values

		/// <summary>
		/// The current overlay width
		/// </summary>
		public float opacity { get; private set; } = 1.0f;

		public bool useSBS { get; private set; } = false;

		public bool sbsCrossedMode { get; private set; } = false;

		/// <summary>
		/// The current sort order of the overlay.
		/// </summary>
		public uint sortOrder { get; private set; } = 0;

		/// <summary>
		/// The handle of the overlay
		/// </summary>
		public ulong handle = OpenVR.k_ulOverlayHandleInvalid;

		/// <summary>
		/// Is the overlay currently visible?
		/// </summary>
		public bool isVisible { get; private set; } = false;

		/// <summary>
		/// Defines if this overlay should be rendered or not
		/// </summary>
		public bool shouldRender {
			get {
				if (isVisible && opacity > 0f) return true;
				else return false;
			}
		}

		/// <summary>
		/// The current width of the overlay
		/// </summary>
		public float width { get; private set; } = 1.0f;

		/// <summary>
		/// The current position of the overlay in SteamVR
		/// </summary>
		public Vector3 pos { get; private set; } = Vector3.zero;

		/// <summary>
		/// The current rotation of the overlay in SteamVR
		/// </summary>
		public Vector3 rot { get; private set; } = Vector3.zero;

		/// <summary>
		/// The current curvature of the overlay
		/// </summary>
		public float curvature { get; private set; } = 0.0f;

		/// <summary>
		/// The tracked device the overlay is anchored to in SteamVR
		/// </summary>
		public DPOverlayTrackedDevice trackedDevice { get; private set; } = DPOverlayTrackedDevice.None;

		/// <summary>
		/// The target texture bounds for the overlay, should be set before any overlay init call if you want to change it.
		/// </summary>
		public VRTextureBounds_t textureBounds = new VRTextureBounds_t() {
			uMin = 1,
			vMin = 1,
			uMax = 0,
			vMax = 0
		};


		//Target overlay settings:

		/// <summary>
		/// The target width for the overlay
		/// </summary>
		public float targetWidth { get; private set; } = 1.0f;

		/// <summary>
		/// The target opacity for the overlay
		/// </summary>
		public float targetOpacity { get; private set; } = 1.0f;

		/// <summary>
		/// The target visibility for the overlay
		/// </summary>
		public bool targetIsVisible { get; private set; } = false;

		/// <summary>
		/// The target position for the overlay
		/// </summary>
		public Vector3 targetPos { get; private set; } = Vector3.zero;

		/// <summary>
		/// The target euler angles for the overlay
		/// </summary>
		public Vector3 targetRot { get; private set; } = Vector3.zero;

		/// <summary>
		/// The target curvature for the overlay
		/// </summary>
		public float targetCurvature { get; private set; } = 0.0f;

		/// <summary>
		/// The target tracked device the overlay is anchored to in SteamVR
		/// </summary>
		public DPOverlayTrackedDevice targetTrackedDevice = DPOverlayTrackedDevice.None;


		public Action<bool> visibilityUpdatedEvent; // = delegate(bool b) {  };

		public Action<float> widthUpdatedEvent;

		public delegate void OverlayTrackedDeviceUpdatedEvent(DPOverlayTrackedDevice device);

		public OverlayTrackedDeviceUpdatedEvent trackedDeviceUpdatedEvent;

		public Action shouldUpdateTrackedDevice;

		/// <summary>
		/// Called whenever the overlay is created
		/// </summary>
		public Action onCreated;


		internal SteamVR_Utils.RigidTransform matrixConverter;


		private EVROverlayError error;


		private ETextureType _graphicsType = ETextureType.Invalid;
		private ETextureType graphicsType {
			get {
				if (_graphicsType == ETextureType.Invalid) {
					_graphicsType = SystemInfo.graphicsDeviceVersion.StartsWith("OpenGL")
						? ETextureType.OpenGL
						: ETextureType.DirectX;
				}
				
				return _graphicsType;
			}
		}

		/// <summary>
		/// Checks a <see cref="EVROverlayError"/> for errors
		/// </summary>
		/// <param name="error">The error to test</param>
		/// <returns>Returns false if all good, returns true if there is an actual error</returns>
		private bool ErrorCheck(EVROverlayError error) {
			bool err = (error != EVROverlayError.None);

			if (err) CLog.Log(OpenVR.Overlay.GetOverlayErrorNameFromEnum(error), CLogLevel.Error);

			return err;
		}

		uint size = (uint) System.Runtime.InteropServices.Marshal.SizeOf(typeof(Valve.VR.VREvent_t));

		/*public bool PollNextOverlayEvent(ref VREvent_t pEvent) {
			if (!CheckValid()) return false;

			return OpenVR.Overlay.PollNextOverlayEvent(handle, ref pEvent, size);
		}*/

		/// <summary>
		/// Returns true if the handle of the overlay is valid.dpc
		/// </summary>
		public bool validHandle {
			get { return handle != OpenVR.k_ulOverlayHandleInvalid; }
		}

		internal bool CheckValid() {
			return SteamVRManager.isConnected && validHandle;
		}


		private bool hasFailedOnce = false;

		/// <summary>
		/// Creates the overlay if it is not already created, and applies the current state values, (not the target values), such as <see cref="width"/> and <see cref="opacity"/>
		/// </summary>
		/// <param name="key">The key of the overlay</param>
		/// <returns>Returns true if the overlay is created or already existing</returns>
		public bool CreateAndApplyOverlay(string key = "") {
			if (CheckValid()) {
				return true;
			}

			if (OpenVR.Overlay == null) {

				if (!hasFailedOnce) {
					hasFailedOnce = true;
					Debug.LogError("OpenVR.Overlay is null! :(");
				}
				
				return false;
			}

			//If the supplied key is empty and the overlay had a key in the past, reuse that same key.
			if (!String.IsNullOrEmpty(overlayKey) && key == "") key = overlayKey;
			overlayKey = key;
			_overlayName = key;

			error = OpenVR.Overlay.CreateOverlay(overlayKey, _overlayName, ref handle);

			if (ErrorCheck(error)) return false;

			
			_overlayTextureType = graphicsType;
			//_overlayTextureType = ETextureType.DirectX;
			
			//_overlayTextureType = ETextureType.DirectX;
			InitOverlayTexture(_overlayTextureType, EColorSpace.Auto);


			//Apply the overlay properties, not the target values

			//OpenVR.Overlay.SetOverlayInputMethod(handle, VROverlayInputMethod.Mouse);
			//OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.ShowTouchPadScrollWheel, true);
			//OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SendVRSmoothScrollEvents, true);


			//SetTrackedDeviceRelativeIndex(trackedDevice);
			shouldUpdateTrackedDevice?.Invoke();

			
			SetSBS(useSBS, sbsCrossedMode);

			SetTransform(pos, rot, false);

			SetWidthInMeters(width, false);

			SetSortOrder(sortOrder);

			SetOpacity(opacity, false);

			SetCurvature(curvature, false);

			SetMouseScale(overlayMouseScale);

			SetTexture(_currentTexture);
			
			SetTextureBounds(textureBounds);
			
			onCreated?.Invoke();

			return true;
		}

		/// <summary>
		/// Sets if the overlay is visible.
		/// If requested to be visible, and the overlay is not visible, it will be created.
		/// Vice-versa for hiding and destroying.
		/// </summary>
		/// <param name="v">Should the overlay be visible?</param>
		/// <param name="setTarget">Should this set the target visibility state for the overlay?</param>
		public void SetVisible(bool v = true, bool setTarget = true) {
			if (setTarget) targetIsVisible = v;

			if (isVisible == v) return;

			isVisible = v;


			if (v == true && !CheckValid()) {
				//Try to recreate the overlay, if it fails return;
				if (!CreateAndApplyOverlay()) return;

				OpenVR.Overlay.ShowOverlay(handle);

				visibilityUpdatedEvent?.Invoke(true);
			}

			else if (v == true) {
				OpenVR.Overlay.ShowOverlay(handle);
				visibilityUpdatedEvent?.Invoke(true);
			}

			else if (v == false && CheckValid()) {
				OpenVR.Overlay.HideOverlay(handle);

				DestroyOverlay();
				visibilityUpdatedEvent?.Invoke(false);
			}

			else {
				OpenVR.Overlay.HideOverlay(handle);
				visibilityUpdatedEvent?.Invoke(false);
			}
		}

		public void SetMouseScale(HmdVector2_t v) {
			overlayMouseScale = v;

			if (!CheckValid()) return;

			OpenVR.Overlay.SetOverlayMouseScale(handle, ref overlayMouseScale);
		}


		public void SetCurvature(float amt, bool setTarget = false) {
			curvature = amt;

			if (setTarget) targetCurvature = amt;

			if (!CheckValid()) return;

			OpenVR.Overlay.SetOverlayCurvature(handle, amt);
		}


		public void InitOverlayTexture(ETextureType texType, EColorSpace colorSpace = EColorSpace.Auto) {
			//TODO: Try different texture types to improve perf
			_overlayTexture_t.eType = texType;
			_overlayTexture_t.eColorSpace = colorSpace;
		}

		private bool isFirst = true;

		internal void SetTexture(Texture tex) {

			if (tex == null) return;
			
			//if (!CheckValid()) return;

			/*if (isFirst) {
				_currentTexture = tex;
				_overlayTexture_t.handle = tex.GetNativeTexturePtr();
				isFirst = false;

			}*/
			
			try {
				//if (_currentTexture != null && _currentTexture.width != tex.width || _currentTexture.height != tex.height) {
				//	_currentTexture = tex;
				//	_overlayTexture_t.handle = tex.GetNativeTexturePtr();
				//}


				//tex.

				/*if (_currentTexture != tex || _currentTexture == null) {
					_overlayTexture_t.handle = tex.GetNativeTexturePtr();
					
					_currentTexture = tex;
				}*/
				_currentTexture = tex;

				if (pauseAllTexturePtrs) return;


				_overlayTexture_t.handle = tex.GetNativeTexturePtr();

				if (!CheckValid()) return;

				EVROverlayError error = OpenVR.Overlay.SetOverlayTexture(handle, ref _overlayTexture_t);

				//if (error != EVROverlayError.None) Debug.Log(error);
			}
			catch (Exception e) {
				Debug.LogError(e);
			}
		}

		/// <summary>
		/// Set the texture bounds of the overlay, and update it if the overlay is visible.
		/// </summary>
		/// <param name="texBounds">The new texture bounds</param>
		internal void SetTextureBounds(VRTextureBounds_t texBounds) {
			textureBounds = texBounds;

			if (!CheckValid()) return;

			error = OpenVR.Overlay.SetOverlayTextureBounds(handle, ref textureBounds);
			//return ErrorCheck(error);
		}

		public void SetSBS(bool enabled, bool crossed = false) {

			useSBS = enabled;
			sbsCrossedMode = crossed;

			if (!CheckValid()) return;
			
			if (crossed && enabled) {
				OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SideBySide_Parallel, false);
				OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SideBySide_Crossed, true);
			}
			else if (enabled) {
				OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SideBySide_Crossed, false);
				OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SideBySide_Parallel, true);
			}

			else {
				OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SideBySide_Crossed, false);
				OpenVR.Overlay.SetOverlayFlag(handle, VROverlayFlags.SideBySide_Parallel, false);
			}
		}


		//public void SetOverlayTransformType(VROverlayTransformType type) {
		//	_overlayTransformType = type;
		//}


		internal void SetTrackedDeviceRelativeIndex(DPOverlayTrackedDevice device, uint customDeviceIndex = 0, bool setTarget = true) {
			uint index = 0;
			switch (device) {
				case DPOverlayTrackedDevice.HMD:
					index = SteamVRManager.hmdIndex;
					break;

				case DPOverlayTrackedDevice.RightHand:
					index = SteamVRManager.rightHandIndex;
					break;

				case DPOverlayTrackedDevice.LeftHand:
					index = SteamVRManager.leftHandIndex;
					break;

				case DPOverlayTrackedDevice.CustomIndex:
					index = customDeviceIndex;
					break;

				default:
					index = 0;
					break;
			}

			VROverlayTransformType tType = device == DPOverlayTrackedDevice.None
				? VROverlayTransformType.VROverlayTransform_Absolute
				: VROverlayTransformType.VROverlayTransform_TrackedDeviceRelative;

			_overlayTransformType = tType;
			_overlayTransformTrackedDeviceRelativeIndex = index;
			trackedDevice = device;

			if (setTarget) targetTrackedDevice = device;

			trackedDeviceUpdatedEvent?.Invoke(device);


			//TODO: Get target device index and transform type working sometime in the future
		}

		/// <summary>
		/// Set the overlay transform in SteamVR
		/// </summary>
		/// <param name="newPos">The new position</param>
		/// <param name="newRot">The new rotation</param>
		/// <param name="setTarget">Should this be the new target position, or is this temporary?</param>
		public void SetTransform(Vector3 newPos, Vector3 newRot, bool setTarget = true) {
			pos = newPos;
			rot = newRot;

			if (setTarget) {
				targetPos = newPos;
				targetRot = newRot;
			}

			matrixConverter.pos = newPos;
			matrixConverter.rot = Quaternion.Euler(newRot);


			_overlayTransform = matrixConverter.ToHmdMatrix34();
			_overlayTransformAbsoluteTrackingOrigin = SteamVRManager.trackingSpace;

			if (!CheckValid()) return;

			switch (_overlayTransformType) {
				default:
				case VROverlayTransformType.VROverlayTransform_Absolute:

					error = OpenVR.Overlay.SetOverlayTransformAbsolute(handle, _overlayTransformAbsoluteTrackingOrigin,
						ref _overlayTransform);

					break;
				
				case VROverlayTransformType.VROverlayTransform_TrackedDeviceRelative:

					error = OpenVR.Overlay.SetOverlayTransformTrackedDeviceRelative(handle,
						_overlayTransformTrackedDeviceRelativeIndex, ref _overlayTransform);


					break;
			}
		}


		public void SetTransformOverlayRelative(ulong parentHandle, Vector3 newPos, Vector3 newRot, bool setTarget = true) {
			pos = newPos;
			rot = newRot;

			if (setTarget) {
				targetPos = newPos;
				targetRot = newRot;
			}

			matrixConverter.pos = newPos;
			matrixConverter.rot = Quaternion.Euler(newRot);

			_overlayTransform = matrixConverter.ToHmdMatrix34();

			if (!CheckValid()) return;

			OpenVR.Overlay.SetOverlayTransformOverlayRelative(handle, parentHandle, ref _overlayTransform);
		}

		/// <summary>
		/// Sets the width in meters for the overlay
		/// </summary>
		/// <param name="w">The new width for the overlay</param>
		/// <param name="setTarget">Should this be the new target width for the overlay</param>
		public void SetWidthInMeters(float w, bool setTarget = true) {
			float previousWidth = width;

			width = w;

			if (setTarget) targetWidth = w;

			widthUpdatedEvent?.Invoke(previousWidth);

			if (!CheckValid()) return;

			error = OpenVR.Overlay.SetOverlayWidthInMeters(handle, w);
		}

		public void SetSortOrder(uint order) {
			sortOrder = order;

			if (!CheckValid()) return;

			OpenVR.Overlay.SetOverlaySortOrder(handle, order);
		}

		internal void _setWidthInMetersYesTarget(float w) {
			SetWidthInMeters(w, true);
		}

		internal void _setWidthInMetersNoTarget(float w) {
			SetWidthInMeters(w, false);
		}


		internal void SetOpacity(float o, bool setTarget = true) {
			opacity = o;

			if (setTarget) targetOpacity = o;

			if (!CheckValid()) return;

			OpenVR.Overlay.SetOverlayAlpha(handle, o);
		}

		internal void _setOpacityYesTarget(float o) {
			SetOpacity(o, true);
		}

		internal void _setOpacityNoTarget(float o) {
			SetOpacity(o, false);
		}

		internal void _setCurvatureYesTarget(float c) {
			SetCurvature(c, true);
		}

		internal void _setCurvatureNoTarget(float c) {
			SetCurvature(c, false);
		}


		/// <summary>
		/// If the overlay handle is currently valid, it destroys the openvr representation of the overlay.
		/// </summary>
		/// <returns>Returns false if the overlay was successfully destroyed</returns>
		public void DestroyOverlay() {
			if (!CheckValid()) return;

			if (OpenVR.Overlay != null) {
				error = OpenVR.Overlay.DestroyOverlay(handle);
				handle = OpenVR.k_ulOverlayHandleInvalid;
			}
		}
	}

	/// <summary>
	/// Represents a tracked device in OpenVR
	/// </summary>
	public enum DPOverlayTrackedDevice {
		None = 0,
		HMD = 1,
		LeftHand = 2,
		RightHand = 3,
		CustomIndex = 4,
		TheBar = 5
	}
}