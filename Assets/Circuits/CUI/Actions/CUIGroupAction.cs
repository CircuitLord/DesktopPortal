using System;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


namespace CUI.Actions {
	
	[RequireComponent(typeof(CUIGroup))]
	public class CUIGroupAction : CUIAction {


		//Any properties with private and SerializeField
		[SerializeField] private bool invertShowingHiding = false;
		

		// --- Any components you need references to, with [HideInInspector] ---
		[HideInInspector]
		[SerializeField] private CUIGroup group;
		
		

		//Used to GetComponent to any references you need
		protected void Reset() {
			group = GetComponent<CUIGroup>();
		}
		

		public override bool Activate(bool instant = false, bool force = false) {
			if (!base.Activate(instant, force)) return false;

			bool showing = true;
			if (invertShowingHiding) showing = false;
			
			CUIManager.Animate(group, showing, -1f, instant);

			return true;
		}

		public override bool Deactivate(bool instant = false, bool force = false) {
			if (!base.Deactivate(instant, force)) return false;
			
			bool showing = false;
			if (invertShowingHiding) showing = true;
			
			CUIManager.Animate(group, showing, -1f, instant);

			return true;
		}


	}
}