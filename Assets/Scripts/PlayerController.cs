using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Rigidbody rigidbody3D;
    [SerializeField] ConfigurableJoint mainJoint;

    [Header("Movement Settings")]
    [SerializeField] float maxSpeed = 3;
    [SerializeField] float acceleration = 30f;
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float rotationSpeed = 300f;

    [Header("Drunk Effects")]
    [SerializeField] float drunkWobbleStrength = 10f;
    [SerializeField] float wobbleFrequency = 2f;
    [SerializeField] float drunkTurnInaccuracy = 0.15f;
    [SerializeField] float stumbleChance = 0.05f; // 1% chance per fixed update
    [SerializeField] float stumbleForce = 5f;
    [SerializeField] float speedVariation = 0.2f; // ±20% speed variation

    [Header("Grabbing")]
[SerializeField] HandGrabber leftHandGrabber;
[SerializeField] HandGrabber rightHandGrabber;
    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isGrounded = false;
    RaycastHit[] raycastHits = new RaycastHit[10];

    SyncPhysicsObject[] syncPhysicsObjects;
    
    // Drunk effect variables
    private float wobbleTimer = 0f;
    private float currentSpeedMultiplier = 1f;
    private float speedVariationTimer = 0f;

    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }

    void Update()
    {
        // Gather Input
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
            isJumpButtonPressed = true;

            // Grab with left hand (Left Mouse Button or Q)
if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0))
{
    if (leftHandGrabber != null)
        leftHandGrabber.TryGrab();
}

// Grab with right hand (Right Mouse Button or E)
if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1))
{
    if (rightHandGrabber != null)
        rightHandGrabber.TryGrab();
}
    }

    void FixedUpdate()
    {
        // Check if grounded
        isGrounded = false;
        int numberOfHits = Physics.SphereCastNonAlloc(rigidbody3D.position, 0.1f, transform.up * -1, raycastHits, 0.5f);
        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform)
                continue;

            isGrounded = true;
            break;
        }

        // Extra gravity when not grounded
        if (!isGrounded)
            rigidbody3D.AddForce(Vector3.down * 10f);

        // Calculate input magnitude
        float inputMagnitude = moveInputVector.magnitude;

        // === DRUNK EFFECTS ===
        
        // 1. Speed variation over time (drunk people don't walk consistently)
        speedVariationTimer += Time.fixedDeltaTime;
        currentSpeedMultiplier = 1f + Mathf.Sin(speedVariationTimer * 0.5f) * speedVariation;

        // 2. Wobble effect when moving
        if (inputMagnitude > 0.1f)
        {
            wobbleTimer += Time.fixedDeltaTime * wobbleFrequency;
            float wobbleX = Mathf.Sin(wobbleTimer) * drunkWobbleStrength;
            float wobbleZ = Mathf.Cos(wobbleTimer * 0.7f) * drunkWobbleStrength;
            
            rigidbody3D.AddForce(new Vector3(wobbleX, 0, wobbleZ), ForceMode.Force);
        }

        // 3. Random stumbles
        if (UnityEngine.Random.value < stumbleChance && isGrounded)
        {
            Vector3 randomStumbleDirection = new Vector3(
                UnityEngine.Random.Range(-1f, 1f),
                0,
                UnityEngine.Random.Range(-1f, 1f)
            ).normalized;
            
            rigidbody3D.AddForce(randomStumbleDirection * stumbleForce, ForceMode.Impulse);
        }

        // === MOVEMENT & ROTATION ===
        
        if (inputMagnitude != 0)
        {
            // Add drunk turning inaccuracy
            Vector2 inaccurateInput = moveInputVector;
            inaccurateInput.x += UnityEngine.Random.Range(-drunkTurnInaccuracy, drunkTurnInaccuracy);
            inaccurateInput.y += UnityEngine.Random.Range(-drunkTurnInaccuracy, drunkTurnInaccuracy);
            
            Quaternion desiredRotation = Quaternion.LookRotation(
                new Vector3(inaccurateInput.x, 0, inaccurateInput.y * -1), 
                transform.up
            );

            mainJoint.targetRotation = Quaternion.RotateTowards(
                mainJoint.targetRotation, 
                desiredRotation, 
                Time.fixedDeltaTime * rotationSpeed
            );

            // Calculate forward velocity
            Vector3 localVelocityVsForward = transform.forward * Vector3.Dot(transform.forward, rigidbody3D.linearVelocity);
            float localForwardVelocity = localVelocityVsForward.magnitude;

            // Apply movement force with drunk speed variation
            if (localForwardVelocity < maxSpeed * currentSpeedMultiplier)
            {
                rigidbody3D.AddForce(transform.forward * inputMagnitude * acceleration);
            }
        }

        // === JUMP ===
        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumpButtonPressed = false;
        }

        // === SYNC PHYSICS (not used in pure physics mode, but keeping for compatibility) ===
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }

        // Fall reset
        if (transform.position.y < -10)
        {
            transform.position = Vector3.zero;
            rigidbody3D.linearVelocity = Vector3.zero;
        }
    }
}