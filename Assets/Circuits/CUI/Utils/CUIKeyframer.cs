using System;
using System.Collections;
using System.Collections.Generic;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;

#endif

namespace CUI.Utils {
	public class CUIKeyframer : MonoBehaviour {
		private Coroutine animate_C;

#if UNITY_EDITOR

		private EditorCoroutine editorAnimate_C;

		[Button]
		public void EditorPreview() {
			if (editorAnimate_C != null) EditorCoroutineUtility.StopCoroutine(editorAnimate_C);
			editorAnimate_C = EditorCoroutineUtility.StartCoroutine(_play(), this);
		}

#endif

		private IEnumerator _play() {
			CUIKeyframe previous = null;

			foreach (var keyframe in keyframes) {
				if (previous == null) {
					keyframe.Select(true);
					previous = keyframe;
					continue;
				}

				keyframe.Select();

#if UNITY_EDITOR
				yield return new EditorWaitForSeconds(keyframe.animTime + keyframe.delayAfterPrevious);
#else
				yield return new WaitForSeconds(keyframe.animTime + keyframe.delayAfterPrevious);
#endif
				
				previous = keyframe;
			}
		}


		[Button]
		public void ResetState() {
			if (keyframes.Count > 0) {
				keyframes[0].Select(true);
			}
		}

		[ListDrawerSettings(CustomAddFunction = "AddNewKeyframe")]
		public List<CUIKeyframe> keyframes;


		private void AddNewKeyframe() {
			keyframes.Add(new CUIKeyframe(gameObject));
		}
		
		public void EditorOnKeyframeSelected(CUIKeyframe keyframe) {
			foreach (CUIKeyframe key in keyframes) {
				key.isSelected = key == keyframe;
			}
		}
		
	}

	[Serializable]
	public class CUIKeyframe {
		[HideInInspector] [SerializeField] private GameObject go;

		[HideInInspector] [SerializeField] private List<CUIKeyframeObjectState> states = new List<CUIKeyframeObjectState>();

		public string name;

		public float delayAfterPrevious = 0f;

		public float animTime = 0.3f;

		public Ease easing = Ease.InOutCubic;
		
		[HideInInspector] public bool isSelected = false;

		public CUIKeyframe(GameObject go) {
			this.go = go;
		}

		//[ShowIf("isSelected")]
		[ButtonGroup]
		public void Set() {
			states.Clear();

			foreach (var child in go.GetComponentsInChildren<Transform>()) {
				CUIKeyframeObjectState state = new CUIKeyframeObjectState();

				state.go = child.gameObject;

				state.pos = child.localPosition;
				state.scale = child.localScale;
				state.rot = child.localEulerAngles;

				if (child.TryGetComponent<RectTransform>(out RectTransform rectTransform)) {
					state.rectTransform = rectTransform;
					state.useRectTransform = true;
					//.rectPos = rectTransform.anchoredPosition;
					state.rectSize = rectTransform.rect.size;
					//state.rectScale = rectTransform.localScale;
				}

				if (child.TryGetComponent<Graphic>(out Graphic graphic)) {
					state.graphic = graphic;
					state.useGraphic = true;
					state.color = graphic.color;
					state.graphicAlpha = graphic.color.a;
				}

				if (child.TryGetComponent<CanvasGroup>(out CanvasGroup canvasGroup)) {
					state.canvasGroup = canvasGroup;
					state.useCanvasGroup = true;
					state.canvasGroupAlpha = canvasGroup.alpha;
				}


				states.Add(state);
			}
		}

		[ButtonGroup]
		public void Select(bool instant = false) {
			List<Tween> tweens = new List<Tween>();

			foreach (var state in states) {
				GameObject cur = state.go;

				if (instant) {
					cur.transform.localPosition = state.pos;
					cur.transform.localScale = state.scale;
					cur.transform.localEulerAngles = state.rot;

					if (state.useRectTransform) {
						state.rectTransform.sizeDelta = state.rectSize;
					}

					if (state.useGraphic) {
						state.graphic.color = state.color;
					}

					if (state.useCanvasGroup) {
						state.canvasGroup.alpha = state.canvasGroupAlpha;
					}
				}

				else {
					if (cur.transform.localPosition != state.pos) tweens.Add(cur.transform.DOLocalMove(state.pos, animTime));
					if (cur.transform.localScale != state.scale) tweens.Add(cur.transform.DOScale(state.scale, animTime));
					if (cur.transform.localEulerAngles != state.rot) tweens.Add(cur.transform.DOLocalRotate(state.rot, animTime));

					if (state.useRectTransform) {
						if (state.rectTransform.sizeDelta != state.rectSize) tweens.Add(state.rectTransform.DOSizeDelta(state.rectSize, animTime));
					}

					if (state.useGraphic) {
						if (state.graphic.color != state.color) tweens.Add(state.graphic.DOColor(state.color, animTime));
					}

					if (state.useCanvasGroup) {
						if (Math.Abs(state.canvasGroup.alpha - state.canvasGroupAlpha) > 0.01f) tweens.Add(state.canvasGroup.DOFade(state.canvasGroupAlpha, animTime));
					}
				}
			}


			if (!instant) {

				foreach (Tween tween in tweens) {
					if (delayAfterPrevious > 0f) tween.SetDelay(delayAfterPrevious);
					tween.SetEase(easing);
				}
				

				CUITweenPreview.PrepareTweens(tweens);
				CUITweenPreview.StartTweens();
			}

			if (!Application.isPlaying) {
				go.GetComponent<CUIKeyframer>().EditorOnKeyframeSelected(this);
			}
		}
	}


	[Serializable]
	public class CUIKeyframeObjectState {
		public GameObject go;

		public Vector3 pos;
		public Vector3 scale;
		public Vector3 rot;

		public RectTransform rectTransform;

		public bool useRectTransform = false;

		//public Vector2 rectPos;
		public Vector2 rectSize;
		//public Vector2 rectScale;

		public Graphic graphic;
		public bool useGraphic = false;
		public Color color;
		public float graphicAlpha;

		public CanvasGroup canvasGroup;
		public bool useCanvasGroup = false;
		public float canvasGroupAlpha;
	}
}