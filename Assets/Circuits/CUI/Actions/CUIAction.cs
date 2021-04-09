using System;
using System.Collections;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace CUI.Actions {
	public abstract class CUIAction : MonoBehaviour {


		public string actionName = "Action Name";

		[SerializeField] protected Ease easing = Ease.InOutCubic;

		[SerializeField] public float duration = 0.3f;
		

		
		protected List<Tween> tweens = new List<Tween>();


		protected float dur = 0.3f;

		[OnValueChanged("IsActivatedChanged")]
		[SerializeField] public bool isActivated = false;

		private bool isActivatedPrevious = false;


		private void IsActivatedChanged() {
			if (isActivated) Activate(false, true);
			else Deactivate(false, true);
		}

		public void Toggle(bool instant = false, bool force = false) {
			if (isActivated) Deactivate(instant, force);
			else Activate(instant, force);

			//isActivatedPrevious = isActivated;
		}
		
		
		public void ActivateNormal() {
			Activate(false, false);
		}
		
		public virtual bool Activate(bool instant = false, bool force = false) {

			if (isActivated && !force) return false;
			
			isActivated = true;
			
			KillTweens();

			dur = duration;
			
			return true;
		}
		
		public void DeactivateNormal() {
			Deactivate(false, false);
		}
		
		public virtual bool Deactivate(bool instant = false, bool force = false) {

			if (!isActivated && !force) return false;
			
			isActivated = false;
			
			KillTweens();
			
			dur = duration;
			
			return true;
		}



		protected void AddActiveTween(Tweener tween) {
			if (tweens.Contains(tween)) return;
			
			tweens.Add(tween);

			tween.onComplete += () => {
				RemoveActiveTween(tween);
			};


		}

		private void RemoveActiveTween(Tweener tween) {
			if (!tweens.Contains(tween)) return;
			tweens.Remove(tween);
		}

		public void KillTweens() {
			foreach (Tweener tween in tweens) {
				tween?.Kill();
			}
			
			tweens.Clear();
		}

		protected void StartEditorTweens() {
			if (!Application.isPlaying) {
				CUITweenPreview.PrepareTweens(tweens);
				CUITweenPreview.StartTweens();
			}
		}
		

	}

}