using System;
using UnityEngine;

namespace CUI.Utils {
	
	[RequireComponent(typeof(RectTransform))]
	public class CUIMoveAtStart : MonoBehaviour {

		[SerializeField] private Vector3 customStartPos = Vector3.zero;
		
		private void Start() {
			GetComponent<RectTransform>().localPosition = customStartPos;
		}


		private void Reset() {
			customStartPos = GetComponent<RectTransform>().localPosition;
		}
	}
}