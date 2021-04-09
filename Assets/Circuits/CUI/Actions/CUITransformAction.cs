using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	[RequireComponent(typeof(Transform))]
	public class CUITransformAction : CUIAction {


		//Any properties with private and SerializeField
		[SerializeField] private Vector3 activatedPos = Vector3.zero;
		[SerializeField] private Vector3 activatedRot = Vector3.zero;
		[SerializeField] private Vector3 activatedScale = Vector3.one;

		[Space]
		
		[SerializeField] private Vector3 deactivatedPos = Vector3.zero;
		[SerializeField] private Vector3 deactivatedRot = Vector3.zero;
		[SerializeField] private Vector3 deactivatedScale = Vector3.one;

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;
			
			if (instant) {
				transform.localScale = activatedScale;
				transform.localPosition = activatedPos;
				transform.localEulerAngles = activatedRot;
			}

			else {
				if (activatedScale != transform.localScale) {
					AddActiveTween(transform.DOScale(activatedScale, dur).SetEase(easing));
				}

				if (activatedPos != transform.localPosition) {
					AddActiveTween(transform.DOLocalMove(activatedPos, dur).SetEase(easing));
				}

				if (activatedRot != transform.localEulerAngles) {
					AddActiveTween(transform.DOLocalRotate(activatedRot, dur));
				}
				
				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			if (instant) {
				transform.localScale = deactivatedScale;
				transform.localPosition = deactivatedPos;
				transform.localEulerAngles = deactivatedRot;
			}

			else {
				if (deactivatedScale != transform.localScale) {
					AddActiveTween(transform.DOScale(deactivatedScale, dur).SetEase(easing));
				}

				if (deactivatedPos != transform.localPosition) {
					AddActiveTween(transform.DOLocalMove(deactivatedPos, dur).SetEase(easing));
				}

				if (deactivatedRot != transform.localEulerAngles) {
					AddActiveTween(transform.DOLocalRotate(deactivatedRot, dur));
				}
				
				StartEditorTweens();
			}
			
			return true;
		}


	}
}