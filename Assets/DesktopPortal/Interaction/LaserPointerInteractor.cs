using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.IO;
using DesktopPortal.Overlays;
using DesktopPortal.Sounds;
using DesktopPortal.UI;
using DG.Tweening;
using DPCore;
using DPCore.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;
using Valve.VR;


namespace DesktopPortal.Interaction {
	public class LaserPointerInteractor : DPInteractor {
		[SerializeField] private DPCameraOverlay laserDP;

		[SerializeField] public Transform pointer;

		//[SerializeField] private UnityEngine.UI.Michsky.UI.ModernUIPack.UIGradient laserGradient;
		


		[SerializeField] private float snapPointRaycastDistance = 10f;

		[SerializeField] private float joystickMultiplier = 3f;


		[SerializeField] private SteamVR_ActionSet mainActionSet;

		[SerializeField] private SteamVR_Action_Single primaryInteractAction;
		[SerializeField] private SteamVR_Action_Boolean secondaryInteractAction;
		[SerializeField] private SteamVR_Action_Vector2 joystickInteractAction;
		[SerializeField] private SteamVR_Action_Boolean grabAction;

		[HideInInspector] public SteamVR_Input_Sources inputSource;


		/// <summary>
		/// True if this laser is not active because it is primary, but active because there is a multi-interact overlay it's pointing at
		/// </summary>
		[HideInInspector] public bool multiInteractActive = false;
		
		private bool activatedTempEatClick = false;

		private bool hasPrimaryClicked = false;

		//Tweens
		private Tweener t_laserGradientOffset;

		private bool isDragging = false;

		private IntersectionResults mostRecentIntersection;


		protected override void Start() {
			base.Start();


			laserDP.onInitialized += delegate {
				Activate(true);
				
				Disable();
			};
		}


		public override void Activate(bool fast = false) {
			if (isActivated) return;
			isActivated = true;

			Vector3 laserPos = pointer.localPosition;
			Vector3 laserRot = pointer.localEulerAngles;
			laserRot.x += 90;


			laserDP.SetOverlayTransform(laserPos, laserRot, true, true);

			laserDP.overlay.SetVisible(true);
			laserDP.SetOverlayOpacity(0.5f);
			
			laserDP.RequestRendering(true);
			


			//DPUIManager.Animate(laserDP, "FadeIn", 0.2f);


			mainActionSet.Activate(inputSource, 0, false);


			if (!fast) activatedTempEatClick = true;
		}


		public override void Disable() {
			if (!isActivated) return;
			isActivated = false;
			targetDP = null;
			isInteracting = false;

			laserDP.SetOverlayOpacity(0.0f);
			laserDP.overlay.SetVisible(false);

			//DPUIManager.Animate(laserDP, "FadeOut", 0.2f);

			mainActionSet.Deactivate(inputSource);
			
		}


		public void AnimateTriggerGradient(float strength) {
			if (!isActivated) return;
			//laserGradient.Offset = strength - 0.5f;
		}


		public override bool HandleInteractionDetection(out List<Vector3> cursorPositions) {
			
			cursorPositions = new List<Vector3>();
			
			
			//DPOverlayBase closestOverlay;

			List<DPLaserCollisionData> collisions = new List<DPLaserCollisionData>();
			
			//bool foundOverlay = false;

			//Foreach of the overlays
			for (int i = 0; i < OverlayManager.I.overlays.Count; i++) {
				DPOverlayBase dpToTest = OverlayManager.I.overlays[i];
				
				//Don't process invisible or non-interactable overlays
				if (!dpToTest.overlay.shouldRender || !dpToTest.isInteractable) continue;
				
				//Ignore look/distance hiding overlays:
				if (dpToTest.lookHidingActive || dpToTest.distanceHidingActive) continue;

				if (!isActivated && !dpToTest.alwaysInteract && !dpToTest.allowMultipuleInteractors) continue;

				//If the laser isn't activated, global interaction isn't activated, and the overlay doesn't want interaction when the bar is closed, just skip it.
				//If the overlay supports multi-interact, it's possible we might want to enable this laser, so we don't skip this quite yet.
				// ^^ if (!isActivated && !OverlayInteractionManager.interactionEnabled && !dpToTest.usePointInteraction && !dpToTest.allowMultipuleInteractors) continue;

				//If the overlay anchor is the same as the interactor, skip
				if (dpToTest.overlay.trackedDevice != DPOverlayTrackedDevice.None && dpToTest.overlay.trackedDevice == trackedDevice) continue;
				
				//If the laser intersects with the overlay
				if (CalculateIntersection(dpToTest, out IntersectionResults hitResults)) {
					
					//Add it to the list of possible intersections
					collisions.Add(new DPLaserCollisionData() { results = hitResults, dpBase = dpToTest});
					
				}
				
			}
			
			
			//Actually handle interaction stuff once we find the closest collided overlay
			if (collisions.Count >= 1) {

				//Find the closest collision data out of all the collisions
				DPLaserCollisionData closest = collisions[0];

				for (int j = 1; j < collisions.Count; j++) {
					if (collisions[j].results.distance < closest.results.distance) {
						closest = collisions[j];
					}
				}
				
				//We've found the closest one, and can now interact.

				mostRecentIntersection = closest.results;
				
				//If interaction is not globally enabled, but the overlay has a flag for pointing interaction, we enable this laser.
				if (!isActivated && !OverlayInteractionManager.interactionEnabled) {
					if (closest.dpBase.alwaysInteract) {
						
						//OverlayInteractionManager.I.TempEnableInteraction(inputSource);

						//tempMultiInteractActive = true;
						
						Activate(true);
						if (closest.dpBase.alwaysInteractBlockInput) OverlayInteractionManager.BlockInput(true);
						//OverlayInteractionManager.I.
					}

					//Else, if interaction is not globally enabled, we return.
					else return false;
				}
				
				
				//If we were using multi-touch on the old overlay, Disable the laser again:
				if (targetDP != closest.dpBase && targetDP != null && OverlayInteractionManager.interactionEnabled && OverlayInteractionManager.I.primaryLaser != this) {
					multiInteractActive = false;
					Disable();
				}
				
				
				//Activate the laser if it's not on and this overlay uses multi-interact:
				if (OverlayInteractionManager.interactionEnabled && !isActivated && closest.dpBase.allowMultipuleInteractors) {
					multiInteractActive = true;
					Activate(true);
				}
				
					
				else if (!isActivated) return false;
					
				
				//HIT A NEW OVERLAY
				//If we hit another overlay, disable interaction on the old overlay.
				if (targetDP != closest.dpBase && targetDP != null) {
					targetDP.isBeingInteracted = false;
					activatedTempEatClick = true;

					//If interaction isn't globally enabled, we need to turn off this laser when we leave this overlay.
					if (!OverlayInteractionManager.interactionEnabled && !TheBarManager.isOpened) {
						Disable();
						if (OverlayInteractionManager.inputBlocked) OverlayInteractionManager.BlockInput(false);
					}
					
				}
				
					
				targetDP = closest.dpBase;
				targetDP.isBeingInteracted = true;

				isInteracting = true;
					
				//Show the window bottom:
				if (OverlayInteractionManager.I.primaryLaser == this) DPToolbar.I.Target(targetDP);


				targetDP.HandleColliderInteracted(this, new List<Vector2>() {closest.results.UVs});
					
					
				cursorPositions.Add(closest.results.point);


				//return since we found an overlay for interaction
				return true;

			}
			
			//Else, we found no overlays to interact with, so reset the state:

			
			//If we were temporarially interacting with a multi-touch overlay, disable this laser again.
			if (OverlayInteractionManager.interactionEnabled && OverlayInteractionManager.I.primaryLaser != this) {
				multiInteractActive = false;
				Disable();
			}
			
			
			if (targetDP != null) {
				targetDP.isBeingInteracted = false;

				//If interaction isn't globally enabled, we need to turn off this laser when we leave this overlay.
				if (!OverlayInteractionManager.interactionEnabled && !TheBarManager.isOpened) {
					
					Disable();
					
					if (OverlayInteractionManager.inputBlocked) OverlayInteractionManager.BlockInput(false);
				}
			}

			targetDP = null;
			isInteracting = false;
			return false;

			
		}


		private bool CalculateIntersection(DPOverlayBase dpBase, out IntersectionResults results) {
			results = new IntersectionResults();

			return SteamVR_Utils.ComputeIntersection(dpBase.overlay.handle, pointer.position, pointer.forward, SteamVRManager.trackingSpace, ref results);
		}


		/*
		private bool tempDisableInput = false;
		private Coroutine tempDisableInput_C;
		private IEnumerator TempDisableInput() {
			tempDisableInput = true;
			yield return new WaitForSeconds(0.4f);
		}
		*/

		public override void ProcessInput() {
			if (!isActivated) return;
		//	if (tempDisableInput) return;
			
			//See if we should reset the tempEatClick
			if (activatedTempEatClick) {
				if (primaryInteractAction[inputSource].axis < 0.05f) {
					activatedTempEatClick = false;
				}
			}

			AnimateTriggerGradient(primaryInteractAction[inputSource].axis);

			//Dragging stuff:

			if (!isDragging && grabAction[inputSource].state == true && targetDP != null && targetDP.isDraggable) {
				isDragging = true;

				OverlayInteractionManager.I.StartDragDP(targetDP);
			}
			else if (isDragging && grabAction[inputSource].state == false) {
				isDragging = false;


				OverlayInteractionManager.I.EndCurrentDrag();
			}


			if (isDragging) {
				Vector2 values = new Vector2();

				//use joystick input from other hand to change size
				/*if (inputSource == SteamVR_Input_Sources.LeftHand) {
					values = joystickInteractAction[SteamVR_Input_Sources.RightHand].axis;
				}
				else {
					values = joystickInteractAction[SteamVR_Input_Sources.LeftHand].axis;
				}*/

				values = joystickInteractAction[SteamVR_Input_Sources.Any].axis;

				if (!OverlayInteractionManager.I.isUsingSnapPoint) {
					//scale window size:
					if (values.x > DPSettings.config.joystickDeadzone || values.x < -DPSettings.config.joystickDeadzone) {
						OverlayInteractionManager.I.ScaleCurrentDrag(values.x);
					}


					//Move window closer or further:
					if (values.y > DPSettings.config.joystickDeadzone || values.y < -DPSettings.config.joystickDeadzone) {
						OverlayInteractionManager.I.ExtendCurrentDrag(values.y);
					}
				}
			}

			//If we're not dragging, use input normally:

			else {

				if (!activatedTempEatClick) {
					targetDP.onPrimaryInteract?.Invoke(this, primaryInteractAction[inputSource].axis);
					
					//TODO: settings
					if (!hasPrimaryClicked && primaryInteractAction[inputSource].axis > 0.25f) {
						hasPrimaryClicked = true;
						SoundManager.I.PlaySoundInWorld(mostRecentIntersection.point, DPSoundEffect.Activation);
					}
					else if (primaryInteractAction[inputSource].axis < 0.05f) {
						hasPrimaryClicked = false;
					}

				}
				targetDP.onSecondaryInteract?.Invoke(this, secondaryInteractAction[inputSource].state);

				if (Mathf.Abs(joystickInteractAction[inputSource].axis.y) > DPSettings.config.joystickDeadzone) {
					targetDP.onScrolled?.Invoke(this, joystickInteractAction[inputSource].axis.y * joystickMultiplier);
				}
				else {
					targetDP.onScrolled?.Invoke(this, 0f);
				}

				if (Mathf.Abs(joystickInteractAction[inputSource].axis.x) > DPSettings.config.joystickDeadzone) {
					targetDP.onScrolledHorz?.Invoke(this, joystickInteractAction[inputSource].axis.x * joystickMultiplier);
				}
				else {
					targetDP.onScrolledHorz?.Invoke(this, 0f);
				}
			}
		}

		
		public override bool DetectSnapPoints(out DPSnapPoint activePoint) {
			
			Ray ray = new Ray {origin = pointer.position, direction = pointer.forward};
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit, snapPointRaycastDistance, layerMask)) {
				activePoint = hit.transform.GetComponent<DPSnapPoint>();

				if (activePoint != null) return true;

			}

			activePoint = null;
			return false;

		}


		public class DPLaserCollisionData {
			public IntersectionResults results;
			public DPOverlayBase dpBase;
		}
		
	}
	
	
	
}