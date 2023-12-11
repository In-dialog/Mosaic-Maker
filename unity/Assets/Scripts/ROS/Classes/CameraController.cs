using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RosMessageTypes.PointCloudProcessing;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;
using Random = System.Random;

[Serializable]
public class CameraController
{
    private ROSConnection _ros;

    [HideInInspector]
    public ScannedObjectSvg objectDetected;


    private RobotArmTaskController _robotArmTaskController;
    private bool _objectDetected = false;
    private bool _objectConfirmed = false;

    private static Random _random = new Random();

    private UiManager _instanceUiManager;

    private int nrOfParts = 0;
    public GameObject PlaceHolderScanned;

    public void Initialize(RobotArmTaskController robotArmTaskController, ROSConnection ros)
    {
        _robotArmTaskController = robotArmTaskController;
        _ros = ros;
        _instanceUiManager = UiManager.Instance;
    }

    public void ObjectItsCorrect()
    {
        _objectConfirmed = true;
    }

    public void ObjectItsIncorrect()
    {
        _objectConfirmed = false;
    }

    public IEnumerator DetectObject()
    {
        objectDetected = null;
        _objectDetected = false;
        while (_objectDetected == false)
        {
            GetCameraData();
            yield return new WaitUntil(() => objectDetected != null);

            if (objectDetected != null)
            {
                _instanceUiManager.SetUp2dVisualization(objectDetected);
                Initialize3DVisualization(objectDetected);
                objectDetected.name = (++nrOfParts).ToString();
            }
            yield return null;
        }
    }

    private void GetCameraData()
    {
        if (_ros.HasConnectionThread)
        {
            ScanningRequest request = new ScanningRequest();
            _ros.SendServiceMessage<ScanningResponse>(RobotManager.mCameraServiceName, request,
                    DecodeCameraData);
        }
        else
        {
            objectDetected = GenerateNewSvgPolygon();
            _objectDetected = true;
        }
    }
    
    private ScannedObjectSvg GenerateNewSvgPolygon()
    {
        GameObject generatedPolygon = new GameObject("Polygon_test");
        ScannedObjectSvg refComponent = generatedPolygon.AddComponent<ScannedObjectSvg>();
        refComponent.GenerateObject(GenerateRectanglePolygonSvg(20, 20, Color.red, 2), ParametersManager.DepthTiles);
        return refComponent;
    }

    private void DecodeCameraData(ScanningResponse response)
    {
        if (response.success)
        {
            InitializeDetectedObject(response);
            _objectDetected = true;
        }
        else
        {
            _objectDetected = false;
            Debug.Log("Error on scanning, msg = " + response.message);
        }
    }

    private void InitializeDetectedObject(ScanningResponse response)
    {
        GameObject objectScanned = new GameObject("Object Scanned");
        ScannedObjectSvg scannedObjectSvg = objectScanned.AddComponent<ScannedObjectSvg>();

        Color32 color = ExtractRGB(response.message);
        // Debug.Log($"the color [{color}]");

        string svgString = GenerateRectanglePolygonSvg(20, 20, color, 4);
        scannedObjectSvg.GenerateObject(svgString, ParametersManager.DepthTiles);
        
        objectDetected = scannedObjectSvg;
    }
    
    

    private void Initialize3DVisualization(ScannedObjectSvg newObject)
    {
        GameObject tmp3DObject = Object.Instantiate(newObject.gameObject);
        tmp3DObject.transform.SetParent(PlaceHolderScanned.transform);
        tmp3DObject.transform.localPosition = new Vector3(newObject.transform.localPosition.x,
                newObject.transform.localPosition.y, newObject.transform.localPosition.z);
        tmp3DObject.transform.localRotation = Quaternion.Euler(newObject.transform.eulerAngles.x, newObject.transform.eulerAngles.y, newObject.transform.eulerAngles.z);
        tmp3DObject.layer = 6;
        newObject.MyClone = tmp3DObject;
    }

    public Color32 ExtractRGB(string input)
    {
        // Regular expression to match numbers inside square brackets
        string pattern = @"\[(\d+),(\d+),(\d+)\]";
        Match match = Regex.Match(input, pattern);

        if (match.Success)
        {
            // Extracting the numbers and parsing them into bytes
            byte b = byte.Parse(match.Groups[1].Value);
            byte g = byte.Parse(match.Groups[2].Value);
            byte r = byte.Parse(match.Groups[3].Value);

            // Assuming full opacity (alpha = 255)
            return new Color32(r, g, b, 255);
        }
        else
        {
            throw new ArgumentException("No valid color data found in the string.");
        }
    }



    public static string GenerateRectanglePolygonSvg(int width, int height, Color32 color, int subdivisions)
    {
        var svgBuilder = new StringBuilder();
        svgBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
        svgBuilder.Append(
                $"<svg baseProfile=\"tiny\" height=\"{height}\" version=\"1.2\" width=\"{width}\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:ev=\"http://www.w3.org/2001/xml-events\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><defs />");

        List<(float, float)> points = new List<(float, float)>
        {
                (0, 0), // Top left
                (width, 0), // Top right
                (width, height), // Bottom right
                (0, height) // Bottom left
        };

        for (int s = 0; s < subdivisions; s++)
        {
            List<(float, float)> newPoints = new List<(float, float)>();
            for (int i = 0; i < points.Count; i++)
            {
                var p1 = points[i];
                var p2 = points[(i + 1) % points.Count];
                var midPoint = ((p1.Item1 + p2.Item1) / 2.0f, (p1.Item2 + p2.Item2) / 2.0f);
            
                newPoints.Add(p1);
                newPoints.Add(midPoint);
            }
            points = newPoints;
        }

        // Append the polygon to the SVG string
        svgBuilder.AppendFormat(
                "<polygon fill=\"rgb({0},{1},{2})\" points=\"{3}\" stroke=\"none\" />",
                color.r, color.g, color.b,
                string.Join(" ", points.Select(p => $"{p.Item1},{p.Item2}")));

        // End SVG string
        svgBuilder.Append("</svg>");

        return svgBuilder.ToString();
    }



    private static void AddIntermediatePoints(List<(int, int)> points, (int, int) start, (int, int) end, int subdivisions)
    {
        int totalPoints = subdivisions + 1; // Including start and end
        for (int i = 0; i < totalPoints; i++)
        {
            float t = (float)i / totalPoints;
            int x = (int)(start.Item1 + t * (end.Item1 - start.Item1));
            int y = (int)(start.Item2 + t * (end.Item2 - start.Item2));
            points.Add((x, y));
        }
    }


    
    // public static string GenerateRectanglePolygonSvg(int width, int height, Color32 color)
    // {
    //     var svgBuilder = new StringBuilder();
    //
    //     // Start SVG string
    //     svgBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
    //     svgBuilder.Append(
    //             $"<svg baseProfile=\"tiny\" height=\"{height}\" version=\"1.2\" width=\"{width}\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:ev=\"http://www.w3.org/2001/xml-events\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><defs />");
    //
    //     // Define corner points
    //     var topLeft = (0, 0);
    //     var topRight = (width, 0);
    //     var bottomRight = (width, height);
    //     var bottomLeft = (0, height);
    //
    //     // Calculate midpoints
    //     var midTop = (width / 2, 0);
    //     var midRight = (width, height / 2);
    //     var midBottom = (width / 2, height);
    //     var midLeft = (0, height / 2);
    //
    //     // Define points in order to form a rectangular shape
    //     var points = new List<(int, int)> { topLeft, midTop, topRight, midRight, bottomRight, midBottom, bottomLeft, midLeft };
    //
    //     // Append the polygon to the SVG string
    //     svgBuilder.AppendFormat(
    //             "<polygon fill=\"rgb({0},{1},{2})\" points=\"{3}\" stroke=\"none\" />",
    //             color.r, color.g, color.b,
    //             string.Join(" ", points.Select(p => $"{p.Item1},{p.Item2}")));
    //
    //     // End SVG string
    //     svgBuilder.Append("</svg>");
    //
    //     return svgBuilder.ToString();
    // }


    public static string GenerateConvexPolygonSvg(int numberOfPoints)
    {
        var svgBuilder = new StringBuilder();

        // Start SVG string
        svgBuilder.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
        svgBuilder.Append(
                "<svg baseProfile=\"tiny\" height=\"20\" version=\"1.2\" width=\"20\" xmlns=\"http://www.w3.org/2000/svg\" xmlns:ev=\"http://www.w3.org/2001/xml-events\" xmlns:xlink=\"http://www.w3.org/1999/xlink\"><defs />");

        List<(int, int)> points = new List<(int, int)>();
        for (int i = 0; i < numberOfPoints; i++)
        {
            points.Add((_random.Next(0, 50), _random.Next(0, 50)));
        }

        points = ComputeConvexHull(points).ToList();

        // Append the polygon to the SVG string
        svgBuilder.AppendFormat(
                "<polygon fill=\"rgb({0},{1},{2})\" points=\"{3}\" stroke=\"none\" />",
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
            while (lower.Count >= 2 &&
                   Cross(lower[lower.Count - 2], lower[lower.Count - 1], point) <= 0)
            {
                lower.RemoveAt(lower.Count - 1);
            }

            lower.Add(point);
        }

        var upper = new List<(int, int)>();
        for (int i = sorted.Count - 1; i >= 0; i--)
        {
            while (upper.Count >= 2 &&
                   Cross(upper[upper.Count - 2], upper[upper.Count - 1], sorted[i]) <= 0)
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
        return (a.Item1 - o.Item1) * (b.Item2 - o.Item2) -
               (b.Item1 - o.Item1) * (a.Item2 - o.Item2);
    }

}
