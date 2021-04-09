using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MirrorRect : MonoBehaviour {

    public ScrollRect Other; 

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
        GetComponent<ScrollRect>().normalizedPosition = Other.normalizedPosition;
    }
}
