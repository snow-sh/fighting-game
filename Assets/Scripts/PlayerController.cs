using System;
using System.Collections;
using System.Collections.Generic;
// using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [Header("Components")]
    [SerializeField] Rigidbody rigidbody3D;
    [SerializeField] ConfigurableJoint mainJoint;
    [SerializeField] Animator animator;

    [Header("Settings")]
    [SerializeField] float maxSpeed = 3f;
    [SerializeField] float acceleration = 30f;
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float rotationSpeed = 300f;

    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;
    bool isGrounded = false;
    RaycastHit[] raycastHits = new RaycastHit[10];

    void Awake()
    {
        Instance = this;
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
        CheckGrounded();
        HandleMovement();
        HandleRotation();
        HandleJump();
        UpdateAnimations();

        // Fall reset
        if (transform.position.y < -10)
        {
            transform.position = Vector3.zero;
            rigidbody3D.linearVelocity = Vector3.zero;
        }
    }

    private void CheckGrounded()
    {
        isGrounded = false;
        int numberOfHits = Physics.SphereCastNonAlloc(
            rigidbody3D.position,
            0.1f,
            -transform.up,
            raycastHits,
            0.5f
        );

        for (int i = 0; i < numberOfHits; i++)
        {
            if (raycastHits[i].transform.root == transform)
                continue;

            isGrounded = true;
            break;
        }

        if (!isGrounded)
            rigidbody3D.AddForce(Vector3.down * 10f);
    }

    private void HandleMovement()
    {
        float inputMagnitude = moveInputVector.magnitude;
        
        // Calculate velocity relative to forward direction
        float localForwardVelocity = Vector3.Dot(transform.forward, rigidbody3D.linearVelocity);

        if (inputMagnitude > 0 && localForwardVelocity < maxSpeed)
        {
            rigidbody3D.AddForce(transform.forward * inputMagnitude * acceleration);
        }
    }

    private void HandleRotation()
    {
        if (moveInputVector.sqrMagnitude > 0.01f)
        {
            Quaternion desiredRotation = Quaternion.LookRotation(
                new Vector3(moveInputVector.x, 0, moveInputVector.y), 
                transform.up
            );

            mainJoint.targetRotation = Quaternion.RotateTowards(
                mainJoint.targetRotation,
                desiredRotation,
                Time.fixedDeltaTime * rotationSpeed
            );
        }
    }

    private void HandleJump()
    {
        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        isJumpButtonPressed = false; // Reset jump flag
    }

    private void UpdateAnimations()
    {
        float localForwardVelocity = Vector3.Dot(transform.forward, rigidbody3D.linearVelocity);
        animator.SetFloat("movementSpeed", Mathf.Abs(localForwardVelocity) * 0.4f);
    }

    // private void SetupCamera()
    // {
    //     CinemachineVirtualCamera vCam = FindFirstObjectByType<CinemachineVirtualCamera>();
    //     if (vCam != null)
    //     {
    //         vCam.Follow = transform;
    //         vCam.LookAt = transform;
    //     }
    // }
}