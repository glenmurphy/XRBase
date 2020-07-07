using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Grip : XRBaseInteractable
{
    public Transform attachTransform;

    protected XRBaseInteractor interactor = null;
    protected Weapon weapon;

    private bool primaryButtonDown = false;
    private bool secondaryButtonDown = false;

    protected override void Awake() {
        base.Awake();

        if (attachTransform == null)
            attachTransform = transform;

        if (colliders.Count == 0) {
            Debug.Log("No colliders for this grip");
        }

        onActivate.AddListener(Activate);
        onDeactivate.AddListener(Deactivate);
        onSelectEnter.AddListener(Grab);
        onSelectExit.AddListener(Drop);
    }
    
    public void Setup(Weapon weapon) {
        this.weapon = weapon;
    }

    public bool IsGrabbable() {
        // If we're currently being held, we're not grabbable
        if (interactor) return false;
        if (weapon)
            return weapon.IsGripGrabble(this);

        return true;
    }

    protected virtual void Activate(XRBaseInteractor handInteractor) {
        weapon.GripActivated(this);
    }

    protected virtual void Deactivate(XRBaseInteractor handInteractor) {
        weapon.GripDeactivated(this);
    }

    protected virtual void Grab(XRBaseInteractor handInteractor) {
        if (interactor) return;
       
        interactor = handInteractor;
        weapon.Grabbed(this);
    }

    protected void Drop(XRBaseInteractor handInteractor) {
        interactor = null;
        weapon.Dropped(this);
    }

    public void CheckDistance() {
        if (interactor && Vector3.Distance(interactor.attachTransform.position, attachTransform.position) > 0.25f) {
            OnSelectExit(interactor);
        }
    }

    public bool IsHeld() {
        return (interactor != null);
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
        
        if (interactor && weapon) {
            InputDevice device = interactor.GetComponent<XRController>().inputDevice;
            bool pressed;
            if (device.TryGetFeatureValue(CommonUsages.primaryButton, out pressed)) {
                if (pressed == true && primaryButtonDown == false) {
                    weapon.ButtonPressed(this, InputHelpers.Button.PrimaryButton);
                    primaryButtonDown = true;
                } else if (pressed == false && primaryButtonDown == true) {
                    weapon.ButtonReleased(this, InputHelpers.Button.PrimaryButton);
                    primaryButtonDown = false;
                }
            }
            if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out pressed)) {
                if (pressed == true && secondaryButtonDown == false) {
                    weapon.ButtonPressed(this, InputHelpers.Button.SecondaryButton);
                    secondaryButtonDown = true;
                } else if (pressed == false && secondaryButtonDown == true) {
                    weapon.ButtonReleased(this, InputHelpers.Button.SecondaryButton);
                    secondaryButtonDown = false;
                }
            }
        }
    }
}
