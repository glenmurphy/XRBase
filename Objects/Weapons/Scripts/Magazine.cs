using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Magazine : XRGrabInteractable, Grabbable
{
    public Transform receiverAttachmentPoint;
    private Rigidbody rigidBody;
    private Receiver receiver;
    private XRBaseInteractor interactor;

    public int capacity = 30;
    public int bullets = 30;

    protected override void Awake()
    {
        base.Awake();
        
        rigidBody = GetComponent<Rigidbody>();
        if (receiverAttachmentPoint == null)
            receiverAttachmentPoint = transform;
    }

    public void Attach(Receiver r) {
        Debug.Log("Attached");
        receiver = r;

        if (interactor) {
            OnSelectExit(interactor);
        } else {
            AffixToReceiver();
        }
    }

    public bool IsPocketable() {
        // TODO: only make this work for recently dropped items
        return !IsHeld();
    }

    public bool IsMagneticallyGrabbable() {
        // TODO: Only make this work if the item was dropped recently
        return true;
    }

    public bool IsGrabbable() {
        return true;
    }

    void DisableRigidbody() {
        rigidBody.isKinematic = true;
        rigidBody.useGravity = false;
        rigidBody.detectCollisions = false;
    }

    void EnableRigidBody() {
        rigidBody.isKinematic = false;
        rigidBody.useGravity = true;
        rigidBody.detectCollisions = true;
    }

    void AffixToReceiver() {
        DisableRigidbody();

        transform.SetParent(receiver.attachmentPoint);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localPosition -= receiverAttachmentPoint.localPosition;
    }

    public void Eject() {
        transform.SetParent(null);
        receiver = null;
        
        EnableRigidBody();
    }

    protected override void OnSelectEnter(XRBaseInteractor handInteractor) {
        base.OnSelectEnter(handInteractor);
        Debug.Log("Grabbed");
        interactor = handInteractor;
    }

    public bool IsHeld() {
        return (interactor && interactor.GetComponent<XRMagneticHandInteractor>());
    }

    protected override void OnSelectExit(XRBaseInteractor handInteractor) {
        Debug.Log("Magazine OnSelectExit");
        base.OnSelectExit(handInteractor);
        interactor = null;

        if (receiver == null) {
            // Being dropped in the world
            EnableRigidBody();
        } else {
            // Being dropped as part of an Attach();
            AffixToReceiver();
        }

    }
}
