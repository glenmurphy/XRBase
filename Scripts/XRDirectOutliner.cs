using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRDirectOutliner : MonoBehaviour
{
        protected void OnTriggerEnter(Collider col)
        {
            Debug.Log("Trigger Enter");
            /*
        	var interactable = interactionManager.TryGetInteractableForCollider(col);
            if (interactable && !m_ValidTargets.Contains(interactable))
                m_ValidTargets.Add(interactable);
                */
        }

        protected void OnTriggerExit(Collider col)
        {   
            Debug.Log("Trigger Exit");
            /*
            var interactable = interactionManager.TryGetInteractableForCollider(col);
            if (interactable && m_ValidTargets.Contains(interactable))
                m_ValidTargets.Remove(interactable);*/
        }
}
