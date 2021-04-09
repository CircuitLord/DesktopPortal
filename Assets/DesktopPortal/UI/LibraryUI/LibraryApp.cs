using System;
using System.Collections;
using System.Collections.Generic;
using CUI;
using DesktopPortal.CustomApps;
using DesktopPortal.Overlays;
using DesktopPortal.Sounds;
using DesktopPortal.Steam;
using DG.Tweening;
using DPCore;
using DPCore.Apps;
using Microsoft.Win32;
//using LevelDB;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPortal.UI {
    public class LibraryApp : DPApp {


	    public static LibraryApp I;

	    public Image blur;
	    public Image blurGradient;

	    //[SerializeField] private LibraryWelcomePanel welcomePanel;
	   // [SerializeField] private LibraryGridPanel gridPanel;

	    
	   [HideInInspector] public List<TemplateGame> gamesList = new List<TemplateGame>();

	   [SerializeField] private CUIGroup welcomeView;
	   [SerializeField] private CUIGroup gridView;
	   [SerializeField] private LibraryDisplayGroup gridDisplayGroup;
	   [SerializeField] private LibraryDisplayGroup favoritesDispalyGroup;
	   

	   [SerializeField] private Texture2D libraryIcon;

	   public AppConfig<LibrarySettings> config;

	  // private CUIGroup activeView;
        
        
        [Header("Configuration")]
		
        public float spawnXOffset = -0.25f;

        public float gamesGridInitialYOffset = 25;

        public int scrollSpeedMultiplier = 5;

        public static bool isShowingGameDetails = false;


        public bool isMonitoringProcesses = false;

        int appsRunning = 0;

        private CUIGroup activeGroup;

        public AudioSource launchGameSource;


        public static Action onFavoritesUpdated;





        public void PlayLaunchGameSound() {

	        if (dpMain == null) return;
	      //  launchGameSource.transform.SetParent(SteamVRManager.I.hmdTrans);
	      launchGameSource.transform.position = dpMain.transform.position;
	        
	      //launchGameSource.transform.localPosition = new Vector3(0f, 0f, 5f);

	       // launchGameSource.transform.DOLocalMoveZ(0.2f, 4f);
	       launchGameSource.Play();
        }
        

        public void Button_Home() {
	       // if (isShowingGameDetails) HideGameDetails();

	        if (activeGroup == welcomeView) return;

	        CUIManager.SwapAnimate(activeGroup, welcomeView);
	        
	        
	        activeGroup = welcomeView;
	        
	        favoritesDispalyGroup.MakeGames();
	        

        }
        
        public void Button_All() {
	      //  if (isShowingGameDetails) HideGameDetails();

	        gridDisplayGroup.displayMode = LibraryDisplayMode.All;
	        ShowGamesGrid();
        }

        public void Button_Revive() {
	        //if (isShowingGameDetails) HideGameDetails();
	        
	        gridDisplayGroup.displayMode = LibraryDisplayMode.Revive;
	        ShowGamesGrid();
        }
        
        public void Button_Misc() {
	        //if (isShowingGameDetails) HideGameDetails();
	        
	        gridDisplayGroup.displayMode = LibraryDisplayMode.Misc;
	        ShowGamesGrid();
        }

        public void Button_SortAZ() {
	        gridDisplayGroup.sortMode = LibrarySortMode.AtoZ;
	        gridDisplayGroup.MakeGames();
        }

        public void Button_SortZA() {
	        gridDisplayGroup.sortMode = LibrarySortMode.ZtoA;
	        gridDisplayGroup.MakeGames();
        }

        public void Button_SortRecent() {
	        gridDisplayGroup.sortMode = LibrarySortMode.Recent;
	        gridDisplayGroup.MakeGames();
        }

        private void ShowGamesGrid() {
	        if (activeGroup != gridView) {
		        CUIManager.SwapAnimate(activeGroup, gridView);
		        activeGroup = gridView;
	        }
	        
	        gridDisplayGroup.MakeGames();
        }
        

        


        protected override void Awake() {
	        base.Awake();
	        
	        I = this;
        }


        protected void Start() {
	        
	        
	        config = new AppConfig<LibrarySettings>(appKey);

	        iconTex = libraryIcon;


	        onFavoritesUpdated += () => {
		        if (favoritesDispalyGroup.gameObject.activeInHierarchy) favoritesDispalyGroup.MakeGames();
	        };

	        // SteamVRManager.I.onSteamVRConnected.AddListener(PreloadLibrary);

        }





        public override void OnInit() {

			base.OnInit();

	        //StartCoroutine(LibraryHelper.I.LoadGameLibraryImages(false));
	        LibraryHelper.I.PopulateDPGamesLibrary(false);
	        
	        //CUIManager.Animate(welcomeView, CUIAnimation.FadeIn);
	       // activeGroup = welcomeView;

	        // welcomePanel.Show();
	        // gridPanel.Hide(true);


	        // StartCoroutine(LoadLibraryGameImages());


	        CUIManager.Animate(welcomeView, CUIAnimation.FadeIn);
	        activeGroup = welcomeView;
	        
	        StartCoroutine(favoritesDispalyGroup.MakeGamesDelayed());
	        //favoritesDispalyGroup./




        }

        public override void OnOpen() {
	        base.OnOpen();
	        
        }

        public override void OnVisibilityChange(bool visible) {
	        base.OnVisibilityChange(visible);


	        if (visible) {
		        
		        
		        
		        
	        }
	        else {
		        config.SaveSettings();
	        }
	        
	        
	        
        }


        public override void OnMinimize() {
	        base.OnMinimize();
			//if (isShowingGameDetails) HideGameDetails();
        }

        public override void OnClose() {
	        base.OnClose();

	        //LibraryGame.showGameDetailsEvent.RemoveListener(HandleGameDetails);
	        
	        config.SaveSettings();
	        
        }
        
        
        
    }

    public enum LibrarySortMode {
	    AtoZ,
	    ZtoA,
	    Recent
    }

    [Serializable]
    public enum LibraryDisplayMode {
	    All,
	    Applications,
	    Favorites,
	    Misc,
	    Revive
    }
    
    public class LibrarySettings {
	    public List<LibraryGameData> games = new List<LibraryGameData>();
    }
    
}