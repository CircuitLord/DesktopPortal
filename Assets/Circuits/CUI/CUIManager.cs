using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CUI.DOTweenUtil;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace CUI {
	public class CUIManager : MonoBehaviour {
		private static CUIManager _instance = null;

		private static CUIManager I {
			get {
				if (_instance == null) _instance = FindObjectOfType<CUIManager>();

				if (_instance == null) {
					GameObject go = new GameObject("[CUIManager]");
					_instance = go.AddComponent<CUIManager>();
				}

				return _instance;
			}
			set { _instance = value; }
		}


		private static float defaultAnimTime = 0.2f;

		private static Ease animEasing = Ease.InOutCubic;


		private static List<CUIAnimationState> currentAnimations = new List<CUIAnimationState>();


		//private readonly Dictionary<CUIAnimation, CUIAnimationData> animDict = {
		//	{ CUIAnimation.FadeIn, new CUIAnimationData {} }
		//};


		private void Awake() {
			I = this;
		}


		public static void AnimateAll(List<CUIGroup> groups, bool showing = true, float totalTime = -1f) {

			groups.First().StartCoroutine(_animateAll(groups, showing, totalTime));

		}

		private static IEnumerator _animateAll(List<CUIGroup> groups, bool showing = true, float totalTime = -1f) {
			
			
			foreach (CUIGroup @group in groups) {
				Animate(@group, showing);
				
				yield return new WaitForSeconds(totalTime / groups.Count);
				
			}
			
		}

		/// <summary>
		/// Used to swap out two groups. One animates in, the other out.
		/// </summary>
		/// <param name="oldGroup">The group to animate out</param>
		/// <param name="newGroup">The group to animate in</param>
		/// <param name="animOld"></param>
		/// <param name="animNew"></param>
		public static void SwapAnimate(CUIGroup oldGroup, CUIGroup newGroup, float overrideTime = -1f, CUIAnimation animOld = CUIAnimation.FadeOut,
			CUIAnimation animNew = CUIAnimation.FadeIn) {
			if (oldGroup == newGroup) return;

			I.AnimateInternal(oldGroup, animOld, overrideTime);
			I.AnimateInternal(newGroup, animNew, overrideTime);
		}

		public static void Animate(CUIGroup view, bool showing = true, float overrideTime = -1f, bool instant = false) {
			CUIAnimation anim;

			if (showing) {
				if (instant) anim = CUIAnimation.InstantIn;
				else anim = view.showingAnimation;
			}
			else {
				if (instant) anim = CUIAnimation.InstantOut;
				else anim = view.hidingAnimation;
			}
			
			
			I.AnimateInternal(view, anim, overrideTime);
		}

		public static void Animate(CUIGroup view, CUIAnimation anim, float overrideTime = -1f) {
			I.AnimateInternal(view, anim, overrideTime);
		}


		private void AnimateInternal(CUIGroup cuiGroup, CUIAnimation anim, float overrideTime = -1f) {
			KillActiveAnimations(cuiGroup);


			float animTime = defaultAnimTime;
			if (overrideTime > 0f) animTime = overrideTime;


			CUIAnimationState state = new CUIAnimationState {
				cuiGroup = cuiGroup,
				finalPos = cuiGroup.rectTransform.localPosition
			};

			Vector3 curPos = cuiGroup.rectTransform.localPosition;

			Vector3 posAmt = Vector3.zero;
			Vector3 scaleAmt = Vector3.one;

			Rect rect = cuiGroup.rectTransform.rect;

			switch (anim) {
				case CUIAnimation.FadeIn:
					state.isShowing = true;
					break;
				
				case CUIAnimation.FadeOut:
					state.isShowing = false;
					break;
				
				case CUIAnimation.InstantIn:
					state.isShowing = true;
					state.isInstant = true;
					break;
				
				case CUIAnimation.InstantOut:
					state.isShowing = false;
					state.isInstant = true;
					break;


				case CUIAnimation.FadeInDown:
					posAmt = new Vector3(0f, rect.height, 0f);
					state.isShowing = true;
					break;
				case CUIAnimation.FadeOutDown:
					posAmt = new Vector3(0f, -rect.height, 0f);
					state.isShowing = false;
					break;

				case CUIAnimation.FadeInUp:
					posAmt = new Vector3(0f, -rect.height, 0f);
					state.isShowing = true;
					break;
				case CUIAnimation.FadeOutUp:
					posAmt = new Vector3(0f, rect.height, 0f);
					state.isShowing = false;
					break;
			}

			state.animLength = animTime;

			if (state.isShowing) state.cuiGroup.gameObject.SetActive(true);


			//POSITION ANIMATION
			if (posAmt != Vector3.zero) {
				Tween posTween;

				//If we're showing, posAmt + curPos is the starting position for the animation, and the current pos is the end.
				if (state.isShowing) {
					cuiGroup.rectTransform.localPosition = curPos + posAmt;
					posTween = cuiGroup.rectTransform.DOLocalMove(curPos, animTime).SetEase(animEasing);
				}
				//Else, posAmt + curPos is the amount we should move when animating
				else {
					posTween = cuiGroup.rectTransform.DOLocalMove(curPos + posAmt, animTime).SetEase(animEasing);
				}

				state.tweens.Add(posTween);
			}

			//SCALING ANIMATION
			if (scaleAmt != Vector3.one) {
				//data.tweeners.Add();
			}

			//FADING ANIMATION
			if (state.isChangingVisibility && state.isShowing) {

				if (state.isInstant) {
					state.cuiGroup.canvasGroup.alpha = 1f;
				}
				else {
					state.tweens.Add(state.cuiGroup.canvasGroup.DOFade(1f, state.animLength));
				}

			}
			else if (state.isChangingVisibility) {
				
				if (state.isInstant) {
					state.cuiGroup.canvasGroup.alpha = 0f;
				}
				else {
					state.tweens.Add(state.cuiGroup.canvasGroup.DOFade(0f, state.animLength));
				}

			}


			StartCoroutine(HandleAnimationData(state));
		}


		private static void KillActiveAnimations(CUIGroup cuiGroup) {
			//Kill any existing animations
			CUIAnimationState existingState = currentAnimations.Find(x => x.cuiGroup == cuiGroup);

			if (existingState != null) {
				currentAnimations.Remove(existingState);

				foreach (Tween tween in existingState.tweens) {
					tween?.Kill();
				}

				//Set the group to whatever final pos it had
				existingState.cuiGroup.rectTransform.localPosition = existingState.finalPos;
			}
		}

		private static IEnumerator HandleAnimationData(CUIAnimationState state) {
			if (state.tweens.Count <= 0 && !state.isInstant) yield break;
			
			//Add the new data to the list
			currentAnimations.Add(state);

			//Trigger events:
			if (state.isChangingVisibility) {
				if (state.isShowing) state.cuiGroup.OnShowing();
				else state.cuiGroup.OnHiding();
			}

			//Wait for the animation to complete
			if (!state.isInstant) {

				//Preview for in-editor
				if (!Application.isPlaying) {
					CUITweenPreview.PrepareTweens(state.tweens);
					CUITweenPreview.StartTweens();
				}
				
				yield return new WaitForSeconds(state.animLength);
				
				while (state.tweens[0].active) {
					yield return null;
				}
			}

			//Check if it's still valid, if so remove it
			if (currentAnimations.Contains(state)) {
				//Move the rect to it's final pos after the animation is done
				state.cuiGroup.rectTransform.localPosition = state.finalPos;

				//Disable the GO if needed
				if (!state.isShowing && state.cuiGroup.disableCanvasWhenHidden) state.cuiGroup.gameObject.SetActive(false);

				currentAnimations.Remove(state);
			}
		}
		
	}


	public class CUIAnimationState {
		public List<Tween> tweens = new List<Tween>();

		public float animLength = 0.3f;

		public CUIGroup cuiGroup;
		public bool isChangingVisibility = true;
		public bool isShowing = true;

		public bool isInstant = false;

		public Vector3 finalPos;
	}


	public enum CUIAnimation {
		None,

		FadeIn,
		FadeOut,

		FadeInDown,
		FadeOutDown,

		FadeInUp,
		FadeOutUp,

		FadeInLeft,
		FadeOutLeft,

		FadeInRight,
		FadeOutRight,
		
		InstantOut,
		InstantIn
	}

	[Serializable]
	public class CUIAnimationData {
		//public string Name = "None";

		[Title("@anim")]

		//public CUIAnimation anim = CUIAnimation.None;

		public string anim;
		
		[BoxGroup("Visibility")]
		public bool changeVisibility = true;

		[BoxGroup("Visibility")]
		[ShowIf("changeVisibility")]
		public bool show = true;

		[BoxGroup("Movement")]
		public bool move = false;

		[BoxGroup("Movement")]
		[ShowIf("move")]
		public Vector2 moveDir = Vector2.zero;

		[BoxGroup("Movement")]
		[ShowIf("move")]
		public Vector2 scaling = Vector2.one;
	}

	[CreateAssetMenu(fileName = "CUIAnimationConfig", menuName = "CUI/AnimationConfig", order = 0)]
	public class CUIAnimationConfig : ScriptableObject {

		
		//public string className = "CUIAnimationEnums";
		
		
		[SerializeField] public List<CUIAnimationData> values = new List<CUIAnimationData>();





		[Button]
		public void AddNewAnimation() {
			values.Add(new CUIAnimationData());
		}
		
		[Button]
		private void GenerateCode() {
			
#if UNITY_EDITOR

			string soSaveFolder = AssetDatabase.GetAssetPath(this);


			string classDef = string.Empty;

			classDef += "public enum " + "CUIAnimations" + " {" + Environment.NewLine;

			foreach (CUIAnimationData data in values) {
				classDef += "	" + data.anim + "," + Environment.NewLine;
			}

			classDef += "}" + Environment.NewLine;
			
			System.IO.File.WriteAllText(Path.Combine(Path.GetDirectoryName(soSaveFolder), "CUIAnimations_Generated" + ".cs"), classDef);


			/*
			foreach (CUIAnimationData data in values) {
				if (data.anim.ToString().Contains("FadeIn")) {
					data.changeVisibility = true;
					data.show = true;
				}
				else if (data.anim.ToString().Contains("FadeOut")) {
					data.changeVisibility = true;
					data.show = false;
				}
			}*/

			/*foreach (CUIAnimation anim in (CUIAnimation[]) Enum.GetValues(typeof(CUIAnimation))) {
				List<CUIAnimationData> found = values.FindAll(x => x.anim == anim);

				if (found.Count <= 1) continue;

				//We found more than one copy
				bool shouldRemove = EditorUtility.DisplayDialog("Duplicate AnimationData found!",
					"More than one copy of " + anim.ToString() + " was found. Remove all but the first one?", "Ok", "Cancel");

				if (shouldRemove) {

					for (int i = 0; i < found.Count; i++) {
						if (i == 0) continue;
						values.Remove(found[i]);
					}
					
				}
				else {
					return;
				}
				
			}*/

#endif
		}
	}
}