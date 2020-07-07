using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class Weapon : TwoHanded
{
    public Slide slide;
    public Receiver receiver;
    public GameObject bulletPrefab;
    public Transform bulletExit;
    public MuzzleFlash muzzleFlash;

    public List<AudioClip> fireSoundList;
    public AudioClip slidePulledSound;
    public AudioClip slideForwardSound;
    public AudioClip magReleaseSound;
    public AudioClip magAttachedSound;
    public AudioClip modeSwitchSound;

    public float accuracy = 1f; // degrees of variance
    public float reloadTime = 0.05f;
    public float bulletSpeed = 200f;
    public bool automatic = true;
    public bool toggleable = false;

    private bool roundChambered = false;
    private float firedTime = 0;
    private bool triggerDown = false;
    private float triggerDownTime = 0;
    private AudioSource audioSource;

    protected override void Awake() {
        audioSource = GetComponent<AudioSource>();

        if (muzzleFlash == null)
            muzzleFlash = GetComponentInChildren<MuzzleFlash>();
        
        if (receiver == null) receiver = GetComponentInChildren<Receiver>();
        receiver.Setup(this);

        if (slide == null) slide = GetComponentInChildren<Slide>();
        slide.Setup(this);

        base.Awake();
    }

    public void MagAttached(Receiver receiver) {
        PlaySound(magAttachedSound);
    }

    private void PlaySound(AudioClip sound) {
        if (sound) audioSource.PlayOneShot(sound);
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

        Bullet bullet = Instantiate(bulletPrefab, bulletExit.position, bulletExit.rotation).GetComponent<Bullet>();
        Rigidbody bulletrb = bullet.GetComponent<Rigidbody>();
        
        bulletrb.velocity = bulletExit.forward * bulletSpeed;
        if (accuracy > 0) {
            Vector2 error = Random.insideUnitCircle * accuracy;
            Quaternion errorRotation = Quaternion.Euler(error.x, error.y, 0);
            bulletrb.velocity = errorRotation * bulletExit.forward * bulletSpeed;
        } else {
            bulletrb.velocity = bulletExit.forward * bulletSpeed;
        }
        bullet.Init(Time.time - time + Time.deltaTime); // Also advance it by one frame to hide lag

        muzzleFlash.Flash();
        firedTime = time;
        roundChambered = false;
        ChamberRound();

        return true;
    }

    public override void GripEvent(Grip grip, int data) {
        if (grip == slide) {
            if ((Slide.State)data == Slide.State.Forward) {
                PlaySound(slideForwardSound);
            }
            if ((Slide.State)data == Slide.State.Pulled) {
                ChamberRound();
                PlaySound(slidePulledSound);
            }
        }
    }

    public override void ButtonPressed(Grip grip, InputHelpers.Button button) {
        if (grip == primaryGrip) {
            if (button == InputHelpers.Button.PrimaryButton && receiver != null) {
                receiver.Release();
                PlaySound(magReleaseSound);
            }
            if (button == InputHelpers.Button.SecondaryButton && toggleable) {
                automatic = !automatic;
                PlaySound(modeSwitchSound);
            }
            if (button == InputHelpers.Button.Trigger) {
                triggerDown = true;
                triggerDownTime = Time.time;
            }
        }
    }

    public override void ButtonReleased(Grip grip, InputHelpers.Button button) {
        if (grip == primaryGrip && button == InputHelpers.Button.Trigger) {
            triggerDown = false;
        }
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
            float lastActionTime = Mathf.Max(triggerDownTime, firedTime + reloadTime);
            
            while (lastActionTime <= Time.time || Mathf.Approximately(lastActionTime, Time.time)) {
                TryFire(lastActionTime);
                lastActionTime += reloadTime;
            }
        }
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase) {
        base.ProcessInteractable(updatePhase);

        // We do this here (after TwoHanded has updated the object) so we can take into account 
        // our transforms - if we do it elsewhere, weirdness may happen. It also lets us generate
        // bullets at framerate, which is usually the fastest
        ProcessFiring();
    }
}
