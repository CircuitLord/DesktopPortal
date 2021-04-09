using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using CUI.Components;
using DesktopPortal.Overlays;
using DesktopPortal.Sounds;
using DesktopPortal.Steam;
using DesktopPortal.UI.Components;
using DG.Tweening;
using DPCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace DesktopPortal.UI {
	public class ShowGameDetailsEvent : UnityEvent<LibraryGame> {
	}

	public class LibraryGame : MonoBehaviour {
		private LibraryDisplayGroup displayGroup;

		//[HideInInspector] public List<LibraryGameData> gameDataGroup;
		[HideInInspector] public LibraryGameData gameData { get; private set; }
		
		[SerializeField] public RawImage gameArt;

		[SerializeField] private RectTransform reviveLogo;
		

		[SerializeField] public TextMeshProUGUI title;



		[SerializeField] private CUIToggle favToggle;
		
		


		//public Action<bool> onFavoriteUpdated;


		private Coroutine gameArt_C;
		
		private void Start() {
			//gameArt.texture = null;
		}




		public void Button_Play() {
			GamingManager.I.LaunchGame(gameData);
		}

		public void Button_Favorite(bool favorite) {
			if (gameData == null) {
//				Debug.Log("Game data was null on Library Game!");
				return;
			}
			gameData.isFavorite = favorite;
		
			
			//LibraryApp.onFavoritesUpdated?.Invoke();
			
			
		}
		


		public void ScrollCellIndex(int index) {
			
			//Find the library display group this is in
			if (displayGroup == null) displayGroup = GetComponentInParent<LibraryDisplayGroup>();
			
			if (index > displayGroup.gameDataList.Count) return;
			
			transform.localScale = new Vector3(displayGroup.capsuleScale, displayGroup.capsuleScale, 1f);
			//GetComponent<DPScrollHandler>().scrollRect = displayGroup.GetComponent<LoopScrollRect>();

			Setup(displayGroup.gameDataList[index]);
			
		}
		

		public void Setup(LibraryGameData newGameData) {

			gameData = newGameData;

			if (gameArt_C != null) StopCoroutine(gameArt_C);
			gameArt_C = StartCoroutine(LoadGameArt());
			
			
			favToggle.SetValueWithoutNotify(gameData.isFavorite);

			
			
			
			if (gameData.gameType == LibraryGameType.Revive) {

				reviveLogo.gameObject.SetActive(true);

				
				//gameArt.texture = gameData.capsuleTexture;
			}
			else {

				reviveLogo.gameObject.SetActive(false);


				//gameArt.texture = gameData.capsuleTexture;
			}
			
			title.SetText(gameData.name);
		}

		private IEnumerator LoadGameArt() {


			string filePath = "";

			if (gameData.gameType == LibraryGameType.Revive) {
				
				//Debug.Log(game.imagePath);
				string dir = Path.GetDirectoryName(gameData.imagePath);

				if (File.Exists(Path.Combine(dir, "cover_square_image.jpg"))) {
						
					//square image
					//byte[] imgData = File.ReadAllBytes(Path.Combine(dir, "cover_square_image.jpg"));
					filePath = Path.Combine(dir, "cover_square_image.jpg");

				}

			}
			else {


				//Load the image:
				string cachedImageName = gameData.appID + "_library_600x900.jpg";
				filePath = Path.Combine(LibraryHelper.dpGamesLibraryImagePath, cachedImageName);

				if (!File.Exists(filePath)) {
					string imgURL = @"https://steamcdn-a.akamaihd.net/steam/apps/" + gameData.appID + "/library_600x900.jpg";
					yield return LibraryHelper.DownloadImage(imgURL, filePath);
				}
			}
			
			ClearTexture();

			if (filePath == "") yield break;

			byte[] imgData = File.ReadAllBytes(filePath);
			Texture2D tex = new Texture2D(1, 1);
			tex.LoadImage(imgData);
			gameArt.texture = tex;
			
		}


		public void ClearTexture() {
			
			if (gameArt.texture != null) {
				/*try {
					Destroy(gameArt.texture);
				}
				catch {
					
				}*/
				gameArt.texture = null;
			}

		}
		
	
	}
}