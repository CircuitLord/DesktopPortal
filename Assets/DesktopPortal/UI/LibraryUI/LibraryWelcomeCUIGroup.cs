using System;
using System.Collections;
using System.Collections.Generic;
using CUI;
using DesktopPortal.Steam;
using DPCore;
using Steamworks;
using TMPro;
using UnityEngine;
using WinStuff;
using Random = System.Random;

namespace DesktopPortal.UI {
	public class LibraryWelcomeCUIGroup : CUIGroup {
		
		
		
		
		[SerializeField] private LibraryDisplayGroup favoritesGroup;
		[SerializeField] private LibraryDisplayGroup recentGroup;


		[SerializeField] private TextMeshProUGUI title;


		[SerializeField] private GameObject libraryGamePF;
		[SerializeField] private RectTransform randomSpawnRect;
		
		

		[SerializeField] private List<string> welcomeMessages;

		
		private Random random = new Random();

		public override void OnInit() {
			base.OnInit();

			StartCoroutine(favoritesGroup.MakeGamesDelayed());
			StartCoroutine(recentGroup.MakeGamesDelayed());
			
			SetRandomWelcomeMessage();


			StartCoroutine(SetRandomGameDelayed());
			

		}


		public override void OnShowing() {
			base.OnShowing();

			//if (favoritesGroup.hasInitialized) {
				favoritesGroup.MakeGames();
			//}

			if (recentGroup.hasInitialized) {
				recentGroup.MakeGames();
				
			}

		}

		private IEnumerator SetRandomGameDelayed() {
			yield return new WaitForSeconds(0.5f);
			SetRandomGame();
		}


		public void SetRandomGame() {

			foreach (RectTransform child in randomSpawnRect) {
				Destroy(child.gameObject);
			}

			LibraryGame game = Instantiate(libraryGamePF, randomSpawnRect).GetComponent<LibraryGame>();
			game.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
			
			
			List<LibraryGameData> possibleGames = new List<LibraryGameData>();

			foreach (LibraryGameData data in LibraryApp.I.config.opts.games) {
				if (data.gameType == LibraryGameType.Application) possibleGames.Add(data);
			}
			
			int index = random.Next(possibleGames.Count);
			
			game.Setup(possibleGames[index]);


		}
		
		
		private void SetRandomWelcomeMessage() {
			

			int index = random.Next(welcomeMessages.Count);

			string username;

			if (SteamManager.isConnected) username = SteamClient.Name;
			else username = "User";

		
		
			string message = "Welcome!";

			try {
				message = welcomeMessages[index].Replace("{0}", username);

				message = message.Replace("{gameCount}", LibraryApp.I.config.opts.games.Count.ToString());
				
				message = message.Replace("{refreshRate}", SteamVRManager.hmdRefreshRate.ToString());
			}
			catch (Exception e) {
				Debug.LogError(e);
			}

			title.SetText(message);

		}
		
	}
}