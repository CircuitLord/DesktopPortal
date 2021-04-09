using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using DPCore.Interaction;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace DPCore {
	
	
	
	
	/*class UIElementPointerList<T> : List<T>
		where T : MonoBehaviour, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
	{
	}*/
	
	
	
	public class DPCameraOverlay : DPOverlayBase {
		[Header("Other Components")] [SerializeField]
		private Camera _camera;

		[SerializeField] private Canvas canvas;


		//[SerializeField] public bool useLowResRT = true;

		[SerializeField] private bool isRTPersistent = false;


		[SerializeField] private bool autoDisableCanvas = true;
		
		

		//PUBLIC stuff:
		[HideInInspector] public bool overlayLoaded = false;
		//[HideInInspector] public bool isFocused = false;


		//PRIVATE stuff:
		private RenderTexture _cameraTexture;
		private RectTransform _rootCanvas;
		private CanvasScaler _canvasScaler;

		private RenderTexture _mainRT;
		private RenderTexture _lowRT;


		private int RTX, RTY;
		private bool rtsGenerated = false;

		private List<IPointerExitHandler> allUIElements = new List<IPointerExitHandler>();

		//private UnityUIDPHandler _uiHandler = new UnityUIDPHandler();
		//private HashSet<Selectable> enterTargets = new HashSet<Selectable>();
		//private HashSet<Selectable> downTargets = new HashSet<Selectable>();

		//private bool textureUpdated = false;


		//Interaction variables
		
		

		//private bool overlayMouseLeftDown = false;
		//private bool overlayMouseRightDown = false;


		//private List<Vector2> previousInteractionPoints = new List<Vector2>();
		
		
		private Dictionary<DPInteractor, UnityUIDPHandlerData> interactionsData = new Dictionary<DPInteractor, UnityUIDPHandlerData>();


		public override void PreInitialize() {
			
			FetchComponents();
			
			
			RTX = _camera.pixelWidth;
			RTY = _camera.pixelHeight;

			
			base.PreInitialize();
			



			//isRTPersistent = true;
			if (isRTPersistent) GenerateRenderTextures();
			
			
			currentTexture = _cameraTexture;
			
			if (isStatic) RequestRendering(true);
			


			if (!startVisible) StartCoroutine(DisableCanvasDelayed());
			
			
			
			overlay.visibilityUpdatedEvent += OnVisibilityUpdated;

			onLookedAt += ToggleTextureRes;

			onInteractedWith += b => {
				if (!b) {

					foreach (var kvp in interactionsData) {
						kvp.Value.uiHandler.UpTargets(kvp.Value.downTargets);
						kvp.Value.uiHandler.ExitTargets(kvp.Value.enterTargets);
						kvp.Value.uiHandler.EndDragTargets(kvp.Value.downTargets);
						
						kvp.Value.downTargets.Clear();
						kvp.Value.enterTargets.Clear();
						kvp.Value.mouseDown = false;
						kvp.Value.hasClicked = false;
					}
					
				}
			};
			
			
			//Interaction events:
			
			onPrimaryInteract += delegate(DPInteractor interactor, float f) {
				if (!interactionsData.ContainsKey(interactor)) return;
				
				if (f > 0.25f) interactionsData[interactor].overlayMouseLeftDown = true;
				else interactionsData[interactor].overlayMouseLeftDown = false;
			};

			onScrolled += delegate(DPInteractor interactor, float f) {
				if (!interactionsData.ContainsKey(interactor)) return;
				
				interactionsData[interactor].scrollStrength.y = f;
				
			};

			onScrolledHorz += delegate(DPInteractor interactor, float f) {
				if (!interactionsData.ContainsKey(interactor)) return;

				interactionsData[interactor].scrollStrength.x = f;

			};
			
		}

		protected override void OnDestroy() {
			base.OnDestroy();

			overlay.visibilityUpdatedEvent -= OnVisibilityUpdated;

			onLookedAt -= ToggleTextureRes;

			//primaryInteractEvent -= HandlePrimaryInteract;
		}

		protected override void Update() {
			//This is active from the start so we have to wait for OpenVR to be connected.
			if (!SteamVRManager.isConnected) return;

			base.Update();


			//if (overlay.shouldRender && isBeingInteracted) {
			//	UpdateMouse();
			//	UpdateUnityMouseSim();
			//}
		}
		
		
		private IEnumerator DisableCanvasDelayed() {
			yield return null;
			//yield return null;
			OnVisibilityUpdated(false);
		}
		
		private void OnVisibilityUpdated(bool active) {

			if (active && !isRTPersistent) {
				GenerateRenderTextures();
			}
			else if (!active && !isRTPersistent) {
				ClearRenderTextures();
			}
			
			if (_rootCanvas && autoDisableCanvas) {
				_rootCanvas.gameObject.SetActive(active);
			}
			
			if (active) RequestRendering(true);
		}


		private void FetchComponents() {
			if (!canvas) canvas = _camera.transform.parent.GetComponentInChildren<Canvas>();
			//if (!_camera) _camera = transform.parent.Find("Camera")?.GetComponent<Camera>();

			if (canvas == null) {
				Debug.Log("Couldn't find canvas on " + gameObject.name);
				return;
			}
			
			_rootCanvas = canvas.GetComponent<RectTransform>();


			var children = canvas.GetComponentsInChildren<MonoBehaviour>();

			var pointerExitHandlers = children.OfType<IPointerExitHandler>();

			foreach (var exit in pointerExitHandlers) {
				allUIElements.Add(exit);
				
			}

			/*_canvasScaler = _rootCanvas.GetComponent<CanvasScaler>();

			if (_canvasScaler == null) {
				_canvasScaler = _rootCanvas.gameObject.AddComponent<CanvasScaler>();
			}*/
		}

		private void GenerateRenderTextures() {

			ClearRenderTextures();

			//Debug.Log(RTX + " " + RTY + " : " + gameObject.name);
			
			_mainRT = new RenderTexture(RTX, RTY, 24) {
				name = "DPCameraTexture-Main",
				autoGenerateMips = false,
				useMipMap = true
			};

			/*useLowResRT = false;
			if (useLowResRT) {
				_lowRT = new RenderTexture(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 24) {
					name = "DPCameraTexture-Low",
					autoGenerateMips = false,
					useMipMap = true
				};
			}*/
			

			_cameraTexture = _mainRT;

			//Destroy(_camera.targetTexture);

			_camera.targetTexture = _cameraTexture;
			
			_camera.enabled = false;

			rtsGenerated = true;
		}

		private void ClearRenderTextures() {

			if (_mainRT != null) {
				_mainRT.Release();
				_mainRT = null;
			}

			if (_lowRT != null) {
				_lowRT.Release();
				_lowRT = null;
			}

			if (_camera.targetTexture != null) {
				_camera.targetTexture.Release();
				_camera.targetTexture = null;
			}

			_cameraTexture = null;
			currentTexture = null;

			rtsGenerated = false;

		}

		private void ToggleTextureRes(bool lookedAt) {
			
			/*return;
			
			if (lookedAt || !useLowResRT) {
				_canvasScaler.scaleFactor = 1.0f;
				_cameraTexture = _mainRT;
			}
			else {
				_canvasScaler.scaleFactor = 0.5f;
				_cameraTexture = _lowRT;
			}

			_camera.targetTexture = _cameraTexture;
			
			RequestRendering(true);*/
		}

		public override void RequestRendering(bool force = false) {

			if (isStatic && !force) return;

			if (!hasBeenPreInitialized || !hasBeenInitialized) return;

			if (_camera == null) return;
			
			if (!rtsGenerated) {
				GenerateRenderTextures();
				return;
			}

			if (force && !_rootCanvas.gameObject.activeSelf) {
				_rootCanvas.gameObject.SetActive(true);

				_camera.Render();

				_rootCanvas.gameObject.SetActive(false);
			}

			else {
				_camera.Render();
			}

			SetOverlayTexture(_cameraTexture);
		}

		public override void ResizeForRatio(int ratioX, int ratioY) {
			
		}


		public override List<Vector3> HandleColliderInteracted(DPInteractor interactor, List<Vector2> interactionPoints) {

			if (interactionPoints.Count <= 0) return null;
			Vector2 point = interactionPoints.FirstOrDefault();

			//offset it so all positive values
			//Vector2 posPoint = new Vector2(point.x + overlay.width / 2f, point.y + overlayHeight / 2f);
			//negPoint.x -= negPoint.x * 0.05f;

			//Convert to be 0-1
			//Vector2 fixedPoint = new Vector2((posPoint.x / overlay.width), posPoint.y / overlayHeight);


			//Flip the y axis
			point.y = (point.y * -1f) + 1;


			//Check if this interactor has been already added

			if (!interactionsData.ContainsKey(interactor)) {
				interactionsData[interactor] = new UnityUIDPHandlerData();


				if (interactionsData.Count <= 1) interactionsData[interactor].graphicRaycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
				
				//Add the raycaster
				else interactionsData[interactor].graphicRaycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
			}

			UnityUIDPHandlerData data = interactionsData[interactor];
			
			
			//Set the pos:
			data.mousePos = new Vector2(_cameraTexture.width * point.x, _cameraTexture.height * point.y);
			
			
			//Mouse pos and clicking
			
			if (data.mouseDown) data.mouseDownTime += Time.deltaTime;
			
			if (data.mouseDown && IsMouseOutsideDragDist(data.mouseDownPos, data.mousePos)) {
				data.mouseDragging = true;
			}

			if (data.overlayMouseLeftDown && !data.mouseDown) {
				data.mouseDown = true;
				data.mouseDownPos = data.mousePos;
			}

			if (data.mouseDown && !data.overlayMouseLeftDown) {
				data.mouseDown = false;
				data.mouseDragging = false;
				data.mouseDownTime = 0f;
				data.hasClicked = false;
			}
			
			
			UpdateUnityMouseSim(data);
	

			return new List<Vector3>() {point};
		}


		public static float distToDrag = 60.0f;

		public bool IsMouseOutsideDragDist(Vector2 downPos, Vector2 curPos) {
			if (Mathf.Abs(downPos.x - curPos.x) > distToDrag) return true;
			else if (Mathf.Abs(downPos.y - curPos.y) > distToDrag) return true;

			else return false;
		}


		void UpdateUnityMouseSim(UnityUIDPHandlerData data) {
			var pd = data.uiHandler.pD;

			pd.position = data.mousePos;
			pd.button = PointerEventData.InputButton.Left;
			pd.scrollDelta = data.scrollStrength;

			if (data.mouseDown && !data.mouseDragging && !data.hasClicked) {
				pd.Reset();
				pd.clickCount = 1;
			}
			else if (data.mouseDown && data.mouseDragging) {
				pd.clickCount = 0;
				pd.clickTime = data.mouseDownTime;
				pd.dragging = true;
			}

			HashSet<Selectable> nTargs = data.uiHandler.GetUITargets(data.graphicRaycaster, pd);

			if (nTargs == null) return;


			data.uiHandler.EnterTargets(nTargs);


			if (!Mathf.Approximately(pd.scrollDelta.sqrMagnitude, 0.0f)) {
				var scrollHandler = ExecuteEvents.GetEventHandler<IScrollHandler>(pd.pointerCurrentRaycast.gameObject);
				ExecuteEvents.ExecuteHierarchy(scrollHandler, pd, ExecuteEvents.scrollHandler);
			}

			//_uiHandler.ScrollTargets(nTargs);

			foreach (Selectable ub in nTargs) {
				if (data.enterTargets.Contains(ub)) {
					data.enterTargets.Remove(ub);
				}
			}


			data.enterTargets.RemoveWhere(x => x == null);
			data.downTargets.RemoveWhere(x => x == null);


			data.uiHandler.ExitTargets(data.enterTargets);
			data.enterTargets = nTargs;

			if (data.mouseDown) {
				if (!data.mouseDragging && !data.hasClicked) {
					foreach (Selectable sel in nTargs)
						data.downTargets.Add(sel);

					data.uiHandler.SubmitTargets(data.downTargets);
					data.uiHandler.StartDragTargets(data.downTargets);
					data.uiHandler.DownTargets(data.downTargets);
					data.hasClicked = true;
				}
				else if (data.mouseDragging) {
					data.uiHandler.MoveTargets(data.downTargets);
					data.uiHandler.DragTargets(data.downTargets);
					data.uiHandler.DownTargets(data.downTargets);
				}
			}
			else {
				data.uiHandler.UpTargets(data.downTargets);
				data.uiHandler.EndDragTargets(data.downTargets);
				data.uiHandler.DropTargets(data.downTargets);


				data.downTargets.Clear();
			}
		}


		protected override void InitOVROverlay() {
			SetOverlayTextureBounds(startTextureBounds);
			
			overlay.SetTexture(currentTexture);

			overlay.CreateAndApplyOverlay(startOverlayKey);

			overlay.SetVisible(startVisible, true);
			//if (!useLookHiding) overlay.SetVisible(startVisible, true);
			
			//else overlay.SetVisible(true, false);

			SetOverlayTrackedDevice(startOverlayTrackedDevice);
			

			hasBeenInitialized = true;

			onInitialized?.Invoke();
		}

		public void SetOtherTransformRelativeToElement(Transform other, RectTransform element, Vector3 offset) {
			
			Vector2 localPos = element.position - _rootCanvas.transform.position;
			
			float overlayHalfX = overlay.width / 2;
			float overlayHalfY = (overlay.width * reverseAspect) / 2;


			float canvasHalfX = (_rootCanvas.rect.width * _rootCanvas.localScale.x) / 2;
			float canvasHalfY = (_rootCanvas.rect.height * _rootCanvas.localScale.y) / 2;
            
            
			Vector3 offsetPos = new Vector3() {
				x = Maths.Linear(localPos.x, -canvasHalfX, canvasHalfX, -overlayHalfX, overlayHalfX),
				y = Maths.Linear(localPos.y, -canvasHalfY, canvasHalfY, -overlayHalfY, overlayHalfY)

			};

			float xOffset = offsetPos.x;
			float yOffset = offsetPos.y;
			float zOffset = 0f;
			
			
			Vector3 spawnPos = transform.position + (transform.right * xOffset);
			spawnPos += transform.up * yOffset;
			spawnPos += transform.forward * zOffset;
			
			
			other.SetParent(transform);
			
			other.localEulerAngles = Vector3.zero;
			other.localPosition = new Vector3(offsetPos.x, offsetPos.z, 0f);
			
			other.eulerAngles = new Vector3(0f, other.eulerAngles.y, 0f);

			//Debug.Log("Local Pos X: " + localPos.x);
			
			//Offset for the curvature
			other.position += other.forward * (-0.025f * Mathf.Abs(overlay.curvature * localPos.x));
			
			
			//Offset the pos
			Vector3 newOffset = (other.right * offset.x);
			newOffset += other.up * offset.y;
			newOffset += other.forward * offset.z;

			other.position += newOffset;
			
			
			//Apply rotation for the curvature:

			float rotationOffset = 7f * overlay.curvature * localPos.x;
			
			other.eulerAngles = new Vector3(0f, other.eulerAngles.y + rotationOffset, 0f);
			
			other.SetParent(SteamVRManager.I.noAnchorTrans);




		}

		public Vector3 GetWorldPositionOverlayElement(Vector3 elementPos, Vector3 spawnOffset) {

			if (!hasBeenInitialized) return Vector3.zero;
			
			Vector2 localPos = elementPos - _rootCanvas.transform.position;
            
			float overlayHalfX = overlay.width / 2;
			float overlayHalfY = (overlay.width * reverseAspect) / 2;


			float canvasHalfX = (_rootCanvas.rect.width * _rootCanvas.localScale.x) / 2;
			float canvasHalfY = (_rootCanvas.rect.height * _rootCanvas.localScale.y) / 2;
            
            
			Vector3 offsetPos = new Vector3() {
				x = Maths.Linear(localPos.x, -canvasHalfX, canvasHalfX, -overlayHalfX, overlayHalfX),
				y = Maths.Linear(localPos.y, -canvasHalfY, canvasHalfY, -overlayHalfY, overlayHalfY),
				//z =  0.1f * Mathf.Abs(overlay.curvature * localPos.x)
			};


			float xOffset = offsetPos.x + spawnOffset.x;
			float yOffset = offsetPos.y + spawnOffset.y;
			float zOffset = spawnOffset.z;

			//zOffset += -overlay.curvature * Mathf.Abs(xOffset);


			Vector3 spawnPos = transform.position + (transform.right * xOffset);
			spawnPos += transform.up * yOffset;
			spawnPos += transform.forward * zOffset;
			
			//float strength1 = Mathf.Abs(Maths.GetCircleValue(transform.eulerAngles.x + 90));



			return spawnPos;
		}



	}


	public class UnityUIDPHandlerData {
		public UnityUIDPHandler uiHandler = new UnityUIDPHandler();
		
		public bool overlayMouseLeftDown = false;
		
		public Vector2 mousePos = new Vector2();
		public bool mouseDown = false;
		public Vector2 scrollStrength = Vector2.zero;
		public bool mouseDragging = false;
		public float mouseDownTime = 0f;
		
		public bool hasClicked = false;
		public Vector2 mouseDownPos = new Vector2();

		public GraphicRaycaster graphicRaycaster;
		
		public HashSet<Selectable> enterTargets = new HashSet<Selectable>();
		public HashSet<Selectable> downTargets = new HashSet<Selectable>();
	}
	
}