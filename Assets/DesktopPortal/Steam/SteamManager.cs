using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DesktopPortal.IO;
using DesktopPortal.Steam;
using Steamworks;
using Steamworks.Data;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Valve.Newtonsoft.Json;
using Valve.Newtonsoft.Json.Linq;

public class SteamManager : MonoBehaviour {
	public static SteamManager I;


	[SerializeField] private uint appID = 1178460;

	[HideInInspector] public static bool isConnected = false;


	[SerializeField] private RawImage avatarImage;


	// public bool enableRigidLoops = true;

	//public DPRenderQuality renderQuality = DPRenderQuality.Normal;

	// Start is called before the first frame update
	void Awake() {
		I = this;
		Init();
	}

	private void OnApplicationQuit() {
		if (SteamClient.IsValid) SteamClient.Shutdown();
	}


	private void Init() {
		try {
			SteamClient.Init(appID);
			isConnected = true;
		}
		catch (System.Exception e) {
			// Couldn't init for some reason (steam is closed etc)
			Debug.Log("Could not connect to steamworks: " + e);
		}
		
		var q = Steamworks.Ugc.Query.All
			.WithTag( "Fun" )
			.WithTag( "Movie" )
			.MatchAllTags();

		//var result = await q.GetPageAsync( 1 );
		
		/*foreach ( Steamworks.Ugc.Item entry in result.Value.Entries )
		{
			Console.WriteLine( $" {entry.Owner.Name}" );
		}*/


		SteamFinder.FindSteam();


		DPSettings.OnLoad(delegate {
			if (DPSettings.config.overrideOverlayRenderQuality) {
				ToggleRigidLoops(true);
			}
		});


		//if (enableRigidLoops) ToggleRigidLoops(enableRigidLoops);

		GetAvatar();
	}


	public async void GetAvatar() {
		var image = await SteamFriends.GetLargeAvatarAsync(SteamClient.SteamId);
		if (!image.HasValue) return;


		//  await new WaitForUpdate();

		Texture2D avatarTex = new Texture2D((int) image.Value.Width, (int) image.Value.Height, TextureFormat.RGBA32, false);

		avatarTex.LoadRawTextureData(image.Value.Data);
		avatarTex.Apply();

		//avatarImage.texture = avatarTex;


		//return MakeTextureFromRGBA( image.Data, image.Width, image.Height );
	}


	private void TestA() {
		var ach = new Achievement("GM_PLAYED_WITH_GARRY");
		ach.Trigger();
	}


	public void ToggleRigidLoops(bool enable) {
		if (!File.Exists(SteamFinder.SteamVRSettingsFile)) return;

		dynamic d = JsonConvert.DeserializeObject(File.ReadAllText(SteamFinder.SteamVRSettingsFile));


		if (enable) {
			//grab the average value for their performance:

			int speedValue = 980;


			switch (DPSettings.config.renderQuality) {
				case DPRenderQuality.Low:
					speedValue = 720;
					break;

				case DPRenderQuality.Normal:
					speedValue = 980;
					break;

				case DPRenderQuality.High:
					speedValue = 1240;
					break;

				case DPRenderQuality.Maximum:
					speedValue = 10000;
					break;
			}


			d["GpuSpeed"]["gpuSpeed0"] = speedValue;
			d["GpuSpeed"]["gpuSpeed1"] = speedValue;
			d["GpuSpeed"]["gpuSpeed2"] = speedValue;
			d["GpuSpeed"]["gpuSpeed3"] = speedValue;
			d["GpuSpeed"]["gpuSpeed4"] = speedValue;
			d["GpuSpeed"]["gpuSpeed5"] = speedValue;
			d["GpuSpeed"]["gpuSpeed6"] = speedValue;
			d["GpuSpeed"]["gpuSpeed7"] = speedValue;
			d["GpuSpeed"]["gpuSpeed8"] = speedValue;
			d["GpuSpeed"]["gpuSpeed9"] = speedValue;
			//d.GpuSpeed.gpuSpeedHorsepower = speedValue;
			d["GpuSpeed"]["gpuSpeedCount"] = 10;
		}
		else {
			d["GpuSpeed"]["gpuSpeedCount"] = 0;
		}

		//Save the file
		File.WriteAllText(SteamFinder.SteamVRSettingsFile, JsonConvert.SerializeObject(d, Formatting.Indented));
	}
}

public enum DPRenderQuality {
	/// <summary>
	/// 0.75x
	/// </summary>
	Low,

	/// <summary>
	/// 1.0x
	/// </summary>
	Normal,

	/// <summary>
	/// 1.25x
	/// </summary>
	High,

	/// <summary>
	/// 1.5x
	/// </summary>
	Maximum
}

[Serializable]
public class SteamNewsItem {
	public string gid;
	public string title;
	public string url;
	public bool is_external_url;
	public string author;
	public string contents;
	public string feedlabel;
	public int date;
	public string feedname;
	public int feed_type;
	public int appid;
	public List<string> tags = new List<string>();
}

[Serializable]
public class SteamAppNews {
	public int appid;
	public List<SteamNewsItem> newsitems = new List<SteamNewsItem>();
	public int count;
}

[Serializable]
public class SteamAppNewsHolder {
	public SteamAppNews appnews = new SteamAppNews();
}