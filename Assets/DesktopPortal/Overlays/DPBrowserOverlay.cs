using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DPCore;
using DPCore.Interaction;
//using MessageLibrary;
//using SimpleWebBrowser;
using UnityEngine;
using UnityEngine.UI;


namespace DesktopPortal.Overlays {
	public class DPBrowserOverlay : DPOverlayBase {
		/*public static List<DPBrowserOverlay> overlays = new List<DPBrowserOverlay>();


		[Header("General settings")] public int Width = 1024;

		public int Height = 768;

		public string MemoryFile = "MainSharedMem";

		public bool RandomMemoryFile = true;

		[Range(8000f, 9000f)] public int Port = 8885;

		public bool RandomPort = true;

		public string InitialURL = "http://www.google.com";

		public bool EnableWebRTC = false;

		[Multiline] public string JSInitializationCode = "";


		[Header("2D setup")] [SerializeField] public RawImage Browser2D = null;


		[Header("UI settings")] [SerializeField]
		public BrowserUI mainUIPanel;

		public bool KeepUIVisible = false;

		[Header("Dialog settings")] [SerializeField]
		public GameObject DialogPanel;

		[SerializeField] public Text DialogText;
		[SerializeField] public Button OkButton;
		[SerializeField] public Button YesButton;
		[SerializeField] public Button NoButton;
		[SerializeField] public InputField DialogPrompt;

		//dialog states - threading
		private bool _showDialog = false;
		private string _dialogMessage = "";
		private string _dialogPrompt = "";

		private DialogEventType _dialogEventType;

		//query - threading
		private bool _startQuery = false;
		private string _jsQueryString = "";

		//status - threading
		private bool _setUrl = false;
		private string _setUrlString = "";

		//input
		//private GraphicRaycaster _raycaster;
		//private StandaloneInputModule _input;

		#region JS Query events

		public delegate void JSQuery(string query);

		public event JSQuery OnJSQuery;

		#endregion


		private Material _mainMaterial;


		private BrowserEngine _mainEngine;


		private bool _focused = false;


		private int posX = 0;
		private int posY = 0;

		private Vector2 uvPos;


		private bool leftDown = false;
		private bool rightDown = false;
		private bool middleDown = false;


		//private Vector2 mousePos;

		//private Camera _mainCamera;


		public override void PreInitialize() {
			base.PreInitialize();

			onPrimaryInteract += OnPrimaryInteract;

			onScrolled += ProcessScrollInput;
		}


		protected void Awake() {
			_mainEngine = new BrowserEngine();

			if (RandomMemoryFile) {
				Guid memid = Guid.NewGuid();
				MemoryFile = memid.ToString();
			}

			if (RandomPort) {
				System.Random r = new System.Random();
				Port = 8000 + r.Next(1000);
			}

			_mainEngine.InitPlugin(1440, 720, MemoryFile, Port, InitialURL, EnableWebRTC);
			//run initialization
			if (JSInitializationCode.Trim() != "")
				_mainEngine.RunJSOnce(JSInitializationCode);
		}


		protected override void InitOVROverlay() {
			if (overlay.validHandle) return;

			SetOverlayTextureBounds(startTextureBounds);

			overlay.CreateAndApplyOverlay(startOverlayKey);

			overlay.SetVisible(startVisible);


			Debug.Log(overlay.handle + " " + overlay.overlayKey);

			hasBeenInitialized = true;

			overlays.Add(this);

			onInitialized?.Invoke();
		}


		public override void RequestRendering(bool force = false) {
			_mainEngine.UpdateTexture();

			SetOverlayTexture(_mainEngine.BrowserTexture);
		}

		public override void ResizeForRatio(int ratioX, int ratioY) {
			//throw new System.NotImplementedException();
		}

		public override List<Vector3> HandleColliderInteracted(DPInteractor interactor, List<Vector2> interactionPoints) {
			int px = (int)(interactionPoints.First().x * currentTexture.width);
			int py = (int)((1f - interactionPoints.First().y) * currentTexture.height);

			//px *= Width;
			//Debug.Log(px);
			//py *= Height;
			//Debug.Log("y " + py);

			//ProcessScrollInput(px, py);

			if (posX != px || posY != py) {
				MouseMessage msg = new MouseMessage {
					Type = MouseEventType.Move,
					X = px,
					Y = py,
					GenericType = MessageLibrary.BrowserEventType.Mouse,
					// Delta = e.Delta,
					Button = MouseButton.None
				};

				//if (leftDown)
				//	msg.Button = MouseButton.Left;
				if (rightDown)
					msg.Button = MouseButton.Right;
				if (middleDown)
					msg.Button = MouseButton.Middle;

				posX = px;
				posY = py;
				_mainEngine.SendMouseEvent(msg);
			}

			uvPos = interactionPoints.First();


			return new List<Vector3>() {interactionPoints.First()};
		}


		private void OnPrimaryInteract(DPInteractor interactor, float amt) {

			
			
			if (!_mainEngine.Initialized) return;

			if (leftDown) {


				if (amt < 0.3f) {
					leftDown = false;
					
					SendMouseButtonEvent(posX, posY, MouseButton.Left,
						MouseEventType.ButtonUp);
					
				}
				
			}

			else {

				if (amt > 0.3f) {
					leftDown = true;
					Debug.Log(posX + " - " + posY);
					SendMouseButtonEvent(posX, posY, MouseButton.Left,
						MouseEventType.ButtonDown);

				}
				
				
				
			}

			
			
		}

		
		private void SendMouseButtonEvent(int x, int y, MouseButton btn, MouseEventType type) {
			MouseMessage msg = new MouseMessage {
				Type = type,
				X = x,
				Y = y,
				GenericType = MessageLibrary.BrowserEventType.Mouse,
				// Delta = e.Delta,
				Button = btn
			};
			_mainEngine.SendMouseEvent(msg);
		}

		private void ProcessScrollInput(DPInteractor interactor, float amt) {
			//float scroll = Input.GetAxis("Mouse ScrollWheel");

			//scrollEvent = scroll * _mainEngine.BrowserTexture.height;

			int scInt = (int) (amt * 200f * Time.deltaTime);

			if (scInt != 0) {
				MouseMessage msg = new MouseMessage {
					Type = MouseEventType.Wheel,
					X = posX,
					Y = posY,
					GenericType = MessageLibrary.BrowserEventType.Mouse,
					Delta = scInt,
					Button = MouseButton.None
				};

				//if (leftDown)
				//	msg.Button = MouseButton.Left;
				if (rightDown)
					msg.Button = MouseButton.Right;
				if (middleDown)
					msg.Button = MouseButton.Middle;

				_mainEngine.SendMouseEvent(msg);
			}
		}


		private void OnApplicationQuit() {
			_mainEngine.Shutdown();
		}*/
		protected override void InitOVROverlay() {
			throw new NotImplementedException();
		}

		public override void RequestRendering(bool force = false) {
			throw new NotImplementedException();
		}

		public override void ResizeForRatio(int ratioX, int ratioY) {
			throw new NotImplementedException();
		}

		public override List<Vector3> HandleColliderInteracted(DPInteractor interactor, List<Vector2> interactionPoints) {
			throw new NotImplementedException();
		}
	}
}