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
        // reusable list of valid targets
        List<XRBaseInteractable> m_ValidTargets = new List<XRBaseInteractable>();
        protected override List<XRBaseInteractable> ValidTargets { get { return m_ValidTargets; } }
        
        protected override void Awake() {
            base.Awake();
            if (!GetComponents<Collider>().Any(x => x.isTrigger))
                Debug.LogWarning("Hand Interactor does not have required Collider set as a trigger.");
            
            onHoverEnter.AddListener(HoverHighlight);
            onHoverExit.AddListener(HoverUnhighlight);
        }

        protected void OnTriggerEnter(Collider col)
        {
            XRBaseInteractable interactable = col.gameObject.GetComponent<XRBaseInteractable>();
            //var interactable = interactionManager.TryGetInteractableForCollider(col);
            if (interactable && !m_ValidTargets.Contains(interactable))
                m_ValidTargets.Add(interactable);
        }

        protected void OnTriggerExit(Collider col)
        {
            XRBaseInteractable interactable = col.gameObject.GetComponent<XRBaseInteractable>();
            //var interactable = interactionManager.TryGetInteractableForCollider(col);
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

        /// <summary>
        /// Retrieve the list of interactables that this interactor could possibly interact with this frame.
        /// Unlike other Interactors, Hand Interactor only allows one interactable to be valid at a time
        /// </summary>
        /// <param name="validTargets">Populated List of interactables that are valid for selection or hover.</param>
        public override void GetValidTargets(List<XRBaseInteractable> validTargets) {
            validTargets.Clear();

            float minDistance = Mathf.Infinity;
            XRBaseInteractable minObject = null;

            foreach(var interactable in m_ValidTargets) {
                if (interactable is Grip) {
                    if (!((Grip)interactable).IsGrabbable()) {
                        continue;
                    }
                }
                float distance = interactable.GetDistanceSqrToInteractor(this);
                if (distance < minDistance) {
                    minDistance = distance;
                    minObject = interactable;
                }
            }
            if (minObject) {
                validTargets.Add(minObject);
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