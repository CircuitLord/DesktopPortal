using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DesktopPortal.Sounds;
using DesktopPortal.Steam;
using DPCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;


namespace DesktopPortal.UI {
	public class LibraryDetailsPanel : MonoBehaviour {


		public static LibraryDetailsPanel I;
		
		[SerializeField] private DPCameraOverlay detailsDP;
		[SerializeField] private DPCameraOverlay detailsGameDP;

		[SerializeField] private RawImage gameArt;
		[SerializeField] private RawImage bg;
		[SerializeField] private RawImage reviveBG;

		[SerializeField] private TextMeshProUGUI title;
		[SerializeField] private TextMeshProUGUI lastPlayed;
		[SerializeField] private TextMeshProUGUI installed;
		[SerializeField] private TextMeshProUGUI addFavorites;
		

		[HideInInspector] public bool isShowing = false;



		private LibraryGameData gameData;

		private void Awake() {
			I = this;
		}


		public void Init(LibraryGameData newData) {

			/*if (newData == null) return;

			gameData = newData;

			gameArt.texture = gameData.capsuleTexture;
			
			detailsGameDP.RequestRendering(true);


			if (gameData.gameType == LibraryGameType.Revive) {
				bg.gameObject.SetActive(false);
				
				reviveBG.gameObject.SetActive(true);
				
				reviveBG.texture = gameData.heroTexture;
			}
			else {
				bg.gameObject.SetActive(true);
				
				reviveBG.gameObject.SetActive(false);
				
				bg.texture = gameData.heroTexture;
				
			}
			
			title.SetText(gameData.name);
			
			lastPlayed.SetText("Last Played: " + gameData.lastPlayedDate.ToShortDateString());
			
			UpdateFavoriteText();
			
			
			
			StartCoroutine(Show());*/
			

		}

		private IEnumerator Show() {

			//Vector3 posDetails = LibraryApp.I.dpMain.transform.position + LibraryApp.I.dpMain.transform.forward * -0.06f + LibraryApp.I.dpMain.transform.right * 0.13f;
			//Vector3 posDetailsGame = LibraryApp.I.dpMain.transform.position + LibraryApp.I.dpMain.transform.forward * -0.06f + LibraryApp.I.dpMain.transform.right * -0.13f;


			//detailsDP.SetOverlayTransform(posDetails, LibraryApp.I.dpMain.transform.eulerAngles, true, true, false);
			//detailsGameDP.SetOverlayTransform(posDetailsGame, LibraryApp.I.dpMain.transform.eulerAngles, true, true, false);
			
			detailsDP.SetOverlayTransform(new Vector3(0.17f, 0f, -0.07f), Vector3.zero, true);
			detailsGameDP.SetOverlayTransform(new Vector3(-0.17f, 0f, -0.07f), Vector3.zero, true);
	        
			DPUIManager.Animate(detailsDP, DPAnimations.FadeInUp);
			DPUIManager.Animate(detailsGameDP, DPAnimations.FadeInUp);
			
			
			isShowing = true;

			yield break;
		}


		public void Hide() {

			if (!isShowing) return;
			
			DPUIManager.Animate(detailsDP, DPAnimations.FadeOutDown);
			DPUIManager.Animate(detailsGameDP, DPAnimations.FadeOutDown);
			
			//SoundManager.I.PlaySoundOnDPCam(detailsGameDP, Vector2.zero, DPSoundEffect.Activation);
			


		}

		public void UpdateFavoriteText() {
			if (gameData.isFavorite) {
				addFavorites.SetText("Remove from Favorites...");
			}
			else {
				addFavorites.SetText("Add to Favorites...");
			}
		}

		public void ToggleFavorite() {
			gameData.isFavorite = !gameData.isFavorite;
	
			LibraryDisplayGroup.onFavoritesUpdated?.Invoke();
			
			UpdateFavoriteText();
			
		}


		public void Button_Launch() {
			GamingManager.I.LaunchGame(gameData);
			LibraryApp.I.PlayLaunchGameSound();
		}

	}
}