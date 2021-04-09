using System.Collections;
using System.Collections.Generic;
using uDesktopDuplication;
using UnityEngine;
using UnityEngine.UI;
using uWindowCapture;


namespace DesktopPortal.DesktopCapture {
    public class DPMonitor : MonoBehaviour {

	    [HideInInspector] public UDDMonitor monitor;
	    
	    public MeshRenderer quad;
	    
	    public Camera camera;

	    public RawImage bg;

	    public UwcWindow targetWindow;

	    public bool isTargetingWindow => targetWindow != null;

	    public Canvas canvas;

	    public bool isSwitchingWindow = false;


	    /// <summary>
	    /// Refreshes the capture tiling and window offset
	    /// </summary>
	    public void Refresh() {

		    if (!isTargetingWindow) return;
		    
		    isSwitchingWindow = true;

		    //Debug.Log(targetWindow.weirdOffset);
		    int height = targetWindow.rawHeight - (int)targetWindow.weirdOffset;
		    int width = targetWindow.rawWidth - (int)targetWindow.weirdOffset * 2;

		    float tilingX = (float)width/ (float)monitor.width;
		    float tilingY = (float)height / (float)monitor.height;
			
		    bg.material.mainTextureScale = new Vector2(tilingX, tilingY);
			

		    float offsetX = ((float)targetWindow.x - monitor.left) / (float)monitor.width;
		    float offsetY = (monitor.height - (targetWindow.y - monitor.top)) / (float)monitor.height;
			
		    bg.material.mainTextureOffset = new Vector2(offsetX, offsetY);
		    
		    
		    
		    StartCoroutine(SwitchingWindowTimer());
		    
	    }

	    private IEnumerator SwitchingWindowTimer() {
		    yield return new WaitForSeconds(0.1f);
		    monitor.Render();
		    isSwitchingWindow = false;
	    }

    }
}