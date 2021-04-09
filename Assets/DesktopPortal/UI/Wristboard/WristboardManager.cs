using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using CUI;
using DesktopPortal.IO;
using UnityEngine;
using DesktopPortal.MusicIntegration;
using DesktopPortal.Overlays;
using DesktopPortal.UI;
using DG.Tweening;
using DPCore;
using TMPro;
using Valve.VR;
using WinStuff;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.Wristboard {
	public class WristboardManager : MonoBehaviour {

		public static WristboardManager I;
		
		[Header("Wristboard Script")] [SerializeField]
		public DPCameraOverlay _wristOverlay;
		
		[Header("UI Elements")] [SerializeField]
		private TextMeshProUGUI currTimeText;

		[SerializeField] private TextMeshProUGUI currDateText;
		[SerializeField] private TextMeshProUGUI currSongText;


		//[SerializeField] private TextMeshProUGUI notifTitleText;
		//[SerializeField] private TextMeshProUGUI notifBodyText;


		[SerializeField] private CUIGroup mediaCUI;
		[SerializeField] private CUIGroup songScrollerCUI;
		[SerializeField] private CUIGroup mediaControlButtonsCUI;
		
		
		
		
		

		[Header("Configuration")] [SerializeField]
		private float fastUpdateRate = 5f;

		[SerializeField] private float mediumUpdateRate = 15f;

		[SerializeField] private float lookAtThreshold = 0.9f;

		[SerializeField] private float pointAtThreshold = 0.8f;

		
		


		[Header("Scripts")] 
		
		[SerializeField] private SongTextScroller _songTextScroller;

		[SerializeField] private TheBarManager _theBarManager;





		[Header("Current State")] public bool isBeingLookedAt = false;
		public bool isBeingPointedAt = false;

		public int anchorHand = 0;

		private bool isVisible = false;
		
		
		//Private stuff
		private string _prevSpotifyState = "";



		[Tooltip("In which interval should the CPU usage be updated?")]
		[SerializeField] private float cpuUpdateInterval = 1;


		private int _processorCount;
		private float _cpuUsage;
		private Thread _cpuThread;
		private float _lastCpuUsage;
		
		
		
		private void OnValidate() {
			// We want only the physical cores but usually
			// this returns the twice as many virtual core count
			//
			// if this returns a wrong value for you comment this method out
			// and set the value manually
			
		}

		private void Awake() {
			I = this;
			WristBubble.wristboardDP = _wristOverlay;
		}
		
		
		
		

		private void Start() {


			SpotifyLink.GetCurrentlyPlayingSongTitle();
			
			
			
			LinkToSpotify();
			
			

			StartCoroutine(UpdateInfoFast());
			StartCoroutine(UpdateInfoMedium());
			
			
			_wristOverlay.onInteractedWith += delegate(bool b) {
			
				//OverlayInteractionManager.I.TryEnableInteraction(b);
				
			};
			
			_wristOverlay.onInitialized += ReloadTransform;
			

			_wristOverlay.onInteractedWith += OnInteractedWith;
			_wristOverlay.onOverlayTempHiding += OnLookShowing;


			_processorCount = SystemInfo.processorCount / 2;
			
			// setup the thread
			_cpuThread = new Thread(UpdateCPUUsage)
			{
				IsBackground = true,
				// we don't want that our measurement thread
				// steals performance
				Priority = System.Threading.ThreadPriority.BelowNormal
			};

			// start the cpu usage thread
			//_cpuThread.Start();

		}
		
		
		private void Update()
		{
			
			return;
			
			// for more efficiency skip if nothing has changed
			if (Mathf.Approximately(_lastCpuUsage, _cpuUsage)) return;

			// the first two values will always be "wrong"
			// until _lastCpuTime is initialized correctly
			// so simply ignore values that are out of the possible range
			if (_cpuUsage < 0 || _cpuUsage > 100) return;

			// I used a float instead of int for the % so use the ToString you like for displaying it
			//cpuCounterText.text = CpuUsage.ToString("F1") + "% CPU";
			Debug.Log(_cpuUsage.ToString("F1") + "% CPU");

			// Update the value of _lasCpuUsage
			_lastCpuUsage = _cpuUsage;
		}
		

		public void ReloadTransform() {
			
			if (DPSettings.config.wristHandLeft) _wristOverlay.SetOverlayTrackedDevice(DPOverlayTrackedDevice.LeftHand);
			else _wristOverlay.SetOverlayTrackedDevice(DPOverlayTrackedDevice.RightHand);
			
			_wristOverlay.SetOverlayTransform(DPSettings.config.wristPos, DPSettings.config.wristRot, true, true);
		}

		public void SetToDefaultTransform() {

			Vector3 goodPos, goodRot;
			
			if (_wristOverlay.overlay.trackedDevice == DPOverlayTrackedDevice.RightHand) {
				goodPos = new Vector3(0.02f, 0.06f, -0.15f);
				goodRot = new Vector3(120f, 25f, -69f);
			}
			else {
				goodPos = new Vector3(-0.03f, 0.07f, -0.16f);
				goodRot = new Vector3(140f, -57f, 43f);
			}
			
			_wristOverlay.SetOverlayTransform(goodPos, goodRot, true, true);

			
		}

		private void OnApplicationQuit() {

			DPSettings.config.wristPos = _wristOverlay.transform.localPosition;
			DPSettings.config.wristRot = _wristOverlay.transform.localEulerAngles;
			
			DPSettings.SaveSettingsJson();
		}




		private void OnInteractedWith(bool interacting) {


			if (interacting) {
				foreach (WristBubble bubble in WristBubble.bubbles) {
					CUIManager.Animate(bubble.cuiGroup, true);
				}
			}
			else {
				foreach (WristBubble bubble in WristBubble.bubbles) {
					if (!bubble.isPinned) CUIManager.Animate(bubble.cuiGroup, false);
				}
			}
			


			if (interacting) {
				
				
				CUIManager.SwapAnimate(songScrollerCUI, mediaControlButtonsCUI);
					
				
				
				
				
			}
			else {
				CUIManager.SwapAnimate(mediaControlButtonsCUI, songScrollerCUI);
			}
			
			
			
		}

		private void OnLookShowing(bool visible) {

			WristBubble.wristVisible = !visible;
			
			CUIManager.Animate(songScrollerCUI, CUIAnimation.FadeOut);

			if (!visible) {
				CUIManager.Animate(mediaCUI, CUIAnimation.FadeInDown, 0.5f);
				CUIManager.Animate(songScrollerCUI, CUIAnimation.FadeIn);
			}
			else {
				CUIManager.Animate(mediaCUI, CUIAnimation.FadeOutUp);
				CUIManager.Animate(songScrollerCUI, CUIAnimation.FadeOut);
			}
			
		}

		
		
		public void TempDisableWrist() {
			
			//If we just opened it, disable the wrist
			if (TheBarManager.isOpened) {
				_wristOverlay.TransitionOverlayOpacity(0f, 0.3f, Ease.InOutCubic, false);
				OverlayInteractionManager.I.TryEnableInteraction(true, true);
			}
		}
		
		
		
		


		public void TogglePlay() {
			WinNative.keybd_event(0xB3, 0, 1, 0);
			//keybd_event(0xB3, 0, 0, IntPtr.Zero);
			
			StartCoroutine(UpdateSongText());
			
			

		}
		
		public void SkipSong() {
			WinNative.keybd_event(0xB0, 0, 1, 0);
			//keybd_event(0xB0, 0, 0, IntPtr.Zero);
			
			StartCoroutine(UpdateSongText());
		}

		public void PreviousSong() {
			WinNative.keybd_event(0xB1, 0, 1, 0);
			//keybd_event(0xB1, 0, 0, IntPtr.Zero);

			StartCoroutine(UpdateSongText());
		}


		private void LinkToSpotify() {
			if (!SpotifyLink.spotifyLinked) {
				StartCoroutine(SpotifyLink.TryLinkSpotify());
			}
		}


		private IEnumerator UpdateSongText() {
			
			yield return new WaitForSeconds(0.4f);
			
			SpotifyLink.RequestUpdateTitle();

			yield return new WaitForSeconds(0.4f);
			
			string spotifyText = SpotifyLink.GetCurrentlyPlayingSongTitle();
			currSongText.SetText(spotifyText);

			if (spotifyText != _prevSpotifyState) {
				_prevSpotifyState = spotifyText;

				if (spotifyText != "N/A" && spotifyText != "Paused...") {
					_songTextScroller.Setup();
				}
				else {
					_songTextScroller.Stop();
				}
			}
		}



		private IEnumerator UpdateInfoFast() {
			
			
			while (true) {
				

				
				
				//Current time:
				currTimeText.SetText(DateTime.Now.ToShortTimeString());

				//Spotify:
				StartCoroutine(UpdateSongText());


				yield return new WaitForSeconds(fastUpdateRate);
			}
		}

		private IEnumerator UpdateInfoMedium() {
			while (true) {
				//Current date:
				currDateText.SetText(DateTime.Now.ToShortDateString());


				yield return new WaitForSeconds(mediumUpdateRate);
			}
		}
		
		/// <summary>
		/// Runs in Thread
		/// </summary>
		private void UpdateCPUUsage()
		{
			var lastCpuTime = new TimeSpan(0);

			// This is ok since this is executed in a background thread
			while (true)
			{
				var cpuTime = new TimeSpan(0);

				// Get a list of all running processes in this PC
				var AllProcesses = Process.GetProcesses();

				// Sum up the total processor time of all running processes
				cpuTime = AllProcesses.Aggregate(cpuTime, (current, process) => current + process.TotalProcessorTime);

				// get the difference between the total sum of processor times
				// and the last time we called this
				var newCPUTime = cpuTime - lastCpuTime;

				// update the value of _lastCpuTime
				lastCpuTime = cpuTime;

				// The value we look for is the difference, so the processor time all processes together used
				// since the last time we called this divided by the time we waited
				// Then since the performance was optionally spread equally over all physical CPUs
				// we also divide by the physical CPU count
				_cpuUsage = 100f * (float)newCPUTime.TotalSeconds / cpuUpdateInterval / _processorCount;

				// Wait for UpdateInterval
				Thread.Sleep(Mathf.RoundToInt(cpuUpdateInterval * 1000));
			}
		}
	}
}