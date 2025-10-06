using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class RopeCreator : MonoBehaviour
{
    [Header("Rope Configuration")]
    [SerializeField] private int segmentCount = 10;
    [SerializeField] private float ropeLength = 1.5f;
    [SerializeField] private float ropeThickness = 0.01f;
    [SerializeField] private Material ropeMaterial;
    
    [Header("Physics Settings")]
    [SerializeField] private float segmentMass = 0.01f;
    [SerializeField] private float drag = 1f;
    [SerializeField] private float angularDrag = 2f;
    
    [Header("Joint Settings")]
    [SerializeField] private float jointSpring = 100f;
    [SerializeField] private float jointDamper = 10f;
    [SerializeField] private float breakForce = Mathf.Infinity;
    
    [Header("Prefab Settings")]
    [SerializeField] private string prefabName = "PhysicsRope";
    [SerializeField] private string savePath = "Assets/Prefabs/";

    [ContextMenu("Create Rope Prefab")]
    public void CreateRopePrefab()
    {
        // Create root object
        GameObject ropeRoot = new GameObject(prefabName);
        
        // Calculate segment length
        float segmentLength = ropeLength / segmentCount;
        
        // Lists to store components
        List<GameObject> segments = new List<GameObject>();
        List<Rigidbody> rigidbodies = new List<Rigidbody>();
        
        // Create rope material if not assigned
        if (ropeMaterial == null)
        {
            ropeMaterial = CreateRopeMaterial();
        }
        
        // Create segments
        for (int i = 0; i < segmentCount; i++)
        {
            // Create segment
            GameObject segment = CreateRopeSegment(i, segmentLength);
            segment.transform.parent = ropeRoot.transform;
            
            // Position segment
            float yPosition = -i * segmentLength;
            segment.transform.localPosition = new Vector3(0, yPosition, 0);
            
            // Add physics
            Rigidbody rb = AddPhysicsToSegment(segment);
            
            // First segment is kinematic (attached to branch)
            if (i == 0)
            {
                rb.isKinematic = true;
                
                // Add attachment point component
                RopeAttachmentPoint attachPoint = segment.AddComponent<RopeAttachmentPoint>();
                attachPoint.isTopAttachment = true;
            }
            
            segments.Add(segment);
            rigidbodies.Add(rb);
        }
        
        // Connect segments with joints
        ConnectSegments(segments, rigidbodies);
        
        // Add rope controller to root
        RopeController ropeController = ropeRoot.AddComponent<RopeController>();
        ropeController.segments = segments.ToArray();
        ropeController.segmentRigidbodies = rigidbodies.ToArray();
        
        // Add tag attachment point to last segment
        if (segments.Count > 0)
        {
            RopeAttachmentPoint tagAttach = segments[segments.Count - 1].AddComponent<RopeAttachmentPoint>();
            tagAttach.isTopAttachment = false;
        }
        
        // Save as prefab
        SaveRopePrefab(ropeRoot);
    }
    
    GameObject CreateRopeSegment(int index, float length)
    {
        // Create cylinder
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        segment.name = $"RopeSegment_{index}";
        
        // Scale to rope dimensions
        segment.transform.localScale = new Vector3(ropeThickness, length / 2f, ropeThickness);
        
        // Apply material
        MeshRenderer renderer = segment.GetComponent<MeshRenderer>();
        renderer.material = ropeMaterial;
        
        // Optimize collider
        CapsuleCollider capsule = segment.GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            // Already has capsule collider from primitive
            capsule.height = 2f; // Cylinder primitive has height 2
            capsule.radius = 0.5f;
        }
        
        return segment;
    }
    
    Rigidbody AddPhysicsToSegment(GameObject segment)
    {
        Rigidbody rb = segment.AddComponent<Rigidbody>();
        rb.mass = segmentMass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        return rb;
    }
    
    void ConnectSegments(List<GameObject> segments, List<Rigidbody> rigidbodies)
    {
        for (int i = 1; i < segments.Count; i++)
        {
            GameObject currentSegment = segments[i];
            Rigidbody previousRb = rigidbodies[i - 1];
            
            // Add configurable joint for rope-like behavior
            ConfigurableJoint joint = currentSegment.AddComponent<ConfigurableJoint>();
            joint.connectedBody = previousRb;
            
            // Configure joint for rope behavior
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Limited;
            joint.zMotion = ConfigurableJointMotion.Locked;
            
            // Set linear limit (slight stretch)
            SoftJointLimit linearLimit = new SoftJointLimit();
            linearLimit.limit = 0.01f; // Very small stretch
            joint.linearLimit = linearLimit;
            
            // Configure rotation
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Free;
            joint.angularZMotion = ConfigurableJointMotion.Limited;
            
            // Set angular limits
            SoftJointLimit angularLimit = new SoftJointLimit();
            angularLimit.limit = 30f; // 30 degree bend limit
            joint.lowAngularXLimit = new SoftJointLimit { limit = -angularLimit.limit };
            joint.highAngularXLimit = angularLimit;
            joint.angularZLimit = angularLimit;
            
            // Set spring for stability
            SoftJointLimitSpring spring = new SoftJointLimitSpring();
            spring.spring = jointSpring;
            spring.damper = jointDamper;
            joint.angularXLimitSpring = spring;
            joint.angularYZLimitSpring = spring;
            
            // Set break force if needed
            if (breakForce < Mathf.Infinity)
            {
                joint.breakForce = breakForce;
                joint.breakTorque = breakForce;
            }
            
            // Auto configure anchor
            joint.autoConfigureConnectedAnchor = true;
        }
    }
    
    Material CreateRopeMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.name = "RopeMaterial";
        mat.color = new Color(0.75f, 0.65f, 0.5f); // Beige rope color
        mat.SetFloat("_Smoothness", 0.2f);
        mat.SetFloat("_Metallic", 0f);
        
        return mat;
    }
    
    void SaveRopePrefab(GameObject ropeObject)
    {
        #if UNITY_EDITOR
        // Ensure the directory exists
        if (!AssetDatabase.IsValidFolder(savePath))
        {
            string[] folders = savePath.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                if (!string.IsNullOrEmpty(folders[i]))
                {
                    string nextPath = currentPath + "/" + folders[i];
                    if (!AssetDatabase.IsValidFolder(nextPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = nextPath;
                }
            }
        }
        
        // Save prefab
        string fullPath = savePath + prefabName + ".prefab";
        
        // Check if prefab exists
        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fullPath);
        if (existingPrefab != null)
        {
            // Update existing prefab
            PrefabUtility.SaveAsPrefabAssetAndConnect(ropeObject, fullPath, InteractionMode.UserAction);
            Debug.Log($"Updated existing rope prefab at: {fullPath}");
        }
        else
        {
            // Create new prefab
            PrefabUtility.SaveAsPrefabAsset(ropeObject, fullPath);
            Debug.Log($"Created new rope prefab at: {fullPath}");
        }
        
        // Clean up the scene object
        DestroyImmediate(ropeObject);
        
        // Refresh asset database
        AssetDatabase.Refresh();
        
        // Select the created prefab
        Object prefab = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        #endif
    }
}