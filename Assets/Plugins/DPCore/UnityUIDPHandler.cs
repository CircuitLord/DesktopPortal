using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DPCore {
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	
	public class UnityUIDPHandler {

		
		public Camera cam;
		public PointerEventData pD = new PointerEventData(EventSystem.current);
		public AxisEventData aD = new AxisEventData(EventSystem.current);

		public HashSet<Selectable> GetUITargets(GraphicRaycaster gRay, PointerEventData pD) {
			if (float.IsNaN(pD.position.y)) return null;
			if (float.IsInfinity(pD.position.y)) return null;

			cam = gRay.eventCamera;

			aD.Reset();
			aD.moveVector = (this.pD.position - pD.position);

			float x1 = this.pD.position.x,
				x2 = pD.position.x,
				y1 = this.pD.position.y,
				y2 = pD.position.y;

			float xDiff = x1 - x2;
			float yDiff = y1 - y2;

			MoveDirection dir = MoveDirection.None;

			if (xDiff > yDiff)
				if (xDiff > 0f)
					dir = MoveDirection.Right;
				else if (xDiff < 0f)
					dir = MoveDirection.Left;
				else if (yDiff > xDiff)
					if (yDiff > 0f)
						dir = MoveDirection.Up;
					else if (yDiff < 0f)
						dir = MoveDirection.Down;

			aD.moveDir = dir;

			var ray = cam.ScreenPointToRay(pD.position);


			//Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);

			List<RaycastResult> hits = new List<RaycastResult>();
			HashSet<Selectable> uibT = new HashSet<Selectable>();

			gRay.Raycast(pD, hits);

			if (hits.Count > 0)
				pD.pointerCurrentRaycast = pD.pointerPressRaycast = hits[0];

			for (int i = 0; i < hits.Count; i++) {
				var go = hits[i].gameObject;
				Selectable u = GOGetter(go);

				if (u)
					uibT.Add(u);
			}

			this.pD = pD;

			return uibT;
		}

		public Selectable GOGetter(GameObject go, bool tryPar = false) {
			Selectable sel = go.GetComponentInParent<Selectable>();

			return sel;
		}

		public void EnterTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerEnterHandler);
		}

		public void ExitTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerExitHandler);
		}

		public void DownTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerDownHandler);
		}

		public void UpTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.pointerUpHandler);
		}

		public void SubmitTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.submitHandler);
		}

		public void StartDragTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.beginDragHandler);
		}

		public void DragTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.dragHandler);
		}

		public void MoveTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, aD, ExecuteEvents.moveHandler);
		}

		public void EndDragTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.endDragHandler);
		}

		public void DropTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.dropHandler);
		}
		
		public void ScrollTargets(HashSet<Selectable> t) {
			foreach (Selectable b in t)
				ExecuteEvents.Execute(b.gameObject, pD, ExecuteEvents.scrollHandler);
		}
	}
}