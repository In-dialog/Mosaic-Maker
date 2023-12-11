using System;
using Unity.VectorGraphics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class SvgSaveData
{
    public string SvgString;
    public string Name;
    public float Depth;
    public float PosX;
    public float PosY;
    public float PosZ;

    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public SvgSaveData(ScannedObjectSvg scannedObjectSvg)
    {
        // if (scannedObjectSvg == null)
        // {
        //     Debug.LogWarning($"Could not save scanned object {scannedObjectSvg.transform.name}");
        //     return;
        // }
        
        this.SvgString = scannedObjectSvg.svg;

        Transform transform = scannedObjectSvg.transform;
        this.Name = transform.name;

        Vector3 position = transform.localPosition;
        this.PosX = position.x;
        this.PosY = position.y;
        this.PosZ = position.z;

        Quaternion rotation = transform.rotation;
        this.rotX = rotation.x;
        this.rotY = rotation.y;
        this.rotZ = rotation.z;
        this.rotW = rotation.w;

        this.Depth = scannedObjectSvg.DepthObject;
    }

    public ScannedObjectSvg InstantiateObjectFromSaveData(Transform parent)
    {
        GameObject savedPolygon = new GameObject(Name);
        if (parent != null)
        {
            savedPolygon.transform.SetParent(parent);
        }
        
        ScannedObjectSvg refComponent = savedPolygon.AddComponent<ScannedObjectSvg>();
        refComponent.GenerateObject(SvgString, Depth);

        refComponent.UpdateTransformLocalVertex(new Quaternion(rotX, rotY, rotZ, rotW), new Vector3(PosX, PosY, PosZ));

        return refComponent;
    }
}

public interface IScannedObject
{
    public Color MColor { get; set; }
    public Vector3 OriginalPosition { get; set; }
    public Vector3 TargetPosition { get; set; }
    public Quaternion TargetRotation { get; set; }
    public BoxCollider MyCollider { get; set; }
    public List<Vector2> OutlineVertices { get; set; }
    public float AreaTotal { get; set; }
    public float DepthObject { get; set; }
    public BinSvg MyBin { get; set; }
}

[Serializable]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ScannedObjectSvg : BaseSvgScript, IScannedObject
{
    public bool testing;
    public Mesh myMesh;
    public Color MColor { get; set; }
    public Vector3 OriginalPosition { get; set; }
    public Vector3 TargetPosition { get; set; }
    public Quaternion TargetRotation { get; set; }
    public BoxCollider MyCollider { get; set; }
    public List<Vector2> OutlineVertices { get; set; }
    public float AreaTotal { get; set; }
    public float DepthObject { get; set; }
    public BinSvg MyBin { get; set; }

    public GameObject MyClone { get; set; }

    public List<Vector2> ExpandedOutlineVertices { get; private set; }

    /// <summary>
    /// Initializes the ScannedObjectSvg object based on testing flag.
    /// </summary>
    public virtual void Awake()
    {
        if (testing)
            GenerateObject(svg, 0.0f);
    }

    public SvgSaveData GetSaveData()
    {
        SvgSaveData data = new SvgSaveData(this);
        return data;
    }

    /// <summary>
    /// Generates the object based on the given SVG string and depth.
    /// </summary>
    /// <param name="svgString">The SVG string to generate the object from.</param>
    /// <param name="depth">The depth to extrude the object.</param>
    public virtual void GenerateObject(string svgString, float depth)
    {
        InitializeSVG(svgString);
        DepthObject = depth;
        var geometries = GetGeometries();
        HandleGeometries(geometries, depth);

        if (OutlineVertices == null)
        {
            Debug.LogWarning($"Unable to construct outline of this svg object {this.transform.name} with this string {svg}");
            return;
        }

        if (this is BinSvg)
            AreaTotal = PolygonUtility.CalculateArea(OutlineVertices);
        else
        {
            UpdateRotation(Quaternion.LookRotation(-this.transform.forward, this.transform.up));
            AreaTotal = PolygonUtility.CalculateArea(ExpandedOutlineVertices);
        }
    }

    /// <summary>
    /// Initializes the SVG parameters.
    /// </summary>
    /// <param name="svgString">The SVG string.</param>
    private void InitializeSVG(string svgString)
    {
        this.svg = svgString;
        this.pixelsPerUnit = 1000;
        this.flipYAxis = false;
    }

    /// <summary>
    /// Handles the geometry creation and mesh generation of the object.
    /// </summary>
    /// <param name="geometries">The geometries to generate the mesh from.</param>
    /// <param name="depth">The depth to extrude the object.</param>
    private void HandleGeometries(List<VectorUtils.Geometry> geometries, float depth)
    {
        if (geometries.Count <= 0)
        {
            Destroy(this.gameObject);
            return;
        }

        GenerateMesh(geometries);
        HandleOutlineVertices();
        ExtrudeAndMaterializeMesh(depth);
        UpdateBoxCollider();
    }

    /// <summary>
    /// Generates the mesh of the object based on the geometries.
    /// </summary>
    /// <param name="geometries">The geometries to generate the mesh from.</param>
    private void GenerateMesh(List<VectorUtils.Geometry> geometries)
    {
        var meshFilter = GetComponent<MeshFilter>();
        myMesh = meshFilter.mesh;
        if (geometries.Count > 0)
        {
            MColor = geometries[0].Color;
            if (this is BinSvg)
                geometries[0].Color.a = 0.1f;

        }

        if (this is BinSvg)
        {
            geometries[0].Vertices =
                    MeshUtility.ScaleVertices(geometries[0].Vertices, Vector2.zero, ParametersManager.SizeMosaic.x);
        }

        VectorUtils.FillMesh(myMesh, geometries, pixelsPerUnit, flipYAxis);

    }

    /// <summary>
    /// Handles the outline vertices generation and centering of the mesh.
    /// </summary>
    private void HandleOutlineVertices()
    {

        List<Vector2> holes;
        (OutlineVertices, holes) = myMesh.Generate2DOutline();
        Vector2 centerMesh = PolyLabel.GetPolyLabel(OutlineVertices.ToArray());
        Vector3 offset;
        if (this is not BinSvg)
            offset = this.transform.position - (Vector3)centerMesh;
        else
        {
            offset = (Vector3)centerMesh - this.transform.position;
            holes = holes.Select(v => v - centerMesh).ToList();
            bool vertexOrderClockwiseHoles = MeshExtension.AreVerticesOrderedClockwise(holes.ToArray());
            // Debug.Log($"The vertex are in clockwiseOrder : {vertexOrderClockwiseHoles}");
            if (vertexOrderClockwiseHoles) holes.Reverse();
            ((BinSvg)this).holeOutline = holes;
        }

        MeshUtility.CenterMeshToVisualCenter(myMesh, centerMesh);
        OutlineVertices = OutlineVertices.Select(v => v - centerMesh).ToList();

        // handle vertex order
        bool vertexOrderClockwise = MeshExtension.AreVerticesOrderedClockwise(this.OutlineVertices.ToArray());
        // Debug.Log($"The vertex are in clockwiseOrder : {vertexOrderClockwise}");
        if (vertexOrderClockwise) this.OutlineVertices.Reverse();

        if (this is not BinSvg)
            ExpandedOutlineVertices = MeshUtility.ScaleVertices(OutlineVertices.ToArray(),
                    Vector2.zero, 1 + ParametersManager.GapTiles).ToList();
        else
            ExpandedOutlineVertices = null;
        this.transform.position = offset;
    }

    /// <summary>
    /// Extrudes the mesh to the specified depth and sets up the material.
    /// </summary>
    /// <param name="depth">The depth to extrude the mesh.</param>
    private void ExtrudeAndMaterializeMesh(float depth)
    {
        MeshUtility.ExtrudeMesh(myMesh, depth);
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials = new Material[] { new Material(Shader.Find("UI/Default")) };
    }

    /// <summary>
    /// Updates the BoxCollider of the object based on the mesh and transform.
    /// </summary>
    private void UpdateBoxCollider()
    {
        Vector3 BoundingBox = MeshUtility.CalculateBoundingBoxSize(myMesh, transform);
        // Get or add a BoxCollider to your object
        BoxCollider boxCollider;
        if (!gameObject.TryGetComponent<BoxCollider>(out boxCollider))
        {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        boxCollider.isTrigger = true;
        boxCollider.size = BoundingBox;

        Vector3 scaledCenter = Vector3.Scale(myMesh.bounds.center, transform.localScale);
        boxCollider.center = scaledCenter;

        MyCollider = boxCollider;
    }

    // Call this method with your mesh to save the data to a text file
    public string SaveMeshData(Mesh mesh)
    {
        string meshData = mesh.MeshToString();
        string filePath = Path.Combine(Application.persistentDataPath, $"MeshData_{gameObject.transform.parent.name}_{gameObject.name}.txt");

        File.WriteAllText(filePath, meshData);
        Debug.Log($"Mesh data saved to {filePath}");

        return filePath;
    }

    /// <summary>
    /// Converts a Vector3 to a Vector2 by ignoring the Y component.
    /// </summary>
    /// <param name="vertex">The Vector3 vertex to convert.</param>
    /// <returns>The converted Vector2 vertex.</returns>
    private Vector2 ToVector2(Vector3 vertex)
    {
        return new Vector2(vertex.x, vertex.y);
    }

    /// <summary>
    /// Updates the transform and stored vertices to a new rotation and position.
    /// </summary>
    /// <param name="desiredRotation">The desired rotation.</param>
    /// <param name="position">The desired position.</param>
    public void UpdateTransformVertex(Quaternion desiredRotation, Vector2 position)
    {
        // Calculate the rotation needed to go from the current rotation to the desired rotation
        Quaternion rotationDifference =
                desiredRotation * Quaternion.Inverse(this.transform.rotation);

        // Apply the rotation difference to the transform
        this.transform.rotation = desiredRotation;
        this.transform.position = position;

        // Apply the same rotation difference to the stored vertices
        OutlineVertices = PolygonUtility.RotateVertex(OutlineVertices, rotationDifference);
        ExpandedOutlineVertices =
                PolygonUtility.RotateVertex(ExpandedOutlineVertices, rotationDifference);
    }

    /// <summary>
    /// Updates the transform and stored vertices to a new rotation and position.
    /// </summary>
    /// <param name="desiredRotation">The desired rotation.</param>
    /// <param name="position">The desired position.</param>
    public void UpdateTransformVertex(Quaternion desiredRotation, Vector3 position)
    {
        // Calculate the rotation needed to go from the current rotation to the desired rotation
        Quaternion rotationDifference =
                desiredRotation * Quaternion.Inverse(this.transform.rotation);

        // Apply the rotation difference to the transform
        this.transform.rotation = desiredRotation;
        this.transform.position = position;

        // Apply the same rotation difference to the stored vertices
        OutlineVertices = PolygonUtility.RotateVertex(OutlineVertices, rotationDifference);
        ExpandedOutlineVertices =
                PolygonUtility.RotateVertex(ExpandedOutlineVertices, rotationDifference);
    }
    
    /// <summary>
    /// Updates the transform and stored vertices to a new rotation and position.
    /// </summary>
    /// <param name="desiredRotation">The desired rotation.</param>
    /// <param name="position">The desired position.</param>
    public void UpdateTransformLocalVertex(Quaternion desiredRotation, Vector3 position)
    {
        // Calculate the rotation needed to go from the current rotation to the desired rotation
        Quaternion rotationDifference =
                desiredRotation * Quaternion.Inverse(this.transform.rotation);

        // Apply the rotation difference to the transform
        this.transform.rotation = desiredRotation;
        this.transform.localPosition = position;

        // Apply the same rotation difference to the stored vertices
        OutlineVertices = PolygonUtility.RotateVertex(OutlineVertices, rotationDifference);
        ExpandedOutlineVertices =
                PolygonUtility.RotateVertex(ExpandedOutlineVertices, rotationDifference);
    }

    /// <summary>
    /// Updates the position of the transform.
    /// </summary>
    /// <param name="position">The desired position.</param>
    public void UpdatePosition(Vector2 position)
    {
        this.transform.position = position;
    }

    /// <summary>
    /// Updates the rotation of the transform and stored vertices.
    /// </summary>
    /// <param name="desiredRotation">The desired rotation.</param>
    public void UpdateRotation(Quaternion desiredRotation)
    {
        Quaternion rotationDifference =
                desiredRotation * Quaternion.Inverse(this.transform.rotation);
        this.transform.rotation = desiredRotation;

        OutlineVertices = PolygonUtility.RotateVertex(OutlineVertices, rotationDifference);
        ExpandedOutlineVertices =
                PolygonUtility.RotateVertex(ExpandedOutlineVertices, rotationDifference);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(this.transform.position, 0.001f);
        // VisualizeVertices(myMesh.vertices.ToList(), Color.white, Color.red, 0.001f);
        VisualizeVertices(OutlineVertices, Color.white, Color.red, 0.001f);

        VisualizeVertices(ExpandedOutlineVertices, Color.black, Color.green, 0.001f);
    }

    /// <summary>
    /// Visualizes 2D vertices by drawing lines and spheres in the editor.
    /// </summary>
    /// <param name="vertices">The vertices to visualize.</param>
    /// <param name="lineColor">The color of the lines.</param>
    /// <param name="sphereColor">The color of the spheres.</param>
    /// <param name="sphereSize">The size of the spheres.</param>
    protected void VisualizeVertices(List<Vector2> vertices, Color lineColor, Color sphereColor,
            float sphereSize)
    {
        DrawVertices2D(vertices, lineColor, sphereColor, sphereSize);
    }

    /// <summary>
    /// Visualizes 3D vertices by drawing lines and spheres in the editor.
    /// </summary>
    /// <param name="vertices">The vertices to visualize.</param>
    /// <param name="lineColor">The color of the lines.</param>
    /// <param name="sphereColor">The color of the spheres.</param>
    /// <param name="sphereSize">The size of the spheres.</param>
    private void VisualizeVertices(List<Vector3> vertices, Color lineColor, Color sphereColor,
            float sphereSize)
    {
        DrawVertices3D(vertices, lineColor, sphereColor, sphereSize);
    }

    private void DrawVertices2D(List<Vector2> vertices, Color lineColor, Color sphereColor,
            float sphereSize)
    {
        if (vertices == null || vertices.Count <= 0)
            return;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 start = (Vector3)vertices[i] + transform.position;
            Vector3 end = (Vector3)vertices[(i + 1) % vertices.Count] + transform.position;
            DrawLineAndSphere(start, end, lineColor, sphereColor, sphereSize, i, vertices.Count);
        }
    }

    private void DrawVertices3D(List<Vector3> vertices, Color lineColor, Color sphereColor,
            float sphereSize)
    {
        if (vertices == null || vertices.Count <= 0)
            return;

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 start = vertices[i] + transform.position;
            Vector3 end = vertices[(i + 1) % vertices.Count] + transform.position;
            DrawLineAndSphere(start, end, lineColor, sphereColor, sphereSize, i, vertices.Count);
        }
    }


    /// <summary>
    /// Draws lines and spheres to visualize vertices in the editor.
    /// </summary>
    /// <param name="start">The starting point of the line.</param>
    /// <param name="end">The ending point of the line.</param>
    /// <param name="lineColor">The color of the line.</param>
    /// <param name="sphereColor">The color of the sphere.</param>
    /// <param name="sphereSize">The size of the sphere.</param>
    /// <param name="index">The index of the current vertex.</param>
    /// <param name="count">The total count of vertices.</param>
    private void DrawLineAndSphere(Vector3 start, Vector3 end, Color lineColor, Color sphereColor,
            float sphereSize, int index, int count)
    {
        Gizmos.color = lineColor;
        Gizmos.DrawLine(start, end);
        // Calculate the interpolated color based on the index
        float t = (float)index / (count - 1);
        Color interpolatedColor = Color.Lerp(Color.white, sphereColor, t);

        Gizmos.color = interpolatedColor;
        Gizmos.DrawSphere(start, sphereSize);
    }
}
