using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


namespace DesktopPortal.Demo {
	
	[ExecuteInEditMode]
	public class DemoManager : MonoBehaviour
	{

		[SerializeField] private List<GameObject> demoObjects;


		
		
		public static bool isDemo = false;
		

		private void OnEnable() {

			foreach (GameObject go in demoObjects) {
				go.SetActive(true);
			}


			isDemo = true;

		}


		private void OnDisable() {
			foreach (GameObject go in demoObjects) {
				go.SetActive(false);
			}

			isDemo = false;
		}
	}
}