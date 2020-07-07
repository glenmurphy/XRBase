using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Slide : Grip
{
    public float slideLength = 0.32f;

    private float slideSnapDistance = 0.005f;
    private Vector3 initialLocalPosition;
    private Vector3 initialAttachmentOffset;

    private float forwardRange;
    private float pullRange;

    public enum State {
        Forward,
        Pulling,
        Pulled,
    }
    public State state = State.Forward;

    protected override void Awake() {
        base.Awake();
        initialLocalPosition = transform.localPosition;

        forwardRange = initialLocalPosition.z - slideSnapDistance;
        pullRange = initialLocalPosition.z - slideLength + slideSnapDistance;
    }

    protected override void Grab(XRBaseInteractor handInteractor) {
        Debug.Log("Slide Grabbed");
        base.Grab(handInteractor);
        initialAttachmentOffset = transform.parent.InverseTransformPoint(handInteractor.transform.position) - initialLocalPosition;
        Debug.Log(initialAttachmentOffset.z);
    }

    void Forward() {
        Debug.Log("Forward");
        state = State.Forward;
        parentObject.GripEvent(this, (int)State.Forward);
    }
    
    void Pulled() {
        Debug.Log("Pulled");
        state = State.Pulled;
        parentObject.GripEvent(this, (int)State.Pulled);
    }

    void Pulling() {
        Debug.Log("Pulling");
        state = State.Pulling;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        if (GetInteractor() == null) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, initialLocalPosition, Time.deltaTime * 20f);
        } else {
            Vector3 currentAttachmentOffset = transform.parent.InverseTransformPoint(GetInteractor().transform.position) - initialLocalPosition;
            
            float offset = currentAttachmentOffset.z - initialAttachmentOffset.z;
            if (offset < -slideLength) {
                offset = -slideLength;
            } else if (offset > 0) {
                offset = 0;
            }

            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, initialLocalPosition.z + offset);
        }

        if (state != State.Forward && transform.localPosition.z >= forwardRange) {
            transform.localPosition = initialLocalPosition;
            Forward();
        }
        else if (state != State.Pulled && transform.localPosition.z <= pullRange) {
            Pulled();
        }
        else if (state != State.Pulling && transform.localPosition.z < forwardRange && transform.localPosition.z > pullRange) {
            Pulling();
        }
    }
}
