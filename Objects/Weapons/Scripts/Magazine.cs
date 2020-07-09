using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Magazine : XRGrabInteractable
{
    public Transform receiverAttachmentPoint;
    private Rigidbody rigidBody;
    private Receiver receiver;
    private XRBaseInteractor interactor;

    public int capacity = 30;
    public int bullets = 30;

    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    void Init() {
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
        Init(); // This might get called before we've Started()
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
        return (interactor != null);
    }

    protected override void OnSelectExit(XRBaseInteractor handInteractor) {
        Debug.Log("Magazine OnSelectExit");
        base.OnSelectExit(handInteractor);
        interactor = null;

        if (receiver == null) {
            // Being dropped in the world
            Debug.Log("No receiver");
            EnableRigidBody();
        } else {
            // Being dropped as part of an Attach();
            AffixToReceiver();
        }

    }
}
