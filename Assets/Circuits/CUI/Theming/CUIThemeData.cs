using UnityEngine;

namespace Circuits.CUI.Theming {
	[CreateAssetMenu(fileName = "CUIThemeData", menuName = "CUI/ThemeData", order = 0)]
	public class CUIThemeData : ScriptableObject {

		public string title;

		public Gradient primaryBGGradient;

		public Gradient primaryAccentGradient;

		public Gradient secondaryAccentGradient;

		public Gradient iconGradient;

	}
}