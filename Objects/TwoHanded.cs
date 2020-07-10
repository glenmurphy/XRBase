using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[DisallowMultipleComponent]
public class TwoHanded : XRGrabInteractable
{
    public Grip primaryGrip;
    public Grip secondaryGrip;
    private Rigidbody rigidBody;

    protected override void Awake() {
        rigidBody = GetComponent<Rigidbody>();

        if (primaryGrip == null)
            Debug.LogWarning("Primary grip not defined");

        if (primaryGrip != null) primaryGrip.Setup(this);
        if (secondaryGrip != null) secondaryGrip.Setup(this);

        WakeBase();
    }

    private void WakeBase() {
        // This is a workaround for how XRBaseInteractable tries to grab all the colliders
        // from the children and make them triggers; this collides with the grips, leading to
        // all sorts of weird behaviors
        List<Collider> colliders = new List<Collider>(GetComponentsInChildren<Collider>());
        foreach(Collider collider in colliders) {
            collider.gameObject.SetActive(false);
        }
        base.Awake();
        foreach(Collider collider in colliders) {
            collider.gameObject.SetActive(true);
        }
    }

    public bool IsGripGrabble(Grip grip) {
        if (grip.IsHeld()) return false;
        if (grip == primaryGrip) return true;

        return primaryGrip.IsHeld();
    }

    public void Grabbed(Grip grip) {
        if (grip == primaryGrip) {
            attachTransform = grip.GetAttachTransform();
            base.OnSelectEnter(grip.GetInteractor());
        }
    }

    public void Dropped(Grip grip) {
        if (grip == primaryGrip) {
            attachTransform = null;
            OnSelectExit(grip.GetInteractor());

            // TODO: If we've been dropped into a pocket, then we need to attach ourselves to it
            // somehow.
        }
    }

    private void SetTwoHandedTransform() {
        Vector3 primaryPosition = primaryGrip.GetInteractor().transform.position;
        Vector3 secondaryPosition = secondaryGrip.GetInteractor().transform.position;

        // TODO: handle offset grips (this code assumes the secondary grip is exactly forward of 
        // the primary grip). Can probably do this by modifying secondaryPosition with the offset,
        // but rotation becomes a weird factor.
        // e.g secondaryPosition -= secondaryGrip.GetInteractor().transform.up * 0.02f + 
        // secondaryGrip.GetInteractor().transform.forward * 0.02f;
        Vector3 target = secondaryPosition - primaryPosition;

        Quaternion lookRotation = Quaternion.LookRotation(target);

        Vector3 gripRotation = Vector3.zero;
        gripRotation.z = primaryGrip.GetInteractor().transform.eulerAngles.z;

        lookRotation *= Quaternion.Euler(gripRotation);

        // This seems better (I'm sure it breaks things though)
        transform.rotation = lookRotation;
        transform.position = primaryPosition + (transform.position - attachTransform.position);
    }

    /*
    private void SetOneHandedTransform() {
        XRBaseInteractor primaryHand = primaryGrip.GetInteractor();

        Vector3 target = primaryHand.transform.forward;
        Quaternion lookRotation = Quaternion.LookRotation(target);

        Vector3 gripRotation = Vector3.zero;
        gripRotation.z = primaryHand.transform.eulerAngles.z;

        lookRotation *= Quaternion.Euler(gripRotation);

        // Modify by the rotation of the attachment point relative to our current transformation
        lookRotation *= Quaternion.Inverse(primaryGrip.attachTransform.rotation) * transform.rotation;
        
        transform.rotation = lookRotation;
        transform.position = primaryHand.transform.position + (transform.position - attachTransform.position);
    }
    */

    public virtual void GripEvent(Grip grip, int data) {}
    public virtual void ButtonPressed(Grip grip, InputHelpers.Button button) {}
    public virtual void ButtonReleased(Grip grip, InputHelpers.Button button) {}

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractable(updatePhase);

        if (primaryGrip.IsHeld()) {
            if (secondaryGrip.IsHeld()) {
                SetTwoHandedTransform();
            }
            /*
            else {
                SetOneHandedTransform();
            }
            */
        }

        // TODO: recoil transform

        // We do this later so we have a chance to catch up after player rotation etc, though 
        // other transforms (recoil) might cause us to revaluate this
        if (secondaryGrip.IsHeld())
            secondaryGrip.CheckDistance();
    }
}
