using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


public class MultipuleComponentsEditor : Editor
{
	public override void OnInspectorGUI() {
		DrawDefaultInspector();


		GameObject go = (GameObject) target;

		foreach (var comp in go.GetComponents<Component>()) {
			
			
			
			Type myObjectType = comp.GetType();
			PropertyInfo[] infos = myObjectType.GetProperties();
			Debug.Log("Component --> " + myObjectType);
 
			foreach (PropertyInfo info in infos) {

				var test = info.GetValue(BindingFlags.Default);

				if (test is MonoBehaviour) {
					
					
					
				}
				
				if (info != null) {
					Debug.Log("Variable --> " + info.Name + "    " + info.GetValue(comp));
				}
			}

		}
			
			
			
		


		



	}

	private void OnValidate() {
		
	}
}
