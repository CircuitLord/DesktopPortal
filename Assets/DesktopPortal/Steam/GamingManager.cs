using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using CUI;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DesktopPortal.Steam;
using DesktopPortal.UI;
using DG.Tweening;
using DPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using WinStuff;
using Debug = UnityEngine.Debug;

public class GamingManager : MonoBehaviour {


	public static GamingManager I;


	[SerializeField] private Texture2D fallbackTexture;
	
	
	[SerializeField] private GameObject avatarGO;
	[SerializeField] private GameObject closeButtonGO;
	
	
	[SerializeField] private RawImage theBarCurrentGameArt;
	[SerializeField] private Texture2D fallbackHeaderArt;
	
	[SerializeField] private TextMeshProUGUI theBarStatusText;
	


	[HideInInspector] public bool gameIsRunning = false;




	[HideInInspector] public LibraryGameData activeGame;
	[HideInInspector] public static uint activePID = 0;
	[HideInInspector] public static Process activeProcess;



	private bool isLaunchingGame = true;


	private Coroutine loadGameHeaderArt_C;
	

	private void Awake() {
		I = this;
	}

	private void Start() {
		
		//Fade out the BG
		//theBarStatusBlurBG.DOFade(0f, 0f);

		StartCoroutine(CheckForActiveGame());

	}



	private IEnumerator CheckForActiveGame() {

		while (true) {
			
			while (!SteamVRManager.isConnected) yield return null;

			CompareCurrentSceneToDP();

			yield return new WaitForSeconds(1);
		}

	}

	
	private void CompareCurrentSceneToDP() {

		if (OpenVR.Applications == null) return;
		
		uint newPID = OpenVR.Applications.GetCurrentSceneProcessId();

		if (newPID == activePID) return;

		activePID = newPID;
		
		//if (isLaunchingGame) {
		//	isLaunchingGame = false;
		//	return;
		//}

		if (activePID == 0) {
			
			activeGame = null;
			gameIsRunning = false;
			activeProcess = null;
			
			//TODO: Fix returning focus:
			
			//we can switch back to the DDA if there's no game open that needs focus
			if (DPSettings.config.focusGameWhenNotInteracting && DPDesktopOverlay.primaryWindowDP != null && DPDesktopOverlay.primaryWindowDP.isTargetingWindow) {
				//DPRenderWindowOverlay.primaryWindowDP.ToggleWindowMonitorCapture(true);
			}
			
			UpdateVisuals();
			
			return;
		}

		gameIsRunning = true;



		if (DPSettings.config.focusGameWhenNotInteracting) {
			if (DPDesktopOverlay.primaryWindowDP != null && DPDesktopOverlay.primaryWindowDP.isTargetingWindow) {
				//DPRenderWindowOverlay.primaryWindowDP.ToggleWindowMonitorCapture(false);
				FocusActiveGame();
			}
		}

		else {
			StartCoroutine(ForcePrimaryOnTop());
		}
		

		
		activeProcess = Process.GetProcessById((int) activePID);
		
		StringBuilder key = new StringBuilder((int) OpenVR.k_unMaxApplicationKeyLength);
		OpenVR.Applications.GetApplicationKeyByProcessId(activePID, key, OpenVR.k_unMaxApplicationKeyLength);

		LibraryGameData gameData = LibraryApp.I.config.opts.games.Find(x => x.appKey == key.ToString());

		if (gameData == null) return;

		activeGame = gameData;
		UpdateVisuals();


	}

	private IEnumerator ForcePrimaryOnTop() {
		yield return new WaitForSeconds(1f);

		float i = 0;
		while (i < 10) {
			
			if (DPDesktopOverlay.primaryWindowDP != null && DPDesktopOverlay.primaryWindowDP.isTargetingWindow) {

				/*if (DPDesktopOverlay.primaryWindowDP.window.isIconic) {
					WinNative.ShowWindow(DPDesktopOverlay.primaryWindowDP.window.handle, ShowWindowCommands.Restore);
				}*/
				WinNative.SetForegroundWindow(DPDesktopOverlay.primaryWindowDP.window.handle);
			}


			i += 1;
			yield return new WaitForSeconds(1f);
		}
		

	}
	

	public static void FocusActiveGame() {

		//if (!DPSettings.config.returnFocusWhenBarClosed) return;


		if (activePID == 0 || activeProcess == null) return;


		//Debug.Log("Focusing game....");
		
		IntPtr[] gameHandles = WinNative.GetProcessWindows(activePID);

		foreach (IntPtr handle in gameHandles) {
			if (handle == IntPtr.Zero) {
				WinNative.ShowWindow(handle, ShowWindowCommands.Restore);
			}
			
			WinNative.SetForegroundWindow(handle);
		}
		
		
		


	}

	private IEnumerator FocusDelayed() {
		yield return new WaitForSeconds(0.2f);
		WinNative.SetForegroundWindow(activeProcess.MainWindowHandle);


	}



	private void UpdateVisuals() {

		if (gameIsRunning && activeGame != null) {

			//theBarStatusBlurBG.texture = activeGame.capsuleTexture;

			if (loadGameHeaderArt_C != null) StopCoroutine(loadGameHeaderArt_C);
			loadGameHeaderArt_C = StartCoroutine(LoadGameHeaderArt());
			
			//theBarStatusText.SetText(activeGame.name);
				

			//avatarGO.SetActive(false);
			//closeButtonGO.SetActive(true);
		}
		else {
		//	avatarGO.SetActive(true);
			//closeButtonGO.SetActive(false);
		
			//theBarStatusBlurBG.texture = fallbackTexture;
			//theBarStatusText.SetText("Hey there!");
			theBarCurrentGameArt.texture = fallbackHeaderArt;
		}
		

		
	}
	

	public void LaunchGame(LibraryGameData gameData) {
		OpenVR.Applications.LaunchApplication(gameData.appKey);
	}


	public void Button_CloseCurrentGame() {
		KillActiveScene();
	}


	private void KillActiveScene() {

		if (!gameIsRunning || activePID == 0) return;

		try {
			activeProcess?.Kill();
		}
		catch (Exception e) {
			Debug.LogError("Failed closing scene application: " + e);
		}

	}

	
	private IEnumerator LoadGameHeaderArt() {


		string filePath = "";

		if (activeGame.gameType == LibraryGameType.Revive) {
				
			//Debug.Log(game.imagePath);
			string dir = Path.GetDirectoryName(activeGame.imagePath);

			if (File.Exists(Path.Combine(dir, "cover_square_image.jpg"))) {
						
				//square image
				//byte[] imgData = File.ReadAllBytes(Path.Combine(dir, "cover_square_image.jpg"));
				filePath = Path.Combine(dir, "cover_square_image.jpg");

			}

		}
		else {


			//Load the image:
			string cachedImageName = activeGame.appID + "_header.jpg";
			filePath = Path.Combine(LibraryHelper.dpGamesLibraryImagePath, cachedImageName);

			if (!File.Exists(filePath)) {
				string imgURL = @"https://steamcdn-a.akamaihd.net/steam/apps/" + activeGame.appID + "/header.jpg";
				yield return LibraryHelper.DownloadImage(imgURL, filePath);
			}
		}
			
		ClearTexture();

		if (filePath == "") yield break;

		byte[] imgData = File.ReadAllBytes(filePath);
		Texture2D tex = new Texture2D(1, 1);
		tex.LoadImage(imgData);
		theBarCurrentGameArt.texture = tex;
			
	}


	public void ClearTexture() {
			
		if (theBarCurrentGameArt.texture != null) {
			//Destroy(theBarCurrentGameArt.texture);
			theBarCurrentGameArt.texture = null;
		}

	}
	
	
}
