using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TMPro;
using WinStuff;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.Keyboard {

	public class KeyboardHelper {

		[DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);
		
		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern IntPtr GetKeyboardLayout(uint dwLayout);
		
		
		[DllImport("user32.dll")]
		static extern uint MapVirtualKey(uint uCode, uint uMapType);
		
		[DllImport("user32.dll")]
		static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);
		
		[DllImport("user32.dll")]
		static extern bool GetKeyboardState(byte[] lpKeyState);
		
		[DllImport("user32.dll")]
		public static extern int ToUnicode(uint virtualKeyCode, uint scanCode,
		                                   byte[] keyboardState,
		                                   [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
		                                   StringBuilder receivingBuffer,
		                                   int bufferSize, uint flags);
		
		
		[DllImport("user32.dll")]
		private static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);


		private static bool isInit = false;

		private static IntPtr HKL;
		private static Process currentProcess;
		private static IntPtr layout;

		
		public static string GetCharsFromKeys(Keys keys, bool shift, bool altGr)
		{
			var buf = new StringBuilder(256);
			var keyboardState = new byte[256];
			if (shift)
				keyboardState[(int) Keys.ShiftKey] = 0xff;
			if (altGr)
			{
				keyboardState[(int) Keys.ControlKey] = 0xff;
				keyboardState[(int) Keys.Menu] = 0xff;
			}
			ToUnicode((uint) keys, 0, keyboardState, buf, 256, 0);
			return buf.ToString();
		}


		private static void Init() {

			HKL = GetKeyboardLayout((uint)Thread.CurrentThread.ManagedThreadId);
			currentProcess = Process.GetCurrentProcess();
			
			layout = GetKeyboardLayout(GetWindowThreadProcessId(currentProcess.Handle, IntPtr.Zero));

			isInit = true;
		}
		

		public static string GetCharFromLayout(KeysEx keys, bool shift, bool altGr) {
			
			if (!isInit) Init();
			
			System.Text.StringBuilder sbString = new System.Text.StringBuilder(5);
			
			var buf = new StringBuilder(256);
			var keyboardState = new byte[256];
			if (shift)
				keyboardState[(int) KeysEx.VK_SHIFT] = 0xff;
			if (altGr)
			{
				keyboardState[(int) KeysEx.VK_CONTROL] = 0xff;
				keyboardState[(int) KeysEx.VK_MENU] = 0xff;
			}


			ToUnicodeEx((uint)keys, 0, keyboardState, buf, 256, 0, layout);
			return buf.ToString();

		}
		
		
		public static string KeyCodeToUnicode(KeysEx key, bool shift, bool altGr) {
			byte[] keyboardState = new byte[255];
			bool keyboardStateStatus = GetKeyboardState(keyboardState);

			
			if (shift)
				keyboardState[(int) KeysEx.VK_SHIFT] = 0xff;
			if (altGr)
			{
				keyboardState[(int) KeysEx.VK_CONTROL] = 0xff;
				keyboardState[(int) KeysEx.VK_MENU] = 0xff;
			}
			
			if (!keyboardStateStatus)
			{
				return "";
			}

			uint virtualKeyCode = (uint)key;
			IntPtr inputLocaleIdentifier = GetKeyboardLayout(0);
			uint scanCode = MapVirtualKeyEx(virtualKeyCode, 0, inputLocaleIdentifier);

			StringBuilder result = new StringBuilder();
			ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, result, (int)5, (uint)0, inputLocaleIdentifier);

			return result.ToString();
		}
		
		



	}
	
	
	public enum KeysEx : byte {
            None                    = 0x00,
            VK_LBUTTON              = Keys.LButton,             // 0x01
            VK_RBUTTON              = Keys.RButton,             // 0x02
            VK_CANCEL               = Keys.Cancel,              // 0x03
            VK_MBUTTON              = Keys.MButton,             // 0x04
            VK_XBUTTON1             = Keys.XButton1,            // 0x05
            VK_XBUTTON2             = Keys.XButton2,            // 0x06
            /*
             * 0x07 : unassigned
             */
            VK_BACK                 = Keys.Back,                // 0x08
            VK_TAB                  = Keys.Tab,                 // 0x09
            /*
             * 0x0A - 0x0B : reserved
             */
            VK_CLEAR                = Keys.Clear,               // 0x0C
            VK_RETURN               = Keys.Return,              // 0x0D, Keys.Enter
            VK_SHIFT                = Keys.ShiftKey,            // 0x10
            VK_CONTROL              = Keys.ControlKey,          // 0x11
            VK_MENU                 = Keys.Menu,                // 0x12
            VK_PAUSE                = Keys.Pause,               // 0x13
            VK_CAPITAL              = Keys.Capital,             // 0x14, Keys.CapsLock
            VK_KANA                 = Keys.KanaMode,            // 0x15
            VK_HANGEUL              = Keys.HanguelMode,         // 0x15, Keys.HangulMode
            VK_JUNJA                = Keys.JunjaMode,           // 0x17
            VK_FINAL                = Keys.FinalMode,           // 0x18
            VK_HANJA                = Keys.HanjaMode,           // 0x19
            VK_KANJI                = Keys.KanjiMode,           // 0x19
            VK_ESCAPE               = Keys.Escape,              // 0x1B
            VK_CONVERT              = Keys.IMEConvert,          // 0x1C
            VK_NONCONVERT           = Keys.IMENonconvert,       // 0x1D
            VK_ACCEPT               = Keys.IMEAceept,           // 0x1E, Keys.IMEAccept
            VK_MODECHANGE           = Keys.IMEModeChange,       // 0x1F
            VK_SPACE                = Keys.Space,               // 0x20
            VK_PRIOR                = Keys.Prior,               // 0x21, Keys.PageUp
            VK_NEXT                 = Keys.Next,                // 0x22, Keys.PageDown
            VK_END                  = Keys.End,                 // 0x23
            VK_HOME                 = Keys.Home,                // 0x24
            VK_LEFT                 = Keys.Left,                // 0x25
            VK_UP                   = Keys.Up,                  // 0x26
            VK_RIGHT                = Keys.Right,               // 0x27
            VK_DOWN                 = Keys.Down,                // 0x28
            VK_SELECT               = Keys.Select,              // 0x29
            VK_PRINT                = Keys.Print,               // 0x2A
            VK_EXECUTE              = Keys.Execute,             // 0x2B
            VK_SNAPSHOT             = Keys.Snapshot,            // 0x2C, Keys.PrintScreen
            VK_INSERT               = Keys.Insert,              // 0x2D
            VK_DELETE               = Keys.Delete,              // 0x2E
            VK_HELP                 = Keys.Help,                // 0x2F
            VK_0                    = Keys.D0,                  // 0x30
            VK_1                    = Keys.D1,                  // 0x31
            VK_2                    = Keys.D2,                  // 0x32
            VK_3                    = Keys.D3,                  // 0x33
            VK_4                    = Keys.D4,                  // 0x34
            VK_5                    = Keys.D5,                  // 0x35
            VK_6                    = Keys.D6,                  // 0x36
            VK_7                    = Keys.D7,                  // 0x37
            VK_8                    = Keys.D8,                  // 0x38
            VK_9                    = Keys.D9,                  // 0x39
            /*
             * 0x40 : unassigned
             */
            VK_A                    = Keys.A,                   // 0x41
            VK_B                    = Keys.B,                   // 0x42
            VK_C                    = Keys.C,                   // 0x43
            VK_D                    = Keys.D,                   // 0x44
            VK_E                    = Keys.E,                   // 0x45
            VK_F                    = Keys.F,                   // 0x46
            VK_G                    = Keys.G,                   // 0x47
            VK_H                    = Keys.H,                   // 0x48
            VK_I                    = Keys.I,                   // 0x49
            VK_J                    = Keys.J,                   // 0x4A
            VK_K                    = Keys.K,                   // 0x4B
            VK_L                    = Keys.L,                   // 0x4C
            VK_M                    = Keys.M,                   // 0x4D
            VK_N                    = Keys.N,                   // 0x4E
            VK_O                    = Keys.O,                   // 0x4F
            VK_P                    = Keys.P,                   // 0x50
            VK_Q                    = Keys.Q,                   // 0x51
            VK_R                    = Keys.R,                   // 0x52
            VK_S                    = Keys.S,                   // 0x53
            VK_T                    = Keys.T,                   // 0x54
            VK_U                    = Keys.U,                   // 0x55
            VK_V                    = Keys.V,                   // 0x56
            VK_W                    = Keys.W,                   // 0x57
            VK_X                    = Keys.X,                   // 0x58
            VK_Y                    = Keys.Y,                   // 0x59
            VK_Z                    = Keys.Z,                   // 0x5A
            VK_LWIN                 = Keys.LWin,                // 0x5B
            VK_RWIN                 = Keys.RWin,                // 0x5C
            VK_APPS                 = Keys.Apps,                // 0x5D
            /*
             * 0x5E : reserved
             */
            VK_SLEEP                = 0x5f,                     // 0x5f, Keys.Sleep
            VK_NUMPAD0              = Keys.NumPad0,             // 0x60
            VK_NUMPAD1              = Keys.NumPad1,             // 0x61
            VK_NUMPAD2              = Keys.NumPad2,             // 0x62
            VK_NUMPAD3              = Keys.NumPad3,             // 0x63
            VK_NUMPAD4              = Keys.NumPad4,             // 0x64
            VK_NUMPAD5              = Keys.NumPad5,             // 0x65
            VK_NUMPAD6              = Keys.NumPad6,             // 0x66
            VK_NUMPAD7              = Keys.NumPad7,             // 0x67
            VK_NUMPAD8              = Keys.NumPad8,             // 0x68
            VK_NUMPAD9              = Keys.NumPad9,             // 0x69
            VK_MULTIPLY             = Keys.Multiply,            // 0x6A
            VK_ADD                  = Keys.Add,                 // 0x6B
            VK_SEPARATOR            = Keys.Separator,           // 0x6C
            VK_SUBTRACT             = Keys.Subtract,            // 0x6D
            VK_DECIMAL              = Keys.Decimal,             // 0x6E
            VK_DIVIDE               = Keys.Divide,              // 0x6F
            VK_F1                   = Keys.F1,                  // 0x70
            VK_F2                   = Keys.F2,                  // 0x71
            VK_F3                   = Keys.F3,                  // 0x72
            VK_F4                   = Keys.F4,                  // 0x73
            VK_F5                   = Keys.F5,                  // 0x74
            VK_F6                   = Keys.F6,                  // 0x75
            VK_F7                   = Keys.F7,                  // 0x76
            VK_F8                   = Keys.F8,                  // 0x77
            VK_F9                   = Keys.F9,                  // 0x78
            VK_F10                  = Keys.F10,                 // 0x79
            VK_F11                  = Keys.F11,                 // 0x7A
            VK_F12                  = Keys.F12,                 // 0x7B
            VK_F13                  = Keys.F13,                 // 0x7C
            VK_F14                  = Keys.F14,                 // 0x7D
            VK_F15                  = Keys.F15,                 // 0x7E
            VK_F16                  = Keys.F16,                 // 0x7F
            VK_F17                  = Keys.F17,                 // 0x80
            VK_F18                  = Keys.F18,                 // 0x81
            VK_F19                  = Keys.F19,                 // 0x82
            VK_F20                  = Keys.F20,                 // 0x83
            VK_F21                  = Keys.F21,                 // 0x84
            VK_F22                  = Keys.F22,                 // 0x85
            VK_F23                  = Keys.F23,                 // 0x86
            VK_F24                  = Keys.F24,                 // 0x87
            /*
             * 0x88 - 0x8F : unassigned
             */
            VK_NUMLOCK              = Keys.NumLock,             // 0x90
            VK_SCROLL               = Keys.Scroll,              // 0x91
            VK_OEM_NEC_EQUAL        = 0x92,                     // 0x92, NEC PC-9800 kbd definition
            VK_OEM_FJ_JISHO         = 0x92,                     // 0x92, Fujitsu/OASYS kbd definition
            VK_OEM_FJ_MASSHOU       = 0x93,                     // 0x93, Fujitsu/OASYS kbd definition
            VK_OEM_FJ_TOUROKU       = 0x94,                     // 0x94, Fujitsu/OASYS kbd definition
            VK_OEM_FJ_LOYA          = 0x95,                     // 0x95, Fujitsu/OASYS kbd definition
            VK_OEM_FJ_ROYA          = 0x96,                     // 0x96, Fujitsu/OASYS kbd definition
            /*
             * 0x97 - 0x9F : unassigned
             */
            VK_LSHIFT               = Keys.LShiftKey,           // 0xA0
            VK_RSHIFT               = Keys.RShiftKey,           // 0xA1
            VK_LCONTROL             = Keys.LControlKey,         // 0xA2
            VK_RCONTROL             = Keys.RControlKey,         // 0xA3
            VK_LMENU                = Keys.LMenu,               // 0xA4
            VK_RMENU                = Keys.RMenu,               // 0xA5
            VK_BROWSER_BACK         = Keys.BrowserBack,         // 0xA6
            VK_BROWSER_FORWARD      = Keys.BrowserForward,      // 0xA7
            VK_BROWSER_REFRESH      = Keys.BrowserRefresh,      // 0xA8
            VK_BROWSER_STOP         = Keys.BrowserStop,         // 0xA9
            VK_BROWSER_SEARCH       = Keys.BrowserSearch,       // 0xAA
            VK_BROWSER_FAVORITES    = Keys.BrowserFavorites,    // 0xAB
            VK_BROWSER_HOME         = Keys.BrowserHome,         // 0xAC
            VK_VOLUME_MUTE          = Keys.VolumeMute,          // 0xAD
            VK_VOLUME_DOWN          = Keys.VolumeDown,          // 0xAE
            VK_VOLUME_UP            = Keys.VolumeUp,            // 0xAF
            VK_MEDIA_NEXT_TRACK     = Keys.MediaNextTrack,      // 0xB0
            VK_MEDIA_PREV_TRACK     = Keys.MediaPreviousTrack,  // 0xB1
            VK_MEDIA_STOP           = Keys.MediaStop,           // 0xB2
            VK_MEDIA_PLAY_PAUSE     = Keys.MediaPlayPause,      // 0xB3
            VK_LAUNCH_MAIL          = Keys.LaunchMail,          // 0xB4
            VK_LAUNCH_MEDIA_SELECT  = Keys.SelectMedia,         // 0xB5
            VK_LAUNCH_APP1          = Keys.LaunchApplication1,  // 0xB6
            VK_LAUNCH_APP2          = Keys.LaunchApplication2,  // 0xB7
            /*
             * 0xB8 - 0xB9 : reserved
             */
            VK_OEM_1                = Keys.OemSemicolon,        // 0xBA, Keys.Oem1
            VK_OEM_PLUS             = Keys.Oemplus,             // 0xBB
            VK_OEM_COMMA            = Keys.Oemcomma,            // 0xBC
            VK_OEM_MINUS            = Keys.OemMinus,            // 0xBD
            VK_OEM_PERIOD           = Keys.OemPeriod,           // 0xBE
            VK_OEM_2                = Keys.OemQuestion,         // 0xBF, Keys.Oem2
            VK_OEM_3                = Keys.Oemtilde,            // 0xC0, Keys.Oem3
            /*
             * 0xC1 - 0xD7 : reserved
             */
            /*
             * 0xD8 - 0xDA : unassigned
             */
            VK_OEM_4                = Keys.OemOpenBrackets,     // 0xDB, Keys.Oem4
            VK_OEM_5                = Keys.OemPipe,             // 0xDC, Keys.Oem5
            VK_OEM_6                = Keys.OemCloseBrackets,    // 0xDD, Keys.Oem6
            VK_OEM_7                = Keys.OemQuotes,           // 0xDE, Keys.Oem7
            VK_OEM_8                = Keys.Oem8,                // 0xDF
            /*
             * 0xE0 : reserved
             */
            VK_OEM_AX               = 0xE1,                     // 0xE1, 'AX' key on Japanese AX kbd
            VK_OEM_102              = Keys.OemBackslash,        // 0xE2, Keys.Oem102
            VK_ICO_HELP             = 0xE3,                     // 0xE3, Help key on ICO
            VK_ICO_00               = 0xE4,                     // 0xE4, 00 key on ICO
            VK_PROCESSKEY           = Keys.ProcessKey,          // 0xE5
            VK_ICO_CLEAR            = 0xE6,                     // 0xE6
            VK_PACKET               = 0xE7,                     // 0xE7, Keys.Packet
            /*
             * 0xE8 : unassigned
             */
            VK_OEM_RESET            = 0xE9,                     // 0xE9, Nokia/Ericsson definition
            VK_OEM_JUMP             = 0xEA,                     // 0xEA, Nokia/Ericsson definition
            VK_OEM_PA1              = 0xEB,                     // 0xEB, Nokia/Ericsson definition
            VK_OEM_PA2              = 0xEC,                     // 0xEC, Nokia/Ericsson definition
            VK_OEM_PA3              = 0xED,                     // 0xED, Nokia/Ericsson definition
            VK_OEM_WSCTRL           = 0xEE,                     // 0xEE, Nokia/Ericsson definition
            VK_OEM_CUSEL            = 0xEF,                     // 0xEF, Nokia/Ericsson definition
            VK_OEM_ATTN             = 0xF0,                     // 0xF0, Nokia/Ericsson definition
            VK_OEM_FINISH           = 0xF1,                     // 0xF1, Nokia/Ericsson definition
            VK_OEM_COPY             = 0xF2,                     // 0xF2, Nokia/Ericsson definition
            VK_OEM_AUTO             = 0xF3,                     // 0xF3, Nokia/Ericsson definition
            VK_OEM_ENLW             = 0xF4,                     // 0xF4, Nokia/Ericsson definition
            VK_OEM_BACKTAB          = 0xF5,                     // 0xF5, Nokia/Ericsson definition
            VK_ATTN                 = Keys.Attn,                // 0xF6
            VK_CRSEL                = Keys.Crsel,               // 0xF7
            VK_EXSEL                = Keys.Exsel,               // 0xF8
            VK_EREOF                = Keys.EraseEof,            // 0xF9
            VK_PLAY                 = Keys.Play,                // 0xFA
            VK_ZOOM                 = Keys.Zoom,                // 0xFB
            VK_NONAME               = Keys.NoName,              // 0xFC
            VK_PA1                  = Keys.Pa1,                 // 0xFD
            VK_OEM_CLEAR            = Keys.OemClear,            // 0xFE
            /*
             * 0xFF : reserved
             */
        }
}