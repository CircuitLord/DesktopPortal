using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using DesktopPortal.Overlays;
using Duplicator;
using UnityEngine;
using Valve.VR;

public class testduplicator : MonoBehaviour {
    
    
    
    private Duplicator.Duplicator _duplicator;
    private DuplicatorOptions _options;
    
    // Start is called before the first frame update
    void Start()
    {
        
        _options = new DuplicatorOptions();
        _options.CaptureMicrophone = false;
        Debug.Log(_options.GetAdapter().Description.DeviceId);
        _options.Adapter = "NVIDIA GeForce GTX 1080";
     
        _duplicator = new Duplicator.Duplicator(_options);
        _duplicator.PropertyChanged += OnDuplicatorPropertyChanged;
        _duplicator.InformationAvailable += OnDuplicatorInformationAvailable;
        _duplicator.Size = new SharpDX.Size2(500, 300);

        _duplicator.StartDuplicating();
    }

    private void OnDuplicatorInformationAvailable(object sender, DuplicatorInformationEventArgs e) {
        Debug.Log("change!");
    }

    private void OnDuplicatorPropertyChanged(object sender, PropertyChangedEventArgs e) {
        Debug.Log("other change!");
    }

    // Update is called once per frame
    void Update()
    {

        if (DPDesktopOverlay.overlays.Count > 1) {
            DPDesktopOverlay.overlays[0].overlay._overlayTexture_t.handle = _duplicator.Hwnd;
            
            EVROverlayError error = OpenVR.Overlay.SetOverlayTexture( DPDesktopOverlay.overlays[0].overlay.handle, ref DPDesktopOverlay.overlays[0].overlay._overlayTexture_t);

            Debug.Log(error);
        }
        
        
        
    }
}
