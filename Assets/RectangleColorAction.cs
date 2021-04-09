using CUI.Actions;
using DG.Tweening;
using ThisOtherThing.UI.Shapes;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace {

	public class RectangleColorAction : CUIAction {

	

		[SerializeField] private Color activatedColor = Color.white;

		[SerializeField] private Color deactivatedColor = Color.black;
		
		

		
		
		// --- Any components you need references to, with [HideInInspector] ---
		[HideInInspector] [SerializeField] private Rectangle graphic;

		//Used to GetComponent to any references you need
		protected void Awake() {
			graphic = GetComponent<Rectangle>();
		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;
			
			//Apply the state instantly
			if (instant) {
				graphic.ShapeProperties.FillColor = activatedColor;
			}
			
			//Create tweens and use AddActiveTween()
			else {

				var t = DOTween.To(() => graphic.ShapeProperties.FillColor, x => graphic.ShapeProperties.FillColor = x, activatedColor, duration);
				t.SetTarget(graphic);
				t.SetEase(easing);
				t.Play();
				AddActiveTween(t);
				
				//AddActiveTween(graphic.DOColor(activatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			//Apply the state instantly
			if (instant) {
				graphic.ShapeProperties.FillColor = deactivatedColor;
			}
			
			//Create tweens and use AddActiveTween()
			else {
				
				var t = DOTween.To(() => graphic.ShapeProperties.FillColor, x => graphic.ShapeProperties.FillColor = x, deactivatedColor, duration);
				t.SetTarget(graphic);
				t.SetEase(easing);
				t.Play();
				AddActiveTween(t);
				
				//AddActiveTween(graphic.DOColor(deactivatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}
			
			return true;
		}

	}

}