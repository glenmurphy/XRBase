using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Interaction.Toolkit 
{
    [DisallowMultipleComponent]
    [AddComponentMenu("XR/Magnetic Hand Interactor")]
    public class XRMagneticHandInteractor : XRBaseControllerInteractor
    {
        public bool isLeftHand = false;
        public Transform magnetismDirection;
        private Vector3 magnetismDirectionVector;
        public float magnetismRadius = 1.5f;
        public float magnetismAngle = 45f;

        private SphereCollider localCollider;
        // reusable list of valid targets
        List<XRBaseInteractable> m_ValidTargets = new List<XRBaseInteractable>();
        protected override List<XRBaseInteractable> ValidTargets { get { return m_ValidTargets; } }
        
        // Working variables; used by GetValidTargets
        private Collider[] sphereCastHits;

        protected override void Awake() {
            base.Awake();
            if (!GetComponents<Collider>().Any(x => x.isTrigger))
                Debug.LogWarning("Hand Interactor does not have required Collider set as a trigger.");

            localCollider = GetComponent<SphereCollider>();
            if (!localCollider) {
                localCollider = gameObject.AddComponent<SphereCollider>();
                localCollider.radius = 0.15f;
                localCollider.isTrigger = true;
            }

            onHoverEnter.AddListener(HoverHighlight);
            onHoverExit.AddListener(HoverUnhighlight);
        }

        protected void OnTriggerEnter(Collider col)
        {
            XRBaseInteractable interactable = col.gameObject.GetComponent<XRBaseInteractable>();
            if (interactable && !m_ValidTargets.Contains(interactable))
                m_ValidTargets.Add(interactable);
        }

        protected void OnTriggerExit(Collider col)
        {
            XRBaseInteractable interactable = col.gameObject.GetComponent<XRBaseInteractable>();
            if (interactable && m_ValidTargets.Contains(interactable))
                m_ValidTargets.Remove(interactable);
        }

        void HoverHighlight(XRBaseInteractable interactable) {
            Outline outline = interactable.GetComponent<Outline>();
            if (outline == null) {
                outline = interactable.gameObject.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineColor = Color.yellow;
                outline.OutlineWidth = 5f;
            }
            outline.enabled = true;
        }

        void HoverUnhighlight(XRBaseInteractable interactable) {
            Outline outline = interactable.GetComponent<Outline>();
            if (outline != null) outline.enabled = false;
        }

        void FixedUpdate() {
        }

        Vector3 magnetismVector() {
            if (magnetismDirection == null) {
                return isLeftHand ? transform.right : -transform.right;
            } else {
                return magnetismDirection.forward;
            }
        }

        bool IsMagnetable(GameObject obj) {
            Grabbable grabbable;
            if (obj.TryGetComponent<Grabbable>(out grabbable)) {
                if (grabbable.IsMagneticallyGrabbable()) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retrieve the list of interactables that this interactor could possibly interact with this frame.
        /// Unlike other Interactors, Hand Interactor only allows one interactable to be valid at a time
        /// </summary>
        /// <param name="validTargets">Populated List of interactables that are valid for selection or hover.</param>
        public override void GetValidTargets(List<XRBaseInteractable> outValidTargets) {
            outValidTargets.Clear();

            float minDistance = Mathf.Infinity;
            XRBaseInteractable minObject = null;

            // TODO, need to make this take into account proximity to the main axis so you can more easily select
            // between things
            foreach(var interactable in m_ValidTargets) {
                if (interactable is Grabbable && !((Grabbable)interactable).IsGrabbable()) {
                    continue;
                }
                float distance = interactable.GetDistanceSqrToInteractor(this);
                if (distance < minDistance) {
                    minDistance = distance;
                    minObject = interactable;
                }
            }

            // If we're not touching something with our local sphere, look for a magnetable
            // target
            if (!minObject) {
                float minAngle = magnetismAngle;

                sphereCastHits = Physics.OverlapSphere(transform.position, magnetismRadius);
                
                for (int i = 0; i < sphereCastHits.Length; i++)
                {
                    Vector3 directionToHit = sphereCastHits[i].transform.position - transform.position;
                    float angleToHit = Vector3.Angle(magnetismVector(), directionToHit);

                    if (angleToHit < minAngle && IsMagnetable(sphereCastHits[i].gameObject))
                    {
                        minObject = sphereCastHits[i].GetComponent<XRBaseInteractable>();
                        minAngle = angleToHit;
                    }
                }
            }

            if (minObject) {
                outValidTargets.Add(minObject);
                return;
            }
        }

        /// <summary>Determines if the interactable is valid for hover this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be hovered over this frame.</returns>
        public override bool CanHover(XRBaseInteractable interactable)
        {
            return base.CanHover(interactable) && (selectTarget == null || selectTarget == interactable);
        }

        /// <summary>Determines if the interactable is valid for selection this frame.</summary>
        /// <param name="interactable">Interactable to check.</param>
        /// <returns><c>true</c> if the interactable can be selected this frame.</returns>
        public override bool CanSelect(XRBaseInteractable interactable)
        {
            return base.CanSelect(interactable) && (selectTarget == null || selectTarget == interactable);
        }
    }
}