using System;
using System.IO;
using Unity.Collections;
using UnityEngine;

namespace Circuits {
	public class CLog : MonoBehaviour {
		public static CLog inst;

		//[SerializeField] private bool useUnityLogging = true;
		//[SerializeField] private bool useFileLogging = false;

		private string logFilePath;


		private void Start() {
			inst = this;

			logFilePath = Path.Combine(Application.persistentDataPath, "log.txt");

			CheckConfigValid();
		}


		public static void Log(object thing, CLogLevel level = CLogLevel.Info) {
			_log(thing.ToString(), level);
		}

		public static void Log(string name, object value, CLogLevel level = CLogLevel.Info) {
			_log(name + " -> " + value.ToString(), level);
		}


		private static void _log(string msg, CLogLevel level) {
			
			if (Application.isEditor) {
				switch (level) {
					case CLogLevel.Warning:
						Debug.LogWarning(msg);
						break;
					case CLogLevel.Error:
						Debug.LogError(msg);
						break;
					default:
						Debug.Log(msg);
						break;
				}
			}
			else {
				LogToFile(msg, level);
			}
			
		}


		private static void LogToFile(string msg, CLogLevel level) {
			string typeText;

			switch (level) {
				case CLogLevel.Warning:
					typeText = "WARNING: ";
					break;
				case CLogLevel.Error:
					typeText = "ERROR: ";
					break;
				default:
					typeText = "INFO: ";
					break;
			}

			File.AppendAllText(inst.logFilePath, typeText + msg);
		}


		private static void CheckConfigValid() {
			if (!File.Exists(inst.logFilePath)) {
				File.Create(inst.logFilePath);
			}
		}
	}

	public enum CLogLevel {
		Info,
		Warning,
		Error
	}
}