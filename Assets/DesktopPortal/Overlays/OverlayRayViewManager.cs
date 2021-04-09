using System;
using System.Collections;
using System.Collections.Generic;
using DPCore;
using UnityEngine;
using Valve.VR;

namespace DesktopPortal.Overlays {
	public class OverlayRayViewManager : UnityEngine.MonoBehaviour {

		[SerializeField] private Transform raycastPointsTrans;
		
		
		private List<Transform> raycastPoints = new List<Transform>();

		public Transform centerTransform;

		[HideInInspector] public List<DPOverlayBase> lookedAtDPs = new List<DPOverlayBase>();


		[SerializeField] private float raycastDistance = 5.0f;
		[SerializeField] private LayerMask overlaysLayerMask;


	//	public static DPRenderWindowOverlay primaryWindowDP;
		

		private void Start() {
			
			
			FetchRaycastPoints();
			
			
			StartCoroutine(HandleRays());
		}
		


		private void FetchRaycastPoints() {
			
			raycastPoints.Clear();
			
			foreach (Transform child in raycastPointsTrans.GetComponentsInChildren<Transform>()) {
				//We only want the actual raycast points, not the holders
				if (child.childCount > 0) continue;

				//If this the center, we ignore it.
				//if (child == centerTransform) continue;
				
				raycastPoints.Add(child);
				
			}

		}




		private IEnumerator HandleRays() {
			while (true) {
				//Set all the currently "looked at" overlays checker to be false 
				for (int i = 0; i < lookedAtDPs.Count; i++) {
					lookedAtDPs[i].lookedAtCheck = false;
				}


				while (!SteamVRManager.isConnected) yield return null;

				
				
				/*
				DPRenderWindowOverlay tempPrimary = null;
				
				//For the center point. For finding the primary overlay
				for (int i = OverlayManager.I.overlays.Count; i --> 0; ) {
					DPOverlayBase dpBase = OverlayManager.I.overlays[i];
					
					if (!dpBase.overlay.shouldRender) continue;
					
					//We're looking for a Desktop Window overlay, so we skip any overlays that aren't like that.
					if ((dpBase is DPRenderWindowOverlay) == false) continue;

					
					//If it's being scrolled, it has to be the primary so mouse input is sent properly.
					if (dpBase.isBeingScrolled) {
						//Debug.Log("found scroller");
						tempPrimary = dpBase as DPRenderWindowOverlay;
						break;
					}
					
					//If not being scrolled, test for collisions:

					//If it collides, we're looking at the DP.
					if (TestComputeIntersection(dpBase.overlay.handle, centerTransform)) {
						HandleDPLookAt(dpBase);

						tempPrimary = dpBase as DPRenderWindowOverlay;

						//Break since we found one:
						break;
					}

				}*/
				
				/*//If we found a primary, and it's not equal to the last one, trigger the events:
				if (tempPrimary != null && tempPrimary != primaryWindowDP) {

					//Disable primary on the previous overlay.
					if (primaryWindowDP != null) primaryWindowDP.isPrimary = false;

					primaryWindowDP = tempPrimary;
					primaryWindowDP.isPrimary = true;

				}*/
				
				

				//yield return null;
				
				
				

				foreach (Transform point in raycastPoints) {

					for (int i = OverlayManager.I.overlays.Count; i --> 0; ) {

						DPOverlayBase dpBase = OverlayManager.I.overlays[i];
						
						//Skip this if we've already confirmed that it's being looked at:
						if (dpBase.lookedAtCheck == true) continue;

						if (!dpBase.overlay.isVisible) continue;

						//If it collides, we're looking at the DP.
						if (TestComputeIntersection(dpBase.overlay.handle, point)) {
							HandleDPLookAt(dpBase);
						}
						
                    }
					

					yield return null;
				}


				//Sets any overlays that have the check still false, but are supposedly being looked at back to the og state
			
				for (int i = lookedAtDPs.Count - 1; i >= 0 ; i--) {
					if (lookedAtDPs[i].isBeingLookedAt && lookedAtDPs[i].lookedAtCheck == false) {
						HandleDPLookAway(lookedAtDPs[i]);
					}
				}
				
				//lookedAtDPs.Clear();

				yield return null;

			}
		}


		private bool TestComputeIntersection(ulong handle, Transform point) {

			IntersectionResults results = new IntersectionResults();
			
			return SteamVR_Utils.ComputeIntersection(handle, point.position, point.forward, SteamVRManager.trackingSpace, ref results);


		}
		
		private void HandleDPLookAt(DPOverlayBase dpBase) {
			dpBase.lookedAtCheck = true;

			if (lookedAtDPs.Contains(dpBase)) return;

			dpBase.isBeingLookedAt = true;
			


			lookedAtDPs.Add(dpBase);
		}

		private void HandleDPLookAway(DPOverlayBase dpBase) {
			if (lookedAtDPs.Contains(dpBase)) {
				dpBase.isBeingLookedAt = false;

				lookedAtDPs.Remove(dpBase);
			}
		}

	}
}
