using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesktopPortal.UI {
	
	public class DPSettingDescription : MonoBehaviour {


		public DPSettingUIBase setting;
		
		public string id;

		[TextArea(3, 5)]
		public string description;

		public Sprite sprite;

		public PointerNotifier pointerNotifier;


		private void Reset() {
			setting = GetComponent<DPSettingUIBase>();
		}
	}
}