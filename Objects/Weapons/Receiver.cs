using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class Receiver : MonoBehaviour
{
    public Transform attachmentPoint;
    public Magazine currentMag;

    // Need to do this because the magazine is a rigidbody, so it has to disable its collision
    // detection when it's attached or otherwise bad physics things happen
    private MeshCollider dummyCollider; 

    private Magazine ejectingMag;
    private Weapon weapon;

    // Start is called before the first frame update
    void Start()
    {
        dummyCollider = GetComponent<MeshCollider>();
        if (attachmentPoint == null)
            attachmentPoint = transform;
        if (currentMag != null) {
            currentMag.Attach(this);
        }
        UpdateDummyCollider();
    }

    void UpdateDummyCollider() {
        if (currentMag) {
            dummyCollider.sharedMesh = currentMag.GetComponent<MeshFilter>().sharedMesh;
        } else {
            dummyCollider.sharedMesh = null;
        }
    }

    public void Setup(Weapon weapon) {
        this.weapon = weapon;
        if (currentMag != null) {
            weapon.ChamberRound();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Magazine GetMagazine() {
        return currentMag;
    }

    public void TryAttachMagazine(Magazine magazine) {
        if (currentMag) return;

        magazine.Attach(this);
        currentMag = magazine;
        UpdateDummyCollider();
        weapon.MagAttached(this);
    }

    public void Release() {
        if (!currentMag) return;

        currentMag.Eject();
        ejectingMag = currentMag;
        currentMag = null;
        UpdateDummyCollider();
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.TryGetComponent(out Magazine magazine)) {
            Debug.Log("Receiver + Magazine");
            if (magazine == ejectingMag) return;
            if (magazine.IsHeld() == false) return;
            TryAttachMagazine(magazine);
        }
    }

    void OnTriggerExit(Collider col) {
        if (col.gameObject.TryGetComponent(out Magazine magazine)) {
            if (magazine == ejectingMag) {
                ejectingMag = null;
            }
        }
    }
}
