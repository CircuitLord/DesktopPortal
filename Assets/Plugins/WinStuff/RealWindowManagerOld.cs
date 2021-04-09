using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using UnityEngine.EventSystems;
//using Valve.Newtonsoft.Json.Utilities;
using Debug = UnityEngine.Debug;

namespace WinStuff {
	public static class RealWindowManagerOld
	{
	
		//public static Dictionary<string, IntPtr> realWindows = new Dictionary<string, IntPtr>();
	
		public static List<IntPtr> windowPtrs = new List<IntPtr>();


		public static List<Process> activeProcesses = new List<Process>();




		/*
		public static void Test(UwcWindow window) {

			//Screen currentScreen = Screen.FromHandle(window.handle);


			var monitor = WinNative.MonitorFromWindow(window.handle, WinNative.MONITOR_DEFAULTTONEAREST);

			if (monitor != IntPtr.Zero) {
				
				var monitorInfo = new NativeMonitorInfo();
				//Win32Stuff.GetMonitorInfo( monitor, monitorInfo );
				
				

				var left = monitorInfo.Work.Left;
				var top = monitorInfo.Work.Top;
				var width = ( monitorInfo.Work.Right - monitorInfo.Work.Left );
				var height = ( monitorInfo.Work.Bottom - monitorInfo.Work.Top );
				

				Debug.Log(left);
				Debug.Log(top);
				
				Debug.Log(width);
				Debug.Log(height);
			}
			
			



		}
		*/


		public static void RefreshWindowList() {
			var windows = WinNative.FindWindowsWithSize();

			var processes = Process.GetProcesses();

			foreach (Process p in processes) {

				if (p.Handle == IntPtr.Zero || p.ProcessName == "explorer") continue;

				activeProcesses.Add(p);
			

			}

			return;
		

			windowPtrs.Clear();
			
			var count = 0;
			foreach (var w in windows) {
			
				windowPtrs.Add(w);
			
				//Win32Stuff.WINDOWINFO winInfo = new Win32Stuff.WINDOWINFO();

				//bool success = Win32Stuff.GetWindowInfo(w, ref winInfo);
			
			
			
				//if (title.Length <= 0) continue;
				//var copy = 0;
				//var found = false;
				//while (!found)
				//{
				//	try
				//	{
				//		realWindows.Add(copy == 0 ? title : string.Format("{0} ({1})", title, copy), w);
//
				//		found = true;
				//	}
				//	catch (ArgumentException)
				//	{
				//		copy++;
				//	}
				//}

				//_titles.Add(copy == 0 ? title : string.Format("{0} ({1})", title, copy));
				//Debug.Log(String.Format("{0}", success));
				count++;
			}
		
			Debug.Log("Found " + count + " windows");
		
		
		}
	
	

	}
}
