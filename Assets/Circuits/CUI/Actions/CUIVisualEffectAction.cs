using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;


namespace CUI.Actions {

	public enum CUIVFXPropertyType {
		BOOL,
		INT,
		FLOAT,
		VECTOR3
	}
	
	[RequireComponent(typeof(VisualEffect))]
	public class CUIVisualEffectAction : CUIAction {


		public string propertyName = "Property";

		public CUIVFXPropertyType propertyType = CUIVFXPropertyType.INT; 
		
		[ShowIf("@propertyType == CUIVFXPropertyType.BOOL")]
		[SerializeField] private bool activatedBool = true;

		[ShowIf("@propertyType == CUIVFXPropertyType.BOOL")]
		[SerializeField] private bool deactivatedBool = false;
		
		[ShowIf("@propertyType == CUIVFXPropertyType.INT")]
		[SerializeField] private int activatedInt = 1;

		[ShowIf("@propertyType == CUIVFXPropertyType.INT")]
		[SerializeField] private int deactivatedInt = 0;
		
		[ShowIf("@propertyType == CUIVFXPropertyType.FLOAT")]
		[SerializeField] private float activatedFloat = 1.0f;

		[ShowIf("@propertyType == CUIVFXPropertyType.FLOAT")]
		[SerializeField] private float deactivatedFloat = 0.0f;
		
		[ShowIf("@propertyType == CUIVFXPropertyType.VECTOR3")]
		[SerializeField] private Vector3 activatedVector3 = Vector3.one;

		[ShowIf("@propertyType == CUIVFXPropertyType.VECTOR3")]
		[SerializeField] private Vector3 deactivatedVector3 = Vector3.zero;
		

		
		
		// --- Any components you need references to, with [HideInInspector] ---
		[HideInInspector] public VisualEffect visualEffect;


		private int intState = 0;
		private float floatState = 0f;
		private Vector3 vector3State = Vector3.zero;
		
		
		
		//Used to GetComponent to any references you need
		protected void Reset() {
			visualEffect = GetComponent<VisualEffect>();
		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;
			
			//Apply the state instantly
			if (instant) {

				switch (propertyType) {
					case CUIVFXPropertyType.BOOL:
						visualEffect.SetBool(propertyName, activatedBool);
						break;
					case CUIVFXPropertyType.INT:
						visualEffect.SetInt(propertyName, activatedInt);
						break;
					case CUIVFXPropertyType.FLOAT:
						visualEffect.SetFloat(propertyName, activatedFloat);
						break;
					case CUIVFXPropertyType.VECTOR3:
						visualEffect.SetVector3(propertyName, activatedVector3);
						break;
				}

			}
			
			//Create tweens and use AddActiveTween()
			else {
				
				switch (propertyType) {
					case CUIVFXPropertyType.BOOL:
						visualEffect.SetBool(propertyName, activatedBool);
						break;
					
					case CUIVFXPropertyType.INT:
						AddActiveTween(DOTween.To((f => {
							visualEffect.SetInt(propertyName, Mathf.RoundToInt(f));
						}), intState, activatedInt, dur));
						
						break;
					
					case CUIVFXPropertyType.FLOAT:
						AddActiveTween(DOTween.To((f => {
							visualEffect.SetFloat(propertyName, f);
						}), floatState, activatedFloat, dur));

						break;
					case CUIVFXPropertyType.VECTOR3:
						
						/*
						DOTween.To((f => { visualEffect.SetInt(propertyName, Mathf.RoundToInt(f)); }), floatState, activatedFloat, dur));

						DOTween.To(() => activatedVector3, x=> activatedVector3 = x, activatedVector3, 2);
						*/
						
						

						break;
				}
				
				//AddActiveTween(graphic.DOColor(activatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}

			floatState = activatedFloat;
			intState = activatedInt;
			vector3State = activatedVector3;
			
			
			
			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
					
			//Apply the state instantly
			if (instant) {

				switch (propertyType) {
					case CUIVFXPropertyType.BOOL:
						visualEffect.SetBool(propertyName, deactivatedBool);
						break;
					case CUIVFXPropertyType.INT:
						visualEffect.SetInt(propertyName, deactivatedInt);
						break;
					case CUIVFXPropertyType.FLOAT:
						visualEffect.SetFloat(propertyName, deactivatedFloat);
						break;
					case CUIVFXPropertyType.VECTOR3:
						visualEffect.SetVector3(propertyName, deactivatedVector3);
						break;
				}

			}
			
			//Create tweens and use AddActiveTween()
			else {
				
				switch (propertyType) {
					case CUIVFXPropertyType.BOOL:
						visualEffect.SetBool(propertyName, deactivatedBool);
						break;
					
					case CUIVFXPropertyType.INT:
						AddActiveTween(DOTween.To((f => {
							visualEffect.SetInt(propertyName, Mathf.RoundToInt(f));
						}), intState, deactivatedInt, dur));
						
						break;
					
					case CUIVFXPropertyType.FLOAT:
						AddActiveTween(DOTween.To((f => {
							visualEffect.SetFloat(propertyName, f);
						}), floatState, deactivatedFloat, dur));

						break;
					case CUIVFXPropertyType.VECTOR3:
						
						/*
						DOTween.To((f => { visualEffect.SetInt(propertyName, Mathf.RoundToInt(f)); }), floatState, deactivatedFloat, dur));

						DOTween.To(() => deactivatedVector3, x=> deactivatedVector3 = x, deactivatedVector3, 2);
						*/
						
						

						break;
				}
				
				//AddActiveTween(graphic.DOColor(deactivatedColor, dur).SetEase(easing));

				StartEditorTweens();
			}

			floatState = deactivatedFloat;
			intState = deactivatedInt;
			vector3State = deactivatedVector3;

			
			return true;
		}


	}
}