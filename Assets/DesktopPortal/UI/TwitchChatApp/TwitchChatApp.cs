using System;
using System.Collections;
using System.Collections.Generic;
using DesktopPortal.Overlays;
using DPCore.Apps;
using UnityEngine;
using UnityEngine.UI;


namespace DesktopPortal.UI {
	public class TwitchChatApp : DPApp {

		/*[SerializeField] private int maximumMessages = 25;


		[SerializeField] private GameObject chatMessagePF;


		[SerializeField] private RectTransform messageSpawnTrans;


		[SerializeField] private Color colorType1;
		[SerializeField] private Color colorType2;



		private List<TwitchChatMessage> messages = new List<TwitchChatMessage>();

		//private static Dictionary<string, Color> cachedUserColors = new Dictionary<string, Color>();



		private bool isType1 = true;

		private void Start() {
			TwitchIRC.I.messageRecievedEvent.AddListener(OnNewMessage);


			foreach (Transform child in messageSpawnTrans) {
				Destroy(child.gameObject);
			}
			
		}


		void OnNewMessage(string msg) {

			int colorIndex = msg.IndexOf("color=");
			string colorString = msg.Substring(colorIndex + 6, 7);
			if (!colorString.StartsWith("#")) colorString = "#FFFFFF";

			int msgIndex = msg.IndexOf("PRIVMSG #");
			string msgString = msg.Substring(msgIndex + TwitchIRC.I.channelName.Length + 11);
			
			
			int userIndex = msg.IndexOf("display-name=");
			string username = "";
			for (int i = userIndex + 13; userIndex < msg.Length; i++) {
				if (msg[i].ToString() == ";") {
					break;
				}
				else {
					username += msg[i];
				}
			}

			AddChatMessage(colorString, username, msgString);
		}



		private void AddChatMessage(string userColor, string username, string content) {

			TwitchChatMessage chat = Instantiate(chatMessagePF, messageSpawnTrans).GetComponent<TwitchChatMessage>();

			if (isType1) chat.bg.color = colorType1;
			else chat.bg.color = colorType2;

			isType1 = !isType1;
			
			chat.text.SetText("<color=" + userColor + ">" + username + "</color>: " + content);
			LayoutRebuilder.ForceRebuildLayoutImmediate(chat.gameObject.GetComponent<RectTransform>());

			messages.Add(chat);

			if (messages.Count > maximumMessages) {
				Destroy(messages[0].gameObject);
				messages.RemoveAt(0);
			}
			
			//LayoutRebuilder.ForceRebuildLayoutImmediate(messageSpawnTrans);
		}
		*/







	
	
	}
}