using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
  public float lifeTime = 1.4f;
  public GameObject contactSmack;

  private Vector3 lastPosition;
  private RaycastHit hit;
  private GameObject enemy;
  private Rigidbody rb;
  //private Enemy enemyComponent;

  // Start is called before the first frame update
  void Start() {
    hit = new RaycastHit();
    rb = GetComponent<Rigidbody>();

    // Manually advance position by one frame because the bullet may have been created during
    // an Update(), causing all kinds of physics havok. We could use the firing time of the
    // weapon to position these more accurately as well.
    // 
    // Randomness is to add visual effect.
    lastPosition = transform.position;
    Debug.Log(Time.fixedDeltaTime);
    transform.position = transform.position + rb.velocity * (Time.deltaTime * (Random.value + 0.5f));
    CheckCollisions();

    Destroy(this.gameObject, lifeTime);
  }

  void Hit(RaycastHit hit) { 
    Debug.Log("Collided with " + hit.collider.name);

    if (hit.rigidbody) {
      enemy = hit.collider.transform.root.gameObject;

      Enemy enemyComponent = enemy.GetComponent<Enemy>();
      if (enemyComponent)
        enemyComponent.Hit(hit, GetComponent<Rigidbody>().velocity);
      else
        hit.rigidbody.AddForceAtPosition(GetComponent<Rigidbody>().velocity, hit.point);
    }
    
    Instantiate(contactSmack, hit.point,
        Quaternion.AngleAxis(Random.Range(0f,360f), hit.normal) * 
        Quaternion.LookRotation(hit.normal));
    
    Destroy(this.gameObject);
  }

  void CheckCollisions() {
    if(Physics.Linecast(lastPosition, transform.position, out hit)) {
      Hit(hit);
    }
  }

  // Update is called once per frame
  void Update() {
    CheckCollisions();

    // Point bullet in the direction of travel
    transform.forward = GetComponent<Rigidbody>().velocity;
    lastPosition = transform.position;
  }
}
