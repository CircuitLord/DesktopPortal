using System;
using DG.Tweening;
using DPCore.Apps;
using UnityEngine;

namespace DPCore {
	
	[Serializable]
	public class DPSnapPoint : MonoBehaviour {

		
		private static float transitionSnapPointSpeed = 0.25f;
		
		/// <summary>
		/// The custom name for this snap point, used when saving where an overlay is.
		/// </summary>
		public string identifier;

		public DPOverlayBase parentDP;
		
		
		[SerializeField] private GameObject snapPointIconPF;
		
		[HideInInspector] public DPApp dpApp;

		[HideInInspector] public DPOverlayBase dpBase;



		[HideInInspector] public bool isPreviewing => tempPreviewDP != null;
		[HideInInspector] private DPOverlayBase tempPreviewDP;

		[HideInInspector] public DPCameraOverlay iconDP;

		

		/// <summary>
		/// Relative poses are used for stuff like TheBar, where the snap points are relative to the center point of the bar.
		/// When using relative poses, the positions of the actual snap point in the scene don't do anything anymore, you need to manually move them based on the relative values.
		/// </summary>
		public bool usesRelativePos = false;

		[Header("Box Collider Settings")] 
		
		[SerializeField] private Vector2 size = new Vector2(0.2f, 0.2f);
		
		[Header("Relative Positions")]
		public Vector3 customRelativePos = Vector3.zero;
		public Vector3 customRelativeRot = Vector3.zero;


		[Header("Resizing")] 
		
		public bool useWindowResizing = false;
		public int resizeRatioX = 4;
		public int resizeRatioY = 3;
		
		//public Vector3 sidebarCustomRelativePos = Vector3.zero;
		//public Vector3 sidebarCustomRelativeRot = Vector3.zero;
		
		
		//public Transform sidebarTrans;

		public float maxOverlayWidth = 0.9f;
		public float maxOverlayHeight = 0.6f;
		public float overlayCurvature = 0.0f;
		public float overlayOpacity = 1.0f;

		[HideInInspector] public BoxCollider collider;
		
		/// <summary>
		/// Returns true if this snap point is already holding an app
		/// </summary>
		public bool isOccupied => dpApp != null || dpBase != null;


		private bool isInit = false;
		
		
		public void Init() {

			if (isInit) return;
			
			if (iconDP == null) {
				iconDP = Instantiate(snapPointIconPF, transform.parent).GetComponentInChildren<DPCameraOverlay>();
				iconDP.PreInitialize();
			}

			collider = gameObject.AddComponent<BoxCollider>();
			collider.size = new Vector3(size.x, size.y, 0f);

			isInit = true;

		}


		public void ToggleIcon(bool show) {
			if (show) {
				iconDP.TransitionOverlayOpacity(1f, transitionSnapPointSpeed);
			}
			else {
				iconDP.TransitionOverlayOpacity(0f, transitionSnapPointSpeed);
			}

		}



		/*public float FindWidthForOverlay(DPOverlayBase newDP) {


			if (newDP.currentTexture != null) {
				newDP.RequestRendering();
			}

			float multiplier = (float)newDP.currentTexture.width / newDP.currentTexture.height;


			float newWidth = maxOverlayHeight * multiplier;
			Debug.Log(newWidth);

			return newWidth;




		}*/



		/// <summary>
		/// Sets a new DPApp that is anchored on this snap point
		/// </summary>
		/// <param name="newDPApp"></param>
		public void SetSnappedApp(DPApp newDPApp) {
			dpApp = newDPApp;
			dpApp.snapPoint = this;
			
			SetSnappedDP(dpApp.dpMain);
		}
		
		/// <summary>
		/// Sets a new DPBase that is anchored on this snap point
		/// </summary>
		/// <param name="newDPBase"></param>
		public void SetSnappedDP(DPOverlayBase newDPBase) {
			dpBase = newDPBase;
			dpBase.snapPoint = this;


			/*
			if (setValues && animate) {
				dpBase.KillTransitions();
				
				dpBase.TransitionOverlayWidth(overlayWidth, transitionSnapPointSpeed, false);
				dpBase.TransitionOverlayCurvature(overlayCurvature, transitionSnapPointSpeed, false);
				dpBase.TransitionOverlayOpacity(overlayOpacity, transitionSnapPointSpeed, Ease.InOutCubic, false);
			
				dpBase.TransitionOverlayPosition(transform.position, transform.eulerAngles, transitionSnapPointSpeed, Ease.InOutCubic, false);

			}
			else if (setValues && !animate) {
				dpBase.KillTransitions();
				
				dpBase.overlay.SetWidthInMeters(overlayWidth, false);
				dpBase.overlay.SetCurvature(overlayCurvature, false);
				dpBase.overlay.SetOpacity(overlayOpacity, false);
				
				dpBase.SetOverlayTransform(transform.position, transform.eulerAngles, true, false);
			}
			*/

			
			//Clear the state of any overlay that used to be snapped here and is now being replaced
			tempPreviewDP = null;


			//Activate the icon:
			//iconDP.TransitionOverlayWidth(0.07f, transitionSnapPointSpeed, false);





		}

		/// <summary>
		/// Used to preview what a DP would look like if snapped in this spot
		/// </summary>
		public void PreviewSnappedDP(DPOverlayBase newDPBase, bool fadeOutCurrent = true) {
			
			if (isOccupied && dpBase == newDPBase) return;
			
			tempPreviewDP = newDPBase;

			
			if (isOccupied && fadeOutCurrent) dpBase.TransitionOverlayOpacity(0.1f, transitionSnapPointSpeed, Ease.InOutCubic, false);

			/*Vector3 inFrontPos = transform.position + transform.forward * -0.015f;
			
			tempPreviewDP.KillTransitions();

			if (animate) {
				if (isOccupied) dpBase.TransitionOverlayOpacity(0.1f, transitionSnapPointSpeed, Ease.InOutCubic, false);
				
				tempPreviewDP.TransitionOverlayWidth(overlayWidth, transitionSnapPointSpeed, false);
				tempPreviewDP.TransitionOverlayCurvature(overlayCurvature, transitionSnapPointSpeed, false);
				tempPreviewDP.TransitionOverlayOpacity(overlayOpacity, transitionSnapPointSpeed, Ease.InOutCubic, false);

				tempPreviewDP.TransitionOverlayPosition(inFrontPos, transform.eulerAngles, transitionSnapPointSpeed, Ease.InOutCubic, false);
			}
			else {
				
				if (isOccupied) dpBase.overlay.SetOpacity(0.1f, false);
				
				tempPreviewDP.overlay.SetWidthInMeters(overlayWidth, false);
				tempPreviewDP.overlay.SetCurvature(overlayCurvature, false);
				tempPreviewDP.overlay.SetOpacity(overlayOpacity, false);
				
				tempPreviewDP.SetOverlayTransform(inFrontPos, transform.eulerAngles, true, false);
			}*/
			
		}



		/// <summary>
		/// If we're previewing what an app would look like here, cancel it, and restore the previous one.
		/// </summary>
		public void CancelPreviewAndRestore(bool fadeBackIn = true) {

			if (!isPreviewing) return;

			//Fade the actual overlay back in
			if (isOccupied && fadeBackIn) dpBase.TransitionOverlayOpacity(dpBase.overlay.targetOpacity, transitionSnapPointSpeed, Ease.InOutCubic, false);


			tempPreviewDP = null;

		}
 		

		public void ClearAllSnapData() {

			//if (!isOccupied) return;

			if (dpApp != null) {
				dpApp.snapPoint = null;
				dpApp = null;
			}

			if (dpBase != null) {
				dpBase.snapPoint = null;
				dpBase = null;
			}

			tempPreviewDP = null;

		}

	}
	

	
}