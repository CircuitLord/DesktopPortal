using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif
using UnityEngine;


namespace CUI.Utils {
	public class CUIRevealer : MonoBehaviour {
		[SerializeField] private float delayBetweenReveal = 0.2f;

		[SerializeField] private bool refreshChildrenEverytime = true;


		private List<CUIGroup> childrenGroups = new List<CUIGroup>();


		private Coroutine reveal_C;
		
		#if UNITY_EDITOR
		private EditorCoroutine reveal_EC;
		#endif

		private void Start() {
			if (!refreshChildrenEverytime) {
				childrenGroups = GetComponentsInChildren<CUIGroup>().ToList();
			}
		}


		[Button("Preview Reveal")]
		private void Preview_Reveal() {
			Reveal(true);
		}

		[Button("Preview Hide")]
		private void Preview_Hide() {
			Reveal(false);
		}

		public void Reveal(bool show = true) {
			if (refreshChildrenEverytime) {
				childrenGroups.Clear();
				childrenGroups = GetComponentsInChildren<CUIGroup>().ToList();
			}

			if (!Application.isPlaying) {
#if UNITY_EDITOR
				if (reveal_EC != null) EditorCoroutineUtility.StopCoroutine(reveal_EC);
				reveal_EC = EditorCoroutineUtility.StartCoroutine(RevealAnimate(show), this);
#endif
			}
			else {
				if (reveal_C != null) StopCoroutine(reveal_C);
				reveal_C = StartCoroutine(RevealAnimate(show));
			}

		}


		private IEnumerator RevealAnimate(bool show = true) {
			foreach (var child in childrenGroups) {
				CUIManager.Animate(child, show);

				yield return new WaitForSeconds(delayBetweenReveal);
			}
		}
	}
}