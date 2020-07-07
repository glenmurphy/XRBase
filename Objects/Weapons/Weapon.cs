using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class Weapon : XRGrabInteractable
{
    public Grip primaryGrip;
    public Grip secondaryGrip;
    public Slide slide;
    public Receiver receiver;
    public GameObject bullet;
    public Transform bulletExit;
    public MuzzleFlash muzzleFlash;

    public List<AudioClip> fireSoundList;
    public AudioClip slidePulledSound;
    public AudioClip slideForwardSound;
    public AudioClip magReleaseSound;
    public AudioClip magAttachedSound;
    public AudioClip modeSwitchSound;

    public float accuracy; // degrees of variance
    public float reloadTime = 0.05f;
    public float bulletSpeed = 200f;
    public bool automatic = true;
    public bool toggleable = false;

    private bool roundChambered = false;
    private float firedTime = 0;
    private bool triggerDown = false;
    private float triggerDownTime = 0;
    private Rigidbody rigidBody;
    private AudioSource audioSource;

    protected override void Awake() {
        rigidBody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        if (muzzleFlash == null)
            muzzleFlash = GetComponentInChildren<MuzzleFlash>();

        if (primaryGrip == null)
            Debug.LogWarning("Primary grip not defined");
        
        if (receiver == null) receiver = GetComponentInChildren<Receiver>();
        receiver.Setup(this);

        if (slide == null) slide = GetComponentInChildren<Slide>();
        slide.Setup(this);

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

    // We stop this from happening because we don't want people grabbing it by the regular
    // colliders
    protected override void OnSelectEnter(XRBaseInteractor interactor) { }

    int bulletsFired = 0;
    public void GripActivated(Grip grip) {
        Debug.Log("Grip Activated");
        
        if (grip == primaryGrip) {
            bulletsFired = 0;
            triggerDown = true;
            triggerDownTime = Time.time;
        }
    }

    public void GripDeactivated(Grip grip) {
        Debug.Log("Grip Deactivated");
        if (grip == primaryGrip) {
            Debug.Log(bulletsFired + " in " + (Time.time - triggerDownTime) + " seconds");
            triggerDown = false;
            triggerDownTime = Time.time;
        }
    }

    public void Grabbed(Grip grip) {
        if (grip == primaryGrip) {
            attachTransform = grip.GetAttachTransform();
            base.OnSelectEnter(grip.GetInteractor());
        }
        if (grip == slide) {

        }
    }

    public void Dropped(Grip grip) {
        if (grip == primaryGrip) {
            attachTransform = null;
            OnSelectExit(grip.GetInteractor());
        }
    }

    public void SlideForward() {
        PlaySound(slideForwardSound);
    }

    public void SlidePulled() {
        ChamberRound();
        PlaySound(slidePulledSound);
    }

    public void MagAttached(Receiver receiver) {
        PlaySound(magAttachedSound);
    }

    private void PlaySound(AudioClip sound) {
        if (sound) audioSource.PlayOneShot(sound);
    }

    private void SetTwoHandedTransform() {

        Vector3 primaryPosition = primaryGrip.GetInteractor().transform.position;

        Vector3 secondaryPosition = secondaryGrip.GetInteractor().transform.position;

        // TODO: handle offset grips (this code assumes the secondary grip is exactly forward of 
        // the primary grip). Can probably do this by modifying secondaryPosition with the offset,
        // but rotation becomes a weird factor.
        // e.g secondaryPosition -= secondaryGrip.GetInteractor().transform.up * 0.02f + secondaryGrip.GetInteractor().transform.forward * 0.02f;

        Vector3 target = secondaryPosition - primaryPosition;

        Quaternion lookRotation = Quaternion.LookRotation(target);

        Vector3 gripRotation = Vector3.zero;
        gripRotation.z = primaryGrip.GetInteractor().transform.eulerAngles.z;

        lookRotation *= Quaternion.Euler(gripRotation);

        // This seems better (I'm sure it breaks things though)
        transform.rotation = lookRotation;
        transform.position = primaryPosition + (transform.position - attachTransform.position);
    }

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

    private void SetSlideTransform() {
        if (slide.IsHeld()) {
            XRBaseInteractor hand = slide.GetInteractor();
        }
    }

    public void ButtonPressed(Grip grip, InputHelpers.Button button) {
        if (grip == primaryGrip && button == InputHelpers.Button.PrimaryButton && receiver != null) {
            receiver.Release();
            PlaySound(magReleaseSound);
        }
        if (grip == primaryGrip && button == InputHelpers.Button.SecondaryButton && toggleable) {
            automatic = !automatic;
            PlaySound(modeSwitchSound);
        }
    }

    public void ButtonReleased(Grip grip, InputHelpers.Button button) {
        //
    }

    public bool ChamberRound() {
        Magazine magazine = receiver.GetMagazine();
        if (roundChambered) {
            // discard the round (e.g. animation)
        }

        if (magazine && magazine.bullets > 0) {
            magazine.bullets--;
            roundChambered = true;
            return true;
        }
        return false;
    }

    // We feed this a time because we need to be able to fire bullets previously in time, for 
    // when we're catching up to the current frame
    public bool TryFire(float time) {
        if (!roundChambered) {
            return false;
        }

        float nextAvailable = firedTime + reloadTime;
        if (nextAvailable > time && !Mathf.Approximately(nextAvailable, time)) {
            return false;
        }

        // Fire!
        if (fireSoundList.Count > 0) {
            PlaySound(fireSoundList[(int)Random.Range(0, fireSoundList.Count)]);
        }

        Rigidbody bulletrb = Instantiate(bullet, bulletExit.position, bulletExit.rotation).GetComponent<Rigidbody>();
        bulletrb.velocity = bulletExit.forward * bulletSpeed;
        if (accuracy > 0) {
            Vector2 error = Random.insideUnitCircle * accuracy;
            Quaternion errorRotation = Quaternion.Euler(error.x, error.y, 0);
            bulletrb.velocity = errorRotation * bulletExit.forward * bulletSpeed;
        } else {
            bulletrb.velocity = bulletExit.forward * bulletSpeed;
        }
        muzzleFlash.Flash();
        bulletsFired++;
        firedTime = time;
        roundChambered = false;
        ChamberRound();

        return true;
    }

    public void ProcessFiring() {
        // We have to use timestep-like mechanics to ensure the same number of bullets come out
        // regardless of the update rate. There's some trickiness in how we track the time so
        // that you can't get a faster firerate by re-pulling the trigger

        if (triggerDown) {
            if (!automatic) {
                if (triggerDownTime > firedTime) {
                    TryFire(triggerDownTime);
                    return;
                }
                return;
            }
            float lastAction = Mathf.Max(triggerDownTime, firedTime + reloadTime);
            
            while (lastAction <= Time.time || Mathf.Approximately(lastAction, Time.time)) {
                TryFire(lastAction);
                lastAction += reloadTime;
            }
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractable(updatePhase);

        if (primaryGrip.IsHeld()) {
            if (secondaryGrip.IsHeld())
                SetTwoHandedTransform();
            else
                SetOneHandedTransform();
            
            if (slide != null) {
                SetSlideTransform();
            }

            // We do this here so we can take into account our transforms - if we do it elsewhere,
            // weirdness may happen. It also lets us generate bullets at framerate, which is
            // usually the fastest
            ProcessFiring();
        }

        // We do this later so we have a chance to catch up after player rotation etc, though 
        // other transforms (recoil) might cause us to revaluate this
        if (secondaryGrip.IsHeld())
            secondaryGrip.CheckDistance();
    }

}
