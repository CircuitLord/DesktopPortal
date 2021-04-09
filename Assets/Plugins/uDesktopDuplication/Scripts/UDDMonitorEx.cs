using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using DPCore;
using UnityEngine;
using WinStuff;

namespace uDesktopDuplication {
	public partial class UDDMonitor {
		    

		private List<DPOverlayBase> dependingDPs = new List<DPOverlayBase>();
		public void AddDependentDP(DPOverlayBase dpBase) {
			if (dependingDPs.Contains(dpBase)) return;
        
			dependingDPs.Add(dpBase);
		}

		public void RemoveDependentDP(DPOverlayBase dpBase) {
			if (!dependingDPs.Contains(dpBase)) return;
        
			dependingDPs.Remove(dpBase);
		}
    

		private bool isBeingUsed = false;

		public int fpsToCaptureAt = 60;

		private float fpsAsMS => fpsToCaptureAt / 1f;

    

		public IntPtr handle;


		public RECT monitorArea;


		private bool foundWorkingArea = false;


		private RECT _workingArea;
		public RECT workingArea {
			get {

				if (foundWorkingArea) return _workingArea;
            
            
				WinNative.MonitorInfoEx info = new WinNative.MonitorInfoEx();
				info.cbSize = (int) Marshal.SizeOf(info);

				WinNative.GetMonitorInfo(handle, ref info);

				_workingArea = info.rcWork;

				foundWorkingArea = true;
            
				return _workingArea;
			}

        
		}

	}
}