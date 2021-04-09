using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Wristboard;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace DesktopPortal.Notifications {
	public class NotificationPopupManager : MonoBehaviour {
	
		[Header("UI Elements")]

		[SerializeField] private Image _iconBG;
		[SerializeField] private Image _iconIMG;
		//[SerializeField] private Image _notificationIcon;

		[Header("Other Components")] 
		
		[SerializeField]
		private NotificationPopupFollower _notifPopupFollower;

		[SerializeField] private WristboardManager _wristboardManager;


		[Header("Configuration")] 
		
		[SerializeField]
		private float popupShowSpeed = 0.3f;

		[SerializeField] private float popupExistLength = 2f;

		[Header("Notification Icons")] 
		
		[SerializeField] private Sprite _ICONDISCORD;

		[SerializeField] private Sprite _ICONDEFAULT;
		
		
		
		
		public List<JsonNotification> notifsToShow = new List<JsonNotification>();

		public void AddNotifToQueue(JsonNotification notif) {
			notifsToShow.Add(notif);
		}
		
		
		
		
		

		private void Start() {
			
			//Hide the icons:
			_iconBG.DOFade(0, 0);
			_iconIMG.DOFade(0, 0);
			
			StartCoroutine(NotifPopupCycle());
			
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.H)) {
				var testNotif = new JsonNotification() {appName = "Discord", title = "Me Myself and I", body = "This is a simulated notification because I'm alone."};
				AddNotifToQueue(testNotif);
				//_wristboardManager.SetActiveNotif(testNotif);
			}
		}


		private IEnumerator NotifPopupCycle() {
			while (true) {

				//If we have a notification to show.
				if (notifsToShow.Count > 0) {

					_notifPopupFollower.isFirstMove = true;
					_notifPopupFollower.popupIsActive = true;
					
					SetNotifIconForAppName(notifsToShow[0].appName);
					
					//Reset it if it isn't already:
					_iconBG.DOFade(0, 0);
					_iconIMG.DOFade(0, 0);
					
					
					//Reset the position and scale:
					_iconBG.rectTransform.localPosition = new Vector3(0, -30f, 0);
					_iconBG.rectTransform.localScale = new Vector3(0, 0, 0);

					_iconIMG.rectTransform.localScale = new Vector3(1, 1, 1);
					
					//Start the animation:
					_iconBG.rectTransform.DOLocalMove(new Vector3(0, 30, 0), popupShowSpeed).SetEase(Ease.InOutCubic);
					_iconBG.rectTransform.DOScale(1, popupShowSpeed).SetEase(Ease.OutBounce);
					_iconBG.DOFade(1f, popupShowSpeed);
					
					yield return new WaitForSeconds(popupShowSpeed);
					
					//Fade in the program icon:
					_iconIMG.DOFade(1f, popupShowSpeed);
					
					//Let it exist for a while:
					yield return new WaitForSeconds(popupExistLength + popupShowSpeed);
					
					_notifPopupFollower.popupIsActive = false;

					_notifPopupFollower.flyToWatch = true;
					
					//Fade it out:
					_iconBG.DOFade(0f, popupShowSpeed);
					_iconBG.rectTransform.DOScale(0, popupShowSpeed);
					
					_iconIMG.DOFade(0f, popupShowSpeed);
					_iconIMG.rectTransform.DOScale(0f, popupShowSpeed);
					
					yield return new WaitForSeconds(popupShowSpeed);
					
					_notifPopupFollower.flyToWatch = false;
					
					//Remove it from the queue:
					notifsToShow.RemoveAt(0);
					
					//Haptics:
					//TODO: Notification haptics
					//_unitySteamVrHandler.SendHapticPulse(_wristboardManager.anchorHand, 1, 1f);
					
					yield return new WaitForSeconds(0.2f);
					
					//_unitySteamVrHandler.SendHapticPulse(_wristboardManager.anchorHand, 1, 1f);
					
					
					

					


				}
				
				
				yield return new WaitForSeconds(2f);
				
			}
			
			
		}
		
		




		private void SetNotifIconForAppName(string appName) {

			appName = appName.ToLower();
			

			switch (appName) {

				case "discord": 
					_iconIMG.sprite = _ICONDISCORD;
					break;
				
				default:
					_iconIMG.sprite = _ICONDEFAULT;
					break;
				
			}
			
		}






	}
}