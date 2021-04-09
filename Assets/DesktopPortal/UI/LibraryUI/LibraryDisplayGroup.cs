using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DesktopPortal.Steam;
using UnityEngine;
using UnityEngine.UI;


namespace DesktopPortal.UI {
	/// <summary>
	/// Handles displaying a group of LibraryGame
	/// </summary>
	public class LibraryDisplayGroup : MonoBehaviour {
		[SerializeField] public LibraryDisplayMode displayMode = LibraryDisplayMode.All;

		[SerializeField] public LibrarySortMode sortMode = LibrarySortMode.AtoZ;

		[SerializeField] public float capsuleScale = 1.0f;

		[SerializeField] private int spawnLimit = 10;


		//[SerializeField] private GameObject libraryGamePrefab;

		[HideInInspector] public List<LibraryGameData> gameDataList = new List<LibraryGameData>();

		[SerializeField] private RectTransform spawnRect;

		public static Action onFavoritesUpdated;

		private LoopScrollRect scrollRect;


		public bool hasInitialized = false;

		private void Start() {
			scrollRect = GetComponent<LoopScrollRect>();

			onFavoritesUpdated += () => {
				if (displayMode == LibraryDisplayMode.Favorites) {
					MakeGames();
				}
			};
		}


		public IEnumerator MakeGamesDelayed(bool force = false) {
			yield return new WaitForSeconds(0.5f);
			MakeGames(force);
			yield break;
		}

		public void MakeGames(bool force = false) {


			Debug.Log("building games " + name);
			
			//if (force || !hasInitialized) {
				scrollRect.ClearCells();
				foreach (RectTransform child in spawnRect) {
					Destroy(child.gameObject);
				}
			//}

			//gamesGroup.Clear();
			gameDataList.Clear();


			//Sort the data, and then get the game data from the sorted:
			List<LibraryGameData> tempSort = LibraryHelper.I.GetSortedGameData(sortMode);


			int spawnCount = 0;
			foreach (LibraryGameData gameData in tempSort) {
				//Based on the display mode, we don't dlisplay some games:

				switch (displayMode) {
					case LibraryDisplayMode.Applications:
						if (gameData.gameType == LibraryGameType.Overlay || gameData.gameType == LibraryGameType.Core) continue;
						break;

					case LibraryDisplayMode.Revive:
						if (gameData.gameType != LibraryGameType.Revive) continue;
						break;

					case LibraryDisplayMode.Favorites:
						if (!gameData.isFavorite) continue;
						break;

					case LibraryDisplayMode.Misc:
						if (gameData.gameType == LibraryGameType.Application || gameData.gameType == LibraryGameType.Revive) continue;
						break;
				}

				//gameDataList.Add(gameData);
				gameDataList.Add(gameData);

				spawnCount++;

				if (spawnCount >= spawnLimit) break;
			}


			/*if (force || !hasInitialized) {
				scrollRect.ClearCells();
				foreach (RectTransform child in spawnRect) {
					Destroy(child.gameObject);
				}
			}*/
			


			scrollRect.totalCount = gameDataList.Count;


			if (force || !hasInitialized) {
				//scrollRect.RefillCellsFromEnd();
				//scrollRect.RefillCells();
			}
			else {
				//scrollRect.RefreshCells();
			}
			
			scrollRect.RefillCells();
			
			scrollRect.SrollToCell(0, 1000);

			hasInitialized = true;
		}


		/*private void PopulateItem(RecyclingListViewItem item, int index) {
			LibraryGame game = (LibraryGame) item;

			if (game == null) return;

			if (gameDataList[index] == null) return;
			
			
			game.Setup(gameDataList[index]);

		}*/
	}
}