using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace DesktopPortal.UI.Components {
	public abstract class DPButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {


		[HideInInspector] public bool isHovered = false;
		[HideInInspector] public bool isPressed = false;

		public UnityEvent onMouseDown = new UnityEvent();
		public UnityEvent onMouseUp = new UnityEvent();
		public UnityEvent onMouseStartHover = new UnityEvent();
		public UnityEvent onMouseEndHover = new UnityEvent();
		
		
		
		[SerializeField] protected float animDur = 0.3f;



		


		public virtual void OnPointerEnter(PointerEventData eventData) {
			if (isHovered) return;
			isHovered = true;
			
			HandleHoverAnim();
			
			onMouseStartHover?.Invoke();
		}

		public virtual void OnPointerExit(PointerEventData eventData) {
			isHovered = false;
			
			TryEndHoverAnim();
			
			onMouseEndHover?.Invoke();
		}

		public virtual void OnPointerDown(PointerEventData eventData) {
			if (isPressed) return;
			isPressed = true;
			
			HandleDownAnim();
			
			onMouseDown.Invoke();
		}

		public virtual void OnPointerUp(PointerEventData eventData) {
			isPressed = false;
			
			TryEndDownAnim();
			
			onMouseUp.Invoke();
		}


		public abstract void HandleHoverAnim();

		public abstract void TryEndHoverAnim();

		public abstract void HandleDownAnim();

		public abstract void TryEndDownAnim();


	}
}