using System;
using System.Collections;
//using DesktopDuplication;
using UnityEngine;
using UnityEngine.UI;

namespace CCapture {
	public class CCaptureDDA : MonoBehaviour {
		
		
		//private DesktopDuplicator desktopDuplicator;
		//private Texture2D frame = null;

		[SerializeField] private RawImage output;


		private bool texSharingInitiated = false;
		
		private IntPtr monitorTextureHandle = IntPtr.Zero;
		
		private void Start() {
			
			
			//desktopDuplicator = new DesktopDuplicator(0);

			StartCoroutine(TestLoop());

		}


		private IEnumerator TestLoop() {

			while (true) {

				try {
					//var frame = desktopDuplicator.GetLatestFrame();
					
					//Texture2D texture = Texture2D.CreateExternalTexture(3440, 1440, TextureFormat.RGBA32, false, true, frame.NativePointer);

					//output.texture = texture;

				}
				catch (Exception e) {
					Debug.Log(e);
				}





				yield return null;
			}
		
		
		
		}
		
		
		

	}
}