using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	[RequireComponent(typeof(RectTransform))]
	public class CUIRectTransformAction : CUIAction {


		//Any properties with private and SerializeField
		[SerializeField] private Vector2 activatedScale = Vector2.one;
		[SerializeField] private Vector2 activatedSize = Vector2.one;
		[SerializeField] private Vector2 activatedPos = Vector2.zero;
		[SerializeField] private float activatedRot = 0f;

		[Space]
		
		[SerializeField] private Vector2 deactivatedScale = Vector2.one;
		[SerializeField] private Vector2 deactivatedSize = Vector2.one;
		[SerializeField] private Vector2 deactivatedPos = Vector2.zero;
		[SerializeField] private float deactivatedRot = 0f;

		

		// --- Any components you need references to, with [HideInInspector] ---
		[HideInInspector] public RectTransform rectTransform;
		

		//Used to GetComponent to any references you need
		protected void Reset() {
			rectTransform = GetComponent<RectTransform>();
		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;
			
			if (instant) {
				rectTransform.localScale = activatedScale;
				rectTransform.sizeDelta = activatedSize;
				rectTransform.anchoredPosition = activatedPos;
				rectTransform.localEulerAngles = new Vector3(rectTransform.localEulerAngles.x, rectTransform.localEulerAngles.y, activatedRot);
			}

			else {
				if (activatedScale != (Vector2)rectTransform.localScale) {
					AddActiveTween(rectTransform.DOScale(activatedScale, dur).SetEase(easing));
				}

				if (activatedSize != rectTransform.rect.size) {
					AddActiveTween(rectTransform.DOSizeDelta(activatedSize, dur).SetEase(easing));
				}

				if (activatedPos != rectTransform.anchoredPosition) {
					AddActiveTween(rectTransform.DOAnchorPos(activatedPos, dur).SetEase(easing));
				}

				if (activatedRot != rectTransform.localEulerAngles.z) {
					AddActiveTween(rectTransform.DOLocalRotate(new Vector3(0f, 0f, activatedRot), dur));
				}
				
				StartEditorTweens();
			}
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			if (instant) {
				rectTransform.localScale = deactivatedScale;
				rectTransform.sizeDelta = deactivatedSize;
				rectTransform.anchoredPosition = deactivatedPos;
				rectTransform.localEulerAngles = new Vector3(rectTransform.localEulerAngles.x, rectTransform.localEulerAngles.y, deactivatedRot);
			}

			else {
				if (deactivatedScale != (Vector2)rectTransform.localScale) {
					AddActiveTween(rectTransform.DOScale(deactivatedScale, dur).SetEase(easing));
				}

				if (deactivatedSize != rectTransform.rect.size) {
					AddActiveTween(rectTransform.DOSizeDelta(deactivatedSize, dur).SetEase(easing));
				}

				if (deactivatedPos != rectTransform.anchoredPosition) {
					AddActiveTween(rectTransform.DOAnchorPos(deactivatedPos, dur).SetEase(easing));
				}

				if (deactivatedRot != rectTransform.localEulerAngles.z) {
					AddActiveTween(rectTransform.DOLocalRotate(new Vector3(0f, 0f, deactivatedRot), dur));
				}
				
				StartEditorTweens();
			}
			
			return true;
		}


	}
}