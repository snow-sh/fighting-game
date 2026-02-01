using UnityEngine;

public class HandGrabber : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform grabPoint;
    
    [Header("Grab Settings")]
    [SerializeField] private float grabRange = 0.8f;
    [SerializeField] private float throwForce = 10f;
    
    private GrabbableObject currentGrabbedObject = null;
    private FixedJoint grabJoint = null;
    private GameObject jointHolder; // Invisible object to hold the joint
    
    public void TryGrab()
    {
        if (currentGrabbedObject != null)
        {
            ReleaseObject();
            return;
        }
        
        // Look for nearby grabbable objects
        Collider[] nearbyObjects = Physics.OverlapSphere(grabPoint.position, grabRange);
        
        GrabbableObject closestObject = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider col in nearbyObjects)
        {
            GrabbableObject grabbable = col.GetComponent<GrabbableObject>();
            
            if (grabbable != null && !grabbable.IsGrabbed())
            {
                float distance = Vector3.Distance(grabPoint.position, col.transform.position);
                
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = grabbable;
                }
            }
        }
        
        if (closestObject != null)
        {
            GrabObject(closestObject);
        }
    }
    
    private void GrabObject(GrabbableObject grabbable)
    {
        currentGrabbedObject = grabbable;
        currentGrabbedObject.SetGrabbed(true);
        
        // Create an invisible holder at the grab point
        jointHolder = new GameObject("JointHolder");
        jointHolder.transform.position = grabPoint.position;
        jointHolder.transform.parent = grabPoint; // Make it child of grab point so it follows hand
        
        // Add rigidbody to the holder
        Rigidbody holderRb = jointHolder.AddComponent<Rigidbody>();
        holderRb.isKinematic = true; // Kinematic = won't be affected by physics
        holderRb.useGravity = false;
        
        // Create joint on the OBJECT, connecting to the holder
        grabJoint = currentGrabbedObject.gameObject.AddComponent<FixedJoint>();
        grabJoint.connectedBody = holderRb;
        grabJoint.breakForce = Mathf.Infinity; // Never break
        grabJoint.breakTorque = Mathf.Infinity;
        
        // Make object lighter when grabbed
        Rigidbody objectRb = currentGrabbedObject.GetRigidbody();
        if (objectRb != null)
        {
            objectRb.mass = 0.3f;
            objectRb.linearDamping = 2f; // Air resistance
            objectRb.angularDamping = 2f;
        }
        
        Debug.Log("Grabbed: " + currentGrabbedObject.name);
    }
    
    public void ReleaseObject()
    {
        if (currentGrabbedObject == null) return;
        
        Debug.Log("Released: " + currentGrabbedObject.name);
        
        Rigidbody objectRb = currentGrabbedObject.GetRigidbody();
        if (objectRb != null)
        {
            // Restore normal physics
            objectRb.mass = 1f;
            objectRb.linearDamping = 0f;
            objectRb.angularDamping = 0.05f;
            
            // Get throw velocity from the grab point (which follows hand)
            objectRb.linearVelocity = (grabPoint.position - objectRb.position) * 5f;
            objectRb.AddForce(grabPoint.forward * throwForce, ForceMode.Impulse);
        }
        
        currentGrabbedObject.SetGrabbed(false);
        currentGrabbedObject = null;
        
        // Destroy joint and holder
        if (grabJoint != null)
        {
            Destroy(grabJoint);
            grabJoint = null;
        }
        
        if (jointHolder != null)
        {
            Destroy(jointHolder);
            jointHolder = null;
        }
    }
    
    void OnDrawGizmos()
    {
        if (grabPoint != null)
        {
            Gizmos.color = currentGrabbedObject != null ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(grabPoint.position, grabRange);
            Gizmos.DrawLine(transform.position, grabPoint.position);
        }
    }
    
    public bool IsGrabbing()
    {
        return currentGrabbedObject != null;
    }
}