using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

// using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // public static PlayerController Instance { get; private set; }

    [Header("Components")]
    [SerializeField] Rigidbody rigidbody3D;
    [SerializeField] ConfigurableJoint mainJoint;
    // [SerializeField] Animator animator;

    [Header("Settings")]
    [SerializeField] float maxSpeed = 3;
    [SerializeField] float acceleration = 30f;
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float rotationSpeed = 300f;

    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isGrounded = false;
    RaycastHit[] raycastHits = new RaycastHit[10];

    SyncPhysicsObject[] syncPhysicsObjects;
    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
        // Instance = this;
    }

    void Start()
    {
        // SetupCamera();
    }

    void Update()
    {
        // Gather Input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            isJumpButtonPressed = true;
    }

    void FixedUpdate()
    {
        // CheckGrounded();
        // HandleMovement();
        // HandleRotation();
        // HandleJump();
        // UpdateAnimations();

        // // Fall reset
        // if (transform.position.y < -10)
        // {
        //     transform.position = Vector3.zero;
        //     rigidbody3D.linearVelocity = Vector3.zero;
        // }

        isGrounded = false;
        int numberOfHits = Physics.SphereCastNonAlloc(rigidbody3D.position, 0.1f, transform.up * -1, raycastHits, 0.5f);
        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform)
                continue;

            isGrounded = true;
            break;
        };

        if (!isGrounded)
            rigidbody3D.AddForce(Vector3.down * 10f);

        float inputMagnitude = moveInputVector.magnitude;
        
        if (inputMagnitude != 0)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(new Vector3(moveInputVector.x, 0, moveInputVector.y * -1), transform.up);

            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredRotation, Time.fixedDeltaTime * 300);

            Vector3 localVelocifyVsForward = transform.forward * Vector3.Dot(transform.forward, rigidbody3D.linearVelocity);

            float localForwardVelocity = localVelocifyVsForward.magnitude;

            if (localForwardVelocity < maxSpeed)
            {
                rigidbody3D.AddForce(transform.forward * inputMagnitude * acceleration);
            }
        }

        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(Vector3.up * 20, ForceMode.Impulse);
            isJumpButtonPressed = false;
        }

        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }

    }
};