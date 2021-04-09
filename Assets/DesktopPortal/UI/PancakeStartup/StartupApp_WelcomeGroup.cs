using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WindowsInput;
using WindowsInput.Native;
using CUI;
using CUI.Utils;
using DesktopPortal.IO;
using DesktopPortal.Keyboard;
using DesktopPortal.Overlays;
using DPCore.Apps;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using uWindowCapture;
using WinStuff;
using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.UI.Welcome {
	public class StartupApp_WelcomeGroup : CUIGroup {
		[SerializeField] private RectTransform bg;

		[SerializeField] private TextMeshProUGUI adminText;
		

		[SerializeField] private Camera camera;

		[SerializeField] private GameObject newsItemPF;

		[SerializeField] private RectTransform tileSpawnRectTrans;


		[SceneObjectsOnly]
		[SerializeField] private GameObject enableAdminMsg;

		[SerializeField] private CUIGroup restartCUI;
		

		//UnityEvent<RectTransform> ytay = new 


		//[ShowInInspector] public BetterEvent<string> testEvent = new BetterEvent<string>();


		protected override void Start() {
			base.Start();


			//testEvent.Invoke("yay");


			bool isElevated = UACHelper.IsProcessElevated;
			
			adminText.SetText("ADMIN: " + isElevated.ToString());
			
			enableAdminMsg.gameObject.SetActive(!isElevated);
			
			


#if !UNITY_EDITOR
				DPSettings.OnLoad(SetupWelcomeScreen);
#endif
		}


		public void Button_EnableAdmin() {

			string taskInstallerPath = Path.Combine(Application.dataPath, @"..\", "DPTaskInstaller.exe");

			ProcessStartInfo info = new ProcessStartInfo() {
				Arguments = "-install",
				FileName = taskInstallerPath
			};

			Process.Start(info);

			StartCoroutine(ShowTaskInstalledScreen());


		}

		private IEnumerator ShowTaskInstalledScreen() {
			yield return new WaitForSeconds(10f);
			
			CUIManager.Animate(restartCUI, true);
		}


		public void Button_Close() {
			Application.Quit();
		}
		

		public void JoinDiscord() {

			StartCoroutine(C_JoinDiscord());


		}

		private IEnumerator C_JoinDiscord() {
			
			//Clipboard.SetText("https://discord.gg/adVEQmY");
			
			Process.Start("https://discord.gg/adVEQmY");

			yield break;

			UwcWindow discordWindow = null;
			DPDesktopOverlay discordDP = null;
			DPApp discordDPApp = null;
			
			//See if discord is opened already
			foreach (DPDesktopOverlay desktopDP in DPDesktopOverlay.overlays) {
				
				if (desktopDP.isTargetingWindow && desktopDP.window.title.EndsWith("- Discord")) {
					discordDP = desktopDP;
					discordWindow = desktopDP.window;

					break;
				}
			}

			//If the overlay isn't already opened, we try to spawn a new one
			if (discordWindow == null) {
				//See if discord is opened
				foreach (UwcWindow window in UwcManager.windows.Values) {

					if (window.title.EndsWith("- Discord")) {
						discordWindow = window;

						discordDP = OverlayManager.I.NewDPWindowOverlay(discordWindow);

						break;
					}

				}

			}

//TODO: Launch discord
			//if (discordWindow == null || discordDP == null) {

				Process.Start("https://discord.gg/adVEQmY");
				
				yield break;
			//}
				
			
			//Find the DPApp
			foreach (DPApp app in TheBarManager.openApps.Values) {
				if (app.dpMain == discordDP) {
					discordDPApp = app;
					break;
				}
			}
			

			if (discordWindow.isIconic) {
				WinNative.ShowWindow(discordWindow.handle, ShowWindowCommands.Restore);
			}
			
			TheBarManager.I.LaunchAppToMainSnap(discordDPApp.appKey);
			
			WinNative.SetForegroundWindow(discordWindow.handle);
			
			yield return new WaitForSeconds(0.2f);

			yield return new WaitForSeconds(0.2f);
			
			//const int WM_KEYDOWN = 0x100;
			//const int WM_KEYUP = 0x101;
			
			//WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_LCONTROL, 0);
			//WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_LSHIFT, 0);
			//WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_N, 0);
			
			InputSimulator inputSimulator = new InputSimulator();

			//inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
			inputSimulator.Keyboard.ModifiedKeyStroke(new[] {VirtualKeyCode.LCONTROL, VirtualKeyCode.LSHIFT}, new[] {VirtualKeyCode.VK_N});

			yield return new WaitForSeconds(0.2f);
			
			inputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
			inputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
			inputSimulator.Keyboard.KeyPress(VirtualKeyCode.TAB);
			
			inputSimulator.Keyboard.KeyPress(VirtualKeyCode.RETURN);
			
			yield return new WaitForSeconds(0.2f);

			inputSimulator.Keyboard.TextEntry("https://discord.gg/adVEQmY");





			/*//Tab 3 times to the join server button:
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_TAB, 0);
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_TAB, 0);
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_TAB, 0);

			yield return null;
			
			//Enter
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_RETURN, 0);
			
			yield return new WaitForSeconds(0.2f);
			
			//Paste in the join link
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_LCONTROL, 0);
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_V, 0);
			yield return null;
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYUP, (int)KeysEx.VK_V, 0);
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYUP, (int)KeysEx.VK_LCONTROL, 0);

			yield return null;
			
			//Enter to join:
			WinNative.PostMessageSafe(discordWindow.handle, WM_KEYDOWN, (int)KeysEx.VK_RETURN, 0);*/

		}
		
		
		
		
		private void SetupWelcomeScreen() {
			StartCoroutine(SetupNewsTiles());
		}


		private IEnumerator SetupNewsTiles() {
			using (UnityWebRequest webRequest =
				UnityWebRequest.Get("http://api.steampowered.com/ISteamNews/GetNewsForApp/v0002/?appid=1178460&count=3&maxlength=110&format=json")) {
				// Request and wait for the desired page.
				yield return webRequest.SendWebRequest();

				if (webRequest.isNetworkError || webRequest.isHttpError) {
					Debug.LogError(webRequest.error);
					yield break;
				}

				//Debug.Log(webRequest.downloadHandler.text);


				SteamAppNewsHolder appNewsHolder = JsonUtility.FromJson<SteamAppNewsHolder>(webRequest.downloadHandler.text);

				bool isFirst = true;

				foreach (SteamNewsItem item in appNewsHolder.appnews.newsitems) {
					StartupApp_WelcomeTile tile = Instantiate(newsItemPF, tileSpawnRectTrans).GetComponent<StartupApp_WelcomeTile>();

					//string version = item.title.Replace("Desktop Portal v", "");
					int index = 0;
					index = item.title.IndexOf("Portal v", StringComparison.Ordinal) + 8;
					if (index != 0) {
						string version = item.title.Substring(index);
						tile.versionText.SetText(version);

						if (isFirst && version != DPSettings.config.lastSeenVersion) {
							tile.newIcon.SetActive(true);
							DPSettings.config.lastSeenVersion = version;
						}
					}


					if (item.tags.Contains("patchnotes")) {
						tile.title.SetText("Patch");
					}

					else {
						tile.title.SetText("Update");
					}

					tile.contents.SetText(item.contents);


					tile.GetComponent<CUIOpenLink>().linkToLaunch = item.url;

					//Get the image
					using (UnityWebRequest announcementPage = UnityWebRequest.Get(item.url)) {
						yield return announcementPage.SendWebRequest();

						//Debug.Log(announcementPage.downloadHandler.text);

						string link = "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/clans/36888522/";

						int j = announcementPage.downloadHandler.text.IndexOf(link, StringComparison.Ordinal) + link.Length;

						for (int i = 0; i < 100; i++) {
							Char yay = announcementPage.downloadHandler.text[j + i];
							if (yay.Equals('"')) {
								break;
							}
							else {
								link += yay;
							}
						}

						//Download image:
						using (UnityWebRequest imageRequest = UnityWebRequestTexture.GetTexture(link)) {
							yield return imageRequest.SendWebRequest();

							if (imageRequest.isNetworkError || imageRequest.isHttpError) {
								Debug.LogError(imageRequest.error);
							}
							else {
								Texture img = ((DownloadHandlerTexture) imageRequest.downloadHandler).texture;

								tile.bgImage.texture = img;
								tile.mainImage.texture = img;
							}
						}
					}


					isFirst = false;
				}
			}
		}


		private void Update() {
			Vector2 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

			MoveParallaxBG(mousePos);
		}


		private void MoveParallaxBG(Vector2 pos) {
			Vector2 bgPos = new Vector2(pos.x * -0.8f, pos.y * -0.8f);

			Vector2 curPos = bg.localPosition;

			bg.localPosition = Vector2.Lerp(curPos, bgPos, 5f * Time.deltaTime);
		}
	}
}