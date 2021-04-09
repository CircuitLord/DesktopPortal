using System;
using System.Collections;
using System.Collections.Generic;
using CUI.Components;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


namespace DesktopPortal.UI {
	
	[RequireComponent(typeof(CUIToggle))]
	public class SettingsListButton : MonoBehaviour {

		private CUIToggle toggle;


		private void Awake() {
			toggle = GetComponent<CUIToggle>();
		}

		[SerializeField] private Image selectedBG;

		//[SerializeField] private Image deselectedBG;

		

		private Tweener t1;
		
		
		public void AnimateSelected() {
			t1.Kill();

			t1 = selectedBG.DOFade(1f, 0.3f);
		}
		
		public void AnimateDeselected() {
			
			t1.Kill();

			t1 = selectedBG.DOFade(0f, 0.3f);

		}
		
		
	}
}