using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit 
{
    [SelectionBase]
    public class XRPocket : XRBaseInteractor
    {
        XRBaseInteractable interactable;

        protected override void Awake() {
            base.Awake();
            onSelectEnter.AddListener(SelectEnter);
            onSelectExit.AddListener(SelectExit);
            base.allowSelect = true;
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void SelectEnter(XRBaseInteractable interactable) {
            this.interactable = interactable;
        }

        void SelectExit(XRBaseInteractable interactable) {
            this.interactable = null;
        }

        bool IsPocketable(GameObject obj) {
            Grip grip;
            if (obj.TryGetComponent<Grip>(out grip)) {
                if (grip.IsMagneticallyGrabbable()) {
                    return true;
                }
            }
            Magazine mag;
            if (obj.TryGetComponent<Magazine>(out mag)) {
                return !mag.IsHeld();
            }
            return false;
        }

        public override void GetValidTargets(List<XRBaseInteractable> validTargets) {
            validTargets.Clear();
            if (interactable) return;

            Collider[] sphereCastHits = Physics.OverlapSphere(transform.position, 0.1f);
            for (int i = 0; i < sphereCastHits.Length; i++) {
                XRBaseInteractable interactable;
                if (IsPocketable(sphereCastHits[i].gameObject)) {
                    validTargets.Add(sphereCastHits[i].GetComponent<XRBaseInteractable>());
                }
            }
        }

        /*
        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);
        }
        */

        public override bool isSelectActive {
            get { 
                if (interactable == null)
                    return true;
                else
                    return true;
            }
        }
    }
}