using System;
using System.Collections;
using System.Collections.Generic;
using CUI.Components;
using CUI.Layouts;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPortal.UI {
	public class DPSettingHideIf : MonoBehaviour {
		[SerializeField] private bool hideIfToggleGroupIndex = false;

		[ShowIf("hideIfToggleGroupIndex")] [SerializeField]
		private CUIToggleGroup toggleGroup;

		[ShowIf("hideIfToggleGroupIndex")] [SerializeField]
		private List<int> toggleGroupValidIndexes;


		[SerializeField] private bool hideIfToggleOn = false;

		[ShowIf("hideIfToggleOn")] [SerializeField]
		private DPSettingUIToggle hideIfToggle;

		private CUILayoutGroup _layoutGroup;

		private void Start() {
			_layoutGroup = transform.parent.GetComponent<CUILayoutGroup>();
			if (_layoutGroup == null) GetComponentInParent<CUILayoutGroup>();

			if (hideIfToggleOn) hideIfToggle.toggle.onToggled.AddListener(UpdateVisibility);


			if (hideIfToggleGroupIndex) {
				toggleGroup.onIndexSelected.AddListener(CheckValidIndex);
			}
		}

		private void CheckValidIndex(int index) {
			UpdateVisibility(toggleGroupValidIndexes.Contains(index));

		}

		private void UpdateVisibility(bool show) {
			gameObject.SetActive(!show);
			_layoutGroup.Solve();
		}

		private void OnDestroy() {
			//showIf.toggle.onToggled.RemoveListener(UpdateVisibility);
		}
	}
}