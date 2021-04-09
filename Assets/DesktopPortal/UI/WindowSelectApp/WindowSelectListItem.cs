using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using uDesktopDuplication;
using UnityEngine;
using UnityEngine.UI;
using uWindowCapture;

namespace DesktopPortal.UI {
    public class WindowSelectListItem : MonoBehaviour {


        public static Action<WindowSelectListItem> onPressed;

        public UDDMonitor monitor;
        
        public bool isDesktop = false;

        public int desktopIndex = 0;

        public bool exists = true;

        public UwcWindow window;

        public TextMeshProUGUI title;

        public RawImage icon;


        public Button button;

        private void Start() {
            button.onClick.AddListener(TriggerEvent);
        }

        private void TriggerEvent() {
            onPressed.Invoke(this);
        }
    }
}