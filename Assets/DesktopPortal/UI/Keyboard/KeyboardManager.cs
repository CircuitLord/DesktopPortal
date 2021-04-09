using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DesktopPortal.IO;
using DesktopPortal.Keyboard;
using DesktopPortal.Overlays;
using DesktopPortal.Sounds;
using DesktopPortal.UI.Keyboard;
using DPCore;
using DPCore.Apps;
using TMPro;
using UnityEngine;
using WinStuff;
using Application = UnityEngine.Application;

namespace DesktopPortal.UI {
	public class KeyboardManager : DPApp {
		public static KeyboardManager I;
		
		
		//public List<KButton> kbRow1 = new List<KButton>();

		public Transform keysTrans;


		public DPCameraOverlay keyboardDP;


		private bool isShiftDown = false;
		private bool isCapsLockDown = false;
		private bool isAltDown = false;

		[SerializeField] private List<KeyboardSuggestion> kbSuggestions;
		

		private List<KButton> keysNonPersist = new List<KButton>();

		private List<KeysEx> _qwertyKeys = new List<KeysEx>() {
			KeysEx.VK_OEM_3,
			KeysEx.VK_1,
			KeysEx.VK_2,
			KeysEx.VK_3,
			KeysEx.VK_4,
			KeysEx.VK_5,
			KeysEx.VK_6,
			KeysEx.VK_7,
			KeysEx.VK_8,
			KeysEx.VK_9,
			KeysEx.VK_0,
			KeysEx.VK_OEM_MINUS,
			KeysEx.VK_OEM_PLUS,

			KeysEx.VK_Q,
			KeysEx.VK_W,
			KeysEx.VK_E,
			KeysEx.VK_R,
			KeysEx.VK_T,
			KeysEx.VK_Y,
			KeysEx.VK_U,
			KeysEx.VK_I,
			KeysEx.VK_O,
			KeysEx.VK_P,
			KeysEx.VK_OEM_4,
			KeysEx.VK_OEM_6,
			KeysEx.VK_OEM_102,


			KeysEx.VK_A,
			KeysEx.VK_S,
			KeysEx.VK_D,
			KeysEx.VK_F,
			KeysEx.VK_G,
			KeysEx.VK_H,
			KeysEx.VK_J,
			KeysEx.VK_K,
			KeysEx.VK_L,
			KeysEx.VK_OEM_1,
			KeysEx.VK_OEM_7,


			KeysEx.VK_Z,
			KeysEx.VK_X,
			KeysEx.VK_C,
			KeysEx.VK_V,
			KeysEx.VK_B,
			KeysEx.VK_N,
			KeysEx.VK_M,
			KeysEx.VK_OEM_COMMA,
			KeysEx.VK_OEM_PERIOD,
			KeysEx.VK_OEM_2
		};


		private SymSpell sym;

		private string currentString = "";


		private DPOverlayBase activeDP;

		/// <summary>
		/// If the overlay has been dragged since we activated the keyboard, don't move it back to it's original spot
		/// </summary>
		private bool activeDPHasBeenDragged = false;


		protected override void Awake() {
			I = this;
		}


		private void Start() {

			TheBarManager.I.onBarOpened += b => {
				if (!b) Button_Close();
			};

			DPOverlayBase.onClickedOn += dpBase => {
				activeDP = dpBase;
			};
			
			KButton.onButtonPress += HandleKeyPress;
			//KConstantButton.onButtonPress += HandleKeyPress;
			
			


			LoadLayout(_qwertyKeys);
			
			
			InitSym();
		}


		private void LoadLayout(List<KeysEx> keys) {
			
			keysNonPersist.Clear();
			
			int i = 0;
			
			foreach (KButton k in keysTrans.GetComponentsInChildren<KButton>()) {
				if (k == null) continue;

				if (k.isPersistent) {
					k.Init();
					continue;
				}

				if (keys.Count <= i) return;

				k.key = keys[i];

				k.mainChar = KeyboardHelper.KeyCodeToUnicode(keys[i], false, false);
				k.shiftChar = KeyboardHelper.KeyCodeToUnicode(keys[i], true, false);
				k.altChar = KeyboardHelper.KeyCodeToUnicode(keys[i], false, true);

				k.Init();

				keysNonPersist.Add(k);
				

				i++;
			}
		}


		private void OnShiftChanged(bool down) {
			foreach (KButton key in keysNonPersist) {
				key.UpdateValues(down, false);
			}

			isShiftDown = down;
		}

		public void OnAltChanged(bool down) {
			
		}


		private void InitSym() {
			//create object
			int initialCapacity = 82765;
			int maxEditDistanceDictionary = 2; //maximum edit distance per dictionary precalculation
			sym = new SymSpell(initialCapacity, maxEditDistanceDictionary);

			//load dictionary
			string dictionaryPath = Path.Combine(Application.streamingAssetsPath, "SymSpell", "frequency_dictionary_en_82_765.txt");
			int termIndex = 0; //column of the term in the dictionary text file
			int countIndex = 1; //column of the term frequency in the dictionary text file
			if (!sym.LoadDictionary(dictionaryPath, termIndex, countIndex)) {
				Debug.LogError("Dictionary file not found! Aborting...");
				return;
			}

		}


		private void UpdatePredicitions() {

			if (currentString == "") {
				foreach (KeyboardSuggestion kbsug in kbSuggestions) {
					kbsug.text.SetText("");
				}
				
				return;
			}

			var suggestions = sym.Lookup(currentString, SymSpell.Verbosity.Closest, 1);

			if (suggestions.Count >= 1) {
				kbSuggestions[1].text.SetText(suggestions[0].term);
			}
			
			if (suggestions.Count >= 2) {
				kbSuggestions[0].text.SetText(suggestions[1].term);
			}
			if (suggestions.Count >= 3) {
				kbSuggestions[2].text.SetText(suggestions[2].term);
			}

		}


		private void HandleKeyPress(KButton kButton, bool down) {
			/*if (DPSettings.config.focusGameWhenNotInteracting) {
				if (DPDesktopOverlay.primaryWindowDP != null && DPDesktopOverlay.primaryWindowDP.isTargetingWindow) {
					WinNative.SetForegroundWindow(DPDesktopOverlay.primaryWindowDP.window.handle);
				}
			}*/

			if (DPDesktopOverlay.primaryWindowDP != null && DPDesktopOverlay.primaryWindowDP.isTargetingWindow) {
				WinNative.SetForegroundWindow(DPDesktopOverlay.primaryWindowDP.window.handle);
			}


			if (kButton.isShift) OnShiftChanged(down);
			
			Char potentialChar = Char.MinValue;
			

			if (kButton.isPersistent && down) {
				
				
				//if (kButton.key == KeysEx.VK_SPACE) currentString = "";
				//if (kButton.key == KeysEx.VK_BACK) currentString = currentString.Remove(currentString.Length - 1, 1);  
				
				
				
			}
			else if (down) {
				potentialChar = kButton.mainChar[0];

				if (isShiftDown) {
					potentialChar = kButton.shiftChar[0];
				} 
				else if (isAltDown) {
					potentialChar = kButton.altChar[0];
				}

				currentString += potentialChar;
				
			}

			activeDP.onKeyboardInput?.Invoke((Keys)kButton.key, potentialChar, down);
			
			//UpdatePredicitions();


			if (down && activeDP != null) {
				activeDP.RequestRendering();
			}
			
			
			
		}



		public void Button_ShowKeyboardOnBar() {
			ShowKeyboard(TheBarManager.I.theBarDP, false);
		}
		

		public void ShowKeyboard(DPOverlayBase dpBase, bool child = true) {
			//if (activeDP == dpBase) return;

			if (dpBase == keyboardDP) return;

			
			activeDP = dpBase;

			//keyboardDP.OrphanOverlay();

			Vector3 goodPos;
			Vector3 goodRot;
			
			/*if (child) {
				activeDP.AddChildOverlay(keyboardDP);
				
				keyboardDP.overlay.SetWidthInMeters(0.8f);
			
				goodPos = new Vector3(0f, activeDP.overlayHeight / -1.9f, -0.21f);

				goodRot = new Vector3(30f, 0f, 0f);
			}
			else {*/
				goodPos = activeDP.transform.position + activeDP.transform.forward * -0.11f;
				goodPos += activeDP.transform.up * (activeDP.overlayHeight / 2f);
				
				goodRot = SteamVRManager.I.hmdTrans.eulerAngles;
				goodRot.z = 0f;
			//}
			
			
			keyboardDP.SetOverlayTransform(goodPos, goodRot);
			

			TheBarManager.I.ToggleDPApp(dpBase.dpAppParent, true, true);
		}


		public void Button_Close() {
			DPUIManager.Animate(keyboardDP, DPAnimation.FadeOut);

			if (DPSettings.config.focusGameWhenNotInteracting) {
				GamingManager.FocusActiveGame();
			}


			//ResetPrevious();
		}

		public override void OnMinimize() {
			base.OnMinimize();
			if (DPSettings.config.focusGameWhenNotInteracting) {
				GamingManager.FocusActiveGame();
			}
		}
		
		public override void OnClose() {
			base.OnMinimize();
			if (DPSettings.config.focusGameWhenNotInteracting) {
				GamingManager.FocusActiveGame();
			}
		}
		
		


		private void ResetPrevious() {
			if (activeDP == null) return;

			if (activeDP.overlay.trackedDevice == DPOverlayTrackedDevice.None && !activeDPHasBeenDragged) {
				Vector3 newPos = activeDP.transform.position + activeDP.transform.up * -0.1f;

				activeDP.TransitionOverlayPosition(newPos, activeDP.transform.eulerAngles, 0.5f);
			}

			//activeDP.onOverlayDragged -= OnActiveDragged;

			activeDP = null;

			activeDPHasBeenDragged = false;
		}
	}
}