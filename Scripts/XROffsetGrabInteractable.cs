using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// From https://www.youtube.com/watch?v=FMu7hKUX3Oo&t=599s
public class XROffsetGrabInteractable : XRGrabInteractable
{
    private Vector3 initialAttachmentPos;
    private Quaternion initialAttachmentRot;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!attachTransform)
        {
            GameObject grab = new GameObject("Grab Pivot");
            grab.transform.SetParent(transform, false);
            attachTransform = grab.transform;
        }

        initialAttachmentPos = attachTransform.localPosition;
        initialAttachmentRot = attachTransform.localRotation;
    }

    protected override void OnSelectEnter(XRBaseInteractor interactor) {
        if (interactor is XRDirectInteractor) {
            attachTransform.position = interactor.transform.position;
            attachTransform.rotation = interactor.transform.rotation;
        } else {
            attachTransform.position = initialAttachmentPos;
            attachTransform.rotation = initialAttachmentRot;
        }
        base.OnSelectEnter(interactor);
    }
}
