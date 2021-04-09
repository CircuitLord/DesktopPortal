using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using DesktopPortal.DesktopCapture;
using DesktopPortal.Interaction;
using DesktopPortal.IO;
using DesktopPortal.Sounds;
using DesktopPortal.UI;
using DesktopPortal.Windows;
using DG.Tweening;
using DPCore;
using DPCore.Interaction;
using TCD.System.TouchInjection;
using uDesktopDuplication;
using UnityEngine.UI;
using UnityEngine.XR.WSA;
using uWindowCapture;
using Valve.VR;
using WinStuff;
using Cursor = System.Windows.Forms.Cursor;
using Debug = UnityEngine.Debug;
using Lib = uDesktopDuplication.Lib;
using Point = System.Drawing.Point;
using Screen = System.Windows.Forms.Screen;


namespace DesktopPortal.Overlays {
	public class DPDesktopOverlay : DPOverlayBase {
		private static float movingFastSmoothness = 0.3f;
		private static float movingSlowSmoothness = 0.04f;

		public static DPInteractor mouseInteractor;


		private static int ddaCaptureRate = 60;

		private static int minXSizeResizeWindow = 450;
		private static int minYSizeResizeWindow = 300;
		
		private static InputSimulator inputSimulator = new InputSimulator();

		//TODO: Remove from this when destroyed
		public static List<DPDesktopOverlay> overlays = new List<DPDesktopOverlay>();
		
		/// <summary>
		/// Used to figure out if mouse movement is from the real mouse or from the overlays faking it
		/// </summary>
		public static bool isInteractingVirtually = false;
		
		//	[SerializeField] private Camera camera;
		//[SerializeField] private RawImage rawImage;


		public bool loaded = false;

		public UwcWindow window;

		public UDDMonitor monitor;

		public bool queuedToRenderMonitor = false;

		//public DPMonitor dpMonitor;

		public bool isTargetingWindow = false;
		

		public static DPDesktopOverlay primaryWindowDP;

		public bool isLocked { get; private set; } = false;

		//private bool isPrimary = false;


		private float ddaCaptureTimer = 0f;


		//Mouse interaction:

		[Header("Mouse Settings")] [SerializeField]
		private float triggerStartClickThreshold = 0f;

		[SerializeField] private float triggerDoClickThreshold = 0.4f;

		[SerializeField] private float distToDragThreshold = 60f;
		[SerializeField] private float mouseMovingFastThreshold = 0.1f;
		[SerializeField] private float triggerBackToClickThreshold = 0.1f;


		private bool isClicking = false;
		private bool potentialClick = false;
		private bool hasClicked = false;

		private float mouseClickHoldTime = 0.0f;

		private bool isDragging = false;

		//private bool triggeredRightClickHaptics = false;
		private Vector2 startClickPos;
		private float mouseDownLeftTime = 0f;


		private Point mousePoint;

		/// <summary>
		/// The absolute position of the mouse on the screen
		/// </summary>
		private Vector2 mousePos;

		/// <summary>
		/// The current smoothed position of the mouse
		/// </summary>
		private Vector2 mousePosSmoothed;
		
		
		private static InputSimulator _inputSimulator = null;


		public override void PreInitialize() {

			base.PreInitialize();
			
			if (_inputSimulator == null) _inputSimulator = new InputSimulator();

			onScrolled += HandleScrollEvent;
			onPrimaryInteract += CheckForMouseClick;

			//primaryEvent += ToggleWindowMonitorCapture;

			onInteractedWith += OnInteractedWith;

			onKeyboardInput += OnKeyboardInput;

			
		}



		protected override void OnDestroy() {
			base.OnDestroy();
			
			onScrolled -= HandleScrollEvent;
			onPrimaryInteract -= CheckForMouseClick;
			onInteractedWith -= OnInteractedWith;
			onKeyboardInput -= OnKeyboardInput;


			if (hasBeenInitialized) overlays.Remove(this);
		}



		private void OnInteractedWith(bool interacting) {

			if (!loaded) return;

			//Debug.Log("Interaction state: " + interacting);

			isInteractingVirtually = interacting;
			
			if (interacting && isTargetingWindow) {

				if (isTargetingWindow && window.isIconic) {
					WinNative.ShowWindow(window.handle, ShowWindowCommands.Restore);
				}
				
				WinNative.SetForegroundWindow(window.handle);
				
				if (primaryWindowDP != this) {

					//Clear the previous
					if (primaryWindowDP != null) {
						primaryWindowDP.isPrimary = false;

						//if (primaryWindowDP.isTargetingWindow) {
							
						//}
						
						primaryWindowDP.RequestRendering();
						//primaryWindowDP.UpdateOverlayUVs();
					}

					//StartCoroutine(C_Lock());
					
					primaryWindowDP = this;
					isPrimary = true;
					
					UDDManager.instance.gameObject.SetActive(false);
					UDDManager.instance.gameObject.SetActive(true);

					//UpdateOverlayUVs();
					
				}
			}
			
			//Update the title
			if (interacting) {
				if (!isTargetingWindow) dpAppParent.title = monitor.name;
				else dpAppParent.title = window.title;
			}

			//This only runs when the bar is closed and the config option is enabled
			if (DPSettings.config.focusGameWhenNotInteracting && !interacting) {
				GamingManager.FocusActiveGame();
			}

		}


		private IEnumerator C_Lock() {
			isLocked = true;
			yield return null;
			isLocked = false;
		}
		

		protected override void Update() {
			if (!SteamVRManager.isConnected) return;

			base.Update();

			//Temp capture:
			/*if (loaded && overlay.shouldRender && !isBeingInteracted && isPrimary) {
				if (captureTimer > 1 / 30f) {
					RequestRendering();
					captureTimer = 0f;
				}
			}*/
			
			
		}
		

		
		/// <summary>
		/// Should be called after the overlay texture has been changed and the UVs need to be recalculated
		/// </summary>
		public void UpdateOverlayUVs() {
			if (!loaded) return;

			Vector4 newBounds = new Vector4();

			//If it's targeting a window:
			if (isTargetingWindow) {


				if (DPSettings.config.focusGameWhenNotInteracting && isPrimary && !isBeingInteracted) {
					newBounds = new Vector4(0, 0, 1, 1);

					if (useWindowCropping) {
						newBounds.x += cropAmount.x;
						newBounds.y += cropAmount.y;
						newBounds.z -= cropAmount.z;
						newBounds.w -= cropAmount.w;
					}
				}


				//If we should use Desktop Duplication and need to crop it
				else if (isPrimary && DPSettings.config.useDDA) {
					//TODO: Move to only on init
					RECT border = WindowManager.GetExtensionSizeForWindow(window);


					int localX, localY;

					localX = window.rawX - monitor.left + border.Left;
					localY = window.rawY - monitor.top + border.Top;

					//left side crop, 0 is default
					float x = (float) localX / monitor.width;

					//top side crop, 0 is default
					float y = (float) localY / monitor.height;
					//y = 1f - y;

					//right side crop, 1 is default
					float z = ((float) localX + (float) window.rawWidth - border.Left - border.Right) / monitor.width;
				
					
					//bottom crop, default 1
					float w = ((float) localY + (float) window.rawHeight - border.Bottom - border.Top) / (float)monitor.height;


					newBounds = new Vector4(x, y, z, w);

					if (useWindowCropping) {
						newBounds.x += cropAmount.x * (window.width / (float)monitor.width);
						newBounds.y += cropAmount.y * (window.height / (float)monitor.height);
						
						newBounds.z -= cropAmount.z * (window.width / (float)monitor.width);
						newBounds.w -= cropAmount.w * (window.height / (float)monitor.height);
					}
					//newBounds.x += cropAmount.x;
					
					
				}
				
				//Else we use window capture
				else {
					
					WindowCaptureCrop:
					
					newBounds = new Vector4(0, 0, 1, 1);

					if (useWindowCropping) {
						newBounds.x += cropAmount.x;
						newBounds.y += cropAmount.y;
						newBounds.z -= cropAmount.z;
						newBounds.w -= cropAmount.w;
					}

				}


				newBounds.x = Mathf.Clamp(newBounds.x, 0, 1);
				newBounds.y = Mathf.Clamp(newBounds.y, 0, 1);
				newBounds.z = Mathf.Clamp(newBounds.z, 0, 1);
				newBounds.w = Mathf.Clamp(newBounds.w, 0, 1);

				/*VRTextureBounds_t bounds = new VRTextureBounds_t() {
					uMin = newBounds.x,
					vMin = newBounds.y,
					uMax = newBounds.z,
					vMax = newBounds.w
				};*/
				
				//add on the crop
				//newBounds.x += (cropAmount.x * ());
				

				SetOverlayTextureBounds(newBounds);
			}
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

			if (!loaded) return;

			if (!force) return;

			//Debug.Log("trying to capture");

			if (!DPSettings.config.useDDA) {
				window?.RequestCapture();
			}

			else if (!isPrimary && isTargetingWindow && !DPSettings.config.focusGameWhenNotInteracting) {
				window.RequestCapture();
			}
			
			else if (isTargetingWindow && DPSettings.config.focusGameWhenNotInteracting && !isBeingInteracted) {
				window.RequestCapture();
			}

			queuedToRenderMonitor = true;
			
			return;


			if (!isPrimary && isTargetingWindow) {
				window.RequestCapture();
				return;
			}


			if (isPrimary || !isTargetingWindow) {
				monitor.Render();
				return;
			}


			/*if (!DPSettings.config.useDDA && isTargetingWindow) {
				window.RequestCapture();
				return;
			}

			
			if (!isBeingInteracted && DPSettings.config.focusGameWhenNotInteracting && isTargetingWindow) {
				//Debug.Log("rendering window");
				window.RequestCapture();
				return;
			}
			
			
			
			if (isTargetingWindow && !isPrimary) {
				window.RequestCapture();
				return;
			}
			else if (isTargetingWindow && dpMonitor.isSwitchingWindow) {
				window.RequestCapture();
				return;
			}*/


			//Display Capture
			//if (!isTargetingWindow) {
			monitor.Render();
			currentTexture = monitor.texture;
		//	overlay.SetTexture(monitor.texture);

			return;
			//}


			//	dpMonitor.camera.Render();

			//	_currentTexture = dpMonitor.camera.targetTexture;

			//	overlay.SetTexture(dpMonitor.camera.targetTexture);
		}


		private void C_OnWindowCaptured() {
			StartCoroutine(OnWindowCaptured());
		}

		private IEnumerator OnWindowCaptured() {
			//if (isPrimary && DPSettings.config.useDDA && !DPSettings.config.focusGameWhenNotInteracting) yield break;

			yield return new WaitForEndOfFrame();


			if (DPSettings.config.focusGameWhenNotInteracting && isPrimary && !isBeingInteracted) {
				SetOverlayTexture(window.texture);
				UpdateOverlayUVs();
				yield break;
			}

			
			if (isPrimary && DPSettings.config.useDDA) yield break;
			//if (!DPSettings.config.focusGameWhenNotInteracting) yield break;

			SetOverlayTexture(window.texture);
			UpdateOverlayUVs();
		}


		public override void ResizeForRatio(int ratioX, int ratioY) {
			if (!isResponsive) return;

			if (!isTargetingWindow) return;

			if (!DPSettings.config.useDDA) return;

			if (window.rawWidth < minXSizeResizeWindow || window.rawHeight < minYSizeResizeWindow) return;

			//TODO: Resize
			//WindowManager.FitWindowOnMonitor(monitor, window, true, ratioX, ratioY);
			//StartCoroutine(C_UpdateOverlayUVs());
		}
		
		
		private void OnKeyboardInput(Keys key, Char text, bool down) {
			
			
			if (key == Keys.LShiftKey && !down) Debug.Log("SHIFT UP :/");

			 if (down) _inputSimulator.Keyboard.KeyDown((VirtualKeyCode)key);
			 else _inputSimulator.Keyboard.KeyUp((VirtualKeyCode)key);
			
			
			
	
			
			//if (down) WinNative.keybd_event((byte)key, 0, 1, 0);
			//else WinNative.keybd_event((byte)key, 0, 2, 0);
			
		}

		public override List<Vector3> HandleColliderInteracted(DPInteractor interactor, List<Vector2> interactionPoints) {
			if (interactionPoints.Count <= 0) return new List<Vector3>();
			Vector2 point = interactionPoints.FirstOrDefault();


			if (mouseInteractor == null) {
				mouseInteractor = interactor;
			}

			if (mouseInteractor != interactor) return null;

			float xPos, yPos;
			
			
			

			
			point.x = Maths.Linear(point.x, textureBounds.x, textureBounds.z, 0f, 1f);
			point.y = Maths.Linear(1f - point.y, textureBounds.y, textureBounds.w, 0f, 1f);

			if (isTargetingWindow) {
				

				xPos = (window.rawWidth * point.x) + window.rawX;
				
				
				//float fixedY = 1f - point.y;
				
				yPos = (window.rawHeight * point.y) + window.rawY;
				
			}

			else {
				xPos = (monitor.width * point.x) + monitor.left;
				//float fixedY = 1f - point.y;
				yPos = (monitor.height * point.y) + monitor.top;
			}


			HandleMouseMove(new Vector2(xPos, yPos));

			return new List<Vector3>() {point};
		}


		private Vector2 HandleMouseMove(Vector2 pos) {
			//Vector2 newPos;

			if (isClicking && !hasClicked && !IsMouseOutsideClickRadius(pos)) {
			}
			else {
				if (Math.Abs((pos.x - mousePos.x) * Time.deltaTime) > mouseMovingFastThreshold ||
				    Math.Abs((pos.y - mousePos.y) * Time.deltaTime) > mouseMovingFastThreshold) {
					//Debug.Log("Moving fast smoothness");

					mousePosSmoothed = Vector2.Lerp(mousePosSmoothed, pos, movingFastSmoothness * 15f);
				}
				else {
					//Debug.Log("Moving slow smoothness");
					mousePosSmoothed = Vector2.Lerp(mousePosSmoothed, pos, movingSlowSmoothness * 5f);
				}

				//curMousePos = result.windowCoord;
			}

			mousePos = pos;
			mousePoint = new Point((int) mousePosSmoothed.x, (int) mousePosSmoothed.y);


			WinNative.SetCursorPos(mousePoint.X, mousePoint.Y);

			return mousePosSmoothed;
		}

		/// <summary>
		/// Returns true if the current mouse click is outside the smoothed-radius.
		/// </summary>
		/// <param name="newPos">The coords to compare to startClickPos</param>
		/// <returns></returns>
		private bool IsMouseOutsideClickRadius(Vector2 newPos) {
			if (newPos.x < startClickPos.x - distToDragThreshold ||
			    newPos.x > startClickPos.x + distToDragThreshold) return true;

			if (newPos.y < startClickPos.y - distToDragThreshold ||
			    newPos.y > startClickPos.y + distToDragThreshold) return true;

			else return false;
		}


		public void CheckForMouseClick(DPInteractor interactor, float triggerStrength) {
			//float triggerStrength = 0.0f; //_unitySteamVrHandler.handTriggerStrengths[activeControllerIndex];

			//Debug.Log(triggerStrength);

			if (isClicking && !hasClicked) {
				mouseClickHoldTime += Time.deltaTime;
			}

			//Haptics
			//if (!triggeredRightClickHaptics && potentialClick && mouseClickHoldTime > rightClickHoldThreshold) {
			//	//HapticsManager.SendHaptics(activeControllerIndex, 1, 0.7f);
			//	triggeredRightClickHaptics = true;
			//}


			if (!isClicking && triggerStrength > triggerStartClickThreshold) {
				isClicking = true;

				startClickPos = mousePos;
			}

			if (isClicking && !hasClicked && triggerStrength > triggerDoClickThreshold) {
				potentialClick = true;
			}

			//Drag click
			if (!isDragging && !hasClicked && potentialClick) {
				isDragging = true;

				//Move the mouse back to the origin of the click:
				Point startPoint = new Point((int) startClickPos.x, (int) startClickPos.y);
				WinNative.SetCursorPos(startPoint.X, startPoint.Y);

				//WinNative.mouse_event(WinMouseEvents.WM_MOUSEMOVE);


				//Debug.Log("starting touch drag");

				//left down at the origin
				//CursorInteraction.CursorSendInput(window.handle,
				//	CursorInteraction.SimulationMode.LeftDown);

				if (useTouchInput) {
					TouchInject.BeginDragging(startPoint);
				}
				else {
					inputSimulator.Mouse.LeftButtonDown();
				}
				
				onClickedOn?.Invoke(this);
				

				//Move back to where the user had the mouse
				//CursorInteraction.SetCursorPos(mousePoint.X, mousePoint.Y);
			}

			if (isDragging) {
				//Debug.Log(mousePoint.X);
				if (useTouchInput) TouchInject.AddToActiveDrag(mousePoint);
			}

			//End drag click
			if (isDragging && triggerStrength <= 0.1f) {
				/*CursorInteraction.CursorSendInput(window.handle,
					CursorInteraction.SimulationMode.LeftUp, new Point(),
					0);
				EndMouseClick();*/

				//Debug.Log("ending drag");

				//if (useTouchInput) {
									
					TouchInject.EndActiveDrag();
				//}
				//else {
					inputSimulator.Mouse.LeftButtonUp();
				//}
				

				EndMouseClick();
			}

			//Right click (we have to check this first to see the mouse hold time)
			/*if (!isDragging && !hasClicked && potentialClick &&
			    triggerStrength <= (triggerDoClickThreshold - triggerBackToClickThreshold) &&
			    mouseClickHoldTime > rightClickHoldThreshold) {
				CursorInteraction.CursorSendInput(selectedDPWindowOverlay.window.handle,
					CursorInteraction.SimulationMode.RightClick,
					new Point((int) curMousePos.x, (int) curMousePos.y), 0);

				hasClicked = true;
			}*/

			//Left click
			/*else if (!isDragging && !hasClicked && potentialClick &&
			         triggerStrength <= (triggerDoClickThreshold - triggerBackToClickThreshold)) {
				CursorInteraction.CursorSendInput(window.handle, CursorInteraction.SimulationMode.LeftClick, new Point(), 0);

				hasClicked = true;
			}*/


			//Allows to click again once the trigger is back below the inital threshold
			if (isClicking && triggerStrength <= triggerStartClickThreshold) {
				EndMouseClick();
			}

			//TODO: clear click state when active controller changed
		}

		public void EndMouseClick() {
			mouseClickHoldTime = 0f;
			potentialClick = false;
			isClicking = false;
			hasClicked = false;
			isDragging = false;

			//WinNative.ShowCursor(true);
		}


		public void HandleScrollEvent(DPInteractor interactor, float delta) {
			uint newDelta = 0;

			if (delta > DPSettings.config.joystickDeadzone || delta < -DPSettings.config.joystickDeadzone) {
				//newDelta = (int) (delta * 0.3f * Time.deltaTime * 144f);
				newDelta = (uint) (delta * 120 * Time.deltaTime * 10);

				//if (isTargetingWindow) CursorInteraction.CursorSendInput(window.handle, CursorInteraction.SimulationMode.ScrollV, mousePoint, newDelta);

				WinNative.mouse_event(0x0800, 0, 0, newDelta, 0);
			}
		}


		public void SetTargetCapture(UwcWindow newWindow) {
			//Remove events
			if (window != null) {
				//window.onSizeChanged.RemoveListener(HandleWindowSizeChange);
				window.onCaptured.RemoveListener(C_OnWindowCaptured);
			}

			if (monitor != null) {
				monitor.RemoveDependentDP(this);
			}

			window = newWindow;


			if (DPSettings.config.useDDA) {
				//IntPtr monitorHandle = WinNative.MonitorFromWindow(window.handle, WinNative.MONITOR_DEFAULTTONEAREST);

				monitor = UDDManager.GetMonitor(window);
				
				monitor.AddDependentDP(this);
				
				//Fit the window on the monitor, don't resize
				WindowManager.FitWindowOnMonitor(monitor, window, false);

			}


			isTargetingWindow = true;
			loaded = true;
			//isPrimary = true;
			
			OnInteractedWith(true);


			//StartCoroutine(C_UpdateOverlayUVs());

			window.onCaptured.AddListener(C_OnWindowCaptured);
			
			RequestRendering(true);
		}


		public void SetTargetCapture(UDDMonitor newMonitor) {
			window = null;
			monitor = newMonitor;


			isTargetingWindow = false;
			loaded = true;
			
			OnInteractedWith(true);
		}

		

		public static void RefreshAllWindows() {
			
			//First, focus all them so the game isn't on top of any:
			foreach (DPOverlayBase dpBase in OverlayManager.I.overlays) {

				if (dpBase is DPDesktopOverlay desktopDP) {
					//renderDP.ToggleWindowMonitorCapture(renderDP.isPrimary);
					desktopDP.UpdateOverlayUVs();
					if (desktopDP.isTargetingWindow && desktopDP.isPrimary) WinNative.SetForegroundWindow(desktopDP.window.handle);

				}
				
			}

		}

	}
}