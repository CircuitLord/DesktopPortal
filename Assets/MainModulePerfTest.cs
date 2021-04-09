using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using uWindowCapture;
using Debug = UnityEngine.Debug;

public class MainModulePerfTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {


        Debug.Log("start");
        UwcManager.onInitialized += () => {
            
            
            Thread thread = new Thread(PopWindows);
            
            thread.Start(UwcManager.windows);
            
        };

    }

    private void PopWindows(object obj) {
        
        Thread.Sleep(1000);


       Dictionary<int, UwcWindow> windows = (Dictionary<int, UwcWindow>) obj;

       

       lock (windows) {
           foreach (UwcWindow window in windows.Values) {

               //  var process = Process.GetProcessById(window.processId);

               // Debug.Log(process.MainModule?.FileName);
            
               window.PopulateFriendlyTitle();

               Debug.Log(window.friendlyTitle);


               //Debug.Log(window.title);
            
           } 
      }


       
       

        Debug.Log("finished finding window titles!");

        
        GC.Collect();
    }
    
    
    
    
    


    
    
}
