using Sirenix.OdinInspector;
using UnityEngine;

namespace CUI.Layouts {
	public class CUILayoutElement : MonoBehaviour {


		public bool overridePadding = false;
		
		[ShowIf("overridePadding")]
		public float paddingOverride = 0f;

	}
}