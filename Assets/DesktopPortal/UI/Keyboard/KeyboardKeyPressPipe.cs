using System.Collections;
using System.Collections.Generic;
using DPCore;
using UnityEngine;
using UnityEngine.EventSystems;

public class KeyboardKeyPressPipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {


    public UnityBoolEvent onPress;


    public void OnPointerDown(PointerEventData eventData) {
        Debug.Log("down");
        onPress?.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData) {
        onPress?.Invoke(false);
    }
}
