using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	[RequireComponent(typeof(CanvasGroup))]
	public class CUICanvasGroupAction : CUIAction {


		//Any properties with private and SerializeField
		[SerializeField] public float activatedFade = 1f;
		[SerializeField] public float deactivatedFade = 0f;
		
		


		// --- Any components you need references to, with [HideInInspector] ---
		[HideInInspector] public CanvasGroup canvasGroup;
		

		//Used to GetComponent to any references you need
		protected void Reset() {
			canvasGroup = GetComponent<CanvasGroup>();
		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;

			//Apply the state instantly
			if (instant) {
				canvasGroup.alpha = activatedFade;
			}
			
			//Create tweens and use AddActiveTween()
			else {

				AddActiveTween(canvasGroup.DOFade(activatedFade, dur).SetEase(easing));
				
				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			//Apply the state instantly
			if (instant) {
				canvasGroup.alpha = deactivatedFade;
			}
			
			//Create tweens and use AddActiveTween()
			else {

				AddActiveTween(canvasGroup.DOFade(deactivatedFade, dur).SetEase(easing));
				
				StartEditorTweens();
			}
			
			return true;
		}


	}
}