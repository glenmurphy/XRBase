using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class VRMain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(InitXR());
        SetRoomScale();
    }

    void SetRoomScale() {
        var xrInput = XRGeneralSettings.Instance.Manager.activeLoader.GetLoadedSubsystem<XRInputSubsystem>();
        xrInput.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
        xrInput.TryRecenter();
    }
    
    public IEnumerator InitXR()
    {
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        XRGeneralSettings.Instance.Manager.StartSubsystems();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
