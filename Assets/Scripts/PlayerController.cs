using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] Rigidbody rigidbody3D;
    [SerializeField] ConfigurableJoint mainJoint;
    [SerializeField] Transform cameraTransform; 

    [Header("Movement Settings")]
    [SerializeField] float maxSpeed = 5;
    [SerializeField] float acceleration = 50f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float rotationSpeed = 600f;

    [Header("Drunk Effects")]
    [SerializeField] float drunkWobbleStrength = 8f;
    [SerializeField] float wobbleFrequency = 1.5f;
    [SerializeField] float drunkTurnInaccuracy = 0.1f;
    [SerializeField] float stumbleChance = 0.002f; 
    [SerializeField] float stumbleForce = 8f;

    [Header("Grabbing")]
    [SerializeField] HandGrabber leftHandGrabber;
    [SerializeField] HandGrabber rightHandGrabber;

    private Vector2 moveInputVector;
    private bool isJumpButtonPressed;
    private bool isGrounded;
    private RaycastHit[] raycastHits = new RaycastHit[5];
    private SyncPhysicsObject[] syncPhysicsObjects;
    
    private float wobbleTimer;
    private Quaternion targetRotation = Quaternion.identity;

    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
        if (cameraTransform == null && Camera.main != null) 
            cameraTransform = Camera.main.transform;
        
        targetRotation = transform.rotation;
    }

    void Update()
    {
        moveInputVector.x = Input.GetAxisRaw("Horizontal");
        moveInputVector.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space)) isJumpButtonPressed = true;

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetMouseButtonDown(0))
            leftHandGrabber?.TryGrab();

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(1))
            rightHandGrabber?.TryGrab();
    }

    void FixedUpdate()
    {
        CheckGrounded();
        
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0; 
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = (camForward * moveInputVector.y + camRight * moveInputVector.x).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            wobbleTimer += Time.fixedDeltaTime * wobbleFrequency;
            float drift = Mathf.Sin(wobbleTimer) * drunkTurnInaccuracy;
            Vector3 driftedDir = Quaternion.Euler(0, drift * 50f, 0) * moveDir;

            targetRotation = Quaternion.LookRotation(driftedDir);

            // Move
            if (rigidbody3D.linearVelocity.magnitude < maxSpeed)
            {
                rigidbody3D.AddForce(driftedDir * acceleration, ForceMode.Acceleration);
            }

            float wobbleX = Mathf.Sin(wobbleTimer) * drunkWobbleStrength;
            rigidbody3D.AddForce(cameraTransform.right * wobbleX, ForceMode.Force);
        }

       
        mainJoint.targetRotation = Quaternion.Inverse(targetRotation);

        if (isGrounded && Random.value < stumbleChance)
        {
            rigidbody3D.AddForce(Random.insideUnitSphere * stumbleForce, ForceMode.Impulse);
        }

        HandleJump();
        
        if (transform.position.y < -10) transform.position = Vector3.up * 2;
    }

    void CheckGrounded()
    {
        isGrounded = Physics.SphereCastNonAlloc(rigidbody3D.position, 0.2f, Vector3.down, raycastHits, 0.6f) > 1;
        if (!isGrounded) rigidbody3D.AddForce(Vector3.down * 15f, ForceMode.Acceleration);
    }

    void HandleJump()
    {
        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumpButtonPressed = false;
        }
    }
}