using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean.Localization
{
	/// <summary>This attribute allows you to select a translation from all the localizations in the scene.</summary>
	public class LeanTranslationNameAttribute : PropertyAttribute
	{
	}
}

#if UNITY_EDITOR
namespace Lean.Localization
{
	[CustomPropertyDrawer(typeof(LeanTranslationNameAttribute))]
	public class LeanTranslationNameDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var left  = position; left.xMax -= 40;
			var right = position; right.xMin = left.xMax + 2;
			var color = GUI.color;

			if (LeanLocalization.CurrentTranslations.ContainsKey(property.stringValue) == false)
			{
				GUI.color = Color.red;
			}

			EditorGUI.PropertyField(left, property);

			GUI.color = color;

			if (GUI.Button(right, "List") == true)
			{
				var menu = new GenericMenu();

				foreach (var translationName in LeanLocalization.CurrentTranslations.Keys)
				{
					menu.AddItem(new GUIContent(translationName), property.stringValue == translationName, () => { property.stringValue = translationName; property.serializedObject.ApplyModifiedProperties(); });
				}

				if (menu.GetItemCount() > 0)
				{
					menu.DropDown(right);
				}
				else
				{
					Debug.LogWarning("Your scene doesn't contain any phrases, so the phrase name list couldn't be created.");
				}
			}
		}
	}
}
#endif