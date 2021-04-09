using System;
using UnityEngine;

namespace CUI.Utils {
	public abstract class CUIThemeamble : MonoBehaviour {
		
		private void OnEnable() {
			CUIThemeManager.RegisterThemeable(this);
		}

		private void OnDisable() {
			CUIThemeManager.UnregisterThemeable(this);
		}


		public abstract void UpdateTheme();
	}
}