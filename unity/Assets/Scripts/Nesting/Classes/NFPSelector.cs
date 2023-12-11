using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class NFPSelector
{
	public BinPlacer BinPlacer;

	private List<Vector2> tmpConvexHull;
	private Vector2 MinBoundingBox;
	private Vector2 MaxBoundingBox;

	public NFPSelector(BinPlacer binPlacer)
	{
		this.BinPlacer = binPlacer;
	}
	
	public IEnumerator FindBestFitResultCoroutine(List<ANFPResult> nfpResults, ScannedObjectSvg newObject, Action<bool> ObjectPlaced)
	{
		if (nfpResults.Count == 0)
		{
			Debug.Log("No valid placement found for the object " + newObject.transform.name);
			yield break;
		}

		float minScore = float.MaxValue;
		ANFPResult? bestResult = null;
		foreach (ANFPResult result in nfpResults)
		{
			newObject.UpdateTransformVertex(result.Rotation, result.Position);
			List<Vector2> movingVertices = newObject.ExpandedOutlineVertices
					.Select(v => v + result.Position).ToList();
			List<Vector2> allVertices = CollectAllVertices(newObject, result.Position);
			float boundingBoxArea = GetBoundingBoxArea(movingVertices);
			List<Vector2> convexHullVertices = PolygonUtility.GenerateConvexHull(allVertices);
			tmpConvexHull = convexHullVertices;
			float convexHullPerimeter = PolygonUtility.CalculatePerimeter(convexHullVertices);
			float minDistanceToNearestObject = CalculateMinDistanceToNearestObject(newObject, result.Position);
			float score = CalculateScore(boundingBoxArea, result.TotalDistanceToBinVertices, convexHullPerimeter, minDistanceToNearestObject);

			if (score < minScore)
			{
				minScore = score;
				bestResult = result;
				yield return new WaitForSeconds(.01f);  // Adjust the delay as needed
			}

			yield return new WaitForSeconds(0.0001f); // Adjust the delay as needed
		}

		if (bestResult != null)
		{
			ProcessBestResult(bestResult.Value, newObject);
			ObjectPlaced?.Invoke(true);
		}
		else
			ObjectPlaced?.Invoke(false);
	}

	private List<Vector2> CollectAllVertices(ScannedObjectSvg movingObject, Vector2 movingObjectPosition)
	{
		// Create a list to hold all the vertices from the moving object and placed objects.
		List<Vector2> allVertices = new List<Vector2>();

		// Get the vertices from the moving object translated to the world position.
		List<Vector2> movingVertices = movingObject.ExpandedOutlineVertices
				.Select(v => v + movingObjectPosition).ToList();
		allVertices.AddRange(movingVertices);

		// Iterate through the placed objects and collect their vertices.
		foreach (var placedObject in BinPlacer.PlacedObjects)
		{
			List<Vector2> placedObjectVertices = placedObject.ExpandedOutlineVertices
					.Select(v => v + (Vector2)placedObject.transform.position).ToList();
			allVertices.AddRange(placedObjectVertices);
		}

		return allVertices;
	}

	private float GetBoundingBoxArea(List<Vector2> movingVertices)
	{
		List<List<Vector2>> allPlacedPolygons = CollectPlacedPolygons(BinPlacer.PlacedObjects);
		float boundingBoxArea = CalculateBoundingBoxArea(movingVertices, allPlacedPolygons);

		return boundingBoxArea;
	}

	private float CalculateMinDistanceToNearestObject(ScannedObjectSvg newObject, Vector2 newPosition)
	{
		float minDistance = float.MaxValue;
		foreach (ScannedObjectSvg placedObject in BinPlacer.PlacedObjects)
		{
			foreach (Vector2 placedVertex in placedObject.ExpandedOutlineVertices)
			{
				foreach (Vector2 newVertex in newObject.ExpandedOutlineVertices)
				{
					float distance = Vector2.Distance(placedVertex + (Vector2)placedObject.transform.position, newVertex + newPosition);
					if (distance < minDistance)
					{
						minDistance = distance;
					}
				}
			}
		}
		return minDistance;
	}
	private float CalculateScore(float boundingBoxArea, float totalDistance, float convexHullPerimeter, float minDistanceToNearestObject)
	{
		float weightBoundingBox = 1.0f;  // Adjust as needed
		float weightDistance = 1.0f;  // Adjust as needed
		float weightConvexHull = 1.5f;  // Adjust as needed
		float weightMinDistance = 2f;  // Adjust as needed

		float score = (weightBoundingBox * boundingBoxArea) + (weightDistance * totalDistance) +
		              (weightConvexHull * convexHullPerimeter) + (weightMinDistance * minDistanceToNearestObject);

		return score;
	}


	private void ProcessBestResult(ANFPResult bestResult, ScannedObjectSvg newObject)
	{
		newObject.UpdateTransformVertex(bestResult.Rotation, bestResult.Position);
		var transformPosition = newObject.transform.position;
		transformPosition.z += newObject.transform.forward.z * newObject.DepthObject;
		newObject.transform.position = transformPosition;
		
		List<Vector2> movingVertices = newObject.ExpandedOutlineVertices
				.Select(v => v + bestResult.Position).ToList();
		List<Vector2> allVertices = CollectAllVertices(newObject, bestResult.Position);
		GetBoundingBoxArea(movingVertices);
		tmpConvexHull = PolygonUtility.GenerateConvexHull(allVertices);
		newObject.MyBin = this.BinPlacer.bin;
		newObject.transform.SetParent(this.BinPlacer.bin.transform);
		BinPlacer.bin.areaLeft -= newObject.AreaTotal;
		GetBoundingBoxArea(movingVertices);
		BinPlacer.PlacedObjects.Add(newObject);
		BinPlacer.Setup3DPosition(newObject);
	}

	private List<List<Vector2>> CollectPlacedPolygons(List<ScannedObjectSvg> placedObjects)
	{
		List<List<Vector2>> allPlacedPolygons = new List<List<Vector2>>();
		foreach (ScannedObjectSvg placedObject in placedObjects)
		{
			List<Vector2> placedObjectVertices = placedObject.ExpandedOutlineVertices
					.Select(v => v + (Vector2)placedObject.transform.position).ToList();
			allPlacedPolygons.Add(placedObjectVertices);
		}
		return allPlacedPolygons;
	}


	private float CalculateBoundingBoxArea(List<Vector2> newObjectVertices,
			List<List<Vector2>> otherObjectsVertices)
	{

		// Get the min and max coordinates for the new object
		float minX = newObjectVertices.Min(v => v.x);
		float minY = newObjectVertices.Min(v => v.y);
		float maxX = newObjectVertices.Max(v => v.x);
		float maxY = newObjectVertices.Max(v => v.y);

		// Update the min and max coordinates based on the other objects
		foreach (var polygon in otherObjectsVertices)
		{
			minX = Math.Min(minX, polygon.Min(v => v.x));
			minY = Math.Min(minY, polygon.Min(v => v.y));
			maxX = Math.Max(maxX, polygon.Max(v => v.x));
			maxY = Math.Max(maxY, polygon.Max(v => v.y));
		}

		float width = maxX - minX;
		float height = maxY - minY;
		MinBoundingBox.x = minX;
		MinBoundingBox.y = minY;
		MaxBoundingBox.x = maxX;
		MaxBoundingBox.y = maxY;
		return width * height;
	}
	
	public void GizmoDrawing()
	{
		Gizmos.color = Color.red;
		Vector3 start = MinBoundingBox;
		Vector3 end = new Vector3(MinBoundingBox.x, MaxBoundingBox.y);
		Gizmos.DrawLine(start, end);

		start = new Vector3(MinBoundingBox.x, MaxBoundingBox.y);
		end = MaxBoundingBox;
		Gizmos.DrawLine(start, end);

		start = MaxBoundingBox;
		end = new Vector3(MaxBoundingBox.x, MinBoundingBox.y);
		
		Gizmos.DrawLine(start, end);

		start = new Vector3(MaxBoundingBox.x, MinBoundingBox.y);
		
		end = MinBoundingBox;
		Gizmos.DrawLine(start, end);
		
		DrawVertices2D(tmpConvexHull, Color.white, Color.green, 0.002f);
	}
	
	private void DrawVertices2D(List<Vector2> vertices, Color lineColor, Color sphereColor,
			float sphereSize)
	{
		if (vertices == null || vertices.Count <= 0)
			return;

		for (int i = 0; i < vertices.Count; i++)
		{
			Vector3 start = (Vector3)vertices[i];
			Vector3 end = (Vector3)vertices[(i + 1) % vertices.Count];
			DrawLineAndSphere(start, end, lineColor, sphereColor, sphereSize, i, vertices.Count);
		}
	}
	
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
