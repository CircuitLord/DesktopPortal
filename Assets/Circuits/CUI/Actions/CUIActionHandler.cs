using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CUI.Actions {
	
	public static class CUIActionHandler {


		/*
		public static void Activate(List<CUIActionRef> actionRefs, bool instant = false, bool force = false) {
			foreach (CUIActionRef actionRef in actionRefs) {
				if (actionRef.action == null) continue;
				actionRef.action.ActivateIndex(actionRef.index);
			}
		}*/

		

		public static void Activate(List<CUIAction> actions, bool instant = false, bool force = false) {
			foreach (CUIAction action in actions) {
				if (action == null) continue;
				action.Activate(instant, force);
			}
		}
		
		public static void Deactivate(List<CUIAction> actions, bool instant = false, bool force = false) {
			foreach (CUIAction action in actions) {
				if (action == null) continue;
				action.Deactivate(instant, force);
			}
		}
		
	
	}

}
