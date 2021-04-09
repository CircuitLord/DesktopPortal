using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using uWindowCapture;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.MusicIntegration {
	public static class SpotifyLink {
		
		private static Process _spotify;
		private static UwcWindow _spotifyWindow;

		public static bool spotifyLinked = false;
		





		public static IEnumerator TryLinkSpotify() {
			
			
			
			if (UwcManager.windows.Count <= 0) {
				yield return new WaitForSeconds(4);
			}

			//bool linkFound = false;

			foreach (var value in UwcManager.windows) {
				UwcWindow window = value.Value;

				if (window.title == "") continue;

				try {
					Process p = Process.GetProcessById(window.processId);
					
					if (p.ProcessName.StartsWith("Spotify")) {
						_spotify = p;
						_spotifyWindow = window;
						
						Debug.Log("Spotify linked!");

						//linkFound = true;

						spotifyLinked = true;

						yield break;
					}
					
				}
				catch {
					continue;
				}
				
			}

			spotifyLinked = false;
			
			Debug.Log("Spotify link failed :(");

		}

		public static void RequestUpdateTitle() {
			if (spotifyLinked) _spotifyWindow.RequestUpdateTitle();
		}


		public static string GetCurrentlyPlayingSongTitle() {
			
			if (!spotifyLinked) return "N/A";
			
			//_spotifyWindow.RequestUpdateTitle();
			
			 if (_spotifyWindow.title.StartsWith("Spotify")) {
				 return "Paused...";
			 }

			 return _spotifyWindow.title;
		}
		
		
	}
}