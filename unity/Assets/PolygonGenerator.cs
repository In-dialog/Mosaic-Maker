using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System;
using UnityEngine.Serialization;
using Random = System.Random;

public class PolygonGenerator : MonoBehaviour
{
    
    private static Random _random = new Random();
    private MosaicNesting _mosaicNesting;
    private GameObject displayPolygon;
    public int numberOfPolygonToGenerate = 10;

    private Transform placeHolderBig;
    // Start is called before the first frame update
    void Start()
    {
        _mosaicNesting = FindObjectOfType<MosaicNesting>();
        
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Alpha0))
    //         StartCoroutine(TestingNestingAlgorithm());
    // }

    IEnumerator TestingNestingAlgorithm()
    {
        for (int i = 0; i < numberOfPolygonToGenerate; i++)
        {
            ScannedObjectSvg newObject = GenerateNewSvgPolygon(i);
            // yield return null;
            yield return _mosaicNesting.PlaceNewObject(newObject);
            Destroy(displayPolygon);
        }
    }

    private ScannedObjectSvg GenerateNewSvgPolygon(int index)
    {
        GameObject generatedPolygon = new GameObject("Polygon_" + index);
        ScannedObjectSvg refComponent = generatedPolygon.AddComponent<ScannedObjectSvg>();
        refComponent.pixelsPerUnit = 1000;
        refComponent.GenerateObject(GenerateConvexPolygonSvg(_random.Next(10, 20)), 0.03f);
        generatedPolygon.transform.position = new Vector3(-0.06f, 0.026f, 0f);

        GameObject scaledForPreview = Instantiate(generatedPolygon);
        scaledForPreview.transform.localScale = new Vector3(10, 10, 1);
        scaledForPreview.transform.position = placeHolderBig.position;
        displayPolygon = scaledForPreview;
        return refComponent;
    }
    
    public static string GenerateConcavePolygonSvg(int numberOfPoints)
    {
        var svgBuilder = new StringBuilder();

        // Start SVG string
        svgBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
        svgBuilder.Append("<svg baseProfile=\"tiny\" height=\"49\" version=\"1.2\" width=\"49\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:ev=\"http://www.w3.org/2001/xml-events\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><defs />");

        List<(int, int)> points = new List<(int, int)>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            points.Add((_random.Next(0, 50), _random.Next(0, 50)));
        }

        // Perturb some points inward to ensure concavity
        for (int i = 0; i < points.Count; i++)
        {
            if (_random.NextDouble() < 0.5) // 50% chance to perturb a point
            {
                var centerX = 24; // Center of bounding box
                var centerY = 24; // Center of bounding box

                var directionX = centerX - points[i].Item1;
                var directionY = centerY - points[i].Item2;

                var perturbAmount = _random.Next(1, 6); // Random perturb amount between 1 and 5

                var newX = points[i].Item1 + (int)(directionX * 0.1 * perturbAmount);
                var newY = points[i].Item2 + (int)(directionY * 0.1 * perturbAmount);

                points[i] = (newX, newY);
            }
        }

        // Append the polygon to the SVG string
        svgBuilder.AppendFormat("<polygon fill=\"rgb({0},{1},{2})\" points=\"{3}\" stroke=\"none\" />",
                _random.Next(0, 256), _random.Next(0, 256), _random.Next(0, 256), 
                string.Join(" ", points.Select(p => $"{p.Item1},{p.Item2}")));

        // End SVG string
        svgBuilder.Append("</svg>");

        return svgBuilder.ToString();
    }
    
        public static string GenerateConvexPolygonSvg(int numberOfPoints)
    {
        var svgBuilder = new StringBuilder();

        // Start SVG string
        svgBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
        svgBuilder.Append("<svg baseProfile=\"tiny\" height=\"49\" version=\"1.2\" width=\"49\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:ev=\"http://www.w3.org/2001/xml-events\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><defs />");

        List<(int, int)> points = new List<(int, int)>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            points.Add((_random.Next(0, 50), _random.Next(0, 50)));
        }

        points = ComputeConvexHull(points).ToList();

        // Append the polygon to the SVG string
        svgBuilder.AppendFormat("<polygon fill=\"rgb({0},{1},{2})\" points=\"{3}\" stroke=\"none\" />",
            _random.Next(0, 256), _random.Next(0, 256), _random.Next(0, 256), 
            string.Join(" ", points.Select(p => $"{p.Item1},{p.Item2}")));

        // End SVG string
        svgBuilder.Append("</svg>");

        return svgBuilder.ToString();
    }

    // Compute the convex hull using the Graham's scan algorithm
    private static IEnumerable<(int, int)> ComputeConvexHull(List<(int, int)> points)
    {
        var sorted = points.OrderBy(p => p.Item1).ThenBy(p => p.Item2).ToList();
        var lower = new List<(int, int)>();
        foreach (var point in sorted)
        {
            while (lower.Count >= 2 && Cross(lower[lower.Count - 2], lower[lower.Count - 1], point) <= 0)
            {
                lower.RemoveAt(lower.Count - 1);
            }
            lower.Add(point);
        }

        var upper = new List<(int, int)>();
        for (int i = sorted.Count - 1; i >= 0; i--)
        {
            while (upper.Count >= 2 && Cross(upper[upper.Count - 2], upper[upper.Count - 1], sorted[i]) <= 0)
            {
                upper.RemoveAt(upper.Count - 1);
            }
            upper.Add(sorted[i]);
        }

        upper.RemoveAt(upper.Count - 1);
        lower.RemoveAt(lower.Count - 1);
        return lower.Concat(upper);
    }

    private static int Cross((int, int) o, (int, int) a, (int, int) b)
    {
        return (a.Item1 - o.Item1) * (b.Item2 - o.Item2) - (b.Item1 - o.Item1) * (a.Item2 - o.Item2);
    }

}
