using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class BranchParameters
{
    public float baseLength = 2f;
    public float baseThickness = 0.3f;
    public float lengthReduction = 0.8f;
    public float thicknessReduction = 0.7f;
    public float minBranchLength = 0.5f;
    public float minBranchThickness = 0.05f;
    public float branchAngleVariance = 30f;
    public int maxBranchDepth = 4;
}

public class ProceduralTree : MonoBehaviour
{
    [Header("Tree Configuration")]
    public BranchParameters branchParams;
    public Material branchMaterial;
    public GameObject tagPrefab;
    public GameObject centerCircle;
    
    [Header("Growth Settings")]
    public int tagsPerBranchExtension = 5;
    public float growthSpeed = 2f;
    
    [Header("Layout Settings")]
    public float horizontalSpread = 10f;
    public float verticalLimit = 3f;
    
    private List<Branch> branches = new List<Branch>();
    private List<TagData> pendingTags = new List<TagData>();
    private int totalTags = 0;
    
    void Start()
    {
        GenerateInitialTree();
    }
    
    void GenerateInitialTree()
    {
        // Create main branches from center
        int mainBranchCount = 2; // One left, one right for horizontal display
        
        for (int i = 0; i < mainBranchCount; i++)
        {
            float angle = i == 0 ? 0f : 180f; // Left and right
            Vector3 direction = Quaternion.Euler(0, 0, angle) * Vector3.right;
            
            Branch mainBranch = CreateBranch(
                centerCircle.transform.position,
                direction,
                branchParams.baseLength,
                branchParams.baseThickness,
                0
            );
            
            branches.Add(mainBranch);
            GenerateSubBranches(mainBranch, 1);
        }
    }
    
    Branch CreateBranch(Vector3 startPos, Vector3 direction, float length, float thickness, int depth)
    {
        GameObject branchObj = new GameObject($"Branch_Depth_{depth}");
        branchObj.transform.SetParent(transform);
        
        Branch branch = branchObj.AddComponent<Branch>();
        branch.Initialize(startPos, direction, length, thickness, depth);
        branch.CreateMesh(branchMaterial);
        
        return branch;
    }
    
    void GenerateSubBranches(Branch parentBranch, int currentDepth)
    {
        if (currentDepth >= branchParams.maxBranchDepth) return;
        
        int subBranchCount = Random.Range(2, 4);
        
        for (int i = 0; i < subBranchCount; i++)
        {
            float t = (i + 1f) / (subBranchCount + 1f);
            Vector3 branchPoint = parentBranch.GetPointAlongBranch(t);
            
            // Create variation in angle but keep mostly horizontal
            float angleVariance = Random.Range(-branchParams.branchAngleVariance, branchParams.branchAngleVariance);
            float baseAngle = parentBranch.direction.x > 0 ? Random.Range(-45f, 45f) : Random.Range(135f, 225f);
            
            Vector3 newDirection = Quaternion.Euler(0, 0, baseAngle + angleVariance) * Vector3.right;
            
            // Clamp vertical component
            newDirection.y = Mathf.Clamp(newDirection.y, -0.3f, 0.3f);
            newDirection.Normalize();
            
            float newLength = parentBranch.length * branchParams.lengthReduction;
            float newThickness = parentBranch.thickness * branchParams.thicknessReduction;
            
            if (newLength < branchParams.minBranchLength || newThickness < branchParams.minBranchThickness)
                continue;
            
            Branch subBranch = CreateBranch(branchPoint, newDirection, newLength, newThickness, currentDepth);
            parentBranch.childBranches.Add(subBranch);
            branches.Add(subBranch);
            
            GenerateSubBranches(subBranch, currentDepth + 1);
        }
    }
    
    public void AddTag(TagData tagData)
    {
        pendingTags.Add(tagData);
        totalTags++;
        
        // Check if we need to grow the tree
        if (totalTags % tagsPerBranchExtension == 0)
        {
            ExtendTree();
        }
        
        PlaceTag(tagData);
    }
    
    void ExtendTree()
    {
        // Find branches that can be extended
        List<Branch> extensibleBranches = branches.FindAll(b => 
            b.depth < branchParams.maxBranchDepth - 1 && 
            b.childBranches.Count < 3
        );
        
        if (extensibleBranches.Count > 0)
        {
            Branch selectedBranch = extensibleBranches[Random.Range(0, extensibleBranches.Count)];
            
            // Add new sub-branches
            int newBranches = Random.Range(1, 3);
            for (int i = 0; i < newBranches; i++)
            {
                float t = Random.Range(0.5f, 0.9f);
                Vector3 branchPoint = selectedBranch.GetPointAlongBranch(t);
                
                float angleVariance = Random.Range(-branchParams.branchAngleVariance, branchParams.branchAngleVariance);
                Vector3 newDirection = Quaternion.Euler(0, 0, angleVariance) * selectedBranch.direction;
                newDirection.y = Mathf.Clamp(newDirection.y, -0.3f, 0.3f);
                newDirection.Normalize();
                
                float newLength = selectedBranch.length * branchParams.lengthReduction * Random.Range(0.8f, 1f);
                float newThickness = selectedBranch.thickness * branchParams.thicknessReduction;
                
                Branch newBranch = CreateBranch(branchPoint, newDirection, newLength, newThickness, selectedBranch.depth + 1);
                selectedBranch.childBranches.Add(newBranch);
                branches.Add(newBranch);
            }
        }
    }
    
    void PlaceTag(TagData tagData)
    {
        // Find suitable branch for tag placement
        List<Branch> suitableBranches = branches.FindAll(b => b.depth >= 1);
        
        if (suitableBranches.Count > 0)
        {
            Branch selectedBranch = suitableBranches[Random.Range(0, suitableBranches.Count)];
            
            // Create tag at random position along branch
            float t = Random.Range(0.2f, 0.8f);
            Vector3 tagPosition = selectedBranch.GetPointAlongBranch(t);
            
            // Offset below branch
            tagPosition.y -= selectedBranch.thickness * 0.5f + 0.1f;
            
            GameObject tag = Instantiate(tagPrefab, tagPosition, Quaternion.identity);
            tag.transform.SetParent(selectedBranch.transform);
            
            // Configure tag with data
            TagDisplay tagDisplay = tag.GetComponent<TagDisplay>();
            if (tagDisplay != null)
            {
                tagDisplay.SetData(tagData);
            }
            
            // Add hanging animation
            HangingAnimation hanging = tag.AddComponent<HangingAnimation>();
            hanging.swingAmount = Random.Range(5f, 15f);
            hanging.swingSpeed = Random.Range(0.5f, 1.5f);
        }
    }
}