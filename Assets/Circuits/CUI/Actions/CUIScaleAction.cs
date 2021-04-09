using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	[RequireComponent(typeof(Transform))]
	public class CUIScaleAction : CUIAction {


		//Any properties with private and SerializeField
		[SerializeField] private Vector3 activatedScale = Vector3.one;

		[Space]
		
		[SerializeField] private Vector3 deactivatedScale = Vector3.one;
		



		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;
			
			if (instant) {
				transform.localScale = activatedScale;
			}

			else {
				if (activatedScale != transform.localScale) {
					AddActiveTween(transform.DOScale(activatedScale, dur).SetEase(easing));
				}

				
				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			if (instant) {
				transform.localScale = deactivatedScale;
			}

			else {
				if (deactivatedScale != transform.localScale) {
					AddActiveTween(transform.DOScale(deactivatedScale, dur).SetEase(easing));
				}

							
				StartEditorTweens();
			}
			
			return true;
		}


	}
}