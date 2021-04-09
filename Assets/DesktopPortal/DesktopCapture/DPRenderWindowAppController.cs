using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DPCore.Apps;
using UnityEngine;

public class DPRenderWindowAppController : DPApp {
	
	
	
	private DPDesktopOverlay windowDP;


	protected override void Awake() {
		base.Awake();

		windowDP = GetComponentInChildren<DPDesktopOverlay>();


	}

	public override void OnMinimize() {
		base.OnMinimize();
		
		//windowDP.ClearTextures();
		
	}


}
