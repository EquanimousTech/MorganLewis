using UnityEngine;

public class RopeAttachmentPoint : MonoBehaviour
{
    public bool isTopAttachment = true;
    
    public void AttachTo(Vector3 worldPosition)
    {
        if (isTopAttachment)
        {
            transform.position = worldPosition;
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
        }
    }
    
    public void AttachObject(GameObject obj)
    {
        if (!isTopAttachment)
        {
            FixedJoint joint = obj.AddComponent<FixedJoint>();
            joint.connectedBody = GetComponent<Rigidbody>();
        }
    }
}