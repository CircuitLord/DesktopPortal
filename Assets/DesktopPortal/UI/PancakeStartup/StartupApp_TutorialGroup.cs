using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using CUI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace DesktopPortal.UI.Welcome {
	
	public class StartupApp_TutorialGroup : CUIGroup {


		[SerializeField] private YoutubePlayer.YoutubePlayer ytPlayer;

		[SerializeField] private VideoPlayer player;

		[SerializeField] private Slider slider;


		[SerializeField] private CUIGroup myGroup;
		[SerializeField] private CUIGroup myGroup2;


		private double savedTime = 0;


		private void Update() {
			
			float yay = (float)player.time / (float)player.length;

			if (yay <= 0f) return;

			if (float.IsNaN(yay)) return;

			//Debug.Log(yay);
			
			slider.SetValueWithoutNotify(yay);


			if (Input.GetKeyDown(KeyCode.A)) {
				CUIManager.SwapAnimate(myGroup, myGroup2);
			}
			
			
			
		}


		public override void OnShowing() {
			base.OnShowing();

			if (ytPlayer.loaded) {
				player.Play();
			
				player.time = savedTime;
			}
			


		}



		public override void OnHiding() {
			base.OnHiding();

			savedTime = player.time;
			
			player.Pause();
		}


		public override void OnInit() {
			base.OnInit();
			
			slider.onValueChanged.AddListener(Seek);
			
			ytPlayer.PlayVideoAsync();

		}


		private void Seek(float amt) {

			player.time = player.length * amt;

		}
	}
	
	
}

