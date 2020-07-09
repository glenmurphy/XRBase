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
        // Figure out where the camera is
        Vector3 cameraCenter = transform.InverseTransformPoint(headsetCamera.transform.position);

        // TODO: Guess where the body could be based on rotation+height of the head - this
        // might allow for better sideways cover peeking

        // Move the main body
        Rigidbody body = GetComponentInChildren<Rigidbody>();
        Vector3 newPosition = new Vector3(headsetCamera.transform.position.x, transform.position.y, headsetCamera.transform.position.z);
        body.rotation = (Quaternion.identity);
        body.MovePosition(Vector3.Lerp (body.transform.position, newPosition, Time.deltaTime * 10f));

        // Update the charactercontroller
        Vector3 headCenter = transform.InverseTransformPoint(headsetCamera.transform.position);
        Vector3 bodyCenter = transform.InverseTransformPoint(body.position);

        // Set the height (TODO: this might not be necessary)
        characterController.height = headsetCamera.transform.position.y - transform.position.y;

        // Figure out how far away the body is from the head - as the body is constrained by physics
        // this will let us know if the player is moving away from game-physics limits (e.g. leaning)
        // over a wall.
        float distance = Vector3.Distance(new Vector3(bodyCenter.x, 0, bodyCenter.z), 
                                          new Vector3(headCenter.x, 0, headCenter.z));

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
                new Vector3(headCenter.x, characterController.height / 2f, headCenter.z),
                overage / distance);
        } else {
            characterController.center = new Vector3(bodyCenter.x, characterController.height / 2f, bodyCenter.z); 
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
