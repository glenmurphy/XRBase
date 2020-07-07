using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Grip : XRBaseInteractable
{
    public Transform attachTransform;

    private bool Held = false;
    protected XRBaseInteractor interactor;
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

    protected virtual void Activate(XRBaseInteractor handInteractor) {
        weapon.GripActivated(this);
    }

    protected virtual void Deactivate(XRBaseInteractor handInteractor) {
        weapon.GripDeactivated(this);
    }

    protected virtual void Grab(XRBaseInteractor handInteractor) {
        if (Held) return;
       
        interactor = handInteractor;
        Held = true;
        
        weapon.Grabbed(this);
    }

    protected void Drop(XRBaseInteractor handInteractor) {
        interactor = null;
        Held = false;
        weapon.Dropped(this);
    }

    public void CheckDistance() {
        if (Held && Vector3.Distance(interactor.attachTransform.position, attachTransform.position) > 0.25f) {
            OnSelectExit(interactor);
        }
    }

    public bool IsHeld() {
        return Held;
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
        
        if (Held && interactor && weapon) {
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
