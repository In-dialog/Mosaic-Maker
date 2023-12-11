using UnityEngine;
using System.Collections.Generic;

public class Edge
{
    public int v1, v2;
    
    public Edge(int a, int b)
    {
        v1 = Mathf.Min(a, b);
        v2 = Mathf.Max(a, b);
    }
    
    public bool Contains(int vertexIndex)
    {
        return v1 == vertexIndex || v2 == vertexIndex;
    }

    public int OtherVertex(int vertexIndex)
    {
        return vertexIndex == v1 ? v2 : v1;
    }
    
    public override bool Equals(object obj)
    {
        var edge = obj as Edge;
        return edge != null && v1 == edge.v1 && v2 == edge.v2;
    }

    public override int GetHashCode()
    {
        return v1.GetHashCode() ^ v2.GetHashCode();
    }
    
    public bool AreSame(Edge other)
    {
        return (v1 == other.v1 && v2 == other.v2) || (v1 == other.v2 && v2 == other.v1);
    }
}

public static class MeshUtility
{
    
    public static Vector2[] ScaleVertices(Vector2[] vertices, Vector2 scaleOrigin, float scaleFactor)
    {
        Vector2[] scaledVertices = new Vector2[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            // Move the vertex position relative to the scale origin
            Vector2 relativePosition = vertices[i] - scaleOrigin;

            // Apply the scaling
            relativePosition *= scaleFactor;

            // Move the vertex back from the scale origin and add to the scaled list
            scaledVertices[i] = (relativePosition + scaleOrigin);
        }
        
        return scaledVertices;
    }
    
    public static void CenterMeshToVisualCenter( Mesh mesh, Vector3 visualCenter)
    {
        // 1. Compute the offset
        Vector3 offset = -visualCenter;

        // 2. Offset mesh vertices
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] += offset;
        }
        mesh.vertices = vertices;
        
        // Ensure mesh updates appropriately
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
    

    private static Color[] AssignColors(Color[] oldColors, int vertexCount)
    {
        Color[] newColors = new Color[vertexCount * 2];

        if (oldColors.Length == 0)
        {
            oldColors = new Color[vertexCount];
            for (int i = 0; i < oldColors.Length; i++) oldColors[i] = Color.white;
        }

        oldColors.CopyTo(newColors, 0);
        for (int i = vertexCount; i < newColors.Length; i++)
        {
            newColors[i] = oldColors[0];
        }

        return newColors;
    }
    
    public static Vector3 CalculateBoundingBoxSize(Mesh mesh, Transform transform)
    {
        Vector3 meshSize = mesh.bounds.size;
        Vector3 scale = transform.localScale;
        Vector3 size = new Vector3(meshSize.x * scale.x, meshSize.y * scale.y, meshSize.z * scale.z);
        return size;
    }

    public static void ExtrudeMesh(Mesh mesh, float depth)
    {
        if (mesh == null)
        {
            Debug.LogError("Mesh is null. Aborting.");
            return;
        }

        Vector3[] oldVertices = mesh.vertices;
        Vector3[] newVertices = ExtrudeBackFace(oldVertices, depth);

        int[] oldTriangles = mesh.triangles;
        List<int> sideTriangles = ExtrudeSides(oldTriangles, oldVertices.Length);
        
        Color[] oldColors = mesh.colors;
        Color[] newColors = AssignColors(oldColors, oldVertices.Length);

        int[] mergedTriangles = new int[oldTriangles.Length * 2 + sideTriangles.Count];
        oldTriangles.CopyTo(mergedTriangles, 0);
        for (int i = 0; i < oldTriangles.Length; i += 3)
        {
            int offset = oldVertices.Length;
            mergedTriangles[oldTriangles.Length + i] = oldTriangles[i + 2] + offset;
            mergedTriangles[oldTriangles.Length + i + 1] = oldTriangles[i + 1] + offset;
            mergedTriangles[oldTriangles.Length + i + 2] = oldTriangles[i] + offset;
        }
        sideTriangles.CopyTo(mergedTriangles, oldTriangles.Length * 2);

        mesh.vertices = newVertices;
        mesh.triangles = mergedTriangles;
        mesh.colors = newColors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
    
    private static Vector3[] ExtrudeBackFace(Vector3[] oldVertices, float depth)
    {
        Vector3[] newVertices = new Vector3[oldVertices.Length * 2];

        for (int i = 0; i < oldVertices.Length; i++)
        {
            newVertices[i] = oldVertices[i];
            newVertices[i + oldVertices.Length] = oldVertices[i] - new Vector3(0, 0, depth);
        }

        return newVertices;
    }

    private static List<int> ExtrudeSides(int[] oldTriangles, int vertexCount)
    {
        Dictionary<Edge, int> edgeCount = new Dictionary<Edge, int>();

        for (int i = 0; i < oldTriangles.Length; i += 3)
        {
            Edge edge1 = new Edge(oldTriangles[i], oldTriangles[i + 1]);
            Edge edge2 = new Edge(oldTriangles[i + 1], oldTriangles[i + 2]);
            Edge edge3 = new Edge(oldTriangles[i + 2], oldTriangles[i]);

            if (edgeCount.ContainsKey(edge1)) edgeCount[edge1]++;
            else edgeCount[edge1] = 1;

            if (edgeCount.ContainsKey(edge2)) edgeCount[edge2]++;
            else edgeCount[edge2] = 1;

            if (edgeCount.ContainsKey(edge3)) edgeCount[edge3]++;
            else edgeCount[edge3] = 1;
        }

        List<int> sideTriangles = new List<int>();
        foreach (var edge in edgeCount.Keys)
        {
            if (edgeCount[edge] == 1)
            {
                int offset = vertexCount;

                sideTriangles.Add(edge.v1);
                sideTriangles.Add(edge.v2);
                sideTriangles.Add(edge.v1 + offset);

                sideTriangles.Add(edge.v2);
                sideTriangles.Add(edge.v2 + offset);
                sideTriangles.Add(edge.v1 + offset);
            }
        }

        return sideTriangles;
    }

}



