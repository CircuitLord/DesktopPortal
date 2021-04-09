using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace DesktopPortal.UI {
    public class RadialButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

        [SerializeField] private RectTransform bg;
        


        public bool isBeingHovered = false;
        
        
        public void OnPointerEnter(PointerEventData eventData) {
            if (isBeingHovered) return;

            isBeingHovered = true;

            bg.DOScale(new Vector3(3f, 3f, 3f), 0.3f);
        }

        public void OnPointerExit(PointerEventData eventData) {
            isBeingHovered = false;
            
            bg.DOScale(Vector3.one, 0.2f);
        }
    }
}