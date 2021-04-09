using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Keyboard;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPortal.UI {
	public class KButton : MonoBehaviour {


		public static Action<KButton, bool> onButtonPress;

		public bool isPersistent = false;

		public bool isShift = false;


		[SerializeField] private bool spamKeyWhenHeld = true;

		[SerializeField] private PointerNotifier pointerNotifier;
		
		
		[SerializeField] private TextMeshProUGUI symbol;

		[SerializeField] private TextMeshProUGUI topRight;
		
		[SerializeField] private TextMeshProUGUI bottomRight;

		[HideInInspector] public string mainChar;
		[HideInInspector] public string shiftChar;
		[HideInInspector] public string altChar;

		public Image icon;


		public KeysEx key;


		private bool isDown = false;

		private float holdTime = 0f;

		private bool spammingKey = false;

		private Coroutine C_SpamKey;

		private void Start() {

			pointerNotifier.onPointerPress += OnPress;
		}

		public void Init() {

			
			//button.onClick.AddListener(HandleKeyPress);

			if (isPersistent) {
				//Debug.Log("init" + key.ToString());
				return;
			}
			
			symbol.SetText(mainChar);
			
			
			//If it's a normal letter when it goes uppercase just don't show it 
			if (shiftChar == mainChar.ToUpper()) {
				topRight.SetText("");
			}
			else {
				topRight.SetText(shiftChar);
			}
			
			
			bottomRight.SetText(altChar);

		}


		private void Update() {
			if (isDown) holdTime += Time.deltaTime;

			if (spamKeyWhenHeld && !spammingKey && holdTime > 0.8f) {
				C_SpamKey = StartCoroutine(SpamKey());
			}
			
			
		}

		private IEnumerator SpamKey() {

			spammingKey = true;

			while (true) {
				onButtonPress?.Invoke(this, true);
				onButtonPress?.Invoke(this, false);
				
				yield return new  WaitForSeconds(0.15f);
				
			}
			

			yield break;
		}

		public void OnPress(bool down) {

			if (isDown == down) return;
			
			isDown = down;

			if (!down) {
				holdTime = 0f;
				if (C_SpamKey != null) {
					StopCoroutine(C_SpamKey);
					C_SpamKey = null;
				}

				spammingKey = false;
			}
			
			onButtonPress?.Invoke(this, down);


		}
		
		
		public void UpdateValues(bool shift = false, bool alt = false) {

			if (shift) {
				symbol.SetText(shiftChar);
				topRight.SetText(mainChar);
				bottomRight.SetText(altChar);
			}
			else if (alt) {
				symbol.SetText(altChar);
				
			}


			//Not shift or alt
			else {
				symbol.SetText(mainChar);
				topRight.SetText(shiftChar);
				bottomRight.SetText(altChar);

			}

		}



	}
}