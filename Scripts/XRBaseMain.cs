using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class XRBaseMain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SetRoomScale();
        if (!SmackPool.Instance) {
            SmackPool pool = gameObject.AddComponent<SmackPool>();
            pool.prefab = Resources.Load("Smack");
        }
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
