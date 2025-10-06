using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class ModernTag : MonoBehaviour
{
    [Header("Components")]
    public TextMeshProUGUI mainText;
    public TextMeshProUGUI departmentText;
    // public TextMeshProUGUI categoryText;
    public LineRenderer connectionLine;
    public GameObject tagBody;

    [Header("Wind Settings")]
    public float windSensitivity = 1f;
    public float swayDamping = 3f;
    public float maxSwayAngle = 15f;
    [Header("Line Attachment")]
    public Transform lineAttachmentPoint;
    private Transform branchTransform;
    private Vector3 localAnchorOffset;
    private DualTreeManager treeManager;

    private Vector3 attachPoint;
    private Vector3 targetPosition;
    private Rigidbody rb;
    private ConfigurableJoint joint;
    private WindZone windZone;
    private Vector3 originalLocalPosition;
    public GameObject ropePrefab;
    public Material ropeMaterial;
    float customLimit;

    void Awake()
    {
        SetupComponents();
        SetupPhysics();
        FindWindZone();
    }

    void SetupComponents()
    {
        // Your existing SetupComponents code stays the same
        if (tagBody == null)
        {
            tagBody = transform.Find("TagBody")?.gameObject;
            if (tagBody == null)
            {
                tagBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tagBody.name = "TagBody";
                tagBody.transform.parent = transform;
                tagBody.transform.localScale = new Vector3(2f, 1f, 0.1f);
            }
        }

        if (mainText == null)
        {
            GameObject textObj = new GameObject("MainText");
            textObj.transform.parent = tagBody.transform;
            textObj.transform.localPosition = Vector3.forward * 0.1f;
            mainText = textObj.AddComponent<TextMeshProUGUI>();
            mainText.fontSize = 2;
            mainText.alignment = TextAlignmentOptions.Center;
        }

        if (connectionLine == null)
        {
            connectionLine = gameObject.AddComponent<LineRenderer>();
            connectionLine.material = ropeMaterial;
            connectionLine.startWidth = 0.04f;
            connectionLine.endWidth = 0.04f;
            connectionLine.positionCount = 2;
        }
    }

    void SetupPhysics()
    {
        // Add Rigidbody
        customLimit = Random.Range(4, 6);
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.mass = 0.1f;
        rb.linearDamping = swayDamping;
        rb.angularDamping = swayDamping;
        rb.useGravity = true;

        // Add ConfigurableJoint for hanging behavior
        joint = gameObject.AddComponent<ConfigurableJoint>();
        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        // Set joint limits
        SoftJointLimit limit = new SoftJointLimit();
        limit.limit = customLimit;
        joint.linearLimit = limit;

        // Angular limits for swaying
        SoftJointLimitSpring angularLimit = new SoftJointLimitSpring();
        angularLimit.spring = 50f;
        angularLimit.damper = 5f;
        joint.angularXLimitSpring = angularLimit;
        joint.angularYZLimitSpring = angularLimit;

        // Lock certain rotations if needed
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Free;
        joint.angularZMotion = ConfigurableJointMotion.Limited;

        // Set angular limits
        joint.lowAngularXLimit = new SoftJointLimit { limit = -maxSwayAngle };
        joint.highAngularXLimit = new SoftJointLimit { limit = maxSwayAngle };
        joint.angularZLimit = new SoftJointLimit { limit = maxSwayAngle };
    }

    void FindWindZone()
    {
        windZone = FindObjectOfType<WindZone>();
        if (windZone == null)
        {
            Debug.Log("No WindZone found in scene. Creating one...");
            GameObject windObj = new GameObject("WindZone");
            windZone = windObj.AddComponent<WindZone>();
            windZone.mode = WindZoneMode.Directional;
            windZone.windMain = 1f;
            windZone.windTurbulence = 0.5f;
            windZone.windPulseFrequency = 0.5f;
            windZone.windPulseMagnitude = 0.5f;
            windObj.transform.rotation = Quaternion.Euler(0, 30, 0); // Wind from side
        }
    }

    // public void Initialize(string content, string department, string category, Vector3 position, Color tagColor, Vector3 attachTo)
    // {
    //     Debug.Log("=== ModernTag Initialize Called ===");
    //     Debug.Log($"Rope Prefab assigned: {ropePrefab != null}");
    //     targetPosition = position;
    //     transform.position = position;
    //     attachPoint = attachTo;
    //     originalLocalPosition = transform.localPosition;

    //     // Configure joint connection point
    //     if (joint != null)
    //     {
    //         joint.connectedAnchor = attachTo;
    //         joint.autoConfigureConnectedAnchor = false;
    //     }

    //     // Your existing text and color setup code
    //     mainText.text = content;
    //     mainText.color = Color.black;

    //     if (departmentText != null)
    //     {
    //         departmentText.text = department;
    //         // departmentText.fontSize = 1.5f;
    //         // departmentText.color = Color.gray;
    //     }

    //     // Set tag body color
    //     if (tagBody != null)
    //     {
    //         Renderer renderer = tagBody.GetComponent<Renderer>();
    //         if (renderer != null)
    //         {
    //             renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    //             renderer.material.color = tagColor;
    //             renderer.material.SetFloat("_Smoothness", 0.3f);
    //         }
    //     }

    //     // Setup connection line
    //     if (connectionLine != null)
    //     {
    //         // Material lineMat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
    //         // lineMat.color = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    //         connectionLine.material = ropeMaterial;

    //         connectionLine.SetPosition(0, attachPoint);

    //         // Use custom attachment point if available
    //         if (lineAttachmentPoint != null)
    //         {
    //             connectionLine.SetPosition(1, lineAttachmentPoint.position);
    //         }
    //         else
    //         {
    //             connectionLine.SetPosition(1, transform.position + Vector3.up * 0.5f);
    //         }
    //     }
    // }

    public void Initialize(string content, string department, string category, Vector3 position, Color tagColor, Vector3 attachTo)
    {
        Debug.Log("=== ModernTag Initialize Called ===");
        Debug.Log($"Rope Prefab assigned: {ropePrefab != null}");
        targetPosition = position;
        transform.position = position;
        attachPoint = attachTo;
        originalLocalPosition = transform.localPosition;

        // ✨ NEW: Store branch transform and calculate local offset
        branchTransform = transform.parent;

        if (branchTransform != null)
        {
            localAnchorOffset = branchTransform.InverseTransformPoint(attachTo);
            Debug.Log($"Local anchor offset calculated: {localAnchorOffset}");
        }

        // ✨ NEW: Find and subscribe to tree manager events
        treeManager = FindObjectOfType<DualTreeManager>();
        if (treeManager != null)
        {
            treeManager.onTreeMoved.AddListener(OnTreeMoved);
            Debug.Log("✅ Subscribed to tree movement events");
        }

        // Configure joint connection point
        if (joint != null)
        {
            joint.connectedAnchor = attachTo;
            joint.autoConfigureConnectedAnchor = false;
        }

        // Your existing text and color setup code
        mainText.text = content;
        mainText.color = Color.black;

        if (departmentText != null)
        {
            departmentText.text = department;
        }

        // Set tag body color
        if (tagBody != null)
        {
            Renderer renderer = tagBody.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.material.color = tagColor;
                renderer.material.SetFloat("_Smoothness", 0.3f);
            }
        }

        // Setup connection line
        if (connectionLine != null)
        {
            connectionLine.material = ropeMaterial;
            connectionLine.SetPosition(0, attachPoint);

            if (lineAttachmentPoint != null)
            {
                connectionLine.SetPosition(1, lineAttachmentPoint.position);
            }
            else
            {
                connectionLine.SetPosition(1, transform.position + Vector3.up * 0.5f);
            }
        }
    }

    // ✨ NEW METHOD 1: Event handler - called ONLY when tree moves
    void OnTreeMoved(Transform movedTree)
    {
        // Check if the moved tree is our parent tree
        if (movedTree == branchTransform || movedTree.IsChildOf(branchTransform) || branchTransform.IsChildOf(movedTree))
        {
            UpdateAnchorPosition();
        }
    }

    // ✨ NEW METHOD 2: Update anchor position based on tree's new position
    void UpdateAnchorPosition()
    {
        if (branchTransform == null || joint == null) return;

        // Convert local offset back to world position
        Vector3 worldAnchor = branchTransform.TransformPoint(localAnchorOffset);

        // Update joint anchor
        joint.connectedAnchor = worldAnchor;

        // Update attach point for line rendering
        attachPoint = worldAnchor;

        // Update line immediately
        if (connectionLine != null)
        {
            connectionLine.SetPosition(0, attachPoint);
        }

        Debug.Log($"📍 Tag anchor updated to: {worldAnchor}");
    }

    // ✨ NEW METHOD 3: Clean up event subscription
    void OnDestroy()
    {
        if (treeManager != null)
        {
            treeManager.onTreeMoved.RemoveListener(OnTreeMoved);
            Debug.Log("🧹 Unsubscribed from tree movement events");
        }
    }

    void FixedUpdate()
    {
        if (windZone != null && rb != null)
        {
            // Calculate wind force
            Vector3 windDirection = windZone.transform.forward;
            float windStrength = windZone.windMain +
                                Mathf.Sin(Time.time * windZone.windPulseFrequency * Random.Range(0.5f, 1)) * windZone.windPulseMagnitude;

            // Add turbulence
            Vector3 turbulence = new Vector3(
                Mathf.PerlinNoise(Time.time * windZone.windTurbulence, 0) - 0.5f,
                0,
                Mathf.PerlinNoise(0, Time.time * windZone.windTurbulence) - 0.5f
            ) * windZone.windTurbulence;

            // Apply wind force
            Vector3 windForce = (windDirection + turbulence) * windStrength * windSensitivity;
            rb.AddForce(windForce);
        }
    }

    void Update()
    {
        // Update connection line - make sure it's visible
        if (connectionLine != null)
        {
            // Line from branch point (high) to tag (low)
            connectionLine.SetPosition(0, attachPoint);

            // Use custom attachment point if available
            if (lineAttachmentPoint != null)
            {
                connectionLine.SetPosition(1, lineAttachmentPoint.position);
            }
            else
            {
                connectionLine.SetPosition(1, transform.position + Vector3.up * 0.5f); // Default position
            }

            // Optional: Make line more visible
            connectionLine.startWidth = 0.03f; // Thicker line
            connectionLine.endWidth = 0.03f;
        }
    }
}