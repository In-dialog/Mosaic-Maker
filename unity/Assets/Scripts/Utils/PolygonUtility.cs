using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Collections;


public static class PolygonUtility
{

    public static List<Vector2> RotateVertex(List<Vector2> vertices, Quaternion rotation)
    {
        List<Vector2> transformedVertices = new List<Vector2>();

        foreach (Vector2 vertex in vertices)
        {
            // Apply rotation directly to the local vertex
            Vector2 rotatedVertex = rotation * vertex;

            transformedVertices.Add(rotatedVertex);
        }

        return transformedVertices;
    }
    
    public static NativeArray<Vector2> RotateVertex(NativeArray<Vector2> vertices, Quaternion rotation)
    {
        NativeArray<Vector2> transformedVertices = new NativeArray<Vector2>();

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector2 rotatedVertex = rotation * vertices[i];
            transformedVertices[i] = rotatedVertex;
        }

        return transformedVertices;
    }
    

    public static List<Vector2> CheckAndFixVertexOrder(List<Vector2> vertices)
    {
        // Calculate the centroid
        Vector2 centroid = CalculateCentroid(vertices);
        
        // Calculate the signed area of the shape
        float signedArea = CalculateSignedArea(vertices, centroid);
        
        // Check the winding direction
        if (signedArea > 0)
        {
            // Vertices are in clockwise order, reverse the list
            vertices.Reverse();
        }

        return vertices;
    }

    private static float CalculateSignedArea(List<Vector2> vertices, Vector2 centroid)
    {
        float signedArea = 0;
        int vertexCount = vertices.Count;

        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 currentVertex = vertices[i];
            Vector2 nextVertex = vertices[(i + 1) % vertexCount];

            float x0 = currentVertex.x - centroid.x;
            float y0 = currentVertex.y - centroid.y;
            float x1 = nextVertex.x - centroid.x;
            float y1 = nextVertex.y - centroid.y;

            signedArea += (x0 * y1 - x1 * y0);
        }

        return signedArea / 2;
    }

    private static Vector2 CalculateCentroid(List<Vector2> vertices)
    {
        Vector2 centroid = Vector2.zero;
        foreach (Vector2 vertex in vertices)
        {
            centroid += vertex;
        }

        centroid /= vertices.Count;
        return centroid;
    }

    public static bool IsObjectInsideBinWithHole(List<Vector2> objectVertices, List<Vector2> binOutlineVertices, List<Vector2> holeVertices)
    {
        foreach (Vector2 vertex in objectVertices)
        {
            // Check if the vertex is inside the bin polygon (outer boundary)
            if (!IsPointInPolygon(binOutlineVertices, vertex))
            {
                return false; // Vertex is outside the bin's outer boundary
            }

            // Check if the vertex is outside the hole (inner boundary)
            if (IsPointInPolygon(holeVertices, vertex))
            {
                return false; // Vertex is inside the hole
            }

            // Check if the vertex is near any vertex of the bin or hole
            if (IsPointNearVertex(binOutlineVertices, vertex) || IsPointNearVertex(holeVertices, vertex))
            {
                return false; // Vertex is too close to a vertex of the bin or hole
            }

            // Check if the vertex is on any edge of the bin
            for (int k = 0; k < binOutlineVertices.Count; k++)
            {
                if (IsPointOnLineSegment(binOutlineVertices[k], binOutlineVertices[(k + 1) % binOutlineVertices.Count], vertex))
                {
                    return false; // Vertex is on the edge of the bin
                }
            }

            // Check if the vertex is on any edge of the hole
            for (int k = 0; k < holeVertices.Count; k++)
            {
                if (IsPointOnLineSegment(holeVertices[k], holeVertices[(k + 1) % holeVertices.Count], vertex))
                {
                    return false; // Vertex is on the edge of the hole
                }
            }
        }

        // If none of the vertices violate the conditions, then the object is inside the bin and outside the hole
        return true;
    }
    

    public static bool IsObjectInsideBin(List<Vector2> objectVertices, List<Vector2> binVertices)
    {
        foreach (Vector2 vertex in objectVertices)
        {
            // Check if the vertex is inside the bin polygon
            if (!IsPointInPolygon(binVertices, vertex))
            {
                return false; // Vertex is outside the bin
            }

            // Check if the vertex is near any vertex of the bin
            if (IsPointNearVertex(binVertices, vertex))
            {
                return false; // Vertex is too close to a vertex of the bin
            }

            // Check if the vertex is on any edge of the bin
            for (int k = 0; k < binVertices.Count; k++)
            {
                if (IsPointOnLineSegment(binVertices[k], binVertices[(k + 1) % binVertices.Count],
                            vertex))
                {
                    return false; // Vertex is on the edge of the bin
                }
            }
        }

        // If none of the vertices violate the conditions, then the object is inside the bin
        return true;
    }

    public static bool IsPointNearVertex(List<Vector2> polygon, Vector2 point)
    {
        FixedPoint
                epsilon = FixedPoint.FromMeters(0.001f); // Define a small epsilon value in meters

        foreach (var vertex in polygon)
        {
            FixedPoint distanceSquared =
                    (FixedPoint.FromMeters(vertex.x) - FixedPoint.FromMeters(point.x)) *
                    (FixedPoint.FromMeters(vertex.x) - FixedPoint.FromMeters(point.x))
                    + (FixedPoint.FromMeters(vertex.y) - FixedPoint.FromMeters(point.y)) *
                    (FixedPoint.FromMeters(vertex.y) - FixedPoint.FromMeters(point.y));

            if (distanceSquared < epsilon * epsilon)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsPointOnLineSegment(Vector2 a, Vector2 b, Vector2 point)
    {
        FixedPoint tolerance = FixedPoint.FromMeters(0.0001f);

        // 1. Bounding Box Check
        FixedPoint minX = FixedPoint.FromMeters(Mathf.Min(a.x, b.x));
        FixedPoint maxX = FixedPoint.FromMeters(Mathf.Max(a.x, b.x));
        FixedPoint minY = FixedPoint.FromMeters(Mathf.Min(a.y, b.y));
        FixedPoint maxY = FixedPoint.FromMeters(Mathf.Max(a.y, b.y));

        FixedPoint pointX = FixedPoint.FromMeters(point.x);
        FixedPoint pointY = FixedPoint.FromMeters(point.y);

        if (pointX < minX - tolerance || pointX > maxX + tolerance ||
            pointY < minY - tolerance || pointY > maxY + tolerance)
        {
            return false; // Point is out of bounding box
        }

        // 2. Distance Check
        FixedPoint lineLength = FixedPoint.FromMeters(Vector2.Distance(a, b));
        FixedPoint distanceToA = FixedPoint.FromMeters(Vector2.Distance(point, a));
        FixedPoint distanceToB = FixedPoint.FromMeters(Vector2.Distance(point, b));

        FixedPoint totalDistances = distanceToA + distanceToB;

        return FixedPoint.Abs(totalDistances - lineLength) < tolerance;
    }
    
    public static bool IsPointInPolygon(List<Vector2> polygon, Vector2 point)
    {
        bool isInside = false;
        int vertexCount = polygon.Count;
    
        for (int i = 0, j = vertexCount - 1; i < vertexCount; j = i++)
        {
            Vector2 vi = polygon[i];
            Vector2 vj = polygon[j];
    
            if (((vi.y > point.y) != (vj.y > point.y)) &&
                (point.x < (vj.x - vi.x) * (point.y - vi.y) / (vj.y - vi.y) + vi.x))
            {
                isInside = !isInside;
            }
        }
    
        return isInside;
    }
    
    public static bool CheckCollision(List<Vector2> polygonA, List<Vector2> polygonB)
    {
        List<Vector2> allAxes = GetAxes(polygonA, polygonB);

        if (ProjectionHasOverlap(polygonA, polygonB, allAxes))
        {
            return true;
        }
        else if (ProjectionHasOverlap(polygonB, polygonA, allAxes))
        {
            return true;
        }

        return false;
    }


    private static List<Vector2> GetAxes(List<Vector2> polygonA, List<Vector2> polygonB)
    {
        List<Vector2> axes = new List<Vector2>();

        for (int i = 0; i < polygonA.Count; i++)
        {
            Vector2 edge = polygonA[(i + 1) % polygonA.Count] - polygonA[i];
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;
            axes.Add(normal);
        }

        for (int i = 0; i < polygonB.Count; i++)
        {
            Vector2 edge = polygonB[(i + 1) % polygonB.Count] - polygonB[i];
            Vector2 normal = new Vector2(-edge.y, edge.x).normalized;
            axes.Add(normal);
        }

        return axes;
    }

    private static bool ProjectionHasOverlap(List<Vector2> aVertices, List<Vector2> bVertices,
            List<Vector2> axes)
    {
        foreach (Vector2 axis in axes)
        {
            float aProjMin = float.MaxValue;
            float bProjMin = float.MaxValue;
            float aProjMax = float.MinValue;
            float bProjMax = float.MinValue;

            foreach (Vector2 vertex in aVertices)
            {
                float val = FindScalarProjection(vertex, axis);
                aProjMin = Mathf.Min(aProjMin, val);
                aProjMax = Mathf.Max(aProjMax, val);
            }

            foreach (Vector2 vertex in bVertices)
            {
                float val = FindScalarProjection(vertex, axis);
                bProjMin = Mathf.Min(bProjMin, val);
                bProjMax = Mathf.Max(bProjMax, val);
            }

            if (!IsOverlap(aProjMin, aProjMax, bProjMin, bProjMax))
            {
                return false; // Separating axis found
            }
        }

        return true; // No separating axis found
    }

    private static float FindScalarProjection(Vector2 point, Vector2 axis)
    {
        return Vector2.Dot(point, axis);
    }

    private static bool IsOverlap(float aStart, float aEnd, float bStart, float bEnd)
    {
        return !(aEnd < bStart || bEnd < aStart);
    }







    public static List<Vector2> ExpandPolygon(List<Vector2> polygon, float offset)
    {
        List<Vector2> expandedPolygon = new List<Vector2>();
        int vertexCount = polygon.Count;

        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 current = polygon[i];
            Vector2 prev = polygon[(i - 1 + vertexCount) % vertexCount];
            Vector2 next = polygon[(i + 1) % vertexCount];

            // Calculate the edge directions
            Vector2 edgeDirPrev = (current - prev).normalized;
            Vector2 edgeDirNext = (next - current).normalized;

            // Calculate the outward normal directions (perpendicular to edge directions)
            Vector2 normalPrev = new Vector2(edgeDirPrev.y, -edgeDirPrev.x);
            Vector2 normalNext = new Vector2(edgeDirNext.y, -edgeDirNext.x);

            // Calculate the average of the two normals (weighted by the offset)
            Vector2 weightedNormal = (normalPrev + normalNext).normalized * offset;

            // Move the current vertex by the weighted normal
            Vector2 expandedVertex = current + weightedNormal;
            expandedPolygon.Add(expandedVertex);
        }

        return expandedPolygon;
    }

    public static List<Vector2> GenerateConvexHull(List<Vector2> points)
    {
        if (points.Count < 3)
        {
            return new List<Vector2>(); // No hull for less than 3 points
        }

        List<Vector2> hull = new List<Vector2>();

        // Step 1: Find the leftmost point
        int leftmost = 0;
        for (int i = 1; i < points.Count; i++)
        {
            if (points[i].x < points[leftmost].x)
            {
                leftmost = i;
            }
        }

        // Step 2: Start from leftmost point, keep moving 
        // counterclockwise until reach the start point again
        int p = leftmost, q;
        do
        {
            // Add current point to result
            hull.Add(points[p]);

            // Search for a point 'q' such that orientation(p, q, r)
            // is counterclockwise for all points 'r'
            q = (p + 1) % points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                if (Orientation(points[p], points[i], points[q]) < 0)
                {
                    q = i;
                }
            }

            // Now q is the most counterclockwise with respect to p
            // Set p as q for next iteration
            p = q;

        } while (p != leftmost); // While we don't come to first point

        return hull;
    }

    public static float CalculatePerimeter(List<Vector2> polygon)
    {
        if (polygon.Count < 3)
        {
            return 0; // No perimeter for less than 3 points
        }

        float perimeter = 0f;
        for (int i = 0; i < polygon.Count; i++)
        {
            int nextIndex = (i + 1) % polygon.Count;
            perimeter += Vector2.Distance(polygon[i], polygon[nextIndex]);
        }

        return perimeter;
    }

    // Helper function to find orientation of ordered triplet (p, q, r).
    // The function returns negative value if p, q, r makes a counterclockwise turn,
    // positive for clockwise turn and zero if p, q, and r are collinear.
    public static float Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        return (q.x - p.x) * (r.y - p.y) - (q.y - p.y) * (r.x - p.x);
    }
    
    //Shoelace Formulat
    public static float CalculateArea(List<Vector2> vertices)
    {
        if (vertices.Count < 3)
        {
            return 0;  // Not a polygon
        }

        float area = 0;
        int vertexCount = vertices.Count;
        for (int i = 0; i < vertexCount; i++)
        {
            Vector2 currentVertex = vertices[i];
            Vector2 nextVertex = vertices[(i + 1) % vertexCount];  // Wrap around to 0 at the end
            area += (currentVertex.x * nextVertex.y - nextVertex.x * currentVertex.y);
        }

        area = Mathf.Abs(area) / 2.0f;
        return area;
    }
}