using UnityEngine;

public interface Hittable {
    void Hit(RaycastHit hit, Vector3 velocity);
}