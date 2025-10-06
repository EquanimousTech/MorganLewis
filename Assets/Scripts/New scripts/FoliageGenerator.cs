using UnityEngine;

public class FoliageGenerator : MonoBehaviour
{
    public static GameObject CreateFoliageCluster(Color color, float size)
    {
        GameObject foliage = new GameObject("FoliageCluster");
        
        // Create multiple sphere shapes for foliage
        int sphereCount = Random.Range(3, 6);
        
        for (int i = 0; i < sphereCount; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.parent = foliage.transform;
            
            // Random position within cluster
            sphere.transform.localPosition = Random.insideUnitSphere * size * 0.5f;
            sphere.transform.localScale = Vector3.one * Random.Range(size * 0.8f, size * 1.2f);
            
            // Apply color with slight variation
            Renderer renderer = sphere.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Add slight color variation
            float h, s, v;
            Color.RGBToHSV(color, out h, out s, out v);
            h += Random.Range(-0.05f, 0.05f);
            s += Random.Range(-0.1f, 0.1f);
            v += Random.Range(-0.1f, 0.1f);
            
            mat.color = Color.HSVToRGB(h, s, v);
            mat.SetFloat("_Smoothness", 0.2f);
            renderer.material = mat;
            
            // Remove collider for performance
            Destroy(sphere.GetComponent<SphereCollider>());
        }
        
        return foliage;
    }
}