using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class PartInfoDisplay : MonoBehaviour
{
    public MeshFilter meshContainer;
    public static PartInfoDisplay Instance { get; private set; }
    [SerializeField] private TextMeshPro infoText;


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


    public void UpdatePart(ScannedObject scannedObject)
    {
        print(scannedObject.Transform.name);
        meshContainer.mesh = scannedObject.Transform.GetComponent<MeshFilter>().mesh;
        meshContainer.transform.localRotation = Quaternion.Euler(Random.Range(-180, 180), -90f, 90f);
        meshContainer.transform.localScale = Vector3.one * 5;

        SetText(scannedObject);
        StartCoroutine(UpdatePositionCoroutine(scannedObject));
    }
    public void StopUpdate()
    {
        StopAllCoroutines();
    }
    public void SetText(ScannedObject scannedObject)
    {
        Bounds bounds = GetBoundingBox(scannedObject.Transform.GetComponent<MeshFilter>().mesh, scannedObject.Transform);
        string boundsString = FormatExtents(bounds);
        string color = FormatColor(scannedObject.Color);
        string formattedPosition = FormatPostion(scannedObject.Transform.position);

        FormatText(scannedObject.Transform.name, color, formattedPosition, boundsString);
    }
    public IEnumerator UpdatePositionCoroutine(ScannedObject scannedObject)
    {
        while (true)
        {
            // Get the position of the scannedObject and format it.
            Vector3 position = scannedObject.Transform.position;
            string formattedPosition = FormatPostion(position);

            // Update the third line of the TextMeshProUGUI component with the formatted position.
            UpdatePosition(formattedPosition);

            // Wait for a short time before updating again.
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void UpdatePosition(string newValue)
    {
        // Split the existing text into lines based on the newline marker.
        string[] lines = infoText.text.Split('\n');

        // Check if there are at least 3 lines.
        if (lines.Length >= 3)
        {
            // Update the third line with the new value.
            lines[2] = newValue;

            // Join the lines back together and update the text in the TextMeshProUGUI component.
            infoText.text = string.Join("\n", lines);
        }
    }

    /// name - collor - postion - AABB size
    public void FormatText(string param1, string param2, string param3, string param4)
    {
        string newText = string.Format("{0}\n{1}\n{2}\n{3}", param1, param2, param3, param4);
        //print(newText);
        infoText.text = newText;
    }

    private Bounds GetBoundingBox(Mesh mesh, Transform scannedTransform)
    {
        Vector3[] vertices = mesh.vertices;

        Vector3 min = scannedTransform.TransformPoint(vertices[0]);
        Vector3 max = scannedTransform.TransformPoint(vertices[0]);

        foreach (Vector3 vertex in vertices)
        {
            Vector3 worldVertex = scannedTransform.TransformPoint(vertex);
            min = Vector3.Min(min, worldVertex);
            max = Vector3.Max(max, worldVertex);
        }

        Bounds bounds = new Bounds();
        bounds.SetMinMax(min, max);

        return bounds;
    }


    public string FormatExtents(Bounds bounds)
    {
        Vector3 extents = bounds.extents * 10;
        int decimalPlaces = 2;

        float x = Mathf.Round(extents.x * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        float y = Mathf.Round(extents.y * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        return string.Format("{0}, {1}", x, y);
    }

    public string FormatColor(Color32 color)
    {
        int decimalPlaces = 2;

        float r = Mathf.Round(color.r * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        float g = Mathf.Round(color.g * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        float b = Mathf.Round(color.b * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);

        return string.Format("{0}, {1}, {2}", r, g, b);
    }

    public string FormatPostion(Vector3 position)
    {
        int decimalPlaces = 2;

        float x = Mathf.Round((position.x * 10) * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        float y = Mathf.Round((position.y * 10) * Mathf.Pow(10, decimalPlaces)) / Mathf.Pow(10, decimalPlaces);
        return string.Format("{0}, {1}", x, y);
    }
}



