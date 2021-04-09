using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DesktopPortal.Wristboard;
using UnityEngine;


namespace DesktopPortal.Notifications {
	public class NotificationBridge : MonoBehaviour {

		[SerializeField] private NotificationPopupManager _notificationPopupManager;

		[SerializeField] private WristboardManager _wristboardManager;
		
		
		private FileSystemWatcher _notifSaveWatcher;
		
		private string notifSavePath;

		private JsonNotification activeNotif;
		private bool newNotif = false;

		
		
		void Start() {
			notifSavePath = Path.Combine(Application.persistentDataPath, "latestNotif.json");

			if (!File.Exists(notifSavePath)) {
				File.Create(notifSavePath);
			}
		
			_notifSaveWatcher = new FileSystemWatcher(Path.GetDirectoryName(notifSavePath), "latestNotif.json");
			_notifSaveWatcher.NotifyFilter = NotifyFilters.LastWrite;
			_notifSaveWatcher.Changed += NotifFileUpdated;
			_notifSaveWatcher.EnableRaisingEvents = true;
			

		}

		private void Update() {

			if (newNotif) {
				_notificationPopupManager.AddNotifToQueue(activeNotif);
				
				//_wristboardManager.SetActiveNotif(activeNotif);
				
				_notifSaveWatcher.EnableRaisingEvents = true;
				newNotif = false;
			}
			
			
		}


		private void NotifFileUpdated(object sender, FileSystemEventArgs e) {

			Debug.Log("File changed");
		
			_notifSaveWatcher.EnableRaisingEvents = false;
		
			JsonNotification notif = JsonUtility.FromJson<JsonNotification>(File.ReadAllText(notifSavePath));
		
			newNotif = true;

			activeNotif = notif;

			return;

		}
		
	}
}