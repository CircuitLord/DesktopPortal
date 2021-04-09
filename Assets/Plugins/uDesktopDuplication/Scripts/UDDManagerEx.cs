using UnityEngine;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using uWindowCapture;
using WinStuff;


namespace uDesktopDuplication {
	public partial class UDDManager : MonoBehaviour {
		public static UDDMonitor GetMonitor(UwcWindow window) {


			if (window.isIconic) {
				WinNative.ShowWindow(window.handle, ShowWindowCommands.ShowMaximized);
			}
			
			RECT rect = new RECT();

			WinNative.GetWindowRect(window.handle, ref rect);

			UDDMonitor closestMonitor;

			int midX = 0, midY = 0;

			midX = window.rawX + (int) (window.rawWidth / 2f);
			midY = window.rawY + (int) (window.rawHeight / 2f);

			foreach (UDDMonitor monitor in monitors) {

				if (monitor.left > midX) continue;
				if (monitor.top > midY) continue;

				if (monitor.right <= midX) continue;
				if (monitor.bottom <= midY) continue;

				closestMonitor = monitor;
				
				return closestMonitor;
				break;

			}
			
			//else, it failed, so return the biggest monitor:

			UDDMonitor biggestMonitor = null;
			
			
			foreach (UDDMonitor monitor in monitors) {

				if (biggestMonitor == null) biggestMonitor = monitor;

				if (monitor.width > biggestMonitor.width && monitor.height > biggestMonitor.height) biggestMonitor = monitor;

			}
			
			return biggestMonitor;
			
		}
	}
}