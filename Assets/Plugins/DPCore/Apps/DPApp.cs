using System;
using System.Collections;
using System.Collections.Generic;
using Circuits.CEvents;
using UnityEngine;

namespace DPCore.Apps {
	/// <summary>
	/// A DP App is a collection of DPOverlayBases that represents an "app".
	/// Apps are required to have one main window, one sidebar, and then other developer customizable panels that can do whatever.
	/// </summary>
	///
	public class DPApp : MonoBehaviour {
		

		/// <summary>
		/// A configuration value for the offset of position of sidebars from their main dp parent
		/// </summary>
		public static readonly Vector3 sidebarOffsetPos = new Vector3(0f, 0f, -0.1f);
		
		/// <summary>
		/// A configuration value for the offset of rotation of sidebars from their main dp parent
		/// </summary>
		public static readonly Vector3 sidebarOffsetRot = new Vector3(0f, 30f, 0f);


		public static readonly float topBarMaxWidth = 1.0f;
		//public static readonly float topBarMinWidth = 0.1f;



		[Header("Configuration")]


		public string appKey;

		public string title;
		

		
		//[HideInInspector] public AppController appController;
		
		/// <summary>
		/// The main screen of the app.
		/// </summary>
		public DPOverlayBase dpMain;
		
		/// <summary>
		/// The sidebar of the app.
		/// </summary>
		[HideInInspector] public DPOverlayBase dpTopBar;
		
		/// <summary>
		/// Any other DP overlays the app might have.
		/// </summary>
		[HideInInspector] public List<DPOverlayBase> otherDPs = new List<DPOverlayBase>();

		/// <summary>
		/// The current snap point the app is on (if any)
		/// </summary>
		[HideInInspector] public DPSnapPoint snapPoint;
		
		[Space]

		//public GameObject dpHolder;

		
		//[HideInInspector]
		//public DPAppState state = DPAppState.NotLoaded;





		
		[Tooltip("If this is true, the app will only be a temporary window on the bar, and can't be dragged around the world.")]
		public bool isSlideoutOnly = true;

		/// <summary>
		/// Defines if this is a window/desktop capture or an app
		/// </summary>
		[SerializeField] public bool isCapture = false;

		[HideInInspector] public GameObject windowListButtonGO;

		[HideInInspector] public Texture2D iconTex;


		[HideInInspector] public bool isMinimized = true;



		//private


		// Is this the first time the user is opening this app in the session?
		private bool isFirstOpen = true;

		[HideInInspector] public bool isVisible { get; private set; } = false;


		
		public bool isUsingSnapPoint => snapPoint != null;

		[HideInInspector] public bool isInitialized = false;

		//protected bool shouldRenderMain => dpMain.overlay.shouldRender;


		private void Start() {

			if (isSlideoutOnly) {
				dpMain.isDraggable = false;
				//dpMain.showToolbar = false;
				dpMain.showToolbarSettings = false;
				dpMain.showToolbarPin = false;
			}
			
		}


		protected virtual void Awake() {
			
			FetchComponents();
			

		}
		
		
		private void FetchComponents() {

			if (dpMain == null) {
				dpMain = transform.Find("Main")?.Find("Overlay")?.GetComponent<DPOverlayBase>();
				if (dpMain == null) dpMain = transform.Find("Overlay").GetComponent<DPOverlayBase>();
			}
			
			dpMain.dpAppParent = this;
			


			/*if (useTopBar && !dpTopBar) {
				dpTopBar = transform.Find("TopBar")?.Find("Overlay")?.GetComponent<DPOverlayBase>();
			}
			if (dpTopBar != null) {
				dpTopBar.dpAppParent = this;
				dpTopBar.showToolbar = false;
				
				dpMain.AddChildOverlay(dpTopBar);
				
			}*/

			
			//Loop over the others and see if we need to add any more.
			Transform others = transform.Find("Others");

			if (others != null) {
				foreach (Transform t in others) {
					DPOverlayBase test = t.Find("Overlay")?.GetComponent<DPOverlayBase>();
					if (test != null) {
						otherDPs.Add(test);
						test.dpAppParent = this;
					}
				}
			}
			

			
			
		}




		/// <summary>
		/// Called when the app is opened for the first time
		/// </summary>
		public virtual void OnInit() {
			isInitialized = true;
		}

		/// <summary>
		/// Called when the app is opening/un-minimizing
		/// </summary>
		public virtual void OnVisibilityChange(bool visible) {
			isVisible = visible;

		}

		/// <summary>
		/// Called when the user specifically opens the app from the bar (NOT when opening because the bar is opened)
		/// </summary>
		public virtual void OnOpen() {
			isMinimized = false;
		}

		/// <summary>
		/// Called when the app is minimized by the user (NOT from closing the bar)
		/// </summary>
		public virtual void OnMinimize() {
			isMinimized = true;
		}

		/// <summary>
		/// Called when the user purposely closes/destroys the app, should be used to cleanup anything.
		/// </summary>
		public virtual void OnClose() {
            
		}

		/// <summary>
		/// Called when the bar is opening
		/// </summary>
		public virtual void OnTheBarToggled(bool open) {
            
		}

		
		
		


	}


	/*/// <summary>
	/// A state for a DPApp
	/// </summary>
	public enum DPAppState {
		
		/// <summary>
		/// The app is completely closed, and is not initialized.
		/// </summary>
		NotLoaded,
		
		/// <summary>
		/// The app is not currently opened, but is loaded in the apps list.
		/// </summary>
		Loaded,

		/// <summary>
		/// The app is opened.
		/// </summary>
		Active
	}*/
	
	
	
}