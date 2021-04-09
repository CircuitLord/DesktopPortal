using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Xml;
using TMPro;
using UnityEngine;
using DG.Tweening;
using Valve.VR;


public class NotificationOverlayManager : MonoBehaviour {

	//[SerializeField] private Unity_Overlay notifOverlay;
	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private TextMeshProUGUI bodyText;

	[SerializeField] private Transform _hmdTransform;

	//[SerializeField] private Vector3 _transformOffset;
	[SerializeField] private float offsetForward = 1.4f;
	[SerializeField] private float offsetDown = 0.3f;
	
	private FileSystemWatcher _notifSaveWatcher;


	private string notifSavePath;

	private JsonNotification activeNotif;
	private bool newNotif = false;


	private bool notifIsActive = false;



	void Start() {
		

		
		notifSavePath = Path.Combine(Application.persistentDataPath, "latestNotif.json");

		if (!File.Exists(notifSavePath)) {
			File.Create(notifSavePath);
		}
		
		_notifSaveWatcher = new FileSystemWatcher(Path.GetDirectoryName(notifSavePath), "latestNotif.json");
		_notifSaveWatcher.NotifyFilter = NotifyFilters.LastWrite;
		_notifSaveWatcher.Changed += NotifFileUpdated;
		_notifSaveWatcher.EnableRaisingEvents = true;
		
		
		//notifOverlay.SetOpacity(0f);
		//notifOverlay.gameObject.SetActive(false);
		
		

	}


	private void Update() {
		if (newNotif) {
			_notifSaveWatcher.EnableRaisingEvents = true;
			newNotif = false;

			Vector3 finalPos = _hmdTransform.position + _hmdTransform.forward * offsetForward;
			finalPos = new Vector3(finalPos.x, finalPos.y - offsetDown, finalPos.z);

			//notifOverlay.transform.position = finalPos;
			
			
			//notifOverlay.transform.eulerAngles = _hmdTransform.eulerAngles;
			//notifOverlay.transform.rotation = _hmdTransform.rotation;
			
			StartCoroutine(ShowNotification(activeNotif.appName, activeNotif.title, activeNotif.body, 5f));
		}


		if (notifIsActive) {
			Vector3 velocity = Vector3.zero;

			Vector3 finalPos = _hmdTransform.position + _hmdTransform.forward * offsetForward;
			finalPos = new Vector3(finalPos.x, finalPos.y - offsetDown, finalPos.z);
			
		//	notifOverlay.transform.position = Vector3.SmoothDamp(notifOverlay.transform.position, finalPos, ref velocity, 0.3f);

			//Vector3 curEul = notifOverlay.transform.eulerAngles;
			//Vector3 desEul = _hmdTransform.eulerAngles;

			//Vector3 newEul = Vector3.Lerp(curEul, desEul, 0.1f);
			
			
			//Quaternion curRot = notifOverlay.transform.rotation;
			Quaternion desRot = _hmdTransform.rotation;

			//curRot = new Quaternion(Mathf.Lerp(curRot.x, desRot.x, Time.deltaTime), Mathf.Lerp(curRot.y, desRot.y, Time.deltaTime), Mathf.Lerp(curRot.z, desRot.z, Time.deltaTime), Mathf.Lerp(curRot.w, desRot.w, Time.deltaTime));

			//notifOverlay.transform.eulerAngles = Vector3.SmoothDamp(notifOverlay.transform.eulerAngles, _hmdTransform.eulerAngles, ref velocity, 0.1f);

			//notifOverlay.transform.rotation = Quaternion.Lerp(curRot, desRot, Time.deltaTime);
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
	





	public IEnumerator ShowNotification(string appName, string title, string body, float length) {
		

		

		StopCoroutine(HideNotification());

		Debug.Log("STARTED");
		
		
	//	notifOverlay.gameObject.SetActive(true);

		notifIsActive = true;
		
		titleText.SetText(appName + " : " + title);
		bodyText.SetText(body);

		//DOTween.To(notifOverlay.SetOpacity, notifOverlay.opacity, 0.4f, 0.5f);
		yield return new WaitForSeconds(0.5f);

		yield return new WaitForSeconds(length);

		StartCoroutine(HideNotification());


	}
	public IEnumerator HideNotification() {
		
		StopCoroutine(ShowNotification("", "", "", 0f));
		
	//	DOTween.To(notifOverlay.SetOpacity, notifOverlay.opacity, 0f, 0.5f);
		yield return new WaitForSeconds(0.5f);

		notifIsActive = false;
		
		//notifOverlay.gameObject.SetActive(false);
		
		
		
		
		_notifSaveWatcher.EnableRaisingEvents = true;
//
	}




	

	
}

[Serializable]
public class JsonNotification {
	public string appName;
	public string title;
	public string body;
}