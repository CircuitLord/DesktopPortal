using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	//[RequireComponent(typeof(Graphic))]
	public class _CUIAction_TEMPLATE : CUIAction {


		//Any properties with private and SerializeField


		// --- Any components you need references to, with [HideInInspector] ---
		

		//Used to GetComponent to any references you need
		protected void Reset() {

		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;
			
			//Apply the state instantly
			if (instant) {

			}
			
			//Create tweens and use AddActiveTween()
			else {
				
				//AddActiveTween(graphic.DOColor(activatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			//Apply the state instantly
			if (instant) {

			}
			
			//Create tweens and use AddActiveTween()
			else {
				
				//AddActiveTween(graphic.DOColor(deactivatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}
			
			return true;
		}


	}
}