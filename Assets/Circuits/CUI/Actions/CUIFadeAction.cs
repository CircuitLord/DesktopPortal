using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	//[RequireComponent(typeof(Graphic))]
	public class CUIFadeAction : CUIAction {


		//Any properties with private and SerializeField
		[SerializeField] public float activatedFade = 1f;
		[SerializeField] public float deactivatedFade = 0f;
		
		


		// --- Any components you need references to, with [HideInInspector] ---
		[HideInInspector] public Graphic graphic;
		

		//Used to GetComponent to any references you need
		protected void Awake() {
			graphic = GetComponent<Graphic>();
		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;

			if (graphic == null) graphic = GetComponent<Graphic>();

			//Apply the state instantly
			if (instant) {
				graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, activatedFade);
			}
			
			//Create tweens and use AddActiveTween()
			else {

				AddActiveTween(graphic.DOFade(activatedFade, dur).SetEase(easing));
				
				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			if (graphic == null) graphic = GetComponent<Graphic>();
			
			//Apply the state instantly
			if (instant) {
				graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, deactivatedFade);
			}
			
			//Create tweens and use AddActiveTween()
			else {

				AddActiveTween(graphic.DOFade(deactivatedFade, dur).SetEase(easing));
				
				StartEditorTweens();
			}
			
			return true;
		}


	}
}