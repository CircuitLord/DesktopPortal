using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DPCore;
using UnityEngine;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;


namespace DesktopPortal.Sounds {
	public class SoundManager : MonoBehaviour {

		public static SoundManager I;

		[SerializeField] private AudioSource _audioSource;

		[SerializeField] private AudioClip _activation;
		[SerializeField] private AudioClip _activationFail;
		[SerializeField] private AudioClip _hover;
		[SerializeField] private AudioClip _focusChange;
		[SerializeField] private AudioClip _focusChangeFail;


		private void Awake() {
			I = this;
			//Test();
			
			
			
			//_audioSource.Play();
			
		}





		private void Test() {
			Debug.Log("getting audio devices");
			
			for (int deviceId = 0; deviceId < WaveOut.DeviceCount; deviceId++)
			{
				var capabilities = WaveOut.GetCapabilities(deviceId);
				Debug.Log(capabilities.NameGuid + " " + capabilities.NameGuid);
			}
			
		
		}



		public void PlaySoundOnDPCam(DPCameraOverlay dpCam, Vector2 elementPos, DPSoundEffect sound) {

			Vector3 pos = dpCam.GetWorldPositionOverlayElement(elementPos, Vector3.zero);
			
			PlaySoundInWorld(pos, sound);
		}
		


		public void PlaySoundInWorld(Vector3 pos, DPSoundEffect sound) {
			
			
			//_audioSource.transform.SetParent();

			_audioSource.transform.position = pos;
			
			switch (sound) {
				
				case DPSoundEffect.Activation:
					_audioSource.PlayOneShot(_activation);
					break;
				
				case DPSoundEffect.ActivationFail:
					_audioSource.PlayOneShot(_activationFail);
					break;
				
				case DPSoundEffect.Hover:
					_audioSource.PlayOneShot(_hover);
					break;
				
				case DPSoundEffect.FocusChange:
					_audioSource.PlayOneShot(_focusChange);
					break;
					
				case DPSoundEffect.FocusChangeFail:
					_audioSource.PlayOneShot(_focusChangeFail);
					break;


			}
			
			
			
			
		}
		
		
		


	}


	public enum DPSoundEffect {
		Activation = 0,
		Hover = 1,
		FocusChange = 2,
		FocusChangeFail = 3,
		ActivationFail = 4
	}
	
}