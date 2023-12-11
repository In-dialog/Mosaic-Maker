
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class MeshExtension
{
	public static (List<Vector2> boundary, List<Vector2> hole) Generate2DOutline(this Mesh mesh)
	{
		Vector2[] vertices = mesh.GetVerticesXYAxis();
		HashSet<Edge> boundaryEdges = IdentifyBoundaryEdges(mesh.triangles);
		List<Vector2> boundary =
				TraceBoundaryFromVertex(boundaryEdges, vertices, FindLeftmostVertex(vertices));
		
		// Check if there are any vertices left for the hole
		List<Vector2> hole = new List<Vector2>();
		if (boundary.Count < vertices.Length)
		{
			// If vertices are left after boundary adjustment, we have a hole
			hole = TraceHole(vertices, mesh.triangles, boundary);
		}

		return (boundary, hole);
	}

	public static Vector2[] GetVerticesXYAxis(this Mesh mesh)
	{
		Vector3[] vertices3D = mesh.vertices;
		Vector2[] vertices2D = new Vector2[vertices3D.Length];

		for (int i = 0; i < vertices3D.Length; i++)
		{
			// Assuming the z-axis is the one to ignore
			vertices2D[i] = new Vector2(vertices3D[i].x, vertices3D[i].y);
		}

		return vertices2D;
	}

	public static List<Vector2> TraceBoundary(Vector2[] vertices, int[] triangles)
	{
		int startIndex = FindLeftmostVertex(vertices);
		var boundaryEdges = IdentifyBoundaryEdges(triangles);

		return TraceBoundaryFromVertex(boundaryEdges, vertices, startIndex);
	}

	private static int FindLeftmostVertex(Vector2[] vertices)
	{
		int leftmostIndex = 0;
		float minX = vertices[0].x;

		for (int i = 1; i < vertices.Length; i++)
		{
			if (vertices[i].x < minX)
			{
				minX = vertices[i].x;
				leftmostIndex = i;
			}
		}

		return leftmostIndex;
	}

	private static List<Vector2> TraceBoundaryFromVertex(HashSet<Edge> boundaryEdges,
			Vector2[] vertices, int startIndex)
	{
		var visited = new HashSet<int>();
		var path = new List<Vector2>();
		int currentIndex = startIndex;
		Vector2 lastDirection = Vector2.up; // Initialize with an upward direction

		while (!visited.Contains(currentIndex))
		{
			path.Add(vertices[currentIndex]);
			visited.Add(currentIndex);

			var possibleNextEdges = boundaryEdges.Where(e =>
							e.Contains(currentIndex) &&
							!visited.Contains(e.OtherVertex(currentIndex)))
					.ToList();

			// Sort edges considering both angle and distance
			possibleNextEdges.Sort((a, b) =>
			{
				Vector2 nextVertexA = vertices[a.OtherVertex(currentIndex)];
				Vector2 nextVertexB = vertices[b.OtherVertex(currentIndex)];
				float angleA =
						Vector2.SignedAngle(lastDirection, nextVertexA - vertices[currentIndex]);
				float angleB =
						Vector2.SignedAngle(lastDirection, nextVertexB - vertices[currentIndex]);
				float distanceA = (nextVertexA - vertices[currentIndex]).magnitude;
				float distanceB = (nextVertexB - vertices[currentIndex]).magnitude;

				// Factor in both angle and distance for sorting
				if (Mathf.Abs(angleA) < 90 && Mathf.Abs(angleB) < 90) // Both angles are less sharp
				{
					return angleA.CompareTo(angleB);
				}
				else if (Mathf.Abs(angleA) < 90) // Only angleA is less sharp
				{
					return -1;
				}
				else if (Mathf.Abs(angleB) < 90) // Only angleB is less sharp
				{
					return 1;
				}
				else // Both angles are sharp, prefer closer vertex
				{
					return distanceA.CompareTo(distanceB);
				}
			});

			if (possibleNextEdges.Count == 0)
			{
				break; // No more edges to follow
			}

			Edge nextEdge = possibleNextEdges.First();
			currentIndex = nextEdge.OtherVertex(currentIndex);
			if (path.Count > 1)
			{
				lastDirection = (vertices[currentIndex] - path[^2]).normalized;
			}
		}

		return path;
	}

	private static HashSet<Edge> IdentifyBoundaryEdges(int[] triangles)
	{
		var edgeCount = new Dictionary<Edge, int>();
		var boundaryEdges = new HashSet<Edge>();

		for (int i = 0; i < triangles.Length; i += 3)
		{
			for (int j = 0; j < 3; j++)
			{
				int index1 = triangles[i + j];
				int index2 = triangles[i + (j + 1) % 3];
				Edge edge = new Edge(index1, index2);

				if (edgeCount.ContainsKey(edge))
				{
					edgeCount[edge]++;
				}
				else
				{
					edgeCount.Add(edge, 1);
				}
			}
		}

		foreach (var pair in edgeCount)
		{
			if (pair.Value == 1) // Boundary edges are part of only one triangle
			{
				boundaryEdges.Add(pair.Key);
			}
		}

		return boundaryEdges;
	}



	private static List<Vector2> TraceHole(Vector2[] vertices, int[] triangles,
			List<Vector2> boundary)
	{
		// This check assumes that if there are no vertices left out of boundary tracing, there's no hole
		if (vertices.Length == boundary.Count)
		{
			return new List<Vector2>(); // No hole found
		}

		var holeVertices = new HashSet<int>(Enumerable.Range(0, vertices.Length)
				.Except(boundary.Select(v => Array.IndexOf(vertices, v))));
		var boundaryEdges = IdentifyBoundaryEdges(triangles);

		int holeStartIndex =
				holeVertices.FirstOrDefault(
						index => boundaryEdges.Any(edge => edge.Contains(index)));
		if (holeStartIndex == -1)
		{
			Debug.Log("No hole found in this mesh.");
			return new List<Vector2>(); // No hole found, or the hole identification logic is faulty
		}

		// Trace the hole boundary starting from the hole's starting vertex
		return TraceBoundaryFromVertex(boundaryEdges, vertices, holeStartIndex);
	}

	/// <summary>
	/// Function to determine whether the input vertices are order clockwise or counter-clockwise.
	/// </summary>
	/// <returns>Returns <c>true</c> if vertices are ordered clockwise, and <c>false</c> if ordered counter-clockwise.</returns>
	/// <param name="vertices">Vector2 Array representing input vertices of the polygon.</param>
	public static bool AreVerticesOrderedClockwise(Vector2[] vertices)
	{
		float edgesSum = 0.0f;
		for (int i = 0; i < vertices.Length; i++)
		{
			if (i + 1 == vertices.Length)
			{
				edgesSum += (vertices[0].x - vertices[i].x) * (vertices[0].y + vertices[i].y);
			}
			else
			{
				edgesSum += (vertices[i + 1].x - vertices[i].x) * (vertices[i + 1].y + vertices[i].y);
			}
		}

		// Reverse the interpretation of edgesSum
		return edgesSum <= 0.0f; // True for clockwise, false for counter-clockwise
	}

	public static string MeshToString(this Mesh mesh)
	{
		StringBuilder sb = new StringBuilder();

		sb.AppendLine("Vertices:");
		foreach (Vector3 vert in mesh.vertices)
		{
			sb.AppendLine($"{vert.x}, {vert.y}, {vert.z}");
		}

		sb.AppendLine("Triangles:");
		foreach (int tri in mesh.triangles)
		{
			sb.AppendLine(tri.ToString());
		}

		return sb.ToString();
	}
}
