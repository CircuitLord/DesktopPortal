using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Keyboard;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

public class KConstantButton : MonoBehaviour, IPointerUpHandler {



	public Button button;

	public static Action<KeysEx> onButtonPress;
	

	public KeysEx key;

	public bool holdKey = false;


	private void Start() {
		button.onClick.AddListener(InvokeEvent);
	}

	private void InvokeEvent() {
		onButtonPress?.Invoke(key);
	}

	public void OnPointerUp(PointerEventData eventData) {
		
	}
}
