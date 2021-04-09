//using DG.DOTweenEditor;

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
#if UNITY_EDITOR
using DG.DOTweenEditor;
using UnityEditor;

#endif

namespace CUI.DOTweenUtil {
	public static class CUITweenPreview {
		private static List<Tween> all = new List<Tween>();

		public static void PrepareTweens(List<Tween> tweens) {
#if UNITY_EDITOR
			foreach (Tween tween in tweens) {
				//DOTweenEditorPreview.PrepareTweenForPreview(tween, false, true, false);
				PrepareTween(tween, false, false, false);
			}
#endif
		}

		public static void PrepareTween(Tween tween, bool clearCallbacks, bool preventAutoKill, bool andPlay) {
#if UNITY_EDITOR
			all.Add(tween);

			tween.onComplete += () => { CheckEndPreview(tween); };

			DOTweenEditorPreview.PrepareTweenForPreview(tween, clearCallbacks, preventAutoKill, andPlay);
#endif
		}


		public static void StartTweens() {
#if UNITY_EDITOR
			DOTweenEditorPreview.Start();
#endif
		}

		public static void StopTweens() {
#if UNITY_EDITOR
			DOTweenEditorPreview.Stop();
#endif
		}


		private static void CheckEndPreview(Tween endingTween) {
			if (!all.Contains(endingTween)) return;
			all.Remove(endingTween);

			if (all.Count <= 0) {
				StopTweens();
			}
		}
	}
}