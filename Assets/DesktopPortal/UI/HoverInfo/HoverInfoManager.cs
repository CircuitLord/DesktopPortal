using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DPCore;
using Lean.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace DesktopPortal.UI {
    public class HoverInfoManager : MonoBehaviour {


        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private TextMeshProUGUI desc;

        [SerializeField] private RectTransform arrow;
        [SerializeField] private RectTransform bg;
        [SerializeField] private ContentSizeFitter fitter;


        [SerializeField] private DPCameraOverlay hoverDP;

        
        
        

        public static HoverInfoManager I;
        
        
        
        
        [Header("Configuration")] 
        
        [SerializeField] private float hoverBeforeShow = 1f;

        [SerializeField] private float unHoverBeforeHide = 0.3f;




        private bool _popupShouldBeActive = false;
        private bool _popupIsActive = false;
        private RectTransform _potentialPopupElement = null;
        


        private void Start() {
            I = this;
        }





        public void TryEndCurrentPopup() {
            StartCoroutine(_tryEndCurrentPopup());
        }

        private IEnumerator _tryEndCurrentPopup() {


            if (_potentialPopupElement) {
                //_potentialPopupElement = null;
                //yield break;
            }
            
            _popupShouldBeActive = false;
            
            
            //Wait and see if anything else triggers show popup...
            yield return new WaitForSeconds(unHoverBeforeHide);

            //If nothing triggered it:
            if (!_popupShouldBeActive) KillCurrentPopupNow();
            
        }


        public void KillCurrentPopupNow() {
            
            hoverDP.KillTransitions();
            //hoverDP.TransitionOverlayOpacity(0f, 0.5f, false);
            DPUIManager.Animate(hoverDP, DPAnimation.FadeOut);

            _popupIsActive = false;
        }





        /// <summary>
        /// Displays a popup with the requested phrase (adds on Title and Desc when needed to the string name).
        /// </summary>
        public void TryShowPopup(string phraseStart, PopupFacingDir dir, DPCameraOverlay dpParent, RectTransform element) {
            StartCoroutine(_tryShowPopup(phraseStart, dir, dpParent, element));
        }


        
        private IEnumerator _tryShowPopup(string phraseStart, PopupFacingDir dir, DPCameraOverlay dpParent, RectTransform element) {

            _potentialPopupElement = element;

            if (!_popupIsActive) {
                
                _popupShouldBeActive = true;
                
                //Wait and then see if they're still hovering over the same thing
                yield return new WaitForSeconds(hoverBeforeShow);
                
                //If the tryEnd got triggered while waiting, break
                if (!_popupShouldBeActive) yield break;
            }


            _showPopup(phraseStart, dir, dpParent, element);
        }
        

        private void _showPopup(string phraseStart, PopupFacingDir dir, DPCameraOverlay dpParent, RectTransform element) {

            
            //Check to see if the user is still hovering on the same element or not
            if (_potentialPopupElement != element) return;

            //Debug.Log(offset);
            
            _popupShouldBeActive = true;
            

            title.SetText(LeanLocalization.GetTranslationText(phraseStart + "Title"));
            desc.SetText(LeanLocalization.GetTranslationText(phraseStart + "Desc"));
            
            title.ForceMeshUpdate();
            desc.ForceMeshUpdate();
            
            
            //Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(bg);
            bg.gameObject.SetActive(false);
            bg.gameObject.SetActive(true);

            //transform.parent.GetComponent<VerticalLayoutGroup>().enabled = false;
            //transform.parent.GetComponent<VerticalLayoutGroup>().enabled = true;

            //fitter.enabled = false;
            //fitter.enabled = true;


            //Figure out the right location:
            Vector3 spawnOffset;
            

            switch (dir) {
                case PopupFacingDir.Left:
                    spawnOffset = new Vector3(1, 0, 0);
                    arrow.anchoredPosition = new Vector2(-123 - 5, ((bg.rect.height + bg.rect.position.y) / 2) + 5);
                    arrow.localEulerAngles = new Vector3(0, 0, -90);

                    break;
                
                default:
                case PopupFacingDir.Down:
                    spawnOffset = new Vector3(0, 0.08f, -0.01f);
                    arrow.anchoredPosition = new Vector2(0, 0);
                    arrow.localEulerAngles = new Vector3(0, 0, 0);

                    break;
                
                
                case PopupFacingDir.Right:
                    spawnOffset = new Vector3(-1, 0, 0);
                    arrow.anchoredPosition = new Vector2(123, ((bg.rect.height + bg.rect.position.y) / 2) + 5);
                    arrow.localEulerAngles = new Vector3(0, 0, 90);

                    break;
                
                case PopupFacingDir.TheBar:
                    spawnOffset = new Vector3(0, 0.1f, 0.06f);
                    arrow.anchoredPosition = new Vector2(0, 0);
                    arrow.localEulerAngles = new Vector3(0, 0, 0);

                    break;

            }


            //Vector3 dpPos = dpParent.transform.position;
            

            Vector3 goodRot = new Vector3(0, dpParent.transform.eulerAngles.y, 0);


            dpParent.SetOtherTransformRelativeToElement(hoverDP.transform, element, spawnOffset);
            
            hoverDP.SetOverlayPositionWithCurrent(true, false);
          

            //hoverDP.SetOverlayPosition(goodPos, goodRot, true, false);
            hoverDP.TransitionOverlayOpacity(1f, 0.5f);


            hoverDP.RequestRendering(true);
            
            _popupIsActive = true;

        }


    }

    public enum PopupFacingDir {
        Left,
        Right,
        Up,
        Down,
        TheBar
    }
    
}