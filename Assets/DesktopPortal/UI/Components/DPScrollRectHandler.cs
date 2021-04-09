using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DPScrollRectHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
	public static bool IsScrolling = false;
	public static GameObject Parent = null;

	public ScrollRect scrollRect;

	private bool foundScrollRect = false;

	private void Start() {
		TryFindScrollRect();
	}


	public void TryFindScrollRect(bool force = false) {
		if (scrollRect != null) {
			foundScrollRect = true;
			if (!force) return;
		}
		scrollRect = GetComponentInParent<ScrollRect>();

		if (scrollRect != null) foundScrollRect = true;
	}
	
	public void OnBeginDrag(PointerEventData eventData) {
		if (!IsScrolling) {
			scrollRect.OnBeginDrag(eventData);
			IsScrolling = true;
			Parent = this.gameObject;
		}
	}

	public void OnDrag(PointerEventData eventData) {
		//if (Parent = this.gameObject) {
		scrollRect.OnDrag(eventData);
		//}
	}

	public void OnEndDrag(PointerEventData eventData) {
		if (IsScrolling) {
			scrollRect.OnEndDrag(eventData);
			IsScrolling = false;
			Parent = null;
		}
	}

	public void OnScroll(PointerEventData eventData) {
		
		if (foundScrollRect) scrollRect.OnScroll(eventData);
	}
}