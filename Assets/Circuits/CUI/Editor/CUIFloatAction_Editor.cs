/*using System.Collections.Generic;
using CUI.Actions;
using UnityEditor;
using UnityEngine;

namespace CUI.Editor {
	[CustomEditor(typeof(CUIGenericAction))]
	public class CUIFloatAction_Editor : UnityEditor.Editor {
		private string fieldName;
		private List<string> fieldNames;
		private MonoBehaviour targetScript;
		private SerializedProperty targetSerializedProperty;

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();

			// Field to select generic script component from scene.
			MonoBehaviour newTargetScript = (MonoBehaviour) EditorGUILayout.ObjectField("Generic Object", this.targetScript, typeof(MonoBehaviour), true);
			if (newTargetScript != this.targetScript) {
				this.fieldName = null;
				this.targetScript = newTargetScript;
			}

			if (this.targetScript != null) {
				// Creating SerializedObject from selected component.
				// Updating serialized object is not necessary as it is created the same frame.
				SerializedObject targetSerializedObject = new SerializedObject(this.targetScript);
				// Lists for relevant fields found:
				this.fieldNames = new List<string>();
				List<SerializedProperty> serializedProperties = new List<SerializedProperty>();
				// Goes through all properties of the selected object.
				// Serialized properties have iterator functionality built in.
				SerializedProperty serializedProperty = targetSerializedObject.GetIterator();
				serializedProperty.Next(true);
				do {
					// Runs code block if property is of correct type.
					switch (serializedProperty.propertyType) {
						case SerializedPropertyType.Float:
						case SerializedPropertyType.Integer:
							// Saves values found
							this.fieldNames.Add(serializedProperty.name);
							break;
						default:
							break;
					}
				} while (serializedProperty.Next(false));

				// Removes two hidden int options one probably shouldn't mess with
				this.fieldNames.RemoveAt(0);
				this.fieldNames.RemoveAt(0);
				// Ensures that there exist fields to choose from
				if (this.fieldNames.Count != 0) {
					// Selection of field
					int oldIndex = this.fieldNames.Contains(this.fieldName) ? this.fieldNames.IndexOf(this.fieldName) : 0;
					int newIndex = EditorGUILayout.Popup(oldIndex, this.fieldNames.ToArray());
					this.fieldName = this.fieldNames[newIndex];
					this.targetSerializedProperty = targetSerializedObject.FindProperty(this.fieldName);
				}
				else {
					EditorGUILayout.HelpBox("Given component has no float or int fields", MessageType.None);
					this.targetSerializedProperty = null;
				}

				if (this.targetSerializedProperty != null) {
					// Displays a field with no label with the selected field in the target script.
					EditorGUILayout.PropertyField(this.targetSerializedProperty, GUIContent.none);
					// Makes changes to target object.
					targetSerializedObject.ApplyModifiedProperties();
				}
			}
		}
	}
}*/