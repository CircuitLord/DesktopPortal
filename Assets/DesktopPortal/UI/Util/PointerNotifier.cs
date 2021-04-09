using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DesktopPortal.UI {
    public class PointerNotifier : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {

        public Action onPointerEnter;
        public Action onPointerExit;

        public Action<bool> onPointerPress;
        
        public void OnPointerEnter(PointerEventData eventData) {
            onPointerEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData) {
            onPointerExit?.Invoke(); 
        }

        public void OnPointerDown(PointerEventData eventData) {
            onPointerPress?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData) {
            onPointerPress?.Invoke(false);
        }
        
    }
}