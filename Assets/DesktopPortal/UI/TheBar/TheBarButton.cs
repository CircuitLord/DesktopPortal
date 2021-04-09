using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Haptics;
using DesktopPortal.Overlays;
using DG.Tweening;
using DPCore;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPortal.UI {
	public class TheBarButton : MonoBehaviour {
		public Image targetImage;

		public bool isHovering = false;

		public DPCameraOverlay dp;
		public RectTransform rootCanvas;

		public void OnDown() {
			targetImage.rectTransform.DOScale(1.1f, 0.3f);
		}

		public void OnUp() {
			targetImage.rectTransform.DOScale(1f, 0.3f);
		}
		

		public void OnHover() {
			if (isHovering) {
				return;
			}

			Vector2 localPos = transform.position - rootCanvas.transform.position;
			

			float overlayHalfX = dp.overlay.width / 2;


			float canvasHalfX = (rootCanvas.rect.width * rootCanvas.localScale.x) / 2;
			float canvasHalfY = (rootCanvas.rect.height * rootCanvas.localScale.y) / 2;

			Vector2 offsetPos = new Vector2();
			
			offsetPos.x = Maths.Linear(localPos.x, -canvasHalfX, canvasHalfX, -overlayHalfX, overlayHalfX);

			//HoverInfoManager.I.ShowPopup("TheBar/HoverSettings", PopupFacingDir.TheBar, dp, rootCanvas);

			isHovering = true;

			targetImage.rectTransform.DOLocalMoveY(4f, 0.5f);
			//TODO: get active hand
			//HapticsManager.SendHaptics(2, HapticsPreset.UIHover);
		}

		public void OnLeave() {
			isHovering = false;

			targetImage.rectTransform.DOLocalMoveY(0, 0.5f);
		}
	}
}