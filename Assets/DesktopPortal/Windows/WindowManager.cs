using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DesktopPortal.Overlays;
using uDesktopDuplication;
using UnityEngine;
using uWindowCapture;
using WinStuff;
using Point = System.Drawing.Point;

namespace DesktopPortal.Windows {
	/// <summary>
	/// Manages things relating to real windows on your desktop
	/// </summary>
	public class WindowManager : MonoBehaviour {
		public static WindowManager I;


		private List<RealWindowData> savedWindowData = new List<RealWindowData>();


		private void Awake() {
			I = this;

			//UwcManager.instance.onInitialized += SaveAllWindows;
		}

		private void Update() {
		}


		/// <summary>
		/// Stores the position of all the windows before the user started DP
		/// </summary>
		public void SaveAllWindows() {
			savedWindowData.Clear();

			foreach (UwcWindow window in UwcManager.windows.Values) {
				if (window.isDesktop) continue;

				if (string.IsNullOrEmpty(window.title)) continue;

				if (window.isChild) continue;


				RealWindowData data = new RealWindowData() {
					pos = new Vector2(window.rawX, window.rawY),
					size = new Vector2(window.rawWidth, window.rawHeight),
					isIconic = window.isIconic
				};

				savedWindowData.Add(data);

				Debug.Log(window.title);
			}
		}




		public static void FitWindowOnMonitor(UDDMonitor monitor, UwcWindow window, bool resize = false, int widthRatio = 0, int heightRatio = 0, float maxWidthFill = 0.9f, float maxHeightFill = 1f) {
			//IntPtr monitorHandle = WinNative.MonitorFromWindow(handle, WinNative.MONITOR_DEFAULTTONEAREST);
			//WinNative.MonitorInfoEx monitorInfo = FetchMonitorInfo(handle);
			
			WinNative.SetForegroundWindow(window.handle);
			
			//WM_GETMINMAXINFO
			//WinNative.SendMessage(window.handle, 0x0024, )
			


			if (monitor == null) {
				Debug.LogError("Monitor was null!");
				return;
			}

			int potentialWidth = 0, potentialHeight = 0;

			if (resize) {
				bool found = false;
				int i = 0;

				while (!found && i < 3000) {
					potentialWidth += widthRatio;
					potentialHeight += heightRatio;

					if (potentialWidth > monitor.workingArea.Width * maxWidthFill) found = true;
					if (potentialHeight > monitor.workingArea.Height * maxHeightFill) found = true;

					//Fallback:
					i++;
				}
			}

			RECT border = GetExtensionSizeForWindow(window);
			
			int xPos = monitor.left + monitor.workingArea.Left - border.Left;
			int yPos = monitor.top + monitor.workingArea.Top;


			if (resize) {

				//int xPos = monitor.workingArea.Left;
				//int yPos = monitor.workingArea.Top;

				potentialWidth += border.Left + border.Right;
				potentialHeight += border.Bottom;
				
				WinNative.SetWindowPos(window.handle, 0, xPos, yPos, potentialWidth, potentialHeight, SetWindowPosFlags.SWP_NOSENDCHANGING | SetWindowPosFlags.SWP_SHOWWINDOW);
			}
			else {
				
				//int xPos = monitor.workingArea.Left - border.Left;
				//int yPos = monitor.workingArea.Top;
				
				//WinNative.SetWindowPos(window.handle, 0, xPos, yPos, 0, 0, SetWindowPosFlags.SWP_NOSENDCHANGING | SetWindowPosFlags.SWP_SHOWWINDOW | SetWindowPosFlags.SWP_NOSIZE);
			}



		}

		public static RECT GetExtensionSizeForWindow(UwcWindow window) {
			int size = Marshal.SizeOf(typeof(RECT));

			WinNative.SetThreadDpiAwarenessContext(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
			RECT rect = new RECT();
			RECT frame = new RECT();

			WinNative.GetWindowRect(window.handle, ref rect);
			WinNative.DwmGetWindowAttribute(window.handle, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out frame, size);
			
			RECT border = new RECT();
			border.Left = frame.Left - rect.Left;
			border.Top = frame.Top - rect.Top;
			border.Right = rect.Right - frame.Right;
			border.Bottom = rect.Bottom - frame.Bottom;

			return border;
		}

		public static IEnumerator UpdateWeirdOffset(UDDMonitor monitor, UwcWindow window) {
			WinNative.SetCursorPos(0, 0);
			WinNative.SetWindowPos(window.handle, 0, 0, 0, 0, 0, SetWindowPosFlags.SWP_NOSIZE);

			int size = Marshal.SizeOf(typeof(RECT));

			WinNative.SetThreadDpiAwarenessContext(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
			RECT rect = new RECT();
			RECT frame = new RECT();

			WinNative.GetWindowRect(window.handle, ref rect);


			WinNative.DwmGetWindowAttribute(window.handle, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out frame, size);

//rect should be `0, 0, 1280, 1024`
//frame should be `7, 0, 1273, 1017`

			RECT border = new RECT();
			border.Left = frame.Left - rect.Left;
			border.Top = frame.Top - rect.Top;
			border.Right = rect.Right - frame.Right;
			border.Bottom = rect.Bottom - frame.Bottom;
			

			Debug.Log(border.Left);

			
			
			yield return new WaitForSeconds(0.1f);
			
			WinNative.MoveOverWindow(window.handle, new Point(0, 0), true);


			
			
				
			WinNative.POINT curCursorPos = new WinNative.POINT();
			WinNative.GetCursorPos(ref curCursorPos);

			//window.weirdOffset = Math.Abs(curCursorPos.x);


			yield break;
			
			while (true) {
				RECT yay = new RECT();
				WinNative.GetWindowRect(window.handle, ref yay);

				Debug.Log(yay.Left);

				yield return null;

			}
			
			//Debug.Log(window.weirdOffset);
		}

		


		public static WinNative.MonitorInfoEx FetchMonitorInfo(IntPtr handle) {
			WinNative.MonitorInfoEx info = new WinNative.MonitorInfoEx();
			info.cbSize = (int) Marshal.SizeOf(info);
			WinNative.GetMonitorInfo(handle, ref info);
			return info;
		}
	}
	
	
	


	public enum RealWindowCorner {
		TopLeft,
		BottomLeft,
		TopRight,
		BottomRight
	}
	
	public class RealWindowData {
		public IntPtr handle;
		public Vector2 pos;
		public Vector2 size;
		public bool isIconic;
	}
}