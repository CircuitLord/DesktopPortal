using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;


namespace DPCore {
	public class DPUIManager : MonoBehaviour {
		public static DPUIManager I;


		//private static float animMoveDist = 0.15f;
		public static float animTime = 0.3f;

		private static Ease animShowEasing = Ease.OutCubic;
		private static Ease animHideEasing = Ease.InCubic;


		private static List<DPAnimationState> currentAnimations = new List<DPAnimationState>();

		//[SerializeField] private DPAnimationConfig animationConfig;


		private void Awake() {
			I = this;
		}


		/*
		/// <summary>
		/// Calculates and plays the animation for a new app state
		/// </summary>
		/// <param name="dpApp"></param>
		/// <param name="newState">The new state. Use app.state for previous state</param>
		public static void CalculatePlayAnimationNewAppState(DPApp dpApp, DPAppState newState) {

		    if (dpApp == null || dpApp.dpMain == null) return;

		    switch (newState) {
		        
		        case DPAppState.Opened:
		            
		            //Play(dpApp.dpMain, DPUIAnimation.FadeInUp);
		            
		            if (dpApp.dpMain.overlay.trackedDevice != DPOverlayTrackedDevice.None) {
		                Play(dpApp.dpMain, DPAnimation.FadeIn);
		            }
		            else {
		                Play(dpApp.dpMain, DPAnimation.FadeInUp);
		            }

		            

		            if (!dpApp.useTopBar) break;
		            
		            Play(dpApp.dpTopBar, DPAnimation.FadeIn);
		            
		            break;
		            
		        
		        
		        case DPAppState.Closed:
		        case DPAppState.Minimized:

		            

		            if (dpApp.dpMain.overlay.trackedDevice != DPOverlayTrackedDevice.None) {
		                
		                Play(dpApp.dpMain, DPAnimation.FadeOut);
		            }
		            else {
		                Play(dpApp.dpMain, DPAnimation.FadeOut);
		            }
		            
		            
		            if (!dpApp.useTopBar) break;

		            Play(dpApp.dpTopBar, DPAnimation.FadeOut);


		            break;
		        
		        
		    }
		    
		}*/


		public static void Animate(DPOverlayBase dpBase, object anim, float overrideTime = -1f) {
			AnimateInternal(dpBase, anim, overrideTime);
		}


		private static void AnimateInternal(DPOverlayBase dp, object anim, float overrideAnimTime = -1f) {
			KillActiveAnimations(dp);

			DPAnimationData data = GetDPAnimationFromEnum(anim.ToString().Replace("DPAnimations.", ""));

			if (data == null) {
				Debug.LogError("DPAnimationData ( " + anim.ToString() + ") could not be found, aborting!");
				return;
			}


			if (data.changeVisibility && data.show) {
				dp.overlay.SetVisible(true, false);
				dp.overlay.SetOpacity(0, false);

				foreach (DPOverlayBase child in dp.children) {
					if (!child.followParentOpacity) continue;
					child.overlay.SetVisible(true, false);
					child.overlay.SetOpacity(0, false);
				}
			}

			//Calculate the animation time
			float finalAnimTime;
			if (overrideAnimTime > 0) finalAnimTime = overrideAnimTime;
			else finalAnimTime = animTime;


			DPAnimationState state = new DPAnimationState() {
				dpBase = dp,
				data = data,
				animLength = finalAnimTime,
				finalPos = dp.transform.localPosition,
				finalWidth = dp.overlay.width
			};


			//MOVEMENT ANIMATION
			if (data.move) {
				Vector3 curPos = dp.transform.localPosition;

				//Animate pos is either the starting position or the ending position of the animation depending on if it is showing or hiding
				Vector3 animatePos = curPos + (Vector3.right * data.moveDir.x * dp.overlay.width) + (Vector3.up * data.moveDir.y * dp.overlay.width) +
				                     (Vector3.forward * data.moveDir.z * dp.overlay.width);

				Vector3 anchoredPos = curPos + (dp.transform.right * data.moveDir.x) + (dp.transform.up * data.moveDir.y) + (dp.transform.forward * data.moveDir.z);

				if (data.changeVisibility) {
					//If it's fading in:
					if (data.show) {
						dp.SetOverlayTransform(animatePos, dp.transform.localEulerAngles, true, false, true);

						dp.TransitionOverlayPosition(curPos, dp.transform.localEulerAngles, finalAnimTime, animShowEasing, false);
					}
					//If it's fading out:
					else {
						dp.TransitionOverlayPosition(animatePos, dp.transform.localEulerAngles, finalAnimTime, animHideEasing, false);
					}
				}

				//If it's not changing visibility:
				else {
					dp.TransitionOverlayPosition(animatePos, dp.transform.localEulerAngles, finalAnimTime, animHideEasing, false);
				}
			}


			//VISIBILITY ANIMATION
			if (data.changeVisibility) {
				if (data.show) {
					if (dp.overlay.targetOpacity <= 0f) Debug.LogError("Overlay target opacity was 0f!");

					dp.TransitionOverlayOpacity(dp.overlay.targetOpacity, finalAnimTime, animShowEasing, false);
				}
				else {
					dp.TransitionOverlayOpacity(0f, finalAnimTime, animHideEasing, false);
				}
			}

			//SCALING ANIMATION
			if (data.scale) {
				if (data.changeVisibility) {
					if (data.show) {
						//float curWidth = dp.overlay.width;
						dp.overlay.SetWidthInMeters(dp.overlay.width * data.widthScaleMulti, false);
						dp.TransitionOverlayWidth(dp.overlay.width, finalAnimTime, false);
					}
					else {
						dp.TransitionOverlayWidth(dp.overlay.width * data.widthScaleMulti, finalAnimTime, false);
					}
				}

				else {
					dp.TransitionOverlayWidth(dp.overlay.width * data.widthScaleMulti, finalAnimTime, false);
				}
			}


			I.StartCoroutine(HandleAnimationData(state));
		}


		private static void KillActiveAnimations(DPOverlayBase dpBase) {
			if (currentAnimations.Count <= 0) return;

			//Kill any existing animations
			DPAnimationState existingState = currentAnimations.Find(x => x.dpBase == dpBase);

			if (existingState != null) {
				currentAnimations.Remove(existingState);

				existingState.dpBase.KillTransitions();

				//Set the overlay to whatever final pos it had
				existingState.dpBase.transform.localPosition = existingState.finalPos;
				existingState.dpBase.SyncTransform();

				existingState.dpBase.overlay.SetWidthInMeters(existingState.finalWidth);

				if (existingState.data.changeVisibility) {
					existingState.dpBase.overlay.SetVisible(existingState.data.show);
				}
			}
		}

		private static IEnumerator HandleAnimationData(DPAnimationState state) {
			//Add the new data to the list
			currentAnimations.Add(state);

			//Trigger events:
			// if (state.isChangingVisibility) {
			//if (state.isShowing) state.dpBase.OnShowing();
			//else state.cuiGroup.OnHiding();
			//}

			//Wait for the animation to complete
			yield return new WaitForSeconds(state.animLength);

			//Wait an extra frame to finish
			yield return null;
			yield return null;


			//Check if it's still valid, if so remove it
			if (currentAnimations.Contains(state)) {
				//Move the overlay to it's final position
				state.dpBase.transform.localPosition = state.finalPos;
				state.dpBase.SyncTransform();

				//Disable the GO if needed
				//if (!state.isShowing) state.cuiGroup.gameObject.SetActive(false);
				//if (!state.isShowing) 

				currentAnimations.Remove(state);
			}
		}

		private static DPAnimationData GetDPAnimationFromEnum(object obj) {
			//Debug.Log(obj.ToString());
			//return I.animationConfig.values.Find(x => x.anim == obj.ToString());


			DPAnimationData data = new DPAnimationData();
			
			switch (obj.ToString()) {
				
				case "FadeIn":
					break;
				
				case "FadeOut":
					data.show = false;
					break;
				
				case "FadeInUp":
					data.moveDir = new Vector3(0f, -0.5f, 0f);
					break;
				
				case "FadeOutDown":
					data.show = false;
					data.moveDir = new Vector3(0f, -0.5f, 0f);
					break;


			}

			return data;

		}
	}

	public enum DPAnimation {
		FadeIn,
		FadeOut,
		FadeInUp,
		FadeOutUp,
		FadeInDown,
		FadeOutDown,
	}

	public class DPAnimationState {
		public DPAnimationData data;

		public float animLength = 0.3f;

		public DPOverlayBase dpBase;

		public Vector3 finalPos;
		public float finalWidth;
	}


	[Serializable]
	public class DPAnimationData {
		//public string Name = "None";

		[Title("@anim")]

		//public CUIAnimation anim = CUIAnimation.None;
		public string anim;

		[BoxGroup("Visibility")] public bool changeVisibility = true;

		[BoxGroup("Visibility")] [ShowIf("changeVisibility")]
		public bool show = true;

		[BoxGroup("Movement")] public bool move = false;

		[BoxGroup("Movement")] [ShowIf("move")]
		public Vector3 moveDir = Vector2.zero;


		[BoxGroup("Scaling")] public bool scale = false;

		[BoxGroup("Scaling")] [ShowIf("scale")]
		public float widthScaleMulti = 1f;
	}

	[CreateAssetMenu(fileName = "DPAnimationConfig", menuName = "DP/DPAnimationConfig", order = 0)]
	public class DPAnimationConfig : ScriptableObject {
		[SerializeField] public List<DPAnimationData> values = new List<DPAnimationData>();

		[Button]
		private void AddNewAnimation() {
			values.Add(new DPAnimationData());
		}

		
		#if UNITY_EDITOR
		
		[Button]
		private void GenerateCode() {
			string soSaveFolder = AssetDatabase.GetAssetPath(this);


			string classDef = string.Empty;

			classDef += "public enum " + "DPAnimations" + " {" + Environment.NewLine;

			foreach (DPAnimationData data in values) {
				classDef += "	" + data.anim + "," + Environment.NewLine;
			}

			classDef += "}" + Environment.NewLine;

			System.IO.File.WriteAllText(Path.Combine(Path.GetDirectoryName(soSaveFolder), "DPAnimations_Generated" + ".cs"), classDef);
		}
		
		#endif
	}
}