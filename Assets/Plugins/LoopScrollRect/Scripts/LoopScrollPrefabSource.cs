using UnityEngine;
using System.Collections;

namespace UnityEngine.UI {
	[System.Serializable]
	public class LoopScrollPrefabSource {
		public GameObject prefab;
		public string poolName;
		public int poolSize = 5;

		private bool initialized = false;

		public virtual GameObject GetObject() {
			if (!initialized) {
				SG.ResourceManager.Instance.InitPool(poolName, poolSize, prefab);
				initialized = true;
			}

			return SG.ResourceManager.Instance.GetObjectFromPool(poolName);
		}

		public virtual void ReturnObject(Transform go) {
			go.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
			SG.ResourceManager.Instance.ReturnObjectToPool(go.gameObject);
		}
	}
}