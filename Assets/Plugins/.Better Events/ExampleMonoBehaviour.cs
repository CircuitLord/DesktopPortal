using Sirenix.OdinInspector;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour {

    public BetterEvent yay;
    
    public BetterEvent<float> MyEvent;

    
    

    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            MyEvent.Invoke(Time.time);
        }
    }

    public void Yay(float f) {
        Debug.Log(f);
    }
}
