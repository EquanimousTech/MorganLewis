using UnityEngine;
using System.Collections.Generic;

public class Branch : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public Vector3 direction;
    public float length;
    public float thickness;
    public int depth;
    public List<Branch> childBranches = new List<Branch>();
    
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    public void Initialize(Vector3 start, Vector3 dir, float len, float thick, int d)
    {
        startPosition = start;
        direction = dir.normalized;
        length = len;
        thickness = thick;
        depth = d;
        endPosition = startPosition + direction * length;
        
        transform.position = startPosition;
    }
    
    public void CreateMesh(Material material)
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        
        // Create a tapered cylinder mesh
        Mesh mesh = new Mesh();
        
        int segments = 8;
        Vector3[] vertices = new Vector3[segments * 2];
        int[] triangles = new int[segments * 6];
        Vector2[] uvs = new Vector2[vertices.Length];
        
        // Generate vertices
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);
            
            // Start circle (thicker)
            vertices[i] = new Vector3(x * thickness, 0, z * thickness);
            
            // End circle (thinner)
            float endThickness = thickness * 0.6f;
            vertices[i + segments] = endPosition - startPosition + new Vector3(x * endThickness, 0, z * endThickness);
        }
        
        // Generate triangles
        for (int i = 0; i < segments; i++)
        {
            int next = (i + 1) % segments;
            
            // First triangle
            triangles[i * 6] = i;
            triangles[i * 6 + 1] = next;
            triangles[i * 6 + 2] = i + segments;
            
            // Second triangle
            triangles[i * 6 + 3] = next;
            triangles[i * 6 + 4] = next + segments;
            triangles[i * 6 + 5] = i + segments;
        }
        
        // Generate UVs
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2((float)(i % segments) / segments, i < segments ? 0 : 1);
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshFilter.mesh = mesh;
    }
    
    public Vector3 GetPointAlongBranch(float t)
    {
        return Vector3.Lerp(startPosition, endPosition, t);
    }
}