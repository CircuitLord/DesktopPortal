using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace DesktopPortal.Wristboard {
	public class SongTextScroller : MonoBehaviour {
		
		public TextMeshProUGUI currSongText;
		public float scrollSpeed = 10.0f;

		public float loopSpacing = 30f;

		public float songAreaWidth = 207.25f;
		

		private TextMeshProUGUI cloneText;

		private bool isReady = false;

		private float widthToLoop = 0f;
		
		// Use this for initialization
		public void Setup() {

			if (cloneText) DestroyImmediate(cloneText.gameObject);

			cloneText = Instantiate(currSongText, currSongText.transform, true) as TextMeshProUGUI;
			
			cloneText.transform.localPosition = new Vector3(currSongText.preferredWidth + loopSpacing, 0, 0);
			
			currSongText.transform.localPosition = new Vector3(-songAreaWidth, 0, 0);

			widthToLoop = (currSongText.preferredWidth * -1) - songAreaWidth - loopSpacing;
			


			isReady = true;
		}

		public void Stop() {
			
			isReady = false;
			
			if (cloneText) DestroyImmediate(cloneText.gameObject);
			
			currSongText.transform.localPosition = new Vector3(-songAreaWidth, 0, 0);
		}


		private void Update() {
			if (isReady) {

				float newX = currSongText.transform.localPosition.x - (scrollSpeed * Time.deltaTime);
				
				currSongText.transform.localPosition = new Vector3(newX, 0, 0);

				if (newX <= widthToLoop) {
					currSongText.transform.localPosition = new Vector3(-songAreaWidth, 0, 0);
				}



			}
		}



	}
}