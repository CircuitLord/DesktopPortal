using System.Collections.Generic;
using CUI.Utils;
using UnityEngine;

namespace CUI {
	public class CUIThemeManager : MonoBehaviour {





		public static List<CUIThemeamble> activeThemeables = new List<CUIThemeamble>();



		public static void RegisterThemeable(CUIThemeamble themeable) {
			
			if (activeThemeables.Contains(themeable)) return;
			
			activeThemeables.Add(themeable);

		}

		public static void UnregisterThemeable(CUIThemeamble themeable) {
			if (!activeThemeables.Contains(themeable)) return;
			
			activeThemeables.Remove(themeable);
		}
		
		
		
	}
}