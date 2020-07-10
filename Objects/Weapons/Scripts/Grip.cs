using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Grip : XRBaseInteractable, Grabbable
{
    public Transform attachTransform;

    protected XRBaseInteractor interactor = null;
    protected TwoHanded parentObject;

    private bool triggerButtonDown = false;
    private bool primaryButtonDown = false;
    private bool secondaryButtonDown = false;

    protected override void Awake() {
        base.Awake();

        if (attachTransform == null)
            attachTransform = transform;

        if (colliders.Count == 0) {
            Debug.Log("No colliders for this grip");
        }

        //onActivate.AddListener(Activate);
        //onDeactivate.AddListener(Deactivate);
        onSelectEnter.AddListener(Grab);
        onSelectExit.AddListener(Drop);
    }
    
    public void Setup(TwoHanded parentObject) {
        this.parentObject = parentObject;
    }

    public bool IsPocketable() {
        // TODO: Only make this work if the item was dropped recently
        return IsMagneticallyGrabbable();
    }

    public bool IsMagneticallyGrabbable() {
        // Only if we're not attached to something already, and if we're the primary grip for
        // the object we're attached to (so we can't magnetically attach slides/foregrips)
        return (!interactor && parentObject.primaryGrip == this);
    }

    public bool IsGrabbable() {
        // If we're currently being held, we're not grabbable
        if (interactor) {
            if (interactor.GetComponent<XRPocket>())
                return true;
            return false;
        }
        if (parentObject)
            return parentObject.IsGripGrabble(this);

        return true;
    }

    protected virtual void Grab(XRBaseInteractor handInteractor) {
        if (interactor) return;
       
        interactor = handInteractor;
        parentObject.Grabbed(this);
    }

    protected void Drop(XRBaseInteractor handInteractor) {
        interactor = null;
        parentObject.Dropped(this);
    }

    public void CheckDistance() {
        if (interactor && Vector3.Distance(interactor.attachTransform.position, attachTransform.position) > 0.25f) {
            OnSelectExit(interactor);
        }
    }

    public bool IsHeld() {
        return (interactor && interactor.GetComponent<XRMagneticHandInteractor>());
    }

    public XRBaseInteractor GetInteractor() {
        return interactor;
    }

    public Transform GetAttachTransform() {
        return attachTransform;
    }

    public void FixedUpdate() {
        // If trigger is down
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractable(updatePhase);
        
        if (interactor && parentObject && interactor.GetComponent<XRController>()) {
            InputDevice device = interactor.GetComponent<XRController>().inputDevice;
            bool pressed;
            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out pressed)) {
                if (pressed == true && primaryButtonDown == false) {
                    parentObject.ButtonPressed(this, InputHelpers.Button.PrimaryButton);
                    primaryButtonDown = true;
                } else if (pressed == false && primaryButtonDown == true) {
                    parentObject.ButtonReleased(this, InputHelpers.Button.PrimaryButton);
                    primaryButtonDown = false;
                }
            }
            if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed)) {
                if (pressed == true && secondaryButtonDown == false) {
                    parentObject.ButtonPressed(this, InputHelpers.Button.SecondaryButton);
                    secondaryButtonDown = true;
                } else if (pressed == false && secondaryButtonDown == true) {
                    parentObject.ButtonReleased(this, InputHelpers.Button.SecondaryButton);
                    secondaryButtonDown = false;
                }
            }
            if (device.TryGetFeatureValue(CommonUsages.triggerButton, out pressed)) {
                if (pressed == true && triggerButtonDown == false) {
                    parentObject.ButtonPressed(this, InputHelpers.Button.Trigger);
                    triggerButtonDown = true;
                } else if (pressed == false && triggerButtonDown == true) {
                    parentObject.ButtonReleased(this, InputHelpers.Button.Trigger);
                    triggerButtonDown = false;
                }
            }
        }
    }
}
