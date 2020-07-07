using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractionOutliner : MonoBehaviour
{
    private GameObject highlightedObject;
    private List<GameObject> validTargets = new List<GameObject>();

    // Start is called before the first frame update
    public void OnTriggerEnter(Collider col) {
        XRBaseInteractable grip = col.gameObject.GetComponent<XRBaseInteractable>();
        if (grip == null) return;
        validTargets.Add(grip.gameObject);
    }

    public void OnTriggerExit(Collider col) {
        XRBaseInteractable grip = col.gameObject.GetComponent<XRBaseInteractable>();
        if (grip == null) return;
        validTargets.Remove(grip.gameObject);
    }

    public void FixedUpdate() {
        HighlightClosest();
    }

    void HighlightClosest() {
        if (validTargets.Count == 0) {
            Highlight(null);
            return;
        }

        float minDistance = Mathf.Infinity;
        GameObject minObject = null;

        foreach(GameObject obj in validTargets) {
            float distance = Vector3.Distance(obj.transform.position, transform.position);
            if (distance < minDistance) {
                minDistance = distance;
                minObject = obj;
            }
        }

        if (minObject) {
            Highlight(minObject);
            // We rehighlight every frame because a different InteractionOutliner may have
            // dehighlighted our object; we really should map these, but this will do for
            // now
        }
    }

    void Highlight(GameObject obj) {
        if (highlightedObject)
            Unhighlight(highlightedObject);
        
        highlightedObject = obj;
        if (obj == null) return;

        Outline outline = obj.GetComponent<Outline>();
        if (outline == null) {
            outline = obj.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineVisible;
            outline.OutlineColor = Color.yellow;
            outline.OutlineWidth = 5f;
        }
        outline.enabled = true;
        highlightedObject = obj;
    }

    void Unhighlight(GameObject obj) {
        if (obj == null) { return; }
        Outline outline = obj.GetComponent<Outline>();
        if (outline != null) {
            outline.enabled = false;
        }
    }
}
