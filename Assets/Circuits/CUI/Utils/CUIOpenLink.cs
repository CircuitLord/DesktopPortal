using Sirenix.OdinInspector;
using UnityEngine;

namespace CUI.Utils {
	public class CUIOpenLink : MonoBehaviour {
		
		
		[Required]
		[SerializeField] public string linkToLaunch;
		
		public void Open() {

			System.Diagnostics.Process.Start(linkToLaunch);
			
		}
		
		
		
	}
}