using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using uDesktopDuplication;
using UnityEngine;
using uWindowCapture;
using WinStuff;
using Lib = uDesktopDuplication.Lib;


namespace DesktopPortal.DesktopCapture {
	public class DPMonitorCaptureManager : MonoBehaviour {


		public static DPMonitorCaptureManager I;

		//[SerializeField] private GameObject monitorCapturePF;

		//[SerializeField] private Shader shaderToUse;


		//[HideInInspector] public List<DPMonitor> dpMonitors = new List<DPMonitor>();
		
		private List<UDDMonitor> queuedMonitorsToRender = new List<UDDMonitor>();


		private void Awake() {
			I = this;
		}

		private void Start() {
			StartCoroutine(HandleRendering());
		}

		private float lastTime;
		
		public IEnumerator HandleRendering() {

			while (true) {

				while (!DPSettings.isLoaded || !DPSettings.config.useDDA) yield return null;

				//Find which monitors need to be rendered:
				foreach (DPDesktopOverlay desktopDP in DPDesktopOverlay.overlays) {
					
					//Fallback if we have DDA disabled
					if (!DPSettings.config.useDDA) break;

					if (!desktopDP.overlay.shouldRender) continue;
					
					//Use window capture if requested
					/*if (!desktopDP.isPrimary && desktopDP.isTargetingWindow && DPSettings.config.focusGameWhenNotInteracting && !desktopDP.isBeingInteracted) {
						continue;
					}*/
					if (desktopDP.isTargetingWindow && !desktopDP.isPrimary && DPSettings.config.focusGameWhenNotInteracting) continue;

					if (desktopDP.isTargetingWindow && DPSettings.config.focusGameWhenNotInteracting && !desktopDP.isBeingInteracted) continue;

					if (desktopDP.isPrimary || !desktopDP.isTargetingWindow) {
						if (!queuedMonitorsToRender.Contains(desktopDP.monitor)) {
							queuedMonitorsToRender.Add(desktopDP.monitor);
							break;
						}
					}

				}

				while (Input.GetKeyDown(KeyCode.G)) {
					yield return null;
				}

				foreach (UDDMonitor monitor in queuedMonitorsToRender) {

					if (monitor == null) continue;
					
					monitor.Render();
					
					//yield return null;

					//Update the textures on the needed overlays
					foreach (DPDesktopOverlay desktopDP in DPDesktopOverlay.overlays) {

						if (desktopDP.isLocked) continue;
						
						//Skip setting the texture if it's a non-primary desktop window (which means it should be using win-capture)
						if (!desktopDP.isPrimary && desktopDP.isTargetingWindow) continue;
						
						//Use window capture if requested
						/*if (!desktopDP.isPrimary && desktopDP.isTargetingWindow && DPSettings.config.focusGameWhenNotInteracting && !desktopDP.isBeingInteracted) {
							continue;
						}*/

						if (desktopDP.isTargetingWindow && !desktopDP.isPrimary && DPSettings.config.focusGameWhenNotInteracting) continue;
						
						if (desktopDP.isTargetingWindow && DPSettings.config.focusGameWhenNotInteracting && !desktopDP.isBeingInteracted) continue;

						if (/*desktopDP.queuedToRenderMonitor &&*/desktopDP.monitor == monitor) {
							desktopDP.SetOverlayTexture(monitor.texture);
							desktopDP.UpdateOverlayUVs();
							desktopDP.queuedToRenderMonitor = false;
						}
					}
				}



				//Wait before looping again:
				// return new WaitForSeconds(1f / 90f);
				yield return null;
				//yield return null;

			}
        
        
        
		}


		/*public DPMonitor TargetWindow(UDDMonitor monitor, UwcWindow window, RenderTexture rt) {
		
			//IntPtr monitorHandle = WinNative.MonitorFromWindow(window.handle, WinNative.MONITOR_DEFAULTTONEAREST);
			//UDDMonitor monitor = UDDManager.GetMonitor(monitorHandle);
			
			DPMonitor dpMonitor = FindDPMonitor(monitor);

			dpMonitor.targetWindow = window;
			
			dpMonitor.camera.targetTexture = rt;

			//StartCoroutine(DelayRefresh(dpMonitor));
			dpMonitor.Refresh();

			return dpMonitor;


		}*/
		

	

		/*private void Setup() {

			dpMonitors.Clear();
			

			foreach (UDDMonitor monitor in UDDManager.monitors) {

				DPMonitor dpMonitor = Instantiate(monitorCapturePF, transform).GetComponent<DPMonitor>();

				dpMonitor.monitor = monitor;
				
				dpMonitor.bg.material = new Material(shaderToUse);

				//dpMonitor.quad.material.mainTexture = monitor.texture;

				dpMonitor.bg.texture = monitor.texture;
				monitor.Render();



				dpMonitors.Add(dpMonitor);



			}
		
		
		
		}*/

		/*
		private DPMonitor FindDPMonitor(UDDMonitor monitor) {

			foreach (DPMonitor dpMonitor in dpMonitors) {
				if (dpMonitor.monitor == monitor) return dpMonitor;
			}

			return null;
		}
		*/
		
	
	
	
	}
}