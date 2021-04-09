using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using DesktopPortal.Haptics;
using DesktopPortal.Sounds;
using DesktopPortal.Steam;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Debug = UnityEngine.Debug;

namespace DesktopPortal.UI {
	public class TemplateGame : MonoBehaviour {


		public RawImage gameArt;

		[HideInInspector] public LibraryGameData libraryGameData;

		public Image statusIcon;

		public Image hoverBlur;
		public Image hoverGradient;

		public Image playIcon;

		public Image gameRunningIcon;
		public Image gameQuitIcon;

		public TextMeshProUGUI fallbackText;


		
		
		
		[Header("Current Status")]

		public bool isHovering = false;

		private bool isInAnimation = false;


		public bool isGameRunning = false;



		public void GameRunning(bool state) {

			if (state == isGameRunning) return;

			isGameRunning = state;

			if (state == true) {
				//StartCoroutine(PlayGameRunningAnim());
			}
			
			



		}

		private void Awake() {
			hoverBlur.material = new Material(hoverBlur.material);
		}


		public void Reset() {
			gameRunningIcon.gameObject.SetActive(false);
			playIcon.gameObject.SetActive(true);
			playIcon.DOFade(0f, 0f);
			hoverGradient.DOFade(0f, 0f);
			gameQuitIcon.DOFade(0f, 0f);
			SetBlurOpacity(0);

		}


		/*public IEnumerator PlayGameRunningAnim() {

			isGameRunning = true;

			playIcon.gameObject.SetActive(false);
			
			gameRunningIcon.gameObject.SetActive(true);
			gameRunningIcon.rectTransform.localScale = Vector3.zero;

			gameRunningIcon.rectTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic);

			
			DOTween.To(SetBlurOpacity, 0f, 2.5f, DPUIConfig.hoverFadeDuration);
			hoverGradient.DOFade(0.3f, DPUIConfig.hoverFadeDuration);

			while (isGameRunning) {
				Vector3 newRot = new Vector3(0, 0, -1440);
				gameRunningIcon.rectTransform.DORotate(newRot, 5f, RotateMode.FastBeyond360).SetRelative();
				
				yield return new WaitForSeconds(4.1f);
			}
			
			gameRunningIcon.rectTransform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InOutCubic);
			
			yield return new WaitForSeconds(0.5f);
			
			
			gameRunningIcon.gameObject.SetActive(false);
			
			
			playIcon.gameObject.SetActive(true);
			
			DOTween.To(SetBlurOpacity, 2.5f, 0f, DPUIConfig.hoverFadeDuration);
			hoverGradient.DOFade(0f, DPUIConfig.hoverFadeDuration);

			yield break;

		}*/

		

		private IEnumerator OnHoverAnim() {
			
			//while (isInAnimation) {
			//	yield return null;
			//}

			//isInAnimation = true;
			
			_animTweeners.Clear();
			
			_animTweeners.Add(gameArt.rectTransform.DOScale(1.07f, 0.2f).SetEase(Ease.OutCubic));
				
			/*_animTweeners.Add(DOTween.To(SetBlurOpacity, 0f, 2.5f, DPUIConfig.hoverFadeDuration));
			_animTweeners.Add(hoverGradient.DOFade(0.3f, DPUIConfig.hoverFadeDuration));

			_animTweeners.Add(playIcon.DOFade(1.0f, DPUIConfig.hoverFadeDuration));*/
			
			yield return new WaitForSeconds(0.3f);

			//isInAnimation = false;
		}
		
		private IEnumerator OnLeaveAnim() {

			//while (isInAnimation) {
			//	yield return null;
			//}

			//isInAnimation = true;
			
			gameArt.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutCubic);
				
			/*DOTween.To(SetBlurOpacity, 2.5f, 0f, DPUIConfig.hoverFadeDuration);
			hoverGradient.DOFade(0f, DPUIConfig.hoverFadeDuration);
			
			playIcon.DOFade(0f, DPUIConfig.hoverFadeDuration);*/

			//	isInAnimation = false;
			
			isHovering = false;

			yield break;
		}


		private Coroutine hoverC;
		private List<Tweener> _animTweeners = new List<Tweener>();
		public void OnHover() {
			if (isHovering) {
				return;
			}
			
			isHovering = true;
			
			//Do different stuff if the game is launched:
			if (isGameRunning) {

				gameRunningIcon.DOFade(0f, 0.3f);
				gameQuitIcon.DOFade(1f, 0.3f);

				hoverGradient.DOFade(0.9f, 0.3f);


			}
			else {
				hoverC = StartCoroutine(OnHoverAnim());
			}
			
			
			HapticsManager.SendHaptics(HapticsPreset.UIHover);
			
		}
		
		public void OnLeave() {

			//Do different stuff if the game is launched:
			if (isGameRunning) {
				
				gameRunningIcon.DOFade(1f, 0.3f);
				gameQuitIcon.DOFade(0f, 0.3f);
				hoverGradient.DOFade(0f, 0.3f);
				isHovering = false;

			}
			else {
				gameQuitIcon.DOFade(0f, 0.3f);


				if (hoverC != null) {
					StopCoroutine(hoverC);
					foreach (Tweener t in _animTweeners) {
						t.Kill();
					}
					_animTweeners.Clear();
				}
				
				StartCoroutine(OnLeaveAnim());
			}

		}

		private bool isDown = false;
		public void OnDown() {

			if (isDown) return;

			isDown = true;
			
			playIcon.rectTransform.DOScale(1.2f, 0.3f);
			gameQuitIcon.rectTransform.DOScale(1.2f, 0.3f);
			
			//SoundManager.I.PlaySound(DPSoundEffect.Activation);
		}

		public void OnUp() {

			isDown = false;

			if (isHovering && isGameRunning) {
				//TODO: show confirmation to quit the game
				//StartCoroutine(libraryUI.AnimFadeTemp(true));
				//PopupDPUI.instance.Init(CloseGame, PopupType.CONFIRMATION, "Are you sure you want to close " + dpGame.name + "?");

				CloseGame(true);
			}

			else if (isHovering) {
				//TODO: Launch game
				//StartCoroutine(libraryUI.AnimFadeTemp(true));
				
				//OpenVR.Applications.

				LaunchGame(true);



				//PopupDPUI.instance.Init(LaunchGame, PopupType.CONFIRMATION, "Are you sure you want to launch " + dpGame.name + "?");
			}
			
			playIcon.rectTransform.DOScale(1f, 0.3f);
			gameQuitIcon.rectTransform.DOScale(1f, 0.3f);
			
			
			
		}

		public bool CloseGame(bool yes) {
			//StartCoroutine(libraryUI.AnimFadeTemp(false));

			if (yes) {
				int pid = (int)OpenVR.Applications.GetApplicationProcessId(libraryGameData.appKey);
				//OpenVR.Applications.
				Debug.Log(libraryGameData.appKey);
				Process p = Process.GetProcessById(pid);
				p.Kill();
			}
			
			return true;
		}

		
		public bool LaunchGame(bool yes) {
			//StartCoroutine(libraryUI.AnimFadeTemp(false));

			if (yes) {
				OpenVR.Applications.LaunchApplication(libraryGameData.appKey);
				
				//TODO: Launcher manager
				//UIStateManager.instance.CloseAll();
				//TheBarManager.I.HideAll();
				
			}
			
			return true;
		}



		private void SetBlurOpacity(float o) {
			hoverBlur.material.SetFloat("_Radius", o);
		}






	}
}