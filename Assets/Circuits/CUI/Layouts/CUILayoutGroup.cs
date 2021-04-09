using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CUI.Layouts {
	public class CUILayoutGroup : MonoBehaviour {
		[OnValueChanged("Solve")] public CUILayoutOrigin origin = CUILayoutOrigin.HorizontalCenter;

		//[OnValueChanged("Solve")]
		//[SerializeField] private bool forceExpandPos = true;


		[HideIf("forceExpand")] [OnValueChanged("Solve")] [SerializeField]
		private float spacing = 10;


		[SerializeField] private bool forceExpand = false;


		[NonSerialized] private RectTransform m_Rect;

		protected RectTransform rectTransform {
			get {
				if (m_Rect == null)
					m_Rect = GetComponent<RectTransform>();
				return m_Rect;
			}
		}

		private List<RectTransform> children = new List<RectTransform>();
		private int childCount => children.Count;


		[Button]
		public void Solve() {
			Vector2 totalSize = Vector2.zero;

			children.Clear();

			foreach (RectTransform t in transform) {
				if (!t.gameObject.activeSelf) continue;
				children.Add(t);
				totalSize.x += t.rect.width;
				totalSize.y += t.rect.height;
			}

			if (childCount <= 0) return;

			totalSize.x += spacing * 2 * childCount;
			totalSize.y += spacing * 2 * childCount;

			float amt = rectTransform.rect.width / (childCount + 1);


			for (int i = 0; i < children.Count; i++) {
				RectTransform child = children[i];

				Vector2 anchor = Vector2.zero;
				Vector2 moveDir = Vector2.zero;

				switch (origin) {
					case CUILayoutOrigin.HorizontalLeft:
						anchor = new Vector2(0, 0.5f);
						moveDir = Vector2.right;
						amt = rectTransform.rect.width / (childCount + 1);
						break;

					case CUILayoutOrigin.HorizontalCenter:
						anchor = new Vector2(0.5f, 0.5f);
						//anchor = new Vector2(0, 0.5f);
						moveDir = Vector2.right;
						amt = rectTransform.rect.width / (childCount + 1);
						break;

					case CUILayoutOrigin.HorizontalRight:
						anchor = new Vector2(1.0f, 0.5f);
						moveDir = Vector2.left;
						amt = rectTransform.rect.width / (childCount + 1);
						break;


					case CUILayoutOrigin.VerticalTop:
						anchor = new Vector2(0.5f, 1f);
						moveDir = Vector2.down;
						amt = rectTransform.rect.height / (childCount + 1);
						break;

					case CUILayoutOrigin.VerticalCenter:
						anchor = new Vector2(0.5f, 0.5f);
						moveDir = Vector2.down;
						amt = rectTransform.rect.height / (childCount + 1);
						break;

					case CUILayoutOrigin.VerticalBottom:
						anchor = new Vector2(0.5f, 0f);
						moveDir = Vector2.up;
						amt = rectTransform.rect.height / (childCount + 1);
						break;


					default:
						anchor = new Vector2(0.5f, 0.5f);
						moveDir = Vector2.right;
						amt = rectTransform.rect.width / (childCount + 1);
						break;
				}

				child.anchorMin = anchor;
				child.anchorMax = anchor;


				Vector2 expandPos = Vector2.zero;

				if (forceExpand) {

					switch (origin) {
						
						case CUILayoutOrigin.HorizontalLeft:
						case CUILayoutOrigin.HorizontalCenter:
							expandPos = new Vector2(amt * (i + 1) * moveDir.x, amt * (i + 1) * moveDir.y);
							child.anchoredPosition = expandPos;
							continue;
						



					}

				}

				// If this is the first object, position it in the right spot so the others can be relative to it
				else if (i == 0) {
					Vector2 val = Vector2.zero;

					//Position the first object
					switch (origin) {
						case CUILayoutOrigin.HorizontalCenter:
							val.x = (totalSize.x / -2f) + (child.rect.width / 2f) + spacing;
							break;

						case CUILayoutOrigin.HorizontalLeft:
						case CUILayoutOrigin.HorizontalRight:
							val.x = ((child.rect.width / 2f) + spacing) * moveDir.x;
							break;

						case CUILayoutOrigin.VerticalCenter:
							val.y = (totalSize.y / 2f) - (child.rect.height / 2f) - spacing;
							break;

						case CUILayoutOrigin.VerticalTop:
						case CUILayoutOrigin.VerticalBottom:
							val.y = ((child.rect.height / 2f) + spacing) * moveDir.y;
							break;
					}

					child.anchoredPosition = new Vector2(val.x, val.y);

					continue;
				}


				float newSpacing = spacing;

				var layoutElement = child.GetComponent<CUILayoutElement>();
				if (layoutElement != null && layoutElement.overridePadding) newSpacing = layoutElement.paddingOverride;


				RectTransform prev = children[i - 1];

				float x = prev.anchoredPosition.x + (((prev.rect.width / 2f) + (newSpacing * 2f) + (child.rect.width / 2f)) * moveDir.x);
				float y = prev.anchoredPosition.y + (((prev.rect.height / 2f) + (newSpacing * 2f) + (child.rect.height / 2f)) * moveDir.y);

				child.anchoredPosition = new Vector2(x, y);


				//else {

				/*Vector2 goodPos = new Vector2();

				if (i <= 0) {
					goodPos = new Vector2(child.rect.width / 2f, 0f);
					//child.anchoredPosition = goodPos;
				}
				
				else if (children[i - 1] != null) {
					RectTransform rect = children[i - 1];

					//Find the edge of the previous child
					goodPos.x = rect.anchoredPosition.x + rect.rect.width / 2f + (spacing / 2f);
					
					//Add on half of this child

					goodPos.x += child.rect.width / 2f + (spacing / 2f);

					child.anchoredPosition = goodPos;

				}*/
				//}
			}
		}
	}


	public enum CUILayoutOrigin {
		HorizontalLeft,
		HorizontalCenter,
		HorizontalRight,
		VerticalTop,
		VerticalCenter,
		VerticalBottom
	}
}