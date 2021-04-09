using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	//[RequireComponent(typeof(Graphic))]
	public class CUIColorAction : CUIAction {

		

		[SerializeField] private Color activatedColor = Color.white;

		[SerializeField] private Color deactivatedColor = Color.black;
		
		

		
		
		// --- Any components you need references to, with [HideInInspector] ---
		[HideInInspector] [SerializeField] private Graphic graphic;

		//Used to GetComponent to any references you need
		protected void Awake() {
			graphic = GetComponent<Graphic>();
		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;

			if (graphic == null) graphic = GetComponent<Graphic>();
			
			//Apply the state instantly
			if (instant) {
				graphic.color = activatedColor;
			}
			
			//Create tweens and use AddActiveTween()
			else {
				
				AddActiveTween(graphic.DOColor(activatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			if (graphic == null) graphic = GetComponent<Graphic>();
			
			//Apply the state instantly
			if (instant) {
				graphic.color = deactivatedColor;
			}
			
			//Create tweens and use AddActiveTween()
			else {
				
				AddActiveTween(graphic.DOColor(deactivatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}
			
			return true;
		}


	}
}