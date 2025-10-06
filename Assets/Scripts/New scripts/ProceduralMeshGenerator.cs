// using UnityEngine;
// using System.Collections.Generic;

// public static class ProceduralMeshGenerator
// {
//     public static GameObject CreateCurvedBranch(Vector3 start, Vector3 end, float startThickness, float endThickness, Material material)
//     {
//         GameObject branch = new GameObject("Branch");
//         MeshFilter meshFilter = branch.AddComponent<MeshFilter>();
//         MeshRenderer meshRenderer = branch.AddComponent<MeshRenderer>();
        
//         // Create bezier curve for natural look
//         Vector3 midPoint = (start + end) / 2f;
//         midPoint += Vector3.up * Vector3.Distance(start, end) * 0.2f; // Add upward curve
        
//         Mesh mesh = new Mesh();
//         List<Vector3> vertices = new List<Vector3>();
//         List<int> triangles = new List<int>();
//         List<Vector2> uvs = new List<Vector2>();
        
//         int segments = 10; // Curve segments
//         int rings = 8; // Cross-section segments
        
//         // Generate vertices along the curve
//         for (int i = 0; i <= segments; i++)
//         {
//             float t = (float)i / segments;
            
//             // Quadratic bezier curve
//             Vector3 position = Mathf.Pow(1 - t, 2) * start + 
//                               2 * (1 - t) * t * midPoint + 
//                               Mathf.Pow(t, 2) * end;
            
//             // Interpolate thickness
//             float radius = Mathf.Lerp(startThickness, endThickness, t);
            
//             // Calculate tangent
//             Vector3 tangent = 2 * (1 - t) * (midPoint - start) + 2 * t * (end - midPoint);
//             tangent.Normalize();
            
//             // Find perpendicular vectors
//             Vector3 perpendicular1 = Vector3.Cross(tangent, Vector3.up).normalized;
//             if (perpendicular1.magnitude < 0.001f)
//                 perpendicular1 = Vector3.Cross(tangent, Vector3.forward).normalized;
            
//             Vector3 perpendicular2 = Vector3.Cross(tangent, perpendicular1).normalized;
            
//             // Create ring of vertices
//             for (int j = 0; j < rings; j++)
//             {
//                 float angle = j * Mathf.PI * 2 / rings;
//                 Vector3 offset = (Mathf.Cos(angle) * perpendicular1 + Mathf.Sin(angle) * perpendicular2) * radius;
                
//                 vertices.Add(position + offset);
//                 uvs.Add(new Vector2((float)j / rings, (float)i / segments));
//             }
//         }
        
//         // Generate triangles
//         for (int i = 0; i < segments; i++)
//         {
//             for (int j = 0; j < rings; j++)
//             {
//                 int current = i * rings + j;
//                 int next = i * rings + (j + 1) % rings;
//                 int currentNext = (i + 1) * rings + j;
//                 int nextNext = (i + 1) * rings + (j + 1) % rings;
                
//                 triangles.Add(current);
//                 triangles.Add(currentNext);
//                 triangles.Add(next);
                
//                 triangles.Add(next);
//                 triangles.Add(currentNext);
//                 triangles.Add(nextNext);
//             }
//         }
        
//         mesh.vertices = vertices.ToArray();
//         mesh.triangles = triangles.ToArray();
//         mesh.uv = uvs.ToArray();
//         mesh.RecalculateNormals();
//         mesh.RecalculateBounds();
        
//         meshFilter.mesh = mesh;
//         meshRenderer.material = material;
        
//         return branch;
//     }
// }

using UnityEngine;
using System.Collections.Generic;

public static class ProceduralMeshGenerator
{
    public static GameObject CreateCurvedBranch(Vector3 start, Vector3 end, float startThickness, float endThickness, Material material)
    {
        GameObject branch = new GameObject("Branch");
        MeshFilter meshFilter = branch.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = branch.AddComponent<MeshRenderer>();
        
        // Create bezier curve for natural look
        Vector3 midPoint = (start + end) / 2f;
        midPoint += Vector3.up * Vector3.Distance(start, end) * 0.2f; // Add upward curve
        
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        
        int segments = 20; // Curve segments (more for smoother curve)
        int rings = 12; // Cross-section segments (more for rounder branch)
        
        // Generate vertices along the curve
        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            
            // Quadratic bezier curve
            Vector3 position = Mathf.Pow(1 - t, 2) * start + 
                              2 * (1 - t) * t * midPoint + 
                              Mathf.Pow(t, 2) * end;
            
            // Interpolate thickness with some variation for organic look
            float radius = Mathf.Lerp(startThickness, endThickness, t);
            
            // Add slight radius variation for more natural look
            radius *= 1f + Mathf.PerlinNoise(t * 5f, 0) * 0.1f;
            
            // Calculate tangent
            Vector3 tangent = 2 * (1 - t) * (midPoint - start) + 2 * t * (end - midPoint);
            tangent.Normalize();
            
            // Find perpendicular vectors
            Vector3 perpendicular1 = Vector3.Cross(tangent, Vector3.up).normalized;
            if (perpendicular1.magnitude < 0.001f)
                perpendicular1 = Vector3.Cross(tangent, Vector3.forward).normalized;
            
            Vector3 perpendicular2 = Vector3.Cross(tangent, perpendicular1).normalized;
            
            // Create ring of vertices (complete circle)
            for (int j = 0; j <= rings; j++)
            {
                float angle = (j % rings) * Mathf.PI * 2 / rings;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular1 + Mathf.Sin(angle) * perpendicular2) * radius;
                Vector3 vertex = position + offset;
                
                vertices.Add(vertex);
                
                // Calculate normal
                Vector3 normal = (vertex - position).normalized;
                normals.Add(normal);
                
                // UV coordinates
                uvs.Add(new Vector2((float)j / rings, t));
            }
        }
        
        // Generate triangles (connecting the rings properly)
        for (int i = 0; i < segments; i++)
        {
            for (int j = 0; j < rings; j++)
            {
                int rowStart = i * (rings + 1);
                int nextRowStart = (i + 1) * (rings + 1);
                
                int v0 = rowStart + j;
                int v1 = nextRowStart + j;
                int v2 = rowStart + j + 1;
                int v3 = nextRowStart + j + 1;
                
                // First triangle
                triangles.Add(v0);
                triangles.Add(v1);
                triangles.Add(v2);
                
                // Second triangle
                triangles.Add(v2);
                triangles.Add(v1);
                triangles.Add(v3);
            }
        }
        
        // Add end caps to make it completely solid
        AddEndCap(vertices, triangles, normals, uvs, 0, rings, true);
        AddEndCap(vertices, triangles, normals, uvs, segments, rings, false);
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateTangents();
        
        meshFilter.mesh = mesh;
        meshRenderer.material = material;
        
        // Add mesh collider for physics (optional)
        branch.AddComponent<MeshCollider>().convex = true;
        
        return branch;
    }
    
    static void AddEndCap(List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs, int segmentIndex, int rings, bool isStart)
    {
        // Get center position for this cap
        int centerVertIndex = vertices.Count;
        int ringStart = segmentIndex * (rings + 1);
        
        // Calculate center position
        Vector3 centerPos = Vector3.zero;
        for (int i = 0; i < rings; i++)
        {
            centerPos += vertices[ringStart + i];
        }
        centerPos /= rings;
        
        // Add center vertex
        vertices.Add(centerPos);
        normals.Add(isStart ? -Vector3.forward : Vector3.forward);
        uvs.Add(new Vector2(0.5f, 0.5f));
        
        // Create triangles for cap
        for (int i = 0; i < rings; i++)
        {
            int v0 = ringStart + i;
            int v1 = ringStart + ((i + 1) % rings);
            
            if (isStart)
            {
                triangles.Add(centerVertIndex);
                triangles.Add(v1);
                triangles.Add(v0);
            }
            else
            {
                triangles.Add(centerVertIndex);
                triangles.Add(v0);
                triangles.Add(v1);
            }
        }
    }
    
    // Alternative method for straight branches
    public static GameObject CreateStraightBranch(Vector3 start, Vector3 end, float startThickness, float endThickness, Material material)
    {
        GameObject branch = new GameObject("StraightBranch");
        MeshFilter meshFilter = branch.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = branch.AddComponent<MeshRenderer>();
        
        meshFilter.mesh = CreateCylinderMesh(start, end, startThickness, endThickness);
        meshRenderer.material = material;
        
        return branch;
    }
    
    static Mesh CreateCylinderMesh(Vector3 start, Vector3 end, float startRadius, float endRadius, int segments = 12)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        
        Vector3 direction = (end - start).normalized;
        float length = Vector3.Distance(start, end);
        
        // Find perpendicular vectors
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;
        if (perpendicular.magnitude < 0.001f)
            perpendicular = Vector3.Cross(direction, Vector3.forward).normalized;
        
        Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular).normalized;
        
        // Create vertices
        for (int i = 0; i <= 1; i++)
        {
            float t = i;
            Vector3 center = Vector3.Lerp(start, end, t);
            float radius = Mathf.Lerp(startRadius, endRadius, t);
            
            for (int j = 0; j <= segments; j++)
            {
                float angle = (j % segments) * Mathf.PI * 2 / segments;
                Vector3 offset = (Mathf.Cos(angle) * perpendicular + Mathf.Sin(angle) * perpendicular2) * radius;
                Vector3 vertex = center + offset;
                
                vertices.Add(vertex);
                normals.Add(offset.normalized);
                uvs.Add(new Vector2((float)j / segments, t));
            }
        }
        
        // Create side triangles
        for (int j = 0; j < segments; j++)
        {
            int bottom = j;
            int top = j + segments + 1;
            
            triangles.Add(bottom);
            triangles.Add(top);
            triangles.Add(bottom + 1);
            
            triangles.Add(bottom + 1);
            triangles.Add(top);
            triangles.Add(top + 1);
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.normals = normals.ToArray();
        mesh.RecalculateTangents();
        
        return mesh;
    }
}