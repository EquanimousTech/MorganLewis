using UnityEngine;

public class RopeController : MonoBehaviour
{
    public GameObject[] segments;
    public Rigidbody[] segmentRigidbodies;
    
    [Header("Runtime Settings")]
    public bool applyWind = true;
    public float windMultiplier = 1f;
    
    private WindZone windZone;
    
    void Start()
    {
        windZone = FindObjectOfType<WindZone>();
    }
    
    void FixedUpdate()
    {
        if (applyWind && windZone != null)
        {
            ApplyWindForce();
        }
    }
    
    void ApplyWindForce()
    {
        Vector3 windDirection = windZone.transform.forward;
        float windStrength = windZone.windMain + 
            Mathf.Sin(Time.time * windZone.windPulseFrequency) * windZone.windPulseMagnitude;
        
        for (int i = 1; i < segmentRigidbodies.Length; i++)
        {
            if (segmentRigidbodies[i] != null && !segmentRigidbodies[i].isKinematic)
            {
                Vector3 windForce = windDirection * windStrength * windMultiplier * 0.01f;
                segmentRigidbodies[i].AddForce(windForce, ForceMode.Force);
            }
        }
    }
    
    public void AttachToPoint(Vector3 attachPoint)
    {
        if (segments.Length > 0)
        {
            segments[0].transform.position = attachPoint;
            RopeAttachmentPoint attachment = segments[0].GetComponent<RopeAttachmentPoint>();
            if (attachment != null)
            {
                attachment.AttachTo(attachPoint);
            }
        }
    }
    
    public void AttachObjectToEnd(GameObject obj)
    {
        if (segments.Length > 0)
        {
            GameObject lastSegment = segments[segments.Length - 1];
            RopeAttachmentPoint attachment = lastSegment.GetComponent<RopeAttachmentPoint>();
            if (attachment != null)
            {
                attachment.AttachObject(obj);
            }
        }
    }
}