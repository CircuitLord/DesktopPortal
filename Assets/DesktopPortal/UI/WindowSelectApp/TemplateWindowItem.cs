using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using uWindowCapture;

namespace DesktopPortal.UI {
	public class TemplateWindowItem : MonoBehaviour, IPointerDownHandler {
		
		
		
		public delegate void WindowSelectAppWindowEvent(UwcWindow window);
		public static WindowSelectAppWindowEvent selectedEvent;
		
		
		
		
		
		public RawImage pic;
		public TextMeshProUGUI title;
		public UwcWindow window;


		public bool exists = false;




		private bool isPointerDown = false;
		
		//TODO: better animations

		

		public void OnPointerDown(PointerEventData eventData) {
			if (!isPointerDown) isPointerDown = true;
			else return;

			selectedEvent.Invoke(window);
		}
		
	}
}