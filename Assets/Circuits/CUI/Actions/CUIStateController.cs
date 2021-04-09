/*using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CUI.Actions {
	public class CUIStateController : MonoBehaviour {
		
		public List<CUIActionRef> state0 = new List<CUIActionRef>();

		public List<CUIActionRef> state1 = new List<CUIActionRef>();
		public List<CUIActionRef> state2 = new List<CUIActionRef>();
		public List<CUIActionRef> state3 = new List<CUIActionRef>();
		public List<CUIActionRef> state4 = new List<CUIActionRef>();

		
		[OnValueChanged("OnPreview")] [SerializeField]
		[PropertyRange(0, 4)]
		private int previewState = 0;


		public void OnPreview() {
			switch (previewState) {
				case 0: 
					CUIActionHandler.Activate(state0);
					break;
				case 1: 
					CUIActionHandler.Activate(state1);
					break;
				case 2: 
					CUIActionHandler.Activate(state2);
					break;
				case 3: 
					CUIActionHandler.Activate(state3);
					break;
				case 4: 
					CUIActionHandler.Activate(state4);
					break;
			}
		}
		

	}
}*/