using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ManegeTetureUV : MonoBehaviour
{
    public bool run = false;
    public Texture2D mainTexture;

    public static ManegeTetureUV Instance { get; private set; }


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (run)
        {
            SetMeshCenter();
            run = false;
        }
#endif
    }

    void SetMeshCenter()
    {
        // Iterate over each child object
        foreach (Transform child in transform)
        {
            // Get MeshFilter component
            MeshFilter meshFilter = child.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                // Get Mesh
                Mesh mesh = meshFilter.sharedMesh;

                // Calculate center
                Vector3 localCenter = CalculateMeshCenter(mesh, child);
                //Vector3 center = child.TransformPoint(localCenter);
                Vector3 center = localCenter;

                //center = TruncateVector3(center, 1);
                Vector2 normalized_center = new Vector2(center.x, center.y) + new Vector2(0.5f, 0.5f);

                // Get MeshRenderer to access the material and the shader
                MeshRenderer meshRenderer = child.GetComponent<MeshRenderer>();

                if (meshRenderer != null)
                {
                    // Set shader property
                    meshRenderer.material.SetTexture("_Texture", mainTexture);
                    meshRenderer.material.SetVector("_UV_position", normalized_center);
                    //if (child.name == "Plane_fracturepart301")
                    //if()
                    print(normalized_center + "  nameOf object:" + child.name);
                }
            }
        }
    }

    Vector3 CalculateMeshCenter(Mesh mesh, Transform objectTransform)
    {
        Vector3 center = Vector3.zero;

        // Iterate over each vertex, transform it to world space, and sum up their positions
        for (int i = 0; i < mesh.vertexCount; i++)
        {
            center += objectTransform.TransformPoint(mesh.vertices[i]);
        }

        // Divide by the number of vertices to get the center
        center /= mesh.vertexCount;

        return center;
    }
    public Vector3 TruncateVector3(Vector3 input, int decimals)
    {
        float multiplier = Mathf.Pow(10.0f, decimals);  // compute the multiplier based on the number of decimals

        // Truncate each component to the specified number of decimal places
        return new Vector3(
            Mathf.Round(input.x * multiplier) / multiplier,
            Mathf.Round(input.y * multiplier) / multiplier,
            Mathf.Round(input.z * multiplier) / multiplier
        );
    }
}
