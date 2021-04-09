using System.Collections.Generic;
using CUI.Layouts;
using UnityEngine;
using UnityEngine.UI;

namespace CUI.Components {
	
	[RequireComponent(typeof(Slider))]
	public class CUISlider : MonoBehaviour {

		public int increments = 10;
		
		
		[SerializeField] private RectTransform incrementsLayout;

		[SerializeField] private RectTransform incrementGO;
		

		private Slider _slider;
		public Slider slider {
			get {
				if (_slider == null) {
					_slider = GetComponent<Slider>();
				}
				return _slider;
			}
		}


		[Sirenix.OdinInspector.Button]
		public void ResetIncrementChildren() {

			var transforms = incrementsLayout.GetComponentsInChildren<Transform>();
			
			foreach (Transform t in transforms) {
				if (t == incrementsLayout) continue;
				DestroyImmediate(t.gameObject);
			}
		}


		[Sirenix.OdinInspector.Button]
		public void SolveIncrements() {


			if (incrementsLayout == null) return;

			//Cancel out the padding of half on each side
			float totalWidth = GetComponent<RectTransform>().rect.width - incrementGO.rect.width;
			float amt = totalWidth / (increments - 1);
			
			
			Vector2 anchor = anchor = new Vector2(0, 0.5f);
			
			for (int i = 0; i < increments; i++) {
				RectTransform go = Instantiate(incrementGO, incrementsLayout.transform);
				
				go.anchorMin = anchor;
				go.anchorMax = anchor;
				
				go.anchoredPosition = new Vector2((amt * i) + (incrementGO.rect.width / 2f), 0f);
				
			}


		}




	}
}