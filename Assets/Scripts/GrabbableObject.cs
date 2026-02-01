using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    [Header("Grabbable Settings")]
    [SerializeField] private float grabRadius = 0.3f; // How close to grab
    
    private Rigidbody rb;
    private bool isGrabbed = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Make sure the object has a rigidbody
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
    }
    
    public bool IsGrabbed()
    {
        return isGrabbed;
    }
    
    public void SetGrabbed(bool grabbed)
    {
        isGrabbed = grabbed;
    }
    
    public Rigidbody GetRigidbody()
    {
        return rb;
    }
    
    // Visual helper in editor
    void OnDrawGizmos()
    {
        Gizmos.color = isGrabbed ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}