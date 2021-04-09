using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace DesktopPortal.UI {
    
    
    public class AppIcon : MonoBehaviour {

        [SerializeField] public RawImage icon;
        [SerializeField] private GameObject isCustomAppImage;
        [SerializeField] private GameObject isFavoriteImage;
        [SerializeField] public TextMeshProUGUI text;

        public Button button;

        [HideInInspector] public bool isCustomApp = false;
        [HideInInspector] public bool isFavorite = false;
        [HideInInspector] public string appKey;
        [HideInInspector] public string filePath;


        public delegate void AppSelectEvent(AppIcon appIcon);

        public static AppSelectEvent appSelectEvent;





        public void Selected() {
            appSelectEvent.Invoke(this);
        }

        public void SetIsCustomApp(bool yes) {
            isCustomApp = yes;
            isCustomAppImage.SetActive(isCustomApp);
        }

        public void SetIsFavorite(bool yes) {
            isFavorite = yes;
            isFavoriteImage.SetActive(yes);
        }


    }
}