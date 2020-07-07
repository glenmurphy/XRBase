using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Player : MonoBehaviour
{
    // Configuration
    public bool useLeftForMovement = true;
    public float speed = 2f;
    public bool useHand = true;
    public float gravity = -9.81f;
    public GameObject leftHand;
    public GameObject rightHand;
    private float movementDeadzone = 0.1f;

    // Mapped objects
    private List<InputDevice> devices;
    private InputDevice leftController;
    private InputDevice rightController;
    private CharacterController characterController;

    // Working variables
    private Vector3 direction;
    private int lastRotation = 0;

    // Start is called before the first frame update
    void Start()
    {
        devices = new List<InputDevice>();
        characterController = GetComponent<CharacterController>();

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

    // Update is called once per frame
    void Update()
    {
        // Gravity
        characterController.Move(new Vector3(0, gravity, 0) * Time.deltaTime);

        //if (leftController == null || rightController == null)
        InitControls();

        RotatePlayer();
        MovePlayer();
    }
}
