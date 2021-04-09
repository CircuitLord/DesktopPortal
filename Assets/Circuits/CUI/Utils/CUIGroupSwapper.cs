using System;
using System.Collections;
using System.Collections.Generic;
using CUI;
using Sirenix.OdinInspector;
using UnityEngine;


namespace CUI.Utils {
	public class CUIGroupSwapper : MonoBehaviour {



		[HideInInspector] public List<CUIGroup> groups = new List<CUIGroup>();

		[SerializeField] private int defaultIndex = -1;
		

		[HideInInspector] public CUIGroup activeGroup;


		private void Start() {
			FindChildrenGroups();
			
			if (defaultIndex > -1) Swap(defaultIndex, true);
		}

		[Button]
		public void FindChildrenGroups() {
			groups.Clear();

			foreach (Transform child in transform) {
				CUIGroup found = child.GetComponent<CUIGroup>();
				
				if (found != null) groups.Add(found);
			}

			if (!Application.isPlaying) Debug.Log(gameObject.name + " found " + groups.Count +  " CUIGroups!");
		}


		public void Swap(int index) {
		
			Swap(groups[index]);
		
		}

		public void Swap(int index, bool instant) {
			Swap(groups[index], instant);
		}
	
		public void Swap(CUIGroup newGroup, bool instant = false) {

			if (newGroup == activeGroup) return;
		
			if (activeGroup) {
				if (instant) CUIManager.SwapAnimate(activeGroup, newGroup, -1f, CUIAnimation.InstantOut, CUIAnimation.InstantIn);
				else CUIManager.SwapAnimate(activeGroup, newGroup);
			}
			else {
				if (instant) CUIManager.Animate(newGroup, CUIAnimation.InstantIn);
				else CUIManager.Animate(newGroup, true);
			}
		
			activeGroup = newGroup;


		}

	
	
	
	
	
	}
}