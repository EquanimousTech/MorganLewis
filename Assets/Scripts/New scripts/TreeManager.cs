using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;

public class TreeManager : MonoBehaviour
{
    [Header("Tree Configuration")]
    public float treeWidth = 10f;
    public float treeHeight = 3f;
    public int mainBranches = 3;
    public float trunkThickness = 0.5f;
    public AnimationCurve thicknessCurve;
    
    [Header("Foliage Configuration")]
    public GameObject foliagePrefab;
    public Gradient foliageGradient;
    public float foliageSize = 2f;
    public int foliageClusters = 20;
    
    [Header("Tag Configuration")]
    public GameObject lanternTagPrefab;
    public int tagsPerBranchGrowth = 5;
    public float tagHangDistance = 1.5f;
    public Color[] tagColors;
    
    [Header("Materials")]
    public Material barkMaterial;
    public Material foliageMaterial;
    
    private TreeNode rootNode;
    private List<PaperLanternTag> allTags = new List<PaperLanternTag>();
    private List<GameObject> foliageObjects = new List<GameObject>();
    
    void Start()
    {
        GenerateHorizontalTree();
        StartCoroutine(DelayedSetup());
    }
    
    IEnumerator DelayedSetup()
    {
        yield return new WaitForSeconds(0.5f);
        // GetComponent<StrapiConnector>().Initialize(this);
    }
    
    void GenerateHorizontalTree()
    {
        // Create main horizontal trunk
        rootNode = new TreeNode(transform.position, Vector3.right, 0);
        
        // Generate S-curved main branch
        CreateMainHorizontalBranch();
        
        // Add foliage clusters
        GenerateFoliage();
    }
    
    void CreateMainHorizontalBranch()
    {
        Vector3 currentPos = transform.position;
        TreeNode currentNode = rootNode;
        
        int segments = 10;
        float segmentLength = treeWidth / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            
            // Create S-curve
            float heightOffset = Mathf.Sin(t * Mathf.PI * 2) * treeHeight * 0.3f;
            Vector3 nextPos = currentPos + Vector3.right * segmentLength + Vector3.up * heightOffset;
            
            // Add some natural variation
            nextPos += new Vector3(0, Random.Range(-0.2f, 0.2f), Random.Range(-0.3f, 0.3f));
            
            Vector3 direction = (nextPos - currentPos).normalized;
            TreeNode newNode = new TreeNode(nextPos, direction, 0);
            currentNode.children.Add(newNode);
            
            // Create branch mesh
            float thickness = trunkThickness * thicknessCurve.Evaluate(t);
            CreateCurvedBranch(currentPos, nextPos, thickness, thickness * 0.8f);
            
            // Add sub-branches
            if (i > 2 && i < segments - 2 && i % 2 == 0)
            {
                CreateSubBranches(newNode, t);
            }
            
            currentPos = nextPos;
            currentNode = newNode;
        }
    }
    
    void CreateSubBranches(TreeNode parentNode, float treeProgress)
    {
        int subBranches = Random.Range(2, 4);
        
        for (int i = 0; i < subBranches; i++)
        {
            Vector3 direction = Quaternion.Euler(
                Random.Range(-30f, 30f),
                Random.Range(-60f, 60f),
                Random.Range(-45f, 45f)
            ) * parentNode.growthDirection;
            
            float branchLength = Random.Range(1f, 2.5f) * (1f - treeProgress * 0.5f);
            Vector3 endPos = parentNode.position + direction * branchLength;
            
            TreeNode subNode = new TreeNode(endPos, direction, parentNode.level + 1);
            parentNode.children.Add(subNode);
            
            float thickness = trunkThickness * 0.3f * (1f - treeProgress * 0.5f);
            CreateCurvedBranch(parentNode.position, endPos, thickness, thickness * 0.6f);
        }
    }
    
    void CreateCurvedBranch(Vector3 start, Vector3 end, float startThickness, float endThickness)
    {
        GameObject branch = ProceduralMeshGenerator.CreateCurvedBranch(
            start, end, startThickness, endThickness, barkMaterial
        );
        branch.transform.parent = transform;
    }
    
    void GenerateFoliage()
    {
        List<TreeNode> allNodes = GetAllNodes(rootNode);
        
        foreach (var node in allNodes)
        {
            if (Random.value > 0.3f) // 70% chance for foliage
            {
                GameObject foliage = Instantiate(foliagePrefab, node.position, Random.rotation);
                foliage.transform.parent = transform;
                
                // Set random color from gradient
                float t = Random.value;
                Color foliageColor = foliageGradient.Evaluate(t);
                foliage.GetComponent<Renderer>().material.color = foliageColor;
                
                // Random scale
                float scale = Random.Range(foliageSize * 0.5f, foliageSize * 1.5f);
                foliage.transform.localScale = Vector3.one * scale;
                
                foliageObjects.Add(foliage);
            }
        }
    }
    
    public void AddNewTag(string content, int tagId)
    {
        TreeNode targetNode = FindBestNodeForTag();
        if (targetNode != null)
        {
            GameObject tagObject = Instantiate(lanternTagPrefab);
            PaperLanternTag lanternTag = tagObject.GetComponent<PaperLanternTag>();
            
            // Set random color
            Color tagColor = tagColors[Random.Range(0, tagColors.Length)];
            lanternTag.Initialize(content, targetNode, tagColor, tagHangDistance);
            
            targetNode.tags.Add(lanternTag);
            allTags.Add(lanternTag);
        }
    }
    
    TreeNode FindBestNodeForTag()
    {
        List<TreeNode> candidates = new List<TreeNode>();
        CollectTagCandidates(rootNode, candidates);
        
        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];
        
        return null;
    }
    
    void CollectTagCandidates(TreeNode node, List<TreeNode> candidates)
    {
        if (node.tags.Count < 3) // Max 3 tags per position
            candidates.Add(node);
        
        foreach (var child in node.children)
            CollectTagCandidates(child, candidates);
    }
    
    List<TreeNode> GetAllNodes(TreeNode node)
    {
        List<TreeNode> nodes = new List<TreeNode> { node };
        foreach (var child in node.children)
            nodes.AddRange(GetAllNodes(child));
        return nodes;
    }
}