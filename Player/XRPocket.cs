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
            // TODO: remove this; this is a workaround for how XRBaseInteractor doesn't
            // create an XRInteractionManager if it can't find one; the Reset() method
            // forces the creation of one with no other side-effects.
            Reset(); 

            base.Awake();

            onSelectEnter.AddListener(SelectEnter);
            onSelectExit.AddListener(SelectExit);
            base.allowSelect = true;
        }

        void SelectEnter(XRBaseInteractable interactable) {
            this.interactable = interactable;
        }

        void SelectExit(XRBaseInteractable interactable) {
            this.interactable = null;
        }

        bool IsPocketable(GameObject obj) {
            Grabbable grabbable;
            if (obj.TryGetComponent<Grabbable>(out grabbable)) {
                if (grabbable.IsPocketable()) {
                    return true;
                }
            }
            return false;
        }

        public override void GetValidTargets(List<XRBaseInteractable> validTargets) {
            validTargets.Clear();
            if (interactable) return;

            Collider[] sphereCastHits = Physics.OverlapSphere(transform.position, 0.1f);
            for (int i = 0; i < sphereCastHits.Length; i++) {
                if (IsPocketable(sphereCastHits[i].gameObject)) {
                    validTargets.Add(sphereCastHits[i].GetComponent<XRBaseInteractable>());
                }
            }
        }

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