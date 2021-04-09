using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CUI;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DesktopPortal.UI;
using DG.Tweening;
using DPCore;
using DPCore.Apps;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Valve.VR;
using WinStuff;
using Application = UnityEngine.Application;
using Cursor = UnityEngine.Cursor;
using Debug = UnityEngine.Debug;

public class StartupScreenApp : DPApp {

	public static StartupScreenApp I;
	

	[SerializeField] private TextMeshProUGUI version;
	
	[SerializeField] private RectTransform canvas;
	[SerializeField] private Camera camera;
	[SerializeField] private GameObject eventSystem;


	[SerializeField] private CUIGroup firstGroup;



	//[SerializeField] private DPCameraOverlay dpMain;



	private bool isFirstBarOpen = true;

	//public static bool realMouseMoving = false;

	/*private float timeWithoutVirtualInteract = 0f;

	private float timeWithoutRealInteract = 0f;

	private WinNative.POINT prevMousePos = new WinNative.POINT();*/


	protected override void Awake() {
		base.Awake();

		I = this;

	}


	private void Start() {
		DPSettings.OnLoad((() => {
			if (DPSettings.config.isFirstTime) {
				DPSettings.config.isFirstTime = false;
				CUIManager.Animate(firstGroup, true);
			}
		}));

		version.SetText("ver." + Application.version + "");
		
		SteamVRManager.I.onHeadsetStateChanged.AddListener(OnHeadsetStateChanged);


		/*
		dpMain.overlay.visibilityUpdatedEvent += b => {
			CalculateShouldBeActive();
		};
		*/


		StartCoroutine(LoopCheckShouldBeActive());

	}


	private IEnumerator LoopCheckShouldBeActive() {

		while (true) {
			yield return new WaitForSeconds(3f);
		
			CalculateShouldBeActive();
		}
		
	}
	

	private void Update() {

		/*
		if (DPDesktopOverlay.isInteractingVirtually) {
			timeWithoutVirtualInteract = 0f;
			realMouseMoving = false;
			OVROverlay.pauseAllTexturePtrs = false;
		}
		else {
			timeWithoutVirtualInteract += Time.deltaTime;
		}
		*/
		
		//CheckIfRealMouseMoving();
		
		
	}

	/*private void CheckIfRealMouseMoving() {

		if (timeWithoutVirtualInteract < 0.5f) return;

		
		WinNative.POINT newMousePos = new WinNative.POINT();
		WinNative.GetCursorPos(ref newMousePos);
		
		//If the pos changed
		if (prevMousePos.x != newMousePos.x || prevMousePos.y != newMousePos.y) {
			realMouseMoving = true;
			OVROverlay.pauseAllTexturePtrs = true;
			
			timeWithoutRealInteract = 0f;
			//Debug.Log("real mouse moving");
		}
		else {
			timeWithoutRealInteract += Time.deltaTime;
		}
		
		
		if (timeWithoutRealInteract > 1f) {
			OVROverlay.pauseAllTexturePtrs = false;
			realMouseMoving = false;
		}


		prevMousePos = newMousePos;



	}*/
	
	


	/*public override void OnTheBarToggled(bool open) {
		base.OnTheBarToggled(open);
	


	if (isFirstBarOpen) {

			isFirstBarOpen = false;
			//TheBarManager.I.LaunchAppToMainSnap(appKey);
			//CalculateShouldBeActive();
		}
		
		
		
	}*/


	public override void OnVisibilityChange(bool visible) {
		base.OnVisibilityChange(visible);
		
		CalculateShouldBeActive();
		
	}




	private void SwapCameraRTOn(bool useRT) {

		//For VR
		if (useRT) {
			camera.enabled = false;
			//canvas.gameObject.SetActive(true);
			//canvas.gameObject.SetActive(true);
		}


		//For Desktop
		else {
			camera.targetTexture = null;
			camera.enabled = true;
			
			//canvas.gameObject.SetActive(true);
		}
		
	}




	/*
	public void Disable() {

		camera.gameObject.SetActive(false);
		canvas.gameObject.SetActive(false);
		eventSystem.SetActive(false);
		

	}
	*/


	public void Exit() {
		Application.Quit();
	}



	

	public void OnHeadsetStateChanged(bool wearing) {

		//CalculateShouldBeActive();


		
	}


	private void CalculateShouldBeActive() {

		bool active = false;

		if (isVisible) {
			active = true;
			SwapCameraRTOn(true);
			
		}
		
		else if (!SteamVRManager.isWearingHeadset) {
			active = true;
			SwapCameraRTOn(false);
		}

		
		camera.gameObject.SetActive(active);
		canvas.gameObject.SetActive(active);
		eventSystem.gameObject.SetActive(active);

	}
	
	
}
