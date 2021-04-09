using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace WinStuff {
	public class WinNative {
		#region Class Variables

		public const int SM_CXSCREEN = 0;
		public const int SM_CYSCREEN = 1;

		public const Int32 CURSOR_SHOWING = 0x00000001;
		
		public const Int32 MONITOR_DEFAULTTOPRIMERTY = 0x00000001;
		public const Int32 MONITOR_DEFAULTTONEAREST = 0x00000002;

		[StructLayout(LayoutKind.Sequential)]
		public struct ICONINFO {
			public bool fIcon; // Specifies whether this structure defines an icon or a cursor. A value of TRUE specifies 

			public Int32
				xHotspot; // Specifies the x-coordinate of a cursor's hot spot. If this structure defines an icon, the hot 

			public Int32
				yHotspot; // Specifies the y-coordinate of the cursor's hot spot. If this structure defines an icon, the hot 

			public IntPtr
				hbmMask; // (HBITMAP) Specifies the icon bitmask bitmap. If this structure defines a black and white icon, 

			public IntPtr hbmColor; // (HBITMAP) Handle to the icon color bitmap. This member can be optional if this 
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT {
			public Int32 x;
			public Int32 y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct CURSORINFO {
			public Int32 cbSize; // Specifies the size, in bytes, of the structure. 
			public Int32 flags; // Specifies the cursor state. This parameter can be one of the following values:
			public IntPtr hCursor; // Handle to the cursor. 
			public POINT ptScreenPos; // A POINT structure that receives the screen coordinates of the cursor. 
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WINDOWINFO {
			public uint cbSize;
			public RECT rcWindow;
			public RECT rcClient;
			public uint dwStyle;
			public uint dwExStyle;
			public uint dwWindowStatus;
			public uint cxWindowBorders;
			public uint cyWindowBorders;
			public ushort atomWindowType;
			public ushort wCreatorVersion;

			public WINDOWINFO(Boolean? filler) :
				this() // Allows automatic initialization of "cbSize" with "new WINDOWINFO(null/true/false)".
			{
				cbSize = (UInt32) (Marshal.SizeOf(typeof(WINDOWINFO)));
			}
		}

		#endregion


		#region Class Functions

		[DllImport("user32.dll", EntryPoint = "GetCursorInfo")]
		public static extern bool GetCursorInfo(out CURSORINFO pci);

		[DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
		public static extern IntPtr GetSafeHwnd();

		[DllImport("user32.dll", EntryPoint = "GetDC")]
		public static extern IntPtr GetDC(IntPtr ptr);

		[DllImport("user32.dll", EntryPoint = "GetSystemMetrics")]
		public static extern int GetSystemMetrics(int abc);

		[DllImport("user32.dll", EntryPoint = "GetWindowDC")]
		public static extern IntPtr GetWindowDC(Int32 ptr);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowInfo(IntPtr hwnd, ref WINDOWINFO pwi);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hwnd, ref RECT lpRect);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy,
			SetWindowPosFlags uFlags);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder strText, int maxCount);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "ReleaseDC")]
		public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

		[DllImport("user32.dll", EntryPoint = "CopyIcon")]
		public static extern IntPtr CopyIcon(IntPtr hIcon);

		[DllImport("user32.dll", EntryPoint = "GetIconInfo")]
		public static extern bool GetIconInfo(IntPtr hIcon, out ICONINFO piconinfo);

		[DllImport("user32.dll")]
		public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool BringWindowToTop(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetForegroundWindow(IntPtr hWnd);
		
		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();
		
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
		

		/// <summary>
		///     Determines the visibility state of the specified window.
		///     <para>
		///     Go to https://msdn.microsoft.com/en-us/library/windows/desktop/ms633530%28v=vs.85%29.aspx for more
		///     information. For WS_VISIBLE information go to
		///     https://msdn.microsoft.com/en-us/library/windows/desktop/ms632600%28v=vs.85%29.aspx
		///     </para>
		/// </summary>
		/// <param name="hWnd">C++ ( hWnd [in]. Type: HWND )<br />A handle to the window to be tested.</param>
		/// <returns>
		///     <c>true</c> or the return value is nonzero if the specified window, its parent window, its parent's parent
		///     window, and so forth, have the WS_VISIBLE style; otherwise, <c>false</c> or the return value is zero.
		/// </returns>
		/// <remarks>
		///     The visibility state of a window is indicated by the WS_VISIBLE[0x10000000L] style bit. When
		///     WS_VISIBLE[0x10000000L] is set, the window is displayed and subsequent drawing into it is displayed as long as the
		///     window has the WS_VISIBLE[0x10000000L] style. Any drawing to a window with the WS_VISIBLE[0x10000000L] style will
		///     not be displayed if the window is obscured by other windows or is clipped by its parent window.
		/// </remarks>
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

		[DllImport( "user32.dll" )]
		public static extern IntPtr MonitorFromWindow( IntPtr handle, Int32 flags );

		[DllImport("user32.dll")]
		public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx monitorInfo);

		
		
		[DllImport("user32.dll")]
		public static extern bool PhysicalToLogicalPointForPerMonitorDPI(IntPtr hWnd, ref POINT lpPoint);
		

		[DllImport("SHcore.dll")]
		public static extern int GetProcessDpiAwareness(IntPtr hWnd, out PROCESS_DPI_AWARENESS value);
		
		
		[DllImport("user32.dll")]
		public static extern int SetThreadDpiAwarenessContext(PROCESS_DPI_AWARENESS dpiContext);

		[DllImport("user32.dll")]
		public static extern int GetDpiForWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetWindowDpiAwarenessContext(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern int GetAwarenessFromDpiAwarenessContext(IntPtr dpiContext);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA,
			IntPtr dpiContextB);
		
		
		
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(ref POINT pt);
		

		public static string GetFilePath(IntPtr hwnd) {
			try {
				int pid = 0;
				GetWindowThreadProcessId(hwnd, out pid);
				Process proc = Process.GetProcessById((int) pid); //Gets the process by ID.
				return proc.MainModule.FileName.ToString(); //Returns the path.
			}
			catch (Exception e) {
				UnityEngine.Debug.LogError(e);
				return null;
			}
		}

		// Delegate to filter which windows to include 
		public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		/// <summary> Get the text for the window pointed to by hWnd </summary>
		public static string GetWindowText(IntPtr hWnd) {
			int size = GetWindowTextLength(hWnd);
			if (size > 0) {
				var builder = new StringBuilder(size + 1);
				GetWindowText(hWnd, builder, builder.Capacity);
				return builder.ToString();
			}

			return String.Empty;
		}

		/// <summary> Find all windows that match the given filter </summary>
		/// <param name="filter"> A delegate that returns true for windows
		///    that should be returned and false for windows that should
		///    not be returned </param>
		public static IEnumerable<IntPtr> FindWindows(EnumWindowsProc filter) {
			List<IntPtr> windows = new List<IntPtr>();

			EnumWindows(delegate(IntPtr wnd, IntPtr param) {
				if (filter(wnd, param)) {
					// only add the windows that pass the filter
					windows.Add(wnd);
				}

				// but return true here so that we iterate all windows
				return true;
			}, IntPtr.Zero);

			return windows;
		}

		/// <summary> Find all windows that contain the given title text </summary>
		/// <param name="titleText"> The text that the window title must contain. </param>
		public static IEnumerable<IntPtr> FindWindowsWithText(string titleText) {
			return FindWindows(delegate(IntPtr wnd, IntPtr param) { return GetWindowText(wnd).Contains(titleText); });
		}

		/// <summary> Find all windows that contain the given title text </summary>
		/// <param name="titleText"> The text that the window title must contain. </param>
		public static IEnumerable<IntPtr> FindWindowsWithSize() {
			return FindWindows(delegate(IntPtr wnd, IntPtr param) { return IsWindowVisible(wnd); });
		}

		#endregion
	
		//My stuff

		public static string GetFileDescForPID(int pid) {
			string fileDesc = "";
			string path = "";
			
			try {
				Process p = Process.GetProcessById(pid);
				path = p.MainModule?.FileName;
				FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
				fileDesc = info.FileDescription;

			}
			catch (Exception e) {
				UnityEngine.Debug.Log(e);
			}

			if (fileDesc == "") {
				fileDesc = Path.GetFileName(path)?.Replace(".exe", "");
			}

			return fileDesc;
		}
	
	
		
		delegate bool MonitorEnumDelegate( IntPtr hMonitor,IntPtr hdcMonitor,ref RECT lprcMonitor, IntPtr dwData );

		[ DllImport( "user32.dll" ) ]
		static extern bool EnumDisplayMonitors( IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData );

		
		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern short VkKeyScanEx(char ch, IntPtr dwhkl);
		
		[DllImport("user32.dll")]
		public static extern bool UnloadKeyboardLayout(IntPtr hkl);
		[DllImport("user32.dll")]
		public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);
		
		[DllImport("user32.dll")]
		public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, int dwExtraInfo);

		public static IntPtr GetMonitorHandleForName(string name) {

			int i = 0;

			IntPtr ptrToReturn = new IntPtr();

			EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
				delegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) {
					
					WinNative.MonitorInfoEx info = new WinNative.MonitorInfoEx();
					info.cbSize = (int)Marshal.SizeOf(info);
					
					WinNative.GetMonitorInfo(hMonitor, ref info);

					
					//TODO: char array to string
					//if (info.DeviceName == name) ptrToReturn = hMonitor;

					i++;
					return true;

				}, IntPtr.Zero);


			return ptrToReturn;
		}
		
		[DllImport("user32.dll")]
		public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, int dwExtraInfo);


		[DllImport("user32.dll")]
		public static extern int ShowCursor(bool bShow);
		


		[StructLayout(LayoutKind.Sequential)]
		public struct MonitorInfoEx {
			public int cbSize;
			public RECT rcMonitor; // Total area
			public RECT rcWork; // Working area
			public int dwFlags;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
			public char[] szDevice;
		}

		
		
		        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);
    

        [DllImport("user32.dll")]
        static extern bool ScreenToClient(IntPtr hWnd, ref Point lpPoint);

        [DllImport("user32.dll")]
        internal static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);
        

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool SendNotifyMessage(IntPtr hWnd, uint Msg, UIntPtr wParam, IntPtr lParam);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);
    
        [DllImport("user32.dll")]
        public static extern long SetCursorPos(int x, int y);

        
        public static void PostMessageSafe(IntPtr hWnd, uint msg, int wParam, int lParam)
        {
	        bool returnValue = PostMessage(hWnd, msg, wParam, lParam);
	        if (!returnValue)
	        {
		        // An error occured
		        throw new Win32Exception(Marshal.GetLastWin32Error());
	        }
        }
        
        
          public static void CursorSendInput(IntPtr wndHandle, SimulationMode mode, Point clickPoint = new Point(), int delta = 0)
        {
            Point oldPos;
            IntPtr lPrm;
        
            SendInput(mode, delta, clickPoint);
        

        }
    

        public static IntPtr GetLParam(IntPtr wndHandle, Point clientPoint, out Point oldPos)
        {
            oldPos = Cursor.Position;
            IntPtr lParam = (IntPtr)((clientPoint.Y << 16) | clientPoint.X);
            ClientToScreen(wndHandle, ref clientPoint);
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);
            return lParam;
        }

        private static void SendInput(SimulationMode mode, int delta, Point p)
        {
            INPUT[] pInputs;
            // Click Mouse
            switch (mode)
            {
                case SimulationMode.RightClick:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.RIGHTDOWN,}}}, new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.RIGHTUP,}}}
                    };
                    break;
                case SimulationMode.LeftClick:
                case SimulationMode.DoubleClick:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.LEFTDOWN,}}}, new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.LEFTUP,}}}
                    };
                    break;
                case SimulationMode.LeftDownAbs:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.LEFTDOWN | MOUSEEVENTF.ABSOLUTE, dx = p.X, dy = p.Y}}}
                    };
                    break;
                case SimulationMode.LeftDown:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.LEFTDOWN,}}}
                    };
                    break;
                case SimulationMode.LeftMove:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.LEFTDOWN | MOUSEEVENTF.MOVE, dx = p.X, dy = p.Y}}}
                    };
                    break;
                case SimulationMode.LeftUp:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.LEFTUP,}}}
                    };
                    break;
                case SimulationMode.MiddleClick:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.MIDDLEDOWN,}}}, new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.MIDDLEUP,}}}
                    };
                    break;
                case SimulationMode.ScrollH:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.HWHEEL, mouseData = delta * 60}}}
                    };
                    break;
                case SimulationMode.ScrollV:
                    pInputs = new[]
                    {
                        new INPUT() {type = InputType.MOUSE, U = new InputUnion() {mi = new MOUSEINPUT() {dwFlags = MOUSEEVENTF.WHEEL, mouseData = delta * 60}}}
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }
            if (pInputs.Length > 0)
                SendInput((uint)pInputs.Length, pInputs, INPUT.Size);
        }

        private static void SendMessage(IntPtr wndHandle, SimulationMode mode, IntPtr lParam, int delta)
        {
            switch (mode)
            {
                case SimulationMode.LeftClick:
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONDOWN, (IntPtr)1, lParam);
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONUP, IntPtr.Zero, lParam);
                    break;
                case SimulationMode.RightClick:
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_RBUTTONDOWN, (IntPtr)2, lParam);
                    WinNative. SendMessage(wndHandle, (uint)WinMouseEvents.WM_RBUTTONUP, IntPtr.Zero, lParam);
                    break;
                case SimulationMode.DoubleClick:
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONDBLCLK, (IntPtr)1, lParam);
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONUP, IntPtr.Zero, lParam);
                    break;
                case SimulationMode.MiddleClick:
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_MBUTTONDOWN, (IntPtr)10, lParam);
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_MBUTTONUP, IntPtr.Zero, lParam);
                    break;
                case SimulationMode.ScrollH:
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_MOUSEHWHEEL, (IntPtr)((delta * 60) << 16), lParam);
                    break;
                case SimulationMode.ScrollV:
                    WinNative.SendMessage(wndHandle, (uint)WinMouseEvents.WM_MOUSEWHEEL, (IntPtr)((delta * 60) << 16), lParam);
                    break;
                case SimulationMode.LeftDownAbs:
                    break;
                case SimulationMode.LeftDown:
                    break;
                case SimulationMode.LeftMove:
                    break;
                case SimulationMode.LeftUp:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }
        }

        private static void SendNotifyMessage(IntPtr wndHandle, SimulationMode mode, IntPtr lParam, int delta)
        {
            switch (mode)
            {
                case SimulationMode.LeftClick:
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONDOWN, (UIntPtr)1, lParam);
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONUP, UIntPtr.Zero, lParam);
                    break;
                case SimulationMode.RightClick:
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_RBUTTONDOWN, (UIntPtr)2, lParam);
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_RBUTTONUP, UIntPtr.Zero, lParam);
                    break;
                case SimulationMode.DoubleClick:
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONDBLCLK, (UIntPtr)1, lParam);
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_LBUTTONUP, UIntPtr.Zero, lParam);
                    break;
                case SimulationMode.MiddleClick:
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_MBUTTONDOWN, (UIntPtr)10, lParam);
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_MBUTTONUP, UIntPtr.Zero, lParam);
                    break;
                case SimulationMode.ScrollH:
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_MOUSEHWHEEL, (UIntPtr)((delta * 60) << 16), lParam);
                    break;
                case SimulationMode.ScrollV:
                    SendNotifyMessage(wndHandle, (uint)WinMouseEvents.WM_MOUSEWHEEL, (UIntPtr)((delta * 60) << 16), lParam);
                    break;
                case SimulationMode.LeftDownAbs:
                    break;
                case SimulationMode.LeftDown:
                    break;
                case SimulationMode.LeftMove:
                    break;
                case SimulationMode.LeftUp:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("mode", mode, null);
            }
        }

        private static IntPtr lastHandle;
        private static Point lastPoint;
        // Move the cursor to a given point over a window
        public static void MoveOverWindow(IntPtr wndHandle, Point clientPoint, bool force = false)
        {
            if (!force && lastPoint.X == clientPoint.X && lastPoint.Y == clientPoint.Y && lastHandle != IntPtr.Zero && lastHandle == wndHandle) return;
            lastHandle = wndHandle;
            lastPoint = clientPoint;
            IntPtr lParam = (IntPtr) ((clientPoint.Y << 16) | clientPoint.X);
            ClientToScreen(wndHandle, ref clientPoint);
            Cursor.Position = new Point(clientPoint.X, clientPoint.Y);
            SendNotifyMessage(wndHandle, (uint) WinMouseEvents.WM_MOUSEMOVE, UIntPtr.Zero, lParam);
        }

        public static Point GetCursorPosRelativeWindow(IntPtr wndHandle)
        {
            var clientPoint = Cursor.Position;
            ScreenToClient(wndHandle, ref clientPoint);
            return clientPoint;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr parentWindow, IntPtr previousChildWindow, string windowClass, string windowTitle);
        
        public static IntPtr[] GetProcessWindows(uint process) {
	        IntPtr[] apRet = (new IntPtr[256]);
	        int iCount = 0;
	        IntPtr pLast = IntPtr.Zero;
	        do {
		        pLast = FindWindowEx(IntPtr.Zero, pLast, null, null);
		        int iProcess_;
		        GetWindowThreadProcessId(pLast, out iProcess_);
		        if(iProcess_ == process) apRet[iCount++] = pLast;
	        } while(pLast != IntPtr.Zero);
	        System.Array.Resize(ref apRet, iCount);
	        return apRet;
        }
        
        
        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out bool pvAttribute, int cbAttribute);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT pvAttribute, int cbAttribute);
        
        public static bool IsRunningAsAdministrator() {
	        return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
		        .IsInRole(WindowsBuiltInRole.Administrator);
        }  
	
		
	}
	
	
	
	public enum DWMWINDOWATTRIBUTE : uint
	{
		NCRenderingEnabled = 1,
		NCRenderingPolicy,
		TransitionsForceDisabled,
		AllowNCPaint,
		CaptionButtonBounds,
		NonClientRtlLayout,
		ForceIconicRepresentation,
		Flip3DPolicy,
		ExtendedFrameBounds,
		HasIconicBitmap,
		DisallowPeek,
		ExcludedFromPeek,
		Cloak,
		Cloaked,
		FreezeRepresentation
	}
	
	public enum WinMouseEvents
	{
		WM_MOUSEMOVE = 0x200,
		WM_LBUTTONDOWN = 0x201,
		WM_LBUTTONUP = 0x202,
		WM_LBUTTONDBLCLK = 0x203,
		WM_RBUTTONDOWN = 0x204,
		WM_RBUTTONUP = 0x205,
		WM_RBUTTONDBLCLK = 0x206,
		WM_MBUTTONDOWN = 0x207,
		WM_MBUTTONUP = 0x208,
		WM_MBUTTONDBLCLK = 0x209,
		WM_MOUSEWHEEL = 0x20A,
		WM_XBUTTONDOWN = 0x20B,
		WM_XBUTTONUP = 0x20C,
		WM_XBUTTONDBLCLK = 0x20D,
		WM_MOUSEHWHEEL = 0x20E,
	}



#pragma warning disable 649
        #region SendInput Structs
        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            internal InputType type;
            internal InputUnion U;
            internal static int Size
            {
                get { return Marshal.SizeOf(typeof(INPUT)); }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct InputUnion
        {
            [FieldOffset(0)]
            internal MOUSEINPUT mi;
            [FieldOffset(0)]
            internal KEYBDINPUT ki;
            [FieldOffset(0)]
            internal HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct MOUSEKEYBDHARDWAREINPUT
        {
            [FieldOffset(0)]
            public MOUSEINPUT Mouse;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct MOUSEINPUT
        {
            internal int dx;
            internal int dy;
            internal int mouseData;
            internal MOUSEEVENTF dwFlags;
            internal uint time;
            internal UIntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct KEYBDINPUT
        {
            internal VirtualKeyShort wVk;
            internal ScanCodeShort wScan;
            internal KEYEVENTF dwFlags;
            internal int time;
            internal UIntPtr dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        internal struct HARDWAREINPUT
        {
            internal int uMsg;
            internal short wParamL;
            internal short wParamH;
        }
        #endregion
        #region Enums
        internal enum InputType : uint
        {
            MOUSE = 0,
            KEYBOARD = 1,
            HARDWARE = 2
        }
        [Flags]
        internal enum MOUSEEVENTF : uint
        {
            ABSOLUTE = 0x8000,
            HWHEEL = 0x01000,
            MOVE = 0x0001,
            MOVE_NOCOALESCE = 0x2000,
            LEFTDOWN = 0x0002,
            LEFTUP = 0x0004,
            RIGHTDOWN = 0x0008,
            RIGHTUP = 0x0010,
            MIDDLEDOWN = 0x0020,
            MIDDLEUP = 0x0040,
            VIRTUALDESK = 0x4000,
            WHEEL = 0x0800,
            XDOWN = 0x0080,
            XUP = 0x0100
        }
        [Flags]
        internal enum KEYEVENTF : uint
        {
            EXTENDEDKEY = 0x0001,
            KEYUP = 0x0002,
            SCANCODE = 0x0008,
            UNICODE = 0x0004
        }
        
        public enum SimulationMode
        {
	        LeftClick,
	        RightClick,
	        DoubleClick,
	        MiddleClick,
	        ScrollH,
	        ScrollV,
	        LeftDownAbs,
	        LeftDown,
	        LeftMove,
	        LeftUp,
        }

		
		
		

        public enum VirtualKeyShort : short
        {
            ///<summary>
            ///Left mouse button
            ///</summary>
            LBUTTON = 0x01,
            ///<summary>
            ///Right mouse button
            ///</summary>
            RBUTTON = 0x02,
            ///<summary>
            ///Control-break processing
            ///</summary>
            CANCEL = 0x03,
            ///<summary>
            ///Middle mouse button (three-button mouse)
            ///</summary>
            MBUTTON = 0x04,
            ///<summary>
            ///Windows 2000/XP: X1 mouse button
            ///</summary>
            XBUTTON1 = 0x05,
            ///<summary>
            ///Windows 2000/XP: X2 mouse button
            ///</summary>
            XBUTTON2 = 0x06,
            ///<summary>
            ///BACKSPACE key
            ///</summary>
            BACK = 0x08,
            ///<summary>
            ///TAB key
            ///</summary>
            TAB = 0x09,
            ///<summary>
            ///CLEAR key
            ///</summary>
            CLEAR = 0x0C,
            ///<summary>
            ///ENTER key
            ///</summary>
            RETURN = 0x0D,
            ///<summary>
            ///SHIFT key
            ///</summary>
            SHIFT = 0x10,
            ///<summary>
            ///CTRL key
            ///</summary>
            CONTROL = 0x11,
            ///<summary>
            ///ALT key
            ///</summary>
            MENU = 0x12,
            ///<summary>
            ///PAUSE key
            ///</summary>
            PAUSE = 0x13,
            ///<summary>
            ///CAPS LOCK key
            ///</summary>
            CAPITAL = 0x14,
            ///<summary>
            ///Input Method Editor (IME) Kana mode
            ///</summary>
            KANA = 0x15,
            ///<summary>
            ///IME Hangul mode
            ///</summary>
            HANGUL = 0x15,
            ///<summary>
            ///IME Junja mode
            ///</summary>
            JUNJA = 0x17,
            ///<summary>
            ///IME final mode
            ///</summary>
            FINAL = 0x18,
            ///<summary>
            ///IME Hanja mode
            ///</summary>
            HANJA = 0x19,
            ///<summary>
            ///IME Kanji mode
            ///</summary>
            KANJI = 0x19,
            ///<summary>
            ///ESC key
            ///</summary>
            ESCAPE = 0x1B,
            ///<summary>
            ///IME convert
            ///</summary>
            CONVERT = 0x1C,
            ///<summary>
            ///IME nonconvert
            ///</summary>
            NONCONVERT = 0x1D,
            ///<summary>
            ///IME accept
            ///</summary>
            ACCEPT = 0x1E,
            ///<summary>
            ///IME mode change request
            ///</summary>
            MODECHANGE = 0x1F,
            ///<summary>
            ///SPACEBAR
            ///</summary>
            SPACE = 0x20,
            ///<summary>
            ///PAGE UP key
            ///</summary>
            PRIOR = 0x21,
            ///<summary>
            ///PAGE DOWN key
            ///</summary>
            NEXT = 0x22,
            ///<summary>
            ///END key
            ///</summary>
            END = 0x23,
            ///<summary>
            ///HOME key
            ///</summary>
            HOME = 0x24,
            ///<summary>
            ///LEFT ARROW key
            ///</summary>
            LEFT = 0x25,
            ///<summary>
            ///UP ARROW key
            ///</summary>
            UP = 0x26,
            ///<summary>
            ///RIGHT ARROW key
            ///</summary>
            RIGHT = 0x27,
            ///<summary>
            ///DOWN ARROW key
            ///</summary>
            DOWN = 0x28,
            ///<summary>
            ///SELECT key
            ///</summary>
            SELECT = 0x29,
            ///<summary>
            ///PRINT key
            ///</summary>
            PRINT = 0x2A,
            ///<summary>
            ///EXECUTE key
            ///</summary>
            EXECUTE = 0x2B,
            ///<summary>
            ///PRINT SCREEN key
            ///</summary>
            SNAPSHOT = 0x2C,
            ///<summary>
            ///INS key
            ///</summary>
            INSERT = 0x2D,
            ///<summary>
            ///DEL key
            ///</summary>
            DELETE = 0x2E,
            ///<summary>
            ///HELP key
            ///</summary>
            HELP = 0x2F,
            ///<summary>
            ///0 key
            ///</summary>
            KEY_0 = 0x30,
            ///<summary>
            ///1 key
            ///</summary>
            KEY_1 = 0x31,
            ///<summary>
            ///2 key
            ///</summary>
            KEY_2 = 0x32,
            ///<summary>
            ///3 key
            ///</summary>
            KEY_3 = 0x33,
            ///<summary>
            ///4 key
            ///</summary>
            KEY_4 = 0x34,
            ///<summary>
            ///5 key
            ///</summary>
            KEY_5 = 0x35,
            ///<summary>
            ///6 key
            ///</summary>
            KEY_6 = 0x36,
            ///<summary>
            ///7 key
            ///</summary>
            KEY_7 = 0x37,
            ///<summary>
            ///8 key
            ///</summary>
            KEY_8 = 0x38,
            ///<summary>
            ///9 key
            ///</summary>
            KEY_9 = 0x39,
            ///<summary>
            ///A key
            ///</summary>
            KEY_A = 0x41,
            ///<summary>
            ///B key
            ///</summary>
            KEY_B = 0x42,
            ///<summary>
            ///C key
            ///</summary>
            KEY_C = 0x43,
            ///<summary>
            ///D key
            ///</summary>
            KEY_D = 0x44,
            ///<summary>
            ///E key
            ///</summary>
            KEY_E = 0x45,
            ///<summary>
            ///F key
            ///</summary>
            KEY_F = 0x46,
            ///<summary>
            ///G key
            ///</summary>
            KEY_G = 0x47,
            ///<summary>
            ///H key
            ///</summary>
            KEY_H = 0x48,
            ///<summary>
            ///I key
            ///</summary>
            KEY_I = 0x49,
            ///<summary>
            ///J key
            ///</summary>
            KEY_J = 0x4A,
            ///<summary>
            ///K key
            ///</summary>
            KEY_K = 0x4B,
            ///<summary>
            ///L key
            ///</summary>
            KEY_L = 0x4C,
            ///<summary>
            ///M key
            ///</summary>
            KEY_M = 0x4D,
            ///<summary>
            ///N key
            ///</summary>
            KEY_N = 0x4E,
            ///<summary>
            ///O key
            ///</summary>
            KEY_O = 0x4F,
            ///<summary>
            ///P key
            ///</summary>
            KEY_P = 0x50,
            ///<summary>
            ///Q key
            ///</summary>
            KEY_Q = 0x51,
            ///<summary>
            ///R key
            ///</summary>
            KEY_R = 0x52,
            ///<summary>
            ///S key
            ///</summary>
            KEY_S = 0x53,
            ///<summary>
            ///T key
            ///</summary>
            KEY_T = 0x54,
            ///<summary>
            ///U key
            ///</summary>
            KEY_U = 0x55,
            ///<summary>
            ///V key
            ///</summary>
            KEY_V = 0x56,
            ///<summary>
            ///W key
            ///</summary>
            KEY_W = 0x57,
            ///<summary>
            ///X key
            ///</summary>
            KEY_X = 0x58,
            ///<summary>
            ///Y key
            ///</summary>
            KEY_Y = 0x59,
            ///<summary>
            ///Z key
            ///</summary>
            KEY_Z = 0x5A,
            ///<summary>
            ///Left Windows key (Microsoft Natural keyboard)
            ///</summary>
            LWIN = 0x5B,
            ///<summary>
            ///Right Windows key (Natural keyboard)
            ///</summary>
            RWIN = 0x5C,
            ///<summary>
            ///Applications key (Natural keyboard)
            ///</summary>
            APPS = 0x5D,
            ///<summary>
            ///Computer Sleep key
            ///</summary>
            SLEEP = 0x5F,
            ///<summary>
            ///Numeric keypad 0 key
            ///</summary>
            NUMPAD0 = 0x60,
            ///<summary>
            ///Numeric keypad 1 key
            ///</summary>
            NUMPAD1 = 0x61,
            ///<summary>
            ///Numeric keypad 2 key
            ///</summary>
            NUMPAD2 = 0x62,
            ///<summary>
            ///Numeric keypad 3 key
            ///</summary>
            NUMPAD3 = 0x63,
            ///<summary>
            ///Numeric keypad 4 key
            ///</summary>
            NUMPAD4 = 0x64,
            ///<summary>
            ///Numeric keypad 5 key
            ///</summary>
            NUMPAD5 = 0x65,
            ///<summary>
            ///Numeric keypad 6 key
            ///</summary>
            NUMPAD6 = 0x66,
            ///<summary>
            ///Numeric keypad 7 key
            ///</summary>
            NUMPAD7 = 0x67,
            ///<summary>
            ///Numeric keypad 8 key
            ///</summary>
            NUMPAD8 = 0x68,
            ///<summary>
            ///Numeric keypad 9 key
            ///</summary>
            NUMPAD9 = 0x69,
            ///<summary>
            ///Multiply key
            ///</summary>
            MULTIPLY = 0x6A,
            ///<summary>
            ///Add key
            ///</summary>
            ADD = 0x6B,
            ///<summary>
            ///Separator key
            ///</summary>
            SEPARATOR = 0x6C,
            ///<summary>
            ///Subtract key
            ///</summary>
            SUBTRACT = 0x6D,
            ///<summary>
            ///Decimal key
            ///</summary>
            DECIMAL = 0x6E,
            ///<summary>
            ///Divide key
            ///</summary>
            DIVIDE = 0x6F,
            ///<summary>
            ///F1 key
            ///</summary>
            F1 = 0x70,
            ///<summary>
            ///F2 key
            ///</summary>
            F2 = 0x71,
            ///<summary>
            ///F3 key
            ///</summary>
            F3 = 0x72,
            ///<summary>
            ///F4 key
            ///</summary>
            F4 = 0x73,
            ///<summary>
            ///F5 key
            ///</summary>
            F5 = 0x74,
            ///<summary>
            ///F6 key
            ///</summary>
            F6 = 0x75,
            ///<summary>
            ///F7 key
            ///</summary>
            F7 = 0x76,
            ///<summary>
            ///F8 key
            ///</summary>
            F8 = 0x77,
            ///<summary>
            ///F9 key
            ///</summary>
            F9 = 0x78,
            ///<summary>
            ///F10 key
            ///</summary>
            F10 = 0x79,
            ///<summary>
            ///F11 key
            ///</summary>
            F11 = 0x7A,
            ///<summary>
            ///F12 key
            ///</summary>
            F12 = 0x7B,
            ///<summary>
            ///F13 key
            ///</summary>
            F13 = 0x7C,
            ///<summary>
            ///F14 key
            ///</summary>
            F14 = 0x7D,
            ///<summary>
            ///F15 key
            ///</summary>
            F15 = 0x7E,
            ///<summary>
            ///F16 key
            ///</summary>
            F16 = 0x7F,
            ///<summary>
            ///F17 key  
            ///</summary>
            F17 = 0x80,
            ///<summary>
            ///F18 key  
            ///</summary>
            F18 = 0x81,
            ///<summary>
            ///F19 key  
            ///</summary>
            F19 = 0x82,
            ///<summary>
            ///F20 key  
            ///</summary>
            F20 = 0x83,
            ///<summary>
            ///F21 key  
            ///</summary>
            F21 = 0x84,
            ///<summary>
            ///F22 key, (PPC only) Key used to lock device.
            ///</summary>
            F22 = 0x85,
            ///<summary>
            ///F23 key  
            ///</summary>
            F23 = 0x86,
            ///<summary>
            ///F24 key  
            ///</summary>
            F24 = 0x87,
            ///<summary>
            ///NUM LOCK key
            ///</summary>
            NUMLOCK = 0x90,
            ///<summary>
            ///SCROLL LOCK key
            ///</summary>
            SCROLL = 0x91,
            ///<summary>
            ///Left SHIFT key
            ///</summary>
            LSHIFT = 0xA0,
            ///<summary>
            ///Right SHIFT key
            ///</summary>
            RSHIFT = 0xA1,
            ///<summary>
            ///Left CONTROL key
            ///</summary>
            LCONTROL = 0xA2,
            ///<summary>
            ///Right CONTROL key
            ///</summary>
            RCONTROL = 0xA3,
            ///<summary>
            ///Left MENU key
            ///</summary>
            LMENU = 0xA4,
            ///<summary>
            ///Right MENU key
            ///</summary>
            RMENU = 0xA5,
            ///<summary>
            ///Windows 2000/XP: Browser Back key
            ///</summary>
            BROWSER_BACK = 0xA6,
            ///<summary>
            ///Windows 2000/XP: Browser Forward key
            ///</summary>
            BROWSER_FORWARD = 0xA7,
            ///<summary>
            ///Windows 2000/XP: Browser Refresh key
            ///</summary>
            BROWSER_REFRESH = 0xA8,
            ///<summary>
            ///Windows 2000/XP: Browser Stop key
            ///</summary>
            BROWSER_STOP = 0xA9,
            ///<summary>
            ///Windows 2000/XP: Browser Search key
            ///</summary>
            BROWSER_SEARCH = 0xAA,
            ///<summary>
            ///Windows 2000/XP: Browser Favorites key
            ///</summary>
            BROWSER_FAVORITES = 0xAB,
            ///<summary>
            ///Windows 2000/XP: Browser Start and Home key
            ///</summary>
            BROWSER_HOME = 0xAC,
            ///<summary>
            ///Windows 2000/XP: Volume Mute key
            ///</summary>
            VOLUME_MUTE = 0xAD,
            ///<summary>
            ///Windows 2000/XP: Volume Down key
            ///</summary>
            VOLUME_DOWN = 0xAE,
            ///<summary>
            ///Windows 2000/XP: Volume Up key
            ///</summary>
            VOLUME_UP = 0xAF,
            ///<summary>
            ///Windows 2000/XP: Next Track key
            ///</summary>
            MEDIA_NEXT_TRACK = 0xB0,
            ///<summary>
            ///Windows 2000/XP: Previous Track key
            ///</summary>
            MEDIA_PREV_TRACK = 0xB1,
            ///<summary>
            ///Windows 2000/XP: Stop Media key
            ///</summary>
            MEDIA_STOP = 0xB2,
            ///<summary>
            ///Windows 2000/XP: Play/Pause Media key
            ///</summary>
            MEDIA_PLAY_PAUSE = 0xB3,
            ///<summary>
            ///Windows 2000/XP: Start Mail key
            ///</summary>
            LAUNCH_MAIL = 0xB4,
            ///<summary>
            ///Windows 2000/XP: Select Media key
            ///</summary>
            LAUNCH_MEDIA_SELECT = 0xB5,
            ///<summary>
            ///Windows 2000/XP: Start Application 1 key
            ///</summary>
            LAUNCH_APP1 = 0xB6,
            ///<summary>
            ///Windows 2000/XP: Start Application 2 key
            ///</summary>
            LAUNCH_APP2 = 0xB7,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_1 = 0xBA,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '+' key
            ///</summary>
            OEM_PLUS = 0xBB,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the ',' key
            ///</summary>
            OEM_COMMA = 0xBC,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '-' key
            ///</summary>
            OEM_MINUS = 0xBD,
            ///<summary>
            ///Windows 2000/XP: For any country/region, the '.' key
            ///</summary>
            OEM_PERIOD = 0xBE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_2 = 0xBF,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_3 = 0xC0,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_4 = 0xDB,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_5 = 0xDC,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_6 = 0xDD,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_7 = 0xDE,
            ///<summary>
            ///Used for miscellaneous characters; it can vary by keyboard.
            ///</summary>
            OEM_8 = 0xDF,
            ///<summary>
            ///Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
            ///</summary>
            OEM_102 = 0xE2,
            ///<summary>
            ///Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
            ///</summary>
            PROCESSKEY = 0xE5,
            ///<summary>
            ///Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes.
            ///The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information,
            ///see Remark in KEYBDINPUT, SendInput, WM_KEYDOWN, and WM_KEYUP
            ///</summary>
            PACKET = 0xE7,
            ///<summary>
            ///Attn key
            ///</summary>
            ATTN = 0xF6,
            ///<summary>
            ///CrSel key
            ///</summary>
            CRSEL = 0xF7,
            ///<summary>
            ///ExSel key
            ///</summary>
            EXSEL = 0xF8,
            ///<summary>
            ///Erase EOF key
            ///</summary>
            EREOF = 0xF9,
            ///<summary>
            ///Play key
            ///</summary>
            PLAY = 0xFA,
            ///<summary>
            ///Zoom key
            ///</summary>
            ZOOM = 0xFB,
            ///<summary>
            ///Reserved
            ///</summary>
            NONAME = 0xFC,
            ///<summary>
            ///PA1 key
            ///</summary>
            PA1 = 0xFD,
            ///<summary>
            ///Clear key
            ///</summary>
            OEM_CLEAR = 0xFE
        }
        public enum ScanCodeShort : short
        {
            LBUTTON = 0,
            RBUTTON = 0,
            CANCEL = 70,
            MBUTTON = 0,
            XBUTTON1 = 0,
            XBUTTON2 = 0,
            BACK = 14,
            TAB = 15,
            CLEAR = 76,
            RETURN = 28,
            SHIFT = 42,
            CONTROL = 29,
            MENU = 56,
            PAUSE = 0,
            CAPITAL = 58,
            KANA = 0,
            HANGUL = 0,
            JUNJA = 0,
            FINAL = 0,
            HANJA = 0,
            KANJI = 0,
            ESCAPE = 1,
            CONVERT = 0,
            NONCONVERT = 0,
            ACCEPT = 0,
            MODECHANGE = 0,
            SPACE = 57,
            PRIOR = 73,
            NEXT = 81,
            END = 79,
            HOME = 71,
            LEFT = 75,
            UP = 72,
            RIGHT = 77,
            DOWN = 80,
            SELECT = 0,
            PRINT = 0,
            EXECUTE = 0,
            SNAPSHOT = 84,
            INSERT = 82,
            DELETE = 83,
            HELP = 99,
            KEY_0 = 11,
            KEY_1 = 2,
            KEY_2 = 3,
            KEY_3 = 4,
            KEY_4 = 5,
            KEY_5 = 6,
            KEY_6 = 7,
            KEY_7 = 8,
            KEY_8 = 9,
            KEY_9 = 10,
            KEY_A = 30,
            KEY_B = 48,
            KEY_C = 46,
            KEY_D = 32,
            KEY_E = 18,
            KEY_F = 33,
            KEY_G = 34,
            KEY_H = 35,
            KEY_I = 23,
            KEY_J = 36,
            KEY_K = 37,
            KEY_L = 38,
            KEY_M = 50,
            KEY_N = 49,
            KEY_O = 24,
            KEY_P = 25,
            KEY_Q = 16,
            KEY_R = 19,
            KEY_S = 31,
            KEY_T = 20,
            KEY_U = 22,
            KEY_V = 47,
            KEY_W = 17,
            KEY_X = 45,
            KEY_Y = 21,
            KEY_Z = 44,
            LWIN = 91,
            RWIN = 92,
            APPS = 93,
            SLEEP = 95,
            NUMPAD0 = 82,
            NUMPAD1 = 79,
            NUMPAD2 = 80,
            NUMPAD3 = 81,
            NUMPAD4 = 75,
            NUMPAD5 = 76,
            NUMPAD6 = 77,
            NUMPAD7 = 71,
            NUMPAD8 = 72,
            NUMPAD9 = 73,
            MULTIPLY = 55,
            ADD = 78,
            SEPARATOR = 0,
            SUBTRACT = 74,
            DECIMAL = 83,
            DIVIDE = 53,
            F1 = 59,
            F2 = 60,
            F3 = 61,
            F4 = 62,
            F5 = 63,
            F6 = 64,
            F7 = 65,
            F8 = 66,
            F9 = 67,
            F10 = 68,
            F11 = 87,
            F12 = 88,
            F13 = 100,
            F14 = 101,
            F15 = 102,
            F16 = 103,
            F17 = 104,
            F18 = 105,
            F19 = 106,
            F20 = 107,
            F21 = 108,
            F22 = 109,
            F23 = 110,
            F24 = 118,
            NUMLOCK = 69,
            SCROLL = 70,
            LSHIFT = 42,
            RSHIFT = 54,
            LCONTROL = 29,
            RCONTROL = 29,
            LMENU = 56,
            RMENU = 56,
            BROWSER_BACK = 106,
            BROWSER_FORWARD = 105,
            BROWSER_REFRESH = 103,
            BROWSER_STOP = 104,
            BROWSER_SEARCH = 101,
            BROWSER_FAVORITES = 102,
            BROWSER_HOME = 50,
            VOLUME_MUTE = 32,
            VOLUME_DOWN = 46,
            VOLUME_UP = 48,
            MEDIA_NEXT_TRACK = 25,
            MEDIA_PREV_TRACK = 16,
            MEDIA_STOP = 36,
            MEDIA_PLAY_PAUSE = 34,
            LAUNCH_MAIL = 108,
            LAUNCH_MEDIA_SELECT = 109,
            LAUNCH_APP1 = 107,
            LAUNCH_APP2 = 33,
            OEM_1 = 39,
            OEM_PLUS = 13,
            OEM_COMMA = 51,
            OEM_MINUS = 12,
            OEM_PERIOD = 52,
            OEM_2 = 53,
            OEM_3 = 41,
            OEM_4 = 26,
            OEM_5 = 43,
            OEM_6 = 27,
            OEM_7 = 40,
            OEM_8 = 0,
            OEM_102 = 86,
            PROCESSKEY = 0,
            PACKET = 0,
            ATTN = 0,
            CRSEL = 0,
            EXSEL = 0,
            EREOF = 93,
            PLAY = 0,
            ZOOM = 98,
            NONAME = 0,
            PA1 = 0,
            OEM_CLEAR = 0,
        }
        #endregion
#pragma warning restore 649
    

		
	
	
	
	

	public enum ShowWindowCommands {
		/// <summary>
		/// Hides the window and activates another window.
		/// </summary>
		Hide = 0,

		/// <summary>
		/// Activates and displays a window. If the window is minimized or
		/// maximized, the system restores it to its original size and position.
		/// An application should specify this flag when displaying the window
		/// for the first time.
		/// </summary>
		Normal = 1,

		/// <summary>
		/// Activates the window and displays it as a minimized window.
		/// </summary>
		ShowMinimized = 2,

		/// <summary>
		/// Maximizes the specified window.
		/// </summary>
		Maximize = 3, // is this the right value?

		/// <summary>
		/// Activates the window and displays it as a maximized window.
		/// </summary>      
		ShowMaximized = 3,

		/// <summary>
		/// Displays a window in its most recent size and position. This value
		/// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except
		/// the window is not activated.
		/// </summary>
		ShowNoActivate = 4,

		/// <summary>
		/// Activates the window and displays it in its current size and position.
		/// </summary>
		Show = 5,

		/// <summary>
		/// Minimizes the specified window and activates the next top-level
		/// window in the Z order.
		/// </summary>
		Minimize = 6,

		/// <summary>
		/// Displays the window as a minimized window. This value is similar to
		/// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the
		/// window is not activated.
		/// </summary>
		ShowMinNoActive = 7,

		/// <summary>
		/// Displays the window in its current size and position. This value is
		/// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the
		/// window is not activated.
		/// </summary>
		ShowNA = 8,

		/// <summary>
		/// Activates and displays the window. If the window is minimized or
		/// maximized, the system restores it to its original size and position.
		/// An application should specify this flag when restoring a minimized window.
		/// </summary>
		Restore = 9,

		/// <summary>
		/// Sets the show state based on the SW_* value specified in the
		/// STARTUPINFO structure passed to the CreateProcess function by the
		/// program that started the application.
		/// </summary>
		ShowDefault = 10,

		/// <summary>
		///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread
		/// that owns the window is not responding. This flag should only be
		/// used when minimizing windows from a different thread.
		/// </summary>
		ForceMinimize = 11
	}

	[Flags]
	public enum SetWindowPosFlags : uint {
		// ReSharper disable InconsistentNaming

		/// <summary>
		///     If the calling thread and the thread that owns the window are attached to different input queues, the system posts the request to the thread that owns the window. This prevents the calling thread from blocking its execution while other threads process the request.
		/// </summary>
		SWP_ASYNCWINDOWPOS = 0x4000,

		/// <summary>
		///     Prevents generation of the WM_SYNCPAINT message.
		/// </summary>
		SWP_DEFERERASE = 0x2000,

		/// <summary>
		///     Draws a frame (defined in the window's class description) around the window.
		/// </summary>
		SWP_DRAWFRAME = 0x0020,

		/// <summary>
		///     Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE is sent only when the window's size is being changed.
		/// </summary>
		SWP_FRAMECHANGED = 0x0020,

		/// <summary>
		///     Hides the window.
		/// </summary>
		SWP_HIDEWINDOW = 0x0080,

		/// <summary>
		///     Does not activate the window. If this flag is not set, the window is activated and moved to the top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter parameter).
		/// </summary>
		SWP_NOACTIVATE = 0x0010,

		/// <summary>
		///     Discards the entire contents of the client area. If this flag is not specified, the valid contents of the client area are saved and copied back into the client area after the window is sized or repositioned.
		/// </summary>
		SWP_NOCOPYBITS = 0x0100,

		/// <summary>
		///     Retains the current position (ignores X and Y parameters).
		/// </summary>
		SWP_NOMOVE = 0x0002,

		/// <summary>
		///     Does not change the owner window's position in the Z order.
		/// </summary>
		SWP_NOOWNERZORDER = 0x0200,

		/// <summary>
		///     Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent window uncovered as a result of the window being moved. When this flag is set, the application must explicitly invalidate or redraw any parts of the window and parent window that need redrawing.
		/// </summary>
		SWP_NOREDRAW = 0x0008,

		/// <summary>
		///     Same as the SWP_NOOWNERZORDER flag.
		/// </summary>
		SWP_NOREPOSITION = 0x0200,

		/// <summary>
		///     Prevents the window from receiving the WM_WINDOWPOSCHANGING message.
		/// </summary>
		SWP_NOSENDCHANGING = 0x0400,

		/// <summary>
		///     Retains the current size (ignores the cx and cy parameters).
		/// </summary>
		SWP_NOSIZE = 0x0001,

		/// <summary>
		///     Retains the current Z order (ignores the hWndInsertAfter parameter).
		/// </summary>
		SWP_NOZORDER = 0x0004,

		/// <summary>
		///     Displays the window.
		/// </summary>
		SWP_SHOWWINDOW = 0x0040,

		// ReSharper restore InconsistentNaming
	}

	/// <summary>
	///     Special window handles
	/// </summary>
	public enum SpecialWindowHandles {
		// ReSharper disable InconsistentNaming
		/// <summary>
		///     Places the window at the top of the Z order.
		/// </summary>
		HWND_TOP = 0,

		/// <summary>
		///     Places the window at the bottom of the Z order. If the hWnd parameter identifies a topmost window, the window loses its topmost status and is placed at the bottom of all other windows.
		/// </summary>
		HWND_BOTTOM = 1,

		/// <summary>
		///     Places the window above all non-topmost windows. The window maintains its topmost position even when it is deactivated.
		/// </summary>
		HWND_TOPMOST = -1,

		/// <summary>
		///     Places the window above all non-topmost windows (that is, behind all topmost windows). This flag has no effect if the window is already a non-topmost window.
		/// </summary>
		HWND_NOTOPMOST = -2
		// ReSharper restore InconsistentNaming
	}
	
	[Serializable, StructLayout( LayoutKind.Sequential )]
	public struct NativeRectangle
	{
		public Int32 Left;
		public Int32 Top;
		public Int32 Right;
		public Int32 Bottom;


		public NativeRectangle( Int32 left, Int32 top, Int32 right, Int32 bottom )
		{
			this.Left = left;
			this.Top = top;
			this.Right = right;
			this.Bottom = bottom;
		}
	}


	[StructLayout( LayoutKind.Sequential, CharSet = CharSet.Auto )]
	public sealed class NativeMonitorInfo
	{
		public Int32 Size = Marshal.SizeOf( typeof( NativeMonitorInfo ) );
		public NativeRectangle Monitor;
		public NativeRectangle Work;
		public Int32 Flags;
	}
	
	
	public enum PROCESS_DPI_AWARENESS {
		PROCESS_DPI_UNAWARE = 0,
		PROCESS_SYSTEM_DPI_AWARE = 1,
		PROCESS_PER_MONITOR_DPI_AWARE = 2
	}

	public enum DPI_AWARENESS {
		DPI_AWARENESS_INVALID = -1,
		DPI_AWARENESS_UNAWARE = 0,
		DPI_AWARENESS_SYSTEM_AWARE = 1,
		DPI_AWARENESS_PER_MONITOR_AWARE = 2
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct MINMAXINFO
	{
		public WinNative.POINT ptReserved;
		public WinNative.POINT ptMaxSize;
		public WinNative.POINT ptMaxPosition;
		public WinNative.POINT ptMinTrackSize;
		public WinNative.POINT ptMaxTrackSize;
	}
	
	

	
}