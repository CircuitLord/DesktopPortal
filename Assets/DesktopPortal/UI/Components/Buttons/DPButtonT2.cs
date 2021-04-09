using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DesktopPortal.UI.Components {
	public class DPButtonT2 : DPButton {
		
		
		//Static configuration:
		
		private static float hoverValueShift = 0.04f;
		private static float pressValueShift = 0.02f;
		
		

		[SerializeField] private RectTransform bgShadow;
		[SerializeField] private Image bg;

		[SerializeField] private RectTransform icon;

		
		
		
		private Color hoverBGColor;
		private Color pressBGColor;
		
		private Color initialBGColor;
		private Vector2 initalBGShadowPos;

		private Vector2 hoverBGShadowPos;

		private Vector2 hoverIconPos;
		
		


		private Tweener bgColor;
		private Tweener bgShadowMove;
		private Tweener iconScale;


		private void Start() {
			initalBGShadowPos = bgShadow.localPosition;

			hoverBGShadowPos = initalBGShadowPos * 2.7f;

			
			initialBGColor = bg.color;

			float h, s, v;
			Color.RGBToHSV(bg.color, out h, out s, out v);

			hoverBGColor = Color.HSVToRGB(h, s, v + hoverValueShift);
			pressBGColor = Color.HSVToRGB(h, s, v + pressValueShift);

		}


		public override void HandleHoverAnim() {

			bgColor = bg.DOColor(hoverBGColor, animDur);

			iconScale = icon.DOScale(1.1f, animDur);
			
			bgShadowMove = bgShadow.DOLocalMove(hoverBGShadowPos, animDur);
			

		}

		public override void TryEndHoverAnim() {
			bgColor?.Kill();
			bgColor = bg.DOColor(initialBGColor, animDur);
			
			iconScale?.Kill();
			iconScale = icon.DOScale(1f, animDur);

			bgShadowMove?.Kill();
			bgShadowMove = bgShadow.DOLocalMove(initalBGShadowPos, animDur);
		}

		public override void HandleDownAnim() {
			bgColor?.Kill();
			bgColor = bg.DOColor(pressBGColor, animDur);

			iconScale?.Kill();
			iconScale = icon.DOScale(0.9f, animDur);
			
			bgShadowMove?.Kill();
			bgShadowMove = bgShadow.DOLocalMove(Vector2.zero, animDur);
		}

		public override void TryEndDownAnim() {
			bgColor?.Kill();
			
			iconScale?.Kill();

			bgShadowMove?.Kill();

			if (isHovered) {
				HandleHoverAnim();
			}
			else {
				TryEndHoverAnim();
			}
			
		}
	}
}