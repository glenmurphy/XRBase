using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    // Configuration
    [SerializeField]
    [Tooltip("Use left stick for movement, right stick for snap turns")]
    public bool useLeftForMovement = true;

    public float speed = 2f;
    public bool useHand = true;
    public float gravity = -9.81f;

    public GameObject leftHand;
    public GameObject rightHand;
    public GameObject hips;
 
    [SerializeField]
    [Tooltip("Deadzone on controller joysticks for movement")]
    private float movementDeadzone = 0.1f;

    [SerializeField]
    [Tooltip("Determines how much the player is allowed to lean away from their body over obstacles")]
    private float maxLean = 0.6f;

    // Mapped objects
    private List<InputDevice> devices;
    private InputDevice leftController;
    private InputDevice rightController;
    private CharacterController characterController;
    private Camera headsetCamera;
    private Rigidbody body;

    // Working variables
    private Vector3 direction;
    private int lastRotation = 0;

    // Start is called before the first frame update
    void Start()
    {
        headsetCamera = GetComponentInChildren<Camera>();
        devices = new List<InputDevice>();
        characterController = GetComponent<CharacterController>();

        body = GetComponentInChildren<Rigidbody>();

        // Make sure the body has a larger collision radius than the character controller - if it
        // doesn't, then the physics gets messed up
        CapsuleCollider bodyCapsule = body.GetComponent<CapsuleCollider>();
        if (bodyCapsule.radius < characterController.radius + characterController.skinWidth) {
            bodyCapsule.radius = characterController.radius + characterController.skinWidth;
        }

        if (leftHand == null)
            leftHand = transform.Find("Left Hand").gameObject;
        if (rightHand == null)
            rightHand = transform.Find("Right Hand").gameObject;
    
        InitControls();
    }

    void InitControls() {
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0) {
            leftController = devices[0];
        }

        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, devices);
        if (devices.Count > 0) {
            rightController = devices[0];
        }
    }
    
    private void MovePlayer() {
        InputDevice movementController;
        Transform movementTransform;

        if (useLeftForMovement) {
            movementController = leftController;
            movementTransform = leftHand.transform;
        } else {
            movementController = rightController;
            movementTransform = rightHand.transform;
        }

        if (movementController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 vec) && vec != Vector2.zero) {
            if(vec.magnitude < movementDeadzone)
                vec = Vector2.zero;
            else
                vec = vec.normalized * ((vec.magnitude - movementDeadzone) / (1.0f - movementDeadzone));
            
            float magnitude = vec.magnitude;
            direction = movementTransform.TransformDirection(new Vector3(vec.x, 0, vec.y));
            direction = magnitude * Vector3.Normalize(Vector3.ProjectOnPlane(direction, Vector3.up));

            // TODO: if the camera is "behind" the charactercontroller, move the character 
            // controller center towards the player; this is so that if the player is leaning away
            // from the direction of motion, the controller doesn't collide with things first

            characterController.Move(speed * Time.deltaTime * direction);
        }
    }

    private void RotatePlayer() {
        InputDevice rotationController = (useLeftForMovement) ? rightController : leftController;

        // TODO: add timer to prevent lots of rotation;
        if (rotationController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 vec)) {
            if (vec.x > 0.5f && lastRotation != 1) {
                transform.Rotate(new Vector3(0, 45, 0));
                lastRotation = 1;
            } else if (vec.x < -0.5f && lastRotation != -1) {
                transform.Rotate(new Vector3(0, -45, 0));
                lastRotation = -1;
            } else if (Mathf.Abs(vec.x) < 0.5f) {
                lastRotation = 0;
            }
        }
    }

    // This moves the body in the world, and allows the player to lean to a certain extent
    void UpdateBodyPosition() {
        Rigidbody body = GetComponentInChildren<Rigidbody>();

        // Figure out where the camera is
        Vector3 neckPosition = headsetCamera.transform.position - headsetCamera.transform.up * 0.25f;
        Vector3 forwardPosition = headsetCamera.transform.position;// + headsetCamera.transform.forward * 1f;

        // TODO: Height
        //body.transform.localScale = new Vector3(1, headsetCamera.transform.position.y - body.transform.position.y, 1);

        // Move and rotate the main/lower body
        Vector3 newPosition = new Vector3(neckPosition.x, transform.position.y, neckPosition.z);
        
        // Rotate the body so it matches the head position relative to the body
        Vector3 targetPostition = new Vector3(
            forwardPosition.x,
            body.transform.position.y, 
            forwardPosition.z) ;
        body.transform.LookAt(targetPostition);
                                
        body.MovePosition(Vector3.Lerp (body.transform.position, newPosition, Time.deltaTime * 10f));

        // Rotate the upper body towards the head
        Vector3 upperBodyDirection = (neckPosition - hips.transform.position).normalized;
        hips.transform.up = Vector3.Lerp(hips.transform.up, upperBodyDirection, Time.deltaTime * 10f);

        // Update the charactercontroller
        // We do this in local coordinates because character controller center is set in local
        // Need to watch if gravity becomes an issue
        Vector3 headLocal = transform.InverseTransformPoint(neckPosition);
        Vector3 bodyLocal = transform.InverseTransformPoint(body.position);

        // Set the height (TODO: this might not be necessary)
        characterController.height = headsetCamera.transform.position.y - transform.position.y;

        // Figure out how far away the body is from the head - as the body is constrained by physics
        // this will let us know if the player is moving away from game-physics limits (e.g. leaning)
        // over a wall.
        float distance = Vector3.Distance(new Vector3(bodyLocal.x, 0, bodyLocal.z), 
                                          new Vector3(headLocal.x, 0, headLocal.z));

        if (distance > maxLean) {
            // If the player is leaning too far, move the character controller towards the head. As
            // the controller is bound by physics, if this causes the controller to move into a wall,
            // the physics system will push the controller back, bringing the head with it.
            // 
            // Might need to blank out the display and/or prevent jerkiness, but the jerkiness
            // probably helps motion sickness
            float overage = distance - maxLean;
            characterController.center = Vector3.Lerp(
                characterController.center, 
                new Vector3(headLocal.x, characterController.height / 2f, headLocal.z),
                overage / distance);
        } else {
            characterController.center = new Vector3(bodyLocal.x, characterController.height / 2f, bodyLocal.z); 
        }
    }

    // Update is called once per frame
    void Update() {
        // Gravity
        characterController.Move(new Vector3(0, gravity, 0) * Time.deltaTime);

        // if (leftController == null || rightController == null)
        // TODO: figure out why sometimes the left controller doesn't map properly
        InitControls();

        RotatePlayer();
        MovePlayer();
    }

    void FixedUpdate() {
        // Here because physics
        UpdateBodyPosition();
    }
}
