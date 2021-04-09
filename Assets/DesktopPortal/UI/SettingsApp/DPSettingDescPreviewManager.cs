using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DesktopPortal.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DPSettingDescPreviewManager : MonoBehaviour
{


	[SerializeField] private List<DPSettingDescription> settings;

	[SerializeField] private Image image;

	[SerializeField] private TextMeshProUGUI title;

	[SerializeField] private TextMeshProUGUI content;

	[SerializeField] private Sprite fallbackSprite;
	


	private void Start() {

		settings = GetComponentsInChildren<DPSettingDescription>().ToList();

		foreach (var desc in settings) {
			desc.pointerNotifier.onPointerEnter += () => Show(desc);
		}

	}


	private void Show(DPSettingDescription desc) {


		if (desc.sprite == null) {
			image.sprite = fallbackSprite;
		}
		else {
			image.sprite = desc.sprite;
		}

		title.text = desc.setting.title.text;
		content.text = desc.description;



	}
	
	
}
