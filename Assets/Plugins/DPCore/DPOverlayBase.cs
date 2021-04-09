using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Circuits.CEvents;
using DPCore.Interaction;
using DG.Tweening;
using DPCore.Apps;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Valve.VR;

namespace DPCore {
	/// <summary>
	/// Base class for any overlay Desktop Portal uses. Classes such as <see cref="DPCameraOverlay"/> extend from this to provide a specific type
	/// of functionality to the overlays.
	/// </summary>
	public abstract class DPOverlayBase : MonoBehaviour {
		public readonly static int overlayLayerMask = 8;

		public static List<DPOverlayBase> needToProcess = new List<DPOverlayBase>();

		public static Action<DPOverlayBase> onClickedOn;

		private const float renderAfterGoIdleTime = 1f;
		

		[Header("DEBUG")] [SerializeField] private bool enableDebug = false;

		[ShowIf("enableDebug")] [SerializeField]
		private bool forceTransformSync = true;

		[ShowIf("enableDebug")] [SerializeField]
		private bool forceApplyInspectorProperties = true;


		//[Header("Configuration")]

		//Starting state

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected DPOverlayTrackedDevice startOverlayTrackedDevice = DPOverlayTrackedDevice.None;

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected float startOverlayWidth = 1f;

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected float startOverlayOpacity = 1f;

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected bool startVisible = false;

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected float startCurvature = 0.0f;

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected int startSortOrder = 2;

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected Vector4 startTextureBounds = new Vector4(0, 1, 1, 0);

		[BoxGroup("Starting Configuration")] [SerializeField]
		protected bool startUseSBS = false;

		[BoxGroup("Starting Configuration")] [SerializeField] [ShowIf("startUseSBS")]
		protected bool startSBSCrossedMode = false;


		//PUBLIC stuff:
		public OVROverlay overlay = new OVROverlay();


		[HideInInspector] public DPOverlayBase parent;

		[HideInInspector] public DPApp dpAppParent = null;

		[HideInInspector] public DPSnapPoint snapPoint = null;
		public bool isUsingSnapPoint => snapPoint != null;
		public bool hasDPAppParent => dpAppParent != null;

		[HideInInspector] public bool snapPointQueuedToResize = false;


		/// <summary>
		/// If true, the overlay will automatically be pre-initialized by the Overlay Manager
		/// </summary>
		[Tooltip("If true, the overlay will automatically be added to a list of overlays to be pre-initialized by the OverlayManager")] [SerializeField]
		public bool autoPreInitialize = true;

		/// <summary>
		/// Does this overlay rely on the existence of another? AKA The sidebar on DPApps. (autoset for sidebars)
		/// </summary>
		[BoxGroup("Toolbar")]
		[SerializeField] public bool showToolbar = true;

		[BoxGroup("Toolbar")]
		[ShowIf("showToolbar")]
		public bool showToolbarPin = true;

		[BoxGroup("Toolbar")]
		[ShowIf("showToolbar")]
		public bool showToolbarSettings = true;

		/// <summary>
		/// Is this overlay visible when the bar is closed?
		/// </summary>
		[HideInInspector] public bool isPinned = false;


		[BoxGroup("Visibility")] [Tooltip("Does the overlay hide when you're not looking at it?")] [SerializeField] [FormerlySerializedAs("useLookShowing")]
		public bool useLookHiding = false;

		[BoxGroup("Visibility")] [ShowIf("useLookHiding")]
		public bool lookHidingFaceHMD = true;

		[BoxGroup("Visibility")] [ShowIf("useLookHiding")]
		public bool lookHidingFaceOverlay = true;

		[BoxGroup("Visibility")] [ShowIf("useLookHiding")]
		public float lookHidingStrength = 0.7f;

		[BoxGroup("Visibility")] [ShowIf("useLookHiding")]
		public float lookHidingOpacity = 0.1f;


		[BoxGroup("Visibility")] public bool useDistanceHiding = false;

		[BoxGroup("Visibility")] [ShowIf("useDistanceHiding")]
		public float distanceHidingDistance = 1f;

		[BoxGroup("Visibility")] [ShowIf("useDistanceHiding")]
		public float distanceHidingOpacity = 0.1f;


		/*[BoxGroup("Visibility")] 
		[SerializeField] private bool _useSBS = false;

		public bool useSBS {
			get { return _useSBS; }
			set {
				_useSBS = value;
				overlay.SetSBS(value, sbsCrossedMode);
				
			}
		}
		
		[BoxGroup("Visibility")] 
		[SerializeField] private bool _sbsCrossedMode = false;
		
		public bool sbsCrossedMode {
			get { return _sbsCrossedMode; }
			set {
				_sbsCrossedMode = value;
				overlay.SetSBS(useSBS, value);
			}
		}*/


		[BoxGroup("Visibility")] public bool useWindowCropping = false;

		[BoxGroup("Visibility")] [ShowIf("useWindowCropping")]
		public Vector4 cropAmount = Vector4.zero;
		
		[BoxGroup("Visibility")]
		public float maxOverlayWidth = -1f;


		//[BoxGroup("Transform")]
		//public bool 

		[BoxGroup("Transform")] [SerializeField]
		private bool _useSmoothAnchoring = false;

		public bool useSmoothAnchoring {
			get { return _useSmoothAnchoring; }
			set {
				_useSmoothAnchoring = value;

				//If it's already anchored to something, we need to refresh  it to use smooth anchoring
				//if (overlay.trackedDevice != DPOverlayTrackedDevice.None) {
				SetOverlayTrackedDevice(overlay.trackedDevice);
				//}
			}
		}

		[BoxGroup("Transform")] [ShowIf("_useSmoothAnchoring")]
		public float smoothAnchoringStrength = 3f;

		[BoxGroup("Transform")] [ShowIf("_useSmoothAnchoring")]
		public Transform smoothAnchoringTarget = null;

		public GameObject smoothAnchoringDummyObject = null;

		[HideInInspector] public DPOverlayTrackedDevice smoothAnchoringTrackedDevice = DPOverlayTrackedDevice.None;


		[BoxGroup("Transform")] [SerializeField]
		private bool _useSnapAnchoring = false;

		public bool useSnapAnchoring {
			get { return _useSnapAnchoring; }
			set {
				_useSnapAnchoring = value;

				//If it's already anchored to something, we need to refresh it
				//if (overlay.trackedDevice != DPOverlayTrackedDevice.None) {
				SetOverlayTrackedDevice(overlay.trackedDevice);
				//}
			}
		}

		[BoxGroup("Transform")] [ShowIf("_useSnapAnchoring")]
		public float snapAnchoringDistance = 0.4f;

		[BoxGroup("Transform")] [ShowIf("_useSnapAnchoring")]
		public bool snapAnchoringNormalizeRot = true;


		[BoxGroup("Transform")] [ShowIf("_useSnapAnchoring")]
		public Transform snapAnchoringTarget = null;


		private bool isSnapAnchoringToNewPos = false;

		public GameObject snapAnchoringDummyObject = null;
		
		[HideInInspector] public DPOverlayTrackedDevice snapAnchoringTrackedDevice = DPOverlayTrackedDevice.None;


		[BoxGroup("Transform")] public bool isAnchoredToTheBar = false;

		[BoxGroup("Transform")]
		public bool overrideAllowedAnchors = false;
		
		[ShowIf("overrideAllowedAnchors")]
		[BoxGroup("Transform")] public List<DPOverlayTrackedDevice> allowedAnchors = new List<DPOverlayTrackedDevice>() {
			DPOverlayTrackedDevice.None,
			DPOverlayTrackedDevice.LeftHand,
			DPOverlayTrackedDevice.RightHand,
			DPOverlayTrackedDevice.TheBar,
			DPOverlayTrackedDevice.CustomIndex
		};
		

		[HideInInspector] public bool isAnimatingForceHighFPS = false;


		/// <summary>
		/// Returns true if the overlay has been pre-initialized already
		/// </summary>
		[HideInInspector] public bool hasBeenPreInitialized = false;

		/// <summary>
		/// Returns true if the overlay has been initialized already
		/// </summary>
		[HideInInspector] public bool hasBeenInitialized = false;


		[Space] [BoxGroup("Interaction")] [SerializeField]
		public bool isInteractable = true;

		[BoxGroup("Interaction")] public bool canUseSnapPoints = true;

		[BoxGroup("Interaction")] [ShowIf("isInteractable")]
		public bool allowMultipuleInteractors = false;

		[BoxGroup("Interaction")] [Tooltip("Does this overlay adapt to ratio changes from the snap points?")]
		public bool isResponsive = false;

		[BoxGroup("Interaction")] [ShowIf("isInteractable")] [SerializeField]
		private bool _alwaysInteract = false;

		/// <summary>
		/// Defines if the overlay should be clickable when the bar is closed. (temporarially enabling interaction when pointed at)
		/// </summary>

		public bool alwaysInteract {
			get { return _alwaysInteract; }
			set {
				if (_alwaysInteract == value) return;
				_alwaysInteract = value;
				onAlwaysInteractChanged?.Invoke(value);
			}
		}
		
		[BoxGroup("Interaction")] [ShowIf("isInteractable")] [SerializeField]
		private bool _alwaysInteractBlockInput = false;
		public bool alwaysInteractBlockInput {
			get { return _alwaysInteractBlockInput; }
			set {
				if (_alwaysInteractBlockInput == value) return;
				_alwaysInteractBlockInput = value;
				onAlwaysInteractBlockInputChanged?.Invoke(value);
			}
		}
		
		

		[BoxGroup("Interaction")]
		public bool useTouchInput = true;


		/// <summary>
		/// If true, this overlay is allowed to be dragged by the OverlayInteractionManager
		/// </summary>
		[BoxGroup("Interaction")] [ShowIf("isInteractable")] [SerializeField]
		public bool isDraggable = true;

		[BoxGroup("Interaction")] [ShowIf("isInteractable")]
		public bool canBeCurvedWhenDragged = true;

		[HideInInspector] public bool isChild = false;

		[Header("Child Settings")] [Tooltip("Does this child follow the scale of it's parent?")]
		public bool followParentScale = true;

		[Tooltip("If point interaction is enabled on the parent, is it on the child?")]
		public bool followParentPointInteraction = true;

		public bool followParentOpacity = true;

		[Space] [SerializeField] public List<DPOverlayBase> children = new List<DPOverlayBase>();


		[Header("Rendering Settings")] public bool renderWhenInteractedWith = true;

		public bool renderWhenLookedAt = false;

		/// <summary>
		/// If an overlay is static, that means it will only be captured once, and never again.
		/// </summary>
		[SerializeField] public bool isStatic = false;


		/// <summary>
		/// Will override the automatic optimization rendering system and always render the overlay at a constant framerate if visible
		/// </summary>
		[SerializeField] public bool forceHighCaptureFramerate = false;

		/// <summary>
		/// If <see cref="overrideAutoRenderingFPS"/> is set to true, this is the capture framerate the overlay should use
		/// </summary>
		[SerializeField] public int fpsToCaptureAt = 30;

		public int idleFPSToCatureAt = 2;
		


		//PRIVATE stuff:
		protected string startOverlayKey;

		protected bool updatedAnchor = false;

		protected bool isTransitioningOverlay = false;
		protected bool isTransitioningSetTargetPos = false;

		protected float captureTimer = 0f;
		protected float fpsAsMS => 1f / (float) fpsToCaptureAt;
		protected float idleFPSAsMS => 1f / (float) idleFPSToCatureAt;

		protected float timeSinceLastInteract = 0f;

		protected VREvent_t pEvent;

		public Texture currentTexture;
		private Vector2 _textureSize = Vector2.zero;
		private Vector4 _prevTextureBounds = Vector4.zero;
		protected Vector2 mouseScale = new Vector2(1f, 1f);
		protected HmdVector2_t mouseScale_t = new HmdVector2_t();
		[HideInInspector] public float reverseAspect = 0f;


		[HideInInspector] public float overlayHeight => overlay.width * reverseAspect;
		[HideInInspector] public Vector4 textureBounds;


		private Vector2 localScrollStrength = Vector2.zero;

		private bool isBeingDragged = false;


		public bool isBeingScrolled { get; protected set; } = false;

		private bool _isBeingInteracted = false;

		/// <summary>
		/// Returns true if the active pointer is on the overlay.
		/// </summary>
		public bool isBeingInteracted {
			get { return _isBeingInteracted; }
			set {
				//if (_isBeingInteracted == value) return;
				_isBeingInteracted = value;
				onInteractedWith?.Invoke(value);
			}
		}

		private bool _isBeingLookedAt = false;

		/// <summary>
		/// Returns true if the overlay is being looked at
		/// </summary>
		public bool isBeingLookedAt {
			get { return _isBeingLookedAt; }
			set {
				if (_isBeingLookedAt == value) return;
				_isBeingLookedAt = value;
				onLookedAt?.Invoke(value);
			}
		}

		[HideInInspector] public bool lookedAtCheck = false;


		private bool _isPrimary = false;

		/// <summary>
		/// Returns true if the overlay is the primary
		/// </summary>
		public bool isPrimary {
			get { return _isPrimary; }
			set {
				if (_isPrimary == value) return;
				_isPrimary = value;
				onBecomePrimary?.Invoke(value);
			}
		}


		private bool _lookHidingActive = false;

		/// <summary>
		/// True if the overlay is being hidden by look hiding
		/// </summary>
		public bool lookHidingActive {
			get { return _lookHidingActive; }
			protected set {
				if (_lookHidingActive == value) return;
				_lookHidingActive = value;
				onOverlayTempHiding?.Invoke(value);
			}
		}

		private bool _distanceHidingActive = false;

		/// <summary>
		/// True if the overlay is being hidden by distance hiding
		/// </summary>
		public bool distanceHidingActive {
			get { return _distanceHidingActive; }
			protected set {
				if (_distanceHidingActive == value) return;
				_distanceHidingActive = value;
				onOverlayTempHiding?.Invoke(value);
			}
		}


		#region Events

		/// <summary>
		/// Called when the overlay has been completely initialized and is ready for any fancy stuff you want to do to it.
		/// </summary>
		public Action onInitialized;

		/// <summary>
		/// Called when the overlay is being interacted with (by the pointer or touch input or something else)
		/// </summary>
		public Action<bool> onInteractedWith;

		/// <summary>
		/// Called when the overlay is presumably visible by the HMD
		/// </summary>
		public Action<bool> onLookedAt;

		/// <summary>
		/// Called when the overlay is the "primary" overlay
		/// </summary>
		public Action<bool> onBecomePrimary;

		/// <summary>
		/// Called when the overlay is facing the HMD
		/// </summary>
		public Action<bool> onLookedFrom;

		/// <summary>
		/// Called when the overlay is attempted to be scrolled on
		/// </summary>
		public Action<DPInteractor, float> onScrolled;

		/// <summary>
		/// Called when the overlay is attempted to be scrolled on sideways
		/// </summary>
		public Action<DPInteractor, float> onScrolledHorz;

		/// <summary>
		/// Called when the primary interact action is directed towards this overlay
		/// </summary>
		public Action<DPInteractor, float> onPrimaryInteract;

		/// <summary>
		/// Called when the secondary interact action is directed towards this overlay
		/// </summary>
		public Action<DPInteractor, bool> onSecondaryInteract;

		/// <summary>
		/// True if the overlay is temporarially hiding from look showing or distance hiding etc. False if it's appearing again.
		/// </summary>
		public Action<bool> onOverlayTempHiding;


		public Action<bool> onAlwaysInteractChanged;

		public Action<bool> onAlwaysInteractBlockInputChanged;

		/// <summary>
		/// Called when the overlay is dragged
		/// </summary>
		public Action<bool> onOverlayDragged;

		public Action<Keys, char, bool> onKeyboardInput;

		public Action<bool> onKeyboardToggled;

		#endregion


		private static int overlaysIndex = 0;

		public static string GetValidOverlayKey() {
			return "DP-" + overlaysIndex++;
		}


		private void Start() {
			//startOverlayKey = GetValidOverlayKey();
			needToProcess.Add(this);
			//startOverlayKey = GetValidOverlayKey();
		}


		/// <summary>
		/// Should be called only once to pre-initialize the overlay. This can setup components and subscribe to any events.
		/// </summary>
		public virtual void PreInitialize() {
			if (hasBeenPreInitialized) return;

			FindGoodGOName();

			overlay.shouldUpdateTrackedDevice += () => {


				if (useSmoothAnchoring) {
					SetOverlayTrackedDevice(smoothAnchoringTrackedDevice);
				}
				else if (useSnapAnchoring) {
					SetOverlayTrackedDevice(snapAnchoringTrackedDevice);
				}
				else {
					SetOverlayTrackedDevice(overlay.trackedDevice);
				}
				
			};

			onOverlayDragged += b => {
				isBeingDragged = b;
			};



			startOverlayKey = GetValidOverlayKey();

			overlay.matrixConverter = new SteamVR_Utils.RigidTransform(transform);


			overlay.SetSortOrder((uint) startSortOrder);

			overlay.SetWidthInMeters(startOverlayWidth, true);

			overlay.SetOpacity(startOverlayOpacity, true);

			//if (useLookHiding) overlay.SetOpacity(0f, false);

			overlay.SetCurvature(startCurvature, true);

			overlay.SetSBS(startUseSBS, startSBSCrossedMode);


			overlay.widthUpdatedEvent += OnWidthUpdated;

			onAlwaysInteractChanged += SyncChildrenUsePointInteraction;

			onScrolled += delegate(DPInteractor interactor, float f) {
				localScrollStrength.y = f;
				CheckIsBeingScrolled();
			};

			onScrolledHorz += delegate(DPInteractor interactor, float f) {
				localScrollStrength.x = f;
				CheckIsBeingScrolled();
			};

			//Debug.Log("preinitialized");

			hasBeenPreInitialized = true;

			//Setup children
			foreach (DPOverlayBase child in children) {
				AddChildOverlay(child);
			}

			if (SteamVRManager.isConnected) {
				InitOVROverlay();
			}
		}


		protected virtual void Update() {
			if (SteamVRManager.isConnected && !hasBeenInitialized && hasBeenPreInitialized) {
				InitOVROverlay();
			}


			HandleSmoothAnchoring();

			HandleSnapAnchoring();


			if (updatedAnchor) {
				overlay.SetTrackedDeviceRelativeIndex(overlay.trackedDevice, 0);
				SyncTransform(true);
				//updatedAnchor = false;
			}


			/*if (overlay.shouldRender && isAnimatingForceHighFPS) {

				overrideAutoRenderingFPS = true;
				fpsToCaptureAt = 60;

			}*/


			HandleWhenToRender();

			/*//Override rendering:
			if (overlay.shouldRender && overrideAutoRenderingFPS && !isStatic) {
				//Check if enough time has passed to render the next frame:
				if (captureTimer > fpsAsMS) {
					AddOverlayToRenderQueue(this);
					captureTimer = 0f;
				}

				captureTimer += Time.deltaTime;
			}*/

			if (overlay.shouldRender) {
				if (currentTexture) TryUpdateScale(currentTexture);
			}

			if (isTransitioningOverlay) {
				if (overlay.trackedDevice != DPOverlayTrackedDevice.None) SetOverlayPositionWithCurrent(isTransitioningSetTargetPos, true);
				else SetOverlayPositionWithCurrent(isTransitioningSetTargetPos, false);
			}

			if (useLookHiding) HandleLookHiding();
			if (useDistanceHiding) HandleDistanceHiding();


			//DEBUG stuff:

			if (enableDebug) {
				if (forceTransformSync) {
					SetOverlayTrackedDevice(startOverlayTrackedDevice);
					SyncTransform(false);
				}


				if (forceApplyInspectorProperties) {
					overlay.SetVisible(startVisible);
					overlay.SetWidthInMeters(startOverlayWidth);
					overlay.SetOpacity(startOverlayOpacity);
					overlay.SetCurvature(startCurvature);
					overlay.SetSortOrder((uint) startSortOrder);
					overlay.SetTextureBounds(new VRTextureBounds_t()
						{uMin = startTextureBounds.x, vMin = startTextureBounds.y, uMax = startTextureBounds.z, vMax = startTextureBounds.w});
				}
			}

			if (snapPointQueuedToResize) {
				HandleSnapPointQueuedToResize();
			}
		}


		public virtual void OnPreDestroy() {
			for (int i = children.Count - 1; i >= 0 ; i--) {
				var child = children[i];
				
				child.OrphanOverlay();
			}
			
			OrphanOverlay();
		}
		
		protected virtual void OnDestroy() {

			overlay.widthUpdatedEvent -= OnWidthUpdated;
			onAlwaysInteractChanged -= SyncChildrenUsePointInteraction;

			overlay.DestroyOverlay();
		}
		
		

		private void HandleSnapPointQueuedToResize() {
			if (!isUsingSnapPoint) return;

			if (reverseAspect == 0f) return;
			

			float newWidth = 0f;

			float increase = 0.01f;

			for (int i = 0; i < 200; i++) {
				newWidth += increase;

				if (newWidth >= snapPoint.maxOverlayWidth) break;
				if (newWidth * reverseAspect >= snapPoint.maxOverlayHeight) break;
				
				if (maxOverlayWidth > 0f) {
					if (newWidth >= maxOverlayWidth) break;
				}
			}
			
			overlay.SetWidthInMeters(newWidth);

			snapPointQueuedToResize = false;
		}

		private void HandleSmoothAnchoring() {
			if (!useSmoothAnchoring) return;
			
			if (isBeingDragged) {

				smoothAnchoringDummyObject.transform.position = transform.position;
				smoothAnchoringDummyObject.transform.rotation = transform.rotation;

			}
			else {
				float strength = Mathf.Clamp(smoothAnchoringStrength * Time.deltaTime, 0.001f, 1f);

				transform.position = Vector3.Lerp(transform.position, smoothAnchoringDummyObject.transform.position, strength);
				transform.rotation = Quaternion.Lerp(transform.rotation, smoothAnchoringDummyObject.transform.rotation, strength);
				
				SyncTransform();
			}

		}

		private void HandleSnapAnchoring() {
			if (!useSnapAnchoring) return;

			if (isBeingDragged) {

				snapAnchoringDummyObject.transform.position = transform.position;
				snapAnchoringDummyObject.transform.rotation = transform.rotation;

			}
			

			if (isSnapAnchoringToNewPos) {
				transform.position = Vector3.Lerp(transform.position, snapAnchoringDummyObject.transform.position, 10f * Time.deltaTime);
				transform.rotation = Quaternion.Lerp(transform.rotation, snapAnchoringDummyObject.transform.rotation, 10f * Time.deltaTime);

				if (snapAnchoringNormalizeRot) transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);

				float distTestDisable = Vector3.Distance(transform.position, snapAnchoringDummyObject.transform.position);

				if (distTestDisable <= 0.05f) isSnapAnchoringToNewPos = false;

				SyncTransform();
			}


			//TODO: Check when overlay ends drag so we can update the dummy object pos

			//Debug.Log(snapAnchoringDummyObject.transform.position);

			float distance = Vector3.Distance(transform.position, snapAnchoringDummyObject.transform.position);

			//Debug.Log(distance);

			if (Mathf.Abs(distance) < snapAnchoringDistance) return;

			isSnapAnchoringToNewPos = true;


			//Vector3 rot = snapAnchoringDummyObject.transform.eulerAngles;

			//if (snapAnchoringNormalizeRot) rot.z = 0f;
			//if (snapAnchoringNormalizeRot) rot.x = 0f;

			//Debug.Log("setting pos");

			//Snap to the position
			//SetOverlayTransform(snapAnchoringDummyObject.transform.position, rot, true);
		}


		private void HandleWhenToRender() {
			if (isStatic) return;

			if (!overlay.shouldRender) return;


			//Check state based on the rendering params
			bool idle = true;

			/*if (renderWhenInteractedWith && renderWhenLookedAt) {
				if (isBeingInteracted && isBeingLookedAt) {
					TimerCheckAddToRenderQueue(false);
					idle = false;
				}
			}*/

			if (renderWhenInteractedWith) {
				if (isBeingInteracted) {
					timeSinceLastInteract = 0f;
					//TimerCheckAddToRenderQueue(false);
					idle = false;
				}
			}

			else if (renderWhenLookedAt) {
				if (isBeingLookedAt) {
					//TimerCheckAddToRenderQueue(false);
					idle = false;
				}
			}


			if (idle && !isBeingInteracted && renderWhenInteractedWith && timeSinceLastInteract < renderAfterGoIdleTime) {

				idle = false;
				
				timeSinceLastInteract += Time.deltaTime;
			}
			
			
			TimerCheckAddToRenderQueue(idle);

			//if (idle) TimerCheckAddToRenderQueue(true);
		}

		private void TimerCheckAddToRenderQueue(bool isIdle) {
			if (isIdle && !forceHighCaptureFramerate) {
				if (captureTimer > idleFPSAsMS) {
					//Debug.Log("rendering idle: " + gameObject.name);
					AddOverlayToRenderQueue(this);
					captureTimer = 0f;
				}
			}
			else {
				if (captureTimer > fpsAsMS) {
					AddOverlayToRenderQueue(this);
					captureTimer = 0f;
				}
			}

			captureTimer += Time.deltaTime;
		}

		private void CheckIsBeingScrolled() {
			if (localScrollStrength.x != 0f || localScrollStrength.y != 0f) {
				isBeingScrolled = true;
			}
			else {
				isBeingScrolled = false;
			}
		}

		private void FindGoodGOName() {
			//Figure out what to name this:
			if (hasDPAppParent) {
				if (dpAppParent.dpMain == this) gameObject.name = "Overlay - " + dpAppParent.name + " - Main";
				else if (dpAppParent.dpTopBar == this) gameObject.name = "Overlay - " + dpAppParent.name + " - TopBar";

				//Else it's in others
				else {
					gameObject.name = "Overlay - " + dpAppParent.name + " - " + transform.parent.parent.name;
				}
			}
			//Else it's standalone
			else {
				gameObject.name = "Overlay - " + transform.parent.name;
			}
		}


		/// <summary>
		/// Updates the width/position of children when this overlay changes width
		/// </summary>
		private void OnWidthUpdated(float previousWidth) {
			transform.localScale = new Vector3(overlay.width, overlay.width, overlay.width);

			SyncChildrenTransform(false);

			//Update the width of the childrenU
			float ratioChange = overlay.width / previousWidth;

			foreach (DPOverlayBase child in children) {
				if (child == this) continue;
				if (!child.followParentScale) continue;

				float newChildWidth = child.overlay.width * ratioChange;
				child.overlay.SetWidthInMeters(newChildWidth, false);
			}
		}


		/// <summary>
		/// Checks if the scale is the same as the previous texture size, and updates the mouse scale and collider size if not.
		/// </summary>
		/// <param name="tex">The texture to test against</param>
		public void TryUpdateScale(Texture tex) {
			if (!hasBeenInitialized) return;

			if (textureBounds == Vector4.zero) return;

			if (_textureSize.x == tex.width && _textureSize.y == tex.height && textureBounds == _prevTextureBounds) return;
			_textureSize.x = tex.width;
			_textureSize.y = tex.height;

			_prevTextureBounds = textureBounds;

			float heightMod = Mathf.Abs(textureBounds.w - textureBounds.y);
			float widthMod = Mathf.Abs(textureBounds.z - textureBounds.x);

			reverseAspect = ((float) tex.height * heightMod) / ((float) tex.width * widthMod);

			mouseScale.y = (float) tex.height / (float) tex.width;

			mouseScale_t.v0 = mouseScale.x;
			mouseScale_t.v1 = mouseScale.y;

			overlay.SetMouseScale(mouseScale_t);

			//If this is the main overlay update the top bar
			//if (hasDPAppParent && dpAppParent.dpMain == this) dpAppParent.UpdateTopBarStats(overlay.width);
		}

		/// <summary>
		/// If this overlay uses look hiding, this toggles it
		/// </summary>
		/// <param name="enable">Should it show or hide</param>
		private void HandleLookHiding() {

			if (!overlay.isVisible) return;
			
			bool hmdToOverlay = false, overlayToHMD = false;


			if (DotProdUtil(transform, SteamVRManager.I.hmdTrans.position, lookHidingStrength, true)) {
				overlayToHMD = true;
			}
			else {
				overlayToHMD = false;
			}


			if (DotProdUtil(SteamVRManager.I.hmdTrans, transform.position, lookHidingStrength)) {
				hmdToOverlay = true;
			}
			else {
				hmdToOverlay = false;
			}


			if (overlayToHMD && hmdToOverlay && lookHidingActive) {
				TransitionOverlayOpacity(overlay.targetOpacity, 0.3f, Ease.InOutCubic, false);

				//foreach (DPOverlayBase child in children) {
				//	if (!child.followParentOpacity) continue;
				//	child.TransitionOverlayOpacity(child.overlay.targetOpacity, 0.3f, Ease.InOutCubic, false);
				//}


				lookHidingActive = false;
			}


			if ((!overlayToHMD || !hmdToOverlay) && !lookHidingActive) {
				TransitionOverlayOpacity(lookHidingOpacity, 0.3f, Ease.InOutCubic, false);

				//foreach (DPOverlayBase child in children) {
				//	if (!child.followParentOpacity) continue;
				//	child.TransitionOverlayOpacity(lookHidingOpacity, 0.3f, Ease.InOutCubic, false);
				//}

				lookHidingActive = true;
			}
		}

		private void HandleDistanceHiding() {
			
			if (!overlay.isVisible) return;
			
			//If the distance is greater than the threshold

			//Fade out
			if (Mathf.Abs(Vector3.Distance(transform.position, SteamVRManager.I.hmdTrans.position)) > distanceHidingDistance) {
				//If it's already faded out
				if (distanceHidingActive) return;

				TransitionOverlayOpacity(distanceHidingOpacity, 0.3f, Ease.InOutCubic, false);

				distanceHidingActive = true;
			}

			//Fade in
			else {
				if (!distanceHidingActive) return;

				TransitionOverlayOpacity(overlay.targetOpacity, 0.3f, Ease.InOutCubic, false);

				distanceHidingActive = false;
			}
		}

		/// <summary>
		/// Should be called only once to init the overlay.
		/// Should call <code>overlay.CreateOverlay()</code> and set the texture bounds.
		/// </summary>
		protected abstract void InitOVROverlay();

		/// <summary>
		/// Called whenever the overlay should attempt to render out a new thing of whatever it displays.
		/// If <see cref="overrideAutoRenderingFPS"/> is set to true, the function should check if <see cref="captureTimer"/> has gone past <see cref="fpsAsMS"/>
		/// If so, reset it, and do whatever normal rendering you do.
		/// <param name="force">If true, the overlay will render no matter what</param>
		/// </summary>
		public abstract void RequestRendering(bool force = false);


		public abstract void ResizeForRatio(int ratioX, int ratioY);


		/// <summary>
		/// Called when focus is ended with an overlay and the state should be cleared.
		/// </summary>
		//public abstract void ClearUIState();


		/// <summary>
		/// Handles mouse movement however is preferred for the overlay.
		/// This movement is completely raw, and can be smoothed out however is preferred for the type of overlay.
		/// </summary>
		/// <param name="interactor">The source of the interactions</param>
		/// <param name="interactionPoints">The local positions of desired interaction points on the overlay</param>
		/// <returns>Returns the position/s to display the cursors at</returns>
		public abstract List<Vector3> HandleColliderInteracted(DPInteractor interactor, List<Vector2> interactionPoints);

		public void ClearAllSnapData() {
			if (hasDPAppParent && dpAppParent.isUsingSnapPoint) {
				dpAppParent.snapPoint.ClearAllSnapData();
			}
			else if (isUsingSnapPoint) {
				snapPoint.ClearAllSnapData();
			}
		}


		public void SetOverlayOpacity(float opacity, bool setTarget = true) {
			overlay.SetOpacity(opacity, setTarget);

			foreach (DPOverlayBase child in children) {
				if (child == this) continue;
				if (!child.followParentOpacity) continue;
				child.SetOverlayOpacity(opacity, setTarget);
			}
		}


		public void SetOverlayTexture(Texture tex) {
			currentTexture = tex;
			overlay.SetTexture(tex);
		}

		public void SetOverlayTextureBounds(Vector4 bounds) {
			VRTextureBounds_t newBounds = new VRTextureBounds_t()
				{uMin = bounds.x, vMin = bounds.y, uMax = bounds.z, vMax = bounds.w};

			textureBounds = bounds;

			overlay.SetTextureBounds(newBounds);
		}

		/// <summary>
		/// Set the position of the SteamVR overlay, and optionally the unity position of the overlay.
		/// </summary>
		/// <param name="pos">New position</param>
		/// <param name="rot">New rotation for euler angles</param>
		/// <param name="setUnityPos">Should it set the unity position too?</param>
		/// <param name="setTarget">Should it set the target position/rotation for the overlay?</param>
		public void SetOverlayTransform(Vector3 pos, Vector3 rot, bool setUnityPos = true, bool setTarget = true, bool setLocal = true) {
			var transform1 = transform;

			//Set the position of the overlay using the method specified
			if (setUnityPos) {
				if (setLocal) {
					transform1.localPosition = pos;
					transform1.localEulerAngles = rot;
				}
				else {
					transform1.position = pos;
					transform1.eulerAngles = rot;
				}
			}

			if (!SteamVRManager.isConnected) return;

			//We need a relative point in the world to use InverseTransformPoint on
			Transform relativeTransform;

			//Get the correct origin for the overlay's tracked device
			switch (overlay.trackedDevice) {
				case DPOverlayTrackedDevice.LeftHand:
					relativeTransform = SteamVRManager.I.lHandTrans;
					break;

				case DPOverlayTrackedDevice.RightHand:
					relativeTransform = SteamVRManager.I.rHandTrans;
					break;

				case DPOverlayTrackedDevice.HMD:
					relativeTransform = SteamVRManager.I.hmdTrans;
					break;

				default:
					relativeTransform = SteamVRManager.I.noAnchorTrans;
					break;
			}

			//Get the pos/rot of the overlay relative to it's tracked device
			Vector3 newPos = relativeTransform.InverseTransformPoint(transform1.position);
			Quaternion newRot = Quaternion.Inverse(relativeTransform.rotation) * transform1.rotation;

			//Set the values:
			overlay.SetTransform(newPos, newRot.eulerAngles, setTarget);

			//Update children:
			SyncChildrenTransform(setTarget);
		}


		/// <summary>
		/// Sets the overlay position/rotation to the current unity pos/rot
		/// </summary>
		/// <param name="setTargetPos">Should it set the new pos/rot as the target pos/rot?</param>
		/// <param name="useLocal">Should it use the local state or the world state?</param>
		[Obsolete("Use SyncTransform instead")]
		public void SetOverlayPositionWithCurrent(bool setTargetPos = true, bool useLocal = true) {
			if (useLocal) SetOverlayTransform(transform.localPosition, transform.localEulerAngles, false, setTargetPos, true);
			else SetOverlayTransform(transform.position, transform.eulerAngles, false, setTargetPos, false);
		}

		/// <summary>
		/// Automatically syncs the position of the overlay with the unity transform, and handles if it's anchored or not
		/// </summary>
		public void SyncTransform(bool setTarget = false) {
			SetOverlayTransform(transform.position, transform.eulerAngles, false, setTarget, false);
		}

		/// <summary>
		/// Kills any active transitions on the overlay
		/// </summary>
		public void KillTransitions() {
			if (_transitionOpacityC != null) StopCoroutine(_transitionOpacityC);
			_opacityTweener?.Kill();

			if (_transitionWidthC != null) StopCoroutine(_transitionWidthC);
			_widthTweener?.Kill();

			if (_transitionPositionC != null) StopCoroutine(_transitionPositionC);
			_positionTweener?.Kill();
			_rotationTweener?.Kill();

			if (_transitionCurvatureC != null) StopCoroutine(_transitionCurvatureC);
			_curvatureTweener?.Kill();

			isTransitioningOverlay = false;
		}


		private Coroutine _transitionOpacityC;
		private Tweener _opacityTweener;

		/// <summary>
		/// Transitions the overlay to a new opacity and hides/shows it
		/// </summary>
		/// <param name="endOpacity">The final opacity for the overlay</param>
		/// <param name="time">The time in seconds</param>
		/// <param name="endVisibleState">Should the overlay be visible at the end?</param>
		/// <param name="setTarget">Should this set the target opacity for the overlay?</param>
		public void TransitionOverlayOpacity(float endOpacity, float time, Ease easeType = Ease.InOutCubic, bool setTarget = true) {
			if (_transitionOpacityC != null) StopCoroutine(_transitionOpacityC);

			_opacityTweener?.Kill();

			bool endVisibleState = endOpacity > 0f;
			if (useLookHiding) endVisibleState = true;
			else if (isChild && parent.useLookHiding) endVisibleState = true;

			_transitionOpacityC = StartCoroutine(_transitionOverlayOpacity(endOpacity, time, endVisibleState, easeType, setTarget));
		}

		private IEnumerator _transitionOverlayOpacity(float endOpacity, float time, bool endVisibleState, Ease easeType = Ease.InOutCubic, bool setTarget = true) {
			//If we're fading in the overlay from being invisible, it needs to be visible during the transition.
			if (!overlay.isVisible) overlay.SetVisible(true, false);

			_opacityTweener = DOTween.To((opacity => { SetOverlayOpacity(opacity, setTarget); }), overlay.opacity, endOpacity, time).SetEase(easeType);

			yield return new WaitForSeconds(time);

			if (endVisibleState == false && !useLookHiding && !useDistanceHiding) {
				overlay.SetVisible(false, true);
			}

			_transitionOpacityC = null;
			_opacityTweener = null;
		}


		private Coroutine _transitionPositionC;
		private Tweener _positionTweener;
		private Tweener _rotationTweener;

		/// <summary>
		/// Transitions the overlay to a new width
		/// </summary>
		/// <param name="newPos">The new position</param>
		/// <param name="newRot">The new rotation</param>
		/// <param name="time">Time for the transition in seconds</param>
		/// <param name="easeType">Easing curve for moving the overlay</param>
		/// <param name="setTarget">Should it set the target position/rotation for the overlay?</param>
		public void TransitionOverlayPosition(Vector3 newPos, Vector3 newRot, float time, Ease easeType = Ease.InOutCubic, bool setTarget = true) {
			if (_transitionPositionC != null) StopCoroutine(_transitionPositionC);

			_positionTweener?.Kill();
			_rotationTweener?.Kill();

			isTransitioningOverlay = false;

			_transitionPositionC = StartCoroutine(_transitionOverlayPosition(newPos, newRot, time, easeType, setTarget));
		}

		private IEnumerator _transitionOverlayPosition(Vector3 newPos, Vector3 newRot, float time, Ease easeType = Ease.InOutCubic, bool setTarget = true) {
			isTransitioningOverlay = true;
			isTransitioningSetTargetPos = setTarget;

			_positionTweener = transform.DOLocalMove(newPos, time).SetEase(easeType);
			_rotationTweener = transform.DOLocalRotate(newRot, time).SetEase(easeType);
			yield return new WaitForSeconds(time);
			isTransitioningOverlay = false;
		}


		private Coroutine _transitionWidthC;
		private Tweener _widthTweener;

		/// <summary>
		/// Transitions the overlay to a new width
		/// </summary>
		/// <param name="endWidth">The final width</param>
		/// <param name="time">Time in seconds</param>
		/// <param name="setTarget">Should this be the new target width for the overlay?</param>
		public void TransitionOverlayWidth(float endWidth, float time, bool setTarget = true) {
			if (_transitionWidthC != null) StopCoroutine(_transitionWidthC);

			_widthTweener?.Kill();

			_transitionWidthC = StartCoroutine(_transitionOverlayWidth(endWidth, time, setTarget));
		}

		private IEnumerator _transitionOverlayWidth(float endWidth, float time, bool setTarget = true) {
			if (setTarget) {
				_widthTweener = DOTween.To(overlay._setWidthInMetersYesTarget, overlay.width, endWidth, time).SetEase(Ease.InOutCubic);
			}
			else {
				_widthTweener = DOTween.To(overlay._setWidthInMetersNoTarget, overlay.width, endWidth, time).SetEase(Ease.InOutCubic);
			}

			yield return new WaitForSeconds(time);
		}

		private Coroutine _transitionCurvatureC;
		private Tweener _curvatureTweener;

		/// <summary>
		/// Transitions the overlay to a new width
		/// </summary>
		/// <param name="endCurvature">The final curvature</param>
		/// <param name="time">Time in seconds</param>
		/// <param name="setTarget">Should this be the new target curvature for the overlay?</param>
		public void TransitionOverlayCurvature(float endCurvature, float time, bool setTarget = true) {
			if (_transitionCurvatureC != null) StopCoroutine(_transitionCurvatureC);

			_curvatureTweener?.Kill();

			_transitionCurvatureC = StartCoroutine(_transitionOverlayCurvature(endCurvature, time, setTarget));
		}

		private IEnumerator _transitionOverlayCurvature(float endCurvature, float time, bool setTarget = true) {
			if (setTarget) {
				_curvatureTweener = DOTween.To(overlay._setCurvatureYesTarget, overlay.curvature, endCurvature, time).SetEase(Ease.InOutCubic);
			}
			else {
				_curvatureTweener = DOTween.To(overlay._setCurvatureNoTarget, overlay.curvature, endCurvature, time).SetEase(Ease.InOutCubic);
			}

			yield return new WaitForSeconds(time);
		}

		private Coroutine disableAnchorC;

		public void SetOverlayTrackedDevice(DPOverlayTrackedDevice device, uint customDeviceIndex = 0, bool setTarget = true) {

			//if (device != DPOverlayTrackedDevice.None) isAnchoredToTheBar = false;
			
			if (useSmoothAnchoring) {

				smoothAnchoringTrackedDevice = device;
				
				//If we're using smooth anchoring, override the device it would set it to
				switch (device) {
					case DPOverlayTrackedDevice.None:
						smoothAnchoringTarget = null;
						break;

					case DPOverlayTrackedDevice.LeftHand:
						smoothAnchoringTarget = SteamVRManager.I.lHandTrans;
						break;

					case DPOverlayTrackedDevice.RightHand:
						smoothAnchoringTarget = SteamVRManager.I.rHandTrans;
						break;

					case DPOverlayTrackedDevice.HMD:
						smoothAnchoringTarget = SteamVRManager.I.hmdTrans;
						break;
				}

				if (smoothAnchoringDummyObject == null) {
					smoothAnchoringDummyObject = new GameObject() {name = "SmoothAnchoringDummy"};
				}

				smoothAnchoringDummyObject.transform.position = transform.position;
				smoothAnchoringDummyObject.transform.rotation = transform.rotation;

				smoothAnchoringDummyObject.transform.SetParent(smoothAnchoringTarget, true);
			}
			else {
				smoothAnchoringTarget = null;
				smoothAnchoringTrackedDevice = DPOverlayTrackedDevice.None;
				if (smoothAnchoringDummyObject) Destroy(smoothAnchoringDummyObject);
			}


			if (useSnapAnchoring) {

				snapAnchoringTrackedDevice = device;
				
				//If we're using snap anchoring, override the device it would set it to
				switch (device) {
					case DPOverlayTrackedDevice.None:
						snapAnchoringTarget = null;
						break;

					case DPOverlayTrackedDevice.LeftHand:
						snapAnchoringTarget = SteamVRManager.I.lHandTrans;
						break;

					case DPOverlayTrackedDevice.RightHand:
						snapAnchoringTarget = SteamVRManager.I.rHandTrans;
						break;

					case DPOverlayTrackedDevice.HMD:
						snapAnchoringTarget = SteamVRManager.I.hmdTrans;
						break;
				}

				if (snapAnchoringDummyObject == null) {
					snapAnchoringDummyObject = new GameObject() {name = "SnapAnchoringDummy"};
				}

				snapAnchoringDummyObject.transform.position = transform.position;
				snapAnchoringDummyObject.transform.rotation = transform.rotation;

				snapAnchoringDummyObject.transform.SetParent(snapAnchoringTarget, true);
			}
			else {
				snapAnchoringTarget = null;
				snapAnchoringTrackedDevice = DPOverlayTrackedDevice.None;
				if (snapAnchoringDummyObject) Destroy(snapAnchoringDummyObject);
			}


			if (useSmoothAnchoring || useSnapAnchoring) {
				device = DPOverlayTrackedDevice.None;
			}


			if (isChild) {
				transform.SetParent(parent.transform);
			}
			else {
				switch (device) {
					case DPOverlayTrackedDevice.None:
						transform.SetParent(SteamVRManager.I.noAnchorTrans, true);
						//transform.SetParent(null, true);
						break;

					case DPOverlayTrackedDevice.LeftHand:
						transform.SetParent(SteamVRManager.I.lHandTrans, true);
						break;

					case DPOverlayTrackedDevice.RightHand:
						transform.SetParent(SteamVRManager.I.rHandTrans, true);
						break;

					case DPOverlayTrackedDevice.HMD:
						transform.SetParent(SteamVRManager.I.hmdTrans, true);
						break;
				}
			}


			overlay.SetTrackedDeviceRelativeIndex(device, customDeviceIndex, setTarget);


			//Update any children:
			foreach (DPOverlayBase child in children) {
				if (child == this) continue;
				child.SetOverlayTrackedDevice(device, customDeviceIndex, setTarget);
			}

			//SetOverlayPositionWithCurrent(true, true);
			SyncTransform(setTarget);

			updatedAnchor = true;


			if (disableAnchorC != null) StopCoroutine(disableAnchorC);

			disableAnchorC = StartCoroutine(DisableUpdateAnchorDelayed());
		}


		private IEnumerator DisableUpdateAnchorDelayed() {
			yield return new WaitForSeconds(3f);
			updatedAnchor = false;
		}


		/// <summary>
		/// Adds a child overlay to this overlay.
		/// Children will follow an overlay the same way as in Unity
		/// </summary>
		/// <param name="child">The child to add</param>
		public void AddChildOverlay(DPOverlayBase child, bool worldPositionStays = false) {
			if (!children.Contains(child)) {
				children.Add(child);
			}

			child.isChild = true;
			child.parent = this;

			//Parent it:
			child.transform.SetParent(transform, worldPositionStays);

			//Set the tracked device:
			if (child.overlay.trackedDevice != overlay.trackedDevice) child.SetOverlayTrackedDevice(overlay.trackedDevice);

			child.SyncTransform(true);
			
			//Sync the properties
			if (child.followParentPointInteraction) child.alwaysInteract = alwaysInteract;
			if (child.followParentOpacity) child.SetOverlayOpacity(overlay.opacity);
		}

		/// <summary>
		/// Removes the specified child from the list of children
		/// </summary>
		/// <param name="child">The child to remove</param>
		public void RemoveChildOverlay(DPOverlayBase child) {
			if (!children.Contains(child)) return;

			children.Remove(child);

			child.isChild = false;
			child.parent = null;

			//Reparent to whatever device it's anchored to at the root
			child.SetOverlayTrackedDevice(overlay.trackedDevice, 0, true);
			//child.transform.SetParent(transform.parent);
		}

		private void SyncChildrenTransform(bool setTarget = false) {

			bool nullChildren = false;
			
			foreach (DPOverlayBase child in children) {
				if (child == this) continue;
				if (child == null) {
					nullChildren = true;
					continue;
				}

				child.SyncTransform(setTarget);
			}
			
			//Really dumb fix to get rid of any null children
			if (nullChildren) children = children.Where(x => x != null).ToList();
			
		}

		private void SyncChildrenUsePointInteraction(bool enable) {
			foreach (DPOverlayBase child in children) {
				if (!child.followParentPointInteraction) continue;
				child.alwaysInteract = enable;
			}
		}

		/*private void SyncChildrenLookShowing(bool enable) {
			foreach (DPOverlayBase child in children) {
				if (!child.followParentLookShowing) continue;
				
			}
		}*/


		/// <summary>
		/// If the overlay is parented, this overlay is un-parented
		/// </summary>
		public void OrphanOverlay() {
			if (!isChild) return;
			parent.RemoveChildOverlay(this);

			SetOverlayTrackedDevice(DPOverlayTrackedDevice.None);
		}


		private static List<DPOverlayBase> _overlaysToRender = new List<DPOverlayBase>();

		private static bool renderingFallingBehind = false;

		public static IEnumerator HandleRendering() {
			while (true) {
				if (renderingFallingBehind) {
					for (int i = 0; i < _overlaysToRender.Count; i++) {
						_overlaysToRender[i].RequestRendering();
					}

					_overlaysToRender.Clear();
					renderingFallingBehind = false;
				}

				else if (_overlaysToRender.Count > 0) {
					_overlaysToRender[0].RequestRendering();
					_overlaysToRender.RemoveAt(0);
				}


				yield return null;
			}
		}

		private static void AddOverlayToRenderQueue(DPOverlayBase dpBase) {
			if (_overlaysToRender.Contains(dpBase)) {
				//renderingFallingBehind = true;
				return;
			}

			_overlaysToRender.Add(dpBase);
		}

		private bool DotProdUtil(Transform from, Vector3 to, float threshold, bool flipForward = false) {
			Vector3 dirFromAtoB = (to - from.position).normalized;
			float dotProd;

			if (flipForward) dotProd = Vector3.Dot(dirFromAtoB, from.forward * -1f);
			else dotProd = Vector3.Dot(dirFromAtoB, from.forward);

			return dotProd > threshold;
		}
	}
}