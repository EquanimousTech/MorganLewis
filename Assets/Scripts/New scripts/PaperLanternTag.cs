using UnityEngine;
using TMPro;

public class PaperLanternTag : MonoBehaviour
{
    [Header("Components")]
    public TextMeshPro textComponent;
    public LineRenderer stringRenderer;
    public GameObject lanternBody;
    public Light lanternLight;
    
    [Header("Animation")]
    public float swayAmount = 0.1f;
    public float swaySpeed = 1f;
    
    private TreeNode attachedNode;
    private Vector3 hangPoint;
    private float swayOffset;
    
    void Awake()
    {
        swayOffset = Random.Range(0f, Mathf.PI * 2);
    }
    
    public void Initialize(string data, TreeNode node, Color color, float hangDistance)
    {
        attachedNode = node;
        
        // Set text
        textComponent.text = data;
        textComponent.fontSize = 3f;
        
        // Position below branch
        hangPoint = node.position + Vector3.down * hangDistance;
        transform.position = hangPoint;
        
        // Apply color to lantern
        lanternBody.GetComponent<Renderer>().material.color = color;
        
        // Set up light
        if (lanternLight != null)
        {
            lanternLight.color = color;
            lanternLight.intensity = 0.5f;
            lanternLight.range = 2f;
        }
        
        // Create string connection
        SetupString(node.position, transform.position);
    }
    
    void Update()
    {
        // Gentle swaying animation
        float sway = Mathf.Sin(Time.time * swaySpeed + swayOffset) * swayAmount;
        transform.position = hangPoint + new Vector3(sway, 0, sway * 0.5f);
        
        // Update string
        if (stringRenderer != null && attachedNode != null)
        {
            stringRenderer.SetPosition(0, attachedNode.position);
            stringRenderer.SetPosition(1, transform.position + Vector3.up * 0.5f);
        }
    }
    
    void SetupString(Vector3 start, Vector3 end)
    {
        stringRenderer.positionCount = 2;
        stringRenderer.SetPosition(0, start);
        stringRenderer.SetPosition(1, end);
        stringRenderer.startWidth = 0.02f;
        stringRenderer.endWidth = 0.02f;
        
        // Dark string color
        stringRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        stringRenderer.material.color = new Color(0.2f, 0.2f, 0.2f);
    }
}