// TreeNode.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TreeNode
{
    public Vector3 position;
    public Vector3 growthDirection;
    public int level;
    public List<TreeNode> children = new List<TreeNode>();
    public List<PaperLanternTag> tags = new List<PaperLanternTag>();
    public GameObject branchObject;
    
    public TreeNode(Vector3 pos, Vector3 dir, int lvl)
    {
        position = pos;
        growthDirection = dir.normalized;
        level = lvl;
    }
    
    public TreeNode AddChild(Vector3 direction)
    {
        float branchLength = 2f * Mathf.Pow(0.8f, level); // Branches get shorter
        Vector3 endPosition = position + direction.normalized * branchLength;
        
        TreeNode child = new TreeNode(endPosition, direction, level + 1);
        children.Add(child);
        return child;
    }
    
    public bool CanAddTag()
    {
        return tags.Count < 10; // Max tags per branch
    }
}