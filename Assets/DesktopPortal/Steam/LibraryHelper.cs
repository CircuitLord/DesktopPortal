using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DesktopPortal.UI;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.Networking;
using Valve.VR;

namespace DesktopPortal.Steam {
	public class LibraryHelper : MonoBehaviour {

		public static LibraryHelper I;
		
		

		[SerializeField] private LibraryApp libraryApp;
		
		
		public static string dpGamesConfigPath;
		public static string dpGamesLibraryImagePath;

		//[HideInInspector] public DPGamesLibraryConfig config = new DPGamesLibraryConfig();

		private VRAppsManifest manifest;
		
		
		DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static Action onLoaded;


		private void Awake() {
			I = this;
		}

		private void Start() {
			Init();
		}

		private void Init() {
			//SteamFinder.FindSteam();

			dpGamesConfigPath = Path.Combine(Application.persistentDataPath, "library.json");
			dpGamesLibraryImagePath = Path.Combine(Application.persistentDataPath, "librarycache");

			//LoadConfig();
		}


		/*public void LoadConfig() {
			if (File.Exists(dpGamesConfigPath)) {
				config = JsonUtility.FromJson<DPGamesLibraryConfig>(File.ReadAllText(dpGamesConfigPath));
			}

			else {
				config = new Librar();
				SaveConfig();
			}
		}*/

		/*public void SaveConfig() {
			if (config != null) {
				File.WriteAllText(dpGamesConfigPath, JsonUtility.ToJson(config));
			}
		}*/

		private void CheckImageCacheValid() {
			if (!Directory.Exists(dpGamesLibraryImagePath)) {
				Directory.CreateDirectory(dpGamesLibraryImagePath);
			}
		}


		public void PopulateDPGamesLibrary(bool refreshAll = false) {

		
			if (!CheckSteamValid()) return;

			List<LibraryGameData> allGames = libraryApp.config.opts.games;

			if (refreshAll) allGames.Clear();
			
			List<string> openVRAppKeys = new List<string>();

			//Loop over all the OpenVR applications:
			for (int i = 0; i < OpenVR.Applications.GetApplicationCount(); i++) {
				StringBuilder sb = new StringBuilder((int) OpenVR.k_unMaxApplicationKeyLength);

				OpenVR.Applications.GetApplicationKeyByIndex((uint) i, sb, OpenVR.k_unMaxApplicationKeyLength);

				openVRAppKeys.Add(sb.ToString());

				//Debug.Log(sb);
			}


			foreach (string key in openVRAppKeys) {
				//Get the application type and it's ID.
				LibraryGameType type = LibraryGameType.Application;

				int testID = 0;
				Int32.TryParse(key.Replace("steam.app.", ""), out testID);

				if (testID == 0) {
					Int32.TryParse(key.Replace("steam.overlay.", ""), out testID);
					if (testID != 0) type = LibraryGameType.Overlay;

					//If all those checks failed, see if it's a revive game:
					else if (key.StartsWith("revive.app.")) {
						type = LibraryGameType.Revive;
						testID = -1;
					}
				}

				//If it's not an app, overlay, or revive game, we skip it.
				if (testID == 0) continue;

				
				EVRApplicationError evrError = new EVRApplicationError();
				
				

				bool exists = allGames.Exists(x => x.appKey == key);

				//We skip this game, but we need to refresh the last launched time:
				if (exists && !refreshAll) {

					ulong tempLaunchTime = OpenVR.Applications.GetApplicationPropertyUint64(key, EVRApplicationProperty.LastLaunchTime_Uint64, ref evrError);
					allGames.Find(x => x.appKey == key).lastPlayedDate = UnixTimeStampToDateTime(tempLaunchTime);

					continue;
				}


				//Else, we go as normal and fetch all the app data
				
				//Get the app launch URL:
				StringBuilder launchString = new StringBuilder(128);
				OpenVR.Applications.GetApplicationPropertyString(key, EVRApplicationProperty.URL_String, launchString, 128, ref evrError);


				//Get the binary path:
				StringBuilder binaryString = new StringBuilder(512);
				OpenVR.Applications.GetApplicationPropertyString(key, EVRApplicationProperty.BinaryPath_String, binaryString, 512, ref evrError);

				//Get the name of the game:
				StringBuilder nameString = new StringBuilder(128);
				OpenVR.Applications.GetApplicationPropertyString(key, EVRApplicationProperty.Name_String, nameString, 128, ref evrError);
				
				//Get the name of the game:
				StringBuilder imageString = new StringBuilder(512);
				OpenVR.Applications.GetApplicationPropertyString(key, EVRApplicationProperty.ImagePath_String, imageString, 512, ref evrError);


				//Get last played time:
				ulong lastLaunchLong = OpenVR.Applications.GetApplicationPropertyUint64(key, EVRApplicationProperty.LastLaunchTime_Uint64, ref evrError);
				
				

				LibraryGameData gameData = new LibraryGameData() {
					appID = testID,
					name = nameString.ToString(),
					launchURL = launchString.ToString(),
					binaryPath = binaryString.ToString(),
					imagePath = imageString.ToString().Replace("file:///", "").Replace("%20", " "),
					lastPlayedDate = UnixTimeStampToDateTime(lastLaunchLong),
					gameType = type,
					appKey = key,
					isInstalled = OpenVR.Applications.IsApplicationInstalled(key)
				};

				allGames.Add(gameData);
			}

			//StartCoroutine(LoadGameLibraryImages(refreshAll));

			libraryApp.config.SaveSettings();
			
			onLoaded?.Invoke();

		}


		public IEnumerator LoadGameLibraryImages(bool refreshAll = false) {
			CheckImageCacheValid();

			foreach (LibraryGameData gameData in libraryApp.config.opts.games) {
				//If it already exists, don't load it again.
				/*
				if (!gameData.needsToRefresh && !refreshAll && gameData.capsuleTexture != null && gameData.heroTexture != null) continue;

				gameData.needsToRefresh = false;

				
				//We load the image way differently for revive games
				if (gameData.gameType == LibraryGameType.Revive) {


					//Debug.Log(game.imagePath);
					string dir = Path.GetDirectoryName(gameData.imagePath);

					if (File.Exists(gameData.imagePath)) {
						//"hero" image
						byte[] heroData = File.ReadAllBytes(gameData.imagePath);
						gameData.heroTexture = new Texture2D(360, 202);
						gameData.heroTexture.LoadImage(heroData);
						
					}

					if (File.Exists(Path.Combine(dir, "cover_square_image.jpg"))) {
						
						//square image
						byte[] imgData = File.ReadAllBytes(Path.Combine(dir, "cover_square_image.jpg"));
						gameData.capsuleTexture = new Texture2D(300, 300);
						gameData.capsuleTexture.LoadImage(imgData);
					}
					
					

					yield return null;
					
					continue;

				}

				//Get the game library image:

				
				
				string cachedImageName = gameData.appID + "_library_600x900.jpg";
				//string cachedHeroName = game.appID + "_library_hero.jpg";

				bool cachedImage = File.Exists(Path.Combine(dpGamesLibraryImagePath, cachedImageName));
				bool cachedHero = File.Exists(Path.Combine(dpGamesLibraryImagePath, cachedHeroName));

				//If we're re-getting all the images, or a cached version doesn't exist:
				if (!cachedImage) {
					//Check the actual steam cache first:
					if (File.Exists(Path.Combine(SteamFinder.SteamLibraryCachePath, cachedImageName))) {
						//If it does, copy it to our cache.
						File.Copy(Path.Combine(SteamFinder.SteamLibraryCachePath, cachedImageName),
							Path.Combine(dpGamesLibraryImagePath, cachedImageName));
					}

					//Else, we download the image to our cache:
					else {
						string imgURL =
							@"https://steamcdn-a.akamaihd.net/steam/apps/" + game.appID + "/library_600x900.jpg";

						game.needsToRefresh = true;
						StartCoroutine(DownloadImage(imgURL, Path.Combine(dpGamesLibraryImagePath, cachedImageName)));
						
						continue;

					}

				}
				
				if (!cachedHero) {
					string imgURL = @"https://steamcdn-a.akamaihd.net/steam/apps/" + game.appID + "/library_hero.jpg";

					game.needsToRefresh = true;
					StartCoroutine(DownloadImage(imgURL, Path.Combine(dpGamesLibraryImagePath, cachedHeroName)));

					continue;
				}

				//Load the images from our cache:
				if (cachedImage) {
					
					byte[] imgData = File.ReadAllBytes(Path.Combine(dpGamesLibraryImagePath, cachedImageName));
					game.capsuleTexture = new Texture2D(1, 1);
					game.capsuleTexture.LoadImage(imgData);
				}


				if (cachedHero) {
					
					byte[] heroData = File.ReadAllBytes(Path.Combine(dpGamesLibraryImagePath, cachedHeroName));
					game.heroTexture = new Texture2D(960, 310);
					game.heroTexture.LoadImage(heroData);
				}
				*/
				
				
				

				//Spread out the loading:
				yield return null;
				
				
			}

		}

		public static IEnumerator DownloadImage(string MediaUrl, string savePath) {
			UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
			yield return request.SendWebRequest();
			if (request.isNetworkError || request.isHttpError) {
				//Debug.Log(MediaUrl);
				//Debug.Log(request.error);
			}
			
			else {
				File.WriteAllBytes(savePath, ((DownloadHandlerTexture) request.downloadHandler).texture.EncodeToJPG());
			}

			//game.capsuleTexture = ((DownloadHandlerTexture) request.downloadHandler).texture;
		}

		public static bool CheckSteamValid() {
			if (SteamFinder.SteamPath != null) return true;

			else return false;
		}
		
		
		public List<LibraryGameData> GetSortedGameData(LibrarySortMode sortMode = LibrarySortMode.AtoZ) {

			List<LibraryGameData> gamesData = libraryApp.config.opts.games;
			
			switch (sortMode) {
				case LibrarySortMode.AtoZ:
					return gamesData.OrderBy(o=>o.name).ToList();

				case LibrarySortMode.ZtoA:
					return gamesData.OrderByDescending(o=>o.name).ToList();

				case LibrarySortMode.Recent:
					return gamesData.OrderByDescending(o=>o.lastPlayedDate).ToList();

			}

			return null;
		}
		
		public DateTime UnixTimeStampToDateTime( double unixTimeStamp ) {
			return epoch.AddSeconds( unixTimeStamp ).ToLocalTime();
		}

		public double DateTimeToUnixTimeStamp(DateTime dateTime) {
			return ((DateTimeOffset) dateTime).ToUnixTimeSeconds();
		}
		
	}
	

	[Serializable]
	public class LibraryGameData {
		public int appID;
		public string appKey;
		public string binaryPath;
		public string launchURL;
		public string imagePath;
		public ulong lastPlayedTimestamp;
		public DateTime lastPlayedDate;
		public LibraryGameType gameType = LibraryGameType.Application;
		public string name;
		public bool isInstalled = true;

		public bool isFavorite = false;

		public string capsuleTexFilePath;
		public string coverTexFilePath;
		
		//[NonSerialized] public Texture2D capsuleTexture;
		//[NonSerialized] public Texture2D coverTexture;

		//[NonSerialized] public bool needsToRefresh = false;
	}


	[Serializable]
	public enum LibraryGameType {
		Core = 0,
		Overlay = 1,
		Application = 2,
		Revive = 3
	}
}