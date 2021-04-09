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
	public class DPSettingShowIf : MonoBehaviour {
		[SerializeField] private bool showIfToggleGroupIndex = false;

		[ShowIf("showIfToggleGroupIndex")] [SerializeField]
		private CUIToggleGroup toggleGroup;

		[ShowIf("showIfToggleGroupIndex")] [SerializeField]
		private List<int> toggleGroupValidIndexes;


		[SerializeField] private bool showIfToggleOn = false;

		[ShowIf("showIfToggleOn")] [SerializeField]
		private DPSettingUIToggle showIfToggle;

		private CUILayoutGroup _layoutGroup;

		private void Start() {
			_layoutGroup = transform.parent.GetComponent<CUILayoutGroup>();
			if (_layoutGroup == null) GetComponentInParent<CUILayoutGroup>();

			if (showIfToggleOn) showIfToggle.toggle.onToggled.AddListener(UpdateVisibility);


			if (showIfToggleGroupIndex) {
				toggleGroup.onIndexSelected.AddListener(CheckValidIndex);
			}
		}

		private void CheckValidIndex(int index) {
			UpdateVisibility(toggleGroupValidIndexes.Contains(index));

		}

		private void UpdateVisibility(bool show) {
			gameObject.SetActive(show);
			_layoutGroup.Solve();
		}

		private void OnDestroy() {
			//showIf.toggle.onToggled.RemoveListener(UpdateVisibility);
		}
	}
}