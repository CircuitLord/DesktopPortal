using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.UI.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


namespace DesktopPortal.UI {
	public class DPTopBarHandler : MonoBehaviour {

		[SerializeField] private List<DPTopBarElement> elements;


		[SerializeField] private GameObject buttonPF;
		[SerializeField] private GameObject spacerPF;
		[SerializeField] private GameObject invisibleSpacerPF;
		



		[SerializeField] private RectTransform holder;
		
		


		private void Start() {
			Generate();
		}


		private void Generate() {

			foreach (Transform child in holder) {
				Destroy(child.gameObject);
			}


			Instantiate(invisibleSpacerPF, holder);
			

			foreach (DPTopBarElement element in elements) {
				
				
				if (element.isSpacer) {
					Instantiate(spacerPF, holder);
				}
				else {


					GameObject go = Instantiate(buttonPF, holder);
					
					go.GetComponent<DPButton>().onMouseUp.AddListener(delegate {
						element.onPress.Invoke();
					});;

					RawImage img = go.GetComponentInChildren<RawImage>();
					img.texture = element.icon;
					SizeToParent(img, element.paddingOverride);
					


				}
				
				
				
				
			}
			
			Instantiate(invisibleSpacerPF, holder);
			
			
			
			
			
		}

		
		public Vector2 SizeToParent(RawImage image, float padding = 0) {
			var parent = image.transform.parent.GetComponent<RectTransform>();
			var imageTransform = image.GetComponent<RectTransform>();
			if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
			padding = 1 - padding;
			float w = 0, h = 0;
			float ratio = image.texture.width / (float)image.texture.height;
			var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
			if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90) {
				//Invert the bounds if the image is rotated
				bounds.size = new Vector2(bounds.height, bounds.width);
			}
			//Size by height first
			h = bounds.height * padding;
			w = h * ratio;
			if (w > bounds.width * padding) { //If it doesn't fit, fallback to width;
				w = bounds.width * padding;
				h = w / ratio;
			}
			imageTransform.sizeDelta = new Vector2(w, h);
			return imageTransform.sizeDelta;
		}
		

	}



	[Serializable]
	class DPTopBarElement {

		public Texture2D icon;
		public float paddingOverride = 0.0f;
		public bool isSpacer = false;

		public UnityEvent onPress;


	}
	
	
	
	
}