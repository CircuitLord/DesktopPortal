using System;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace DPCore.Interaction {
	public abstract class DPInteractor : MonoBehaviour {
		
		public static List<DPInteractor> all = new List<DPInteractor>();

		[SerializeField] protected LayerMask layerMask;
		
		
		[HideInInspector] public DPOverlayBase targetDP;
		
		//public SteamVR_Input_Sources inputSource;
		public DPOverlayTrackedDevice trackedDevice = DPOverlayTrackedDevice.LeftHand;

		public bool isActivated { get; protected set; } = false;
		[HideInInspector] public bool isInteracting = false;


		protected virtual void Start() {
			all.Add(this);
		}

		/// <summary>
		/// Active the interactor
		/// </summary>
		public abstract void Activate(bool fast = false);

		public abstract void Disable();

		public abstract bool HandleInteractionDetection(out List<Vector3> cursorPositions);

		/// <summary>
		/// Called when the interactor is active and pointing at an overlay that it should send input to
		/// </summary>
		public abstract void ProcessInput();

		/// <summary>
		/// Called when the interactor should test to see if it's pointing at/on a snap point
		/// </summary>
		/// <returns>If it hit a snap point</returns>
		public abstract bool DetectSnapPoints(out DPSnapPoint activePoint);


	}
}