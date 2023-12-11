using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public struct PlaceObjectData
{
	public int StartIndex; // Start index of the vertices in a global NativeArray
	public int Length;    // Number of vertices
	
	// Method to get a slice of the vertices
	public NativeArray<Vector2> CreateSubArray(NativeArray<Vector2> allVertices)
	{
		NativeArray<Vector2> subArray = new NativeArray<Vector2>(Length, Allocator.Temp);

		for (int i = 0; i < Length; i++)
		{
			subArray[i] = allVertices[StartIndex + i];
		}

		return subArray;
	}
}

public struct AllPlacedObjects
{
	
	[ReadOnly] public NativeArray<Vector2> allVertices;
	[ReadOnly] public NativeArray<Vector2> allVerticesExpanded;
	[ReadOnly] public NativeArray<PlaceObjectData> ObjectsData;
}

public struct NFPGeneratorJob : IJobParallelFor
{
	[ReadOnly] public NativeArray<Vector2> BinOutlineVertices;
	[ReadOnly] public NativeArray<Vector2>? BinHoleVertices;
	public AllPlacedObjects AllPlacedObjects; // All vertices for all objects
	public NativeArray<Vector2> newObject;
	public Vector2 newObjectPos;
	public Quaternion newObjectRot;

	public NativeArray<ANFPResult> Results;
	
	public void Execute(int index)
	{
		
		Debug.Log($"index thread {index}");
		GenerateNfp(AllPlacedObjects.ObjectsData[index], newObject);
	}
	
	public void GenerateNfp(PlaceObjectData stationary, NativeArray<Vector2> newObjectVertices)
	{
		Vector2 originalMovingPosition =newObjectPos;
		Quaternion originalMovingRotation = newObjectRot;

		NativeArray<Vector2> newObjectExpendedVertices = newObjectVertices;
		NativeArray<Vector2> stationaryVertices =
				stationary.CreateSubArray(AllPlacedObjects.allVerticesExpanded);
		
		// Define a range of rotation angles (e.g., from 0 to 360 degrees)
		float minRotation = 0f;
		float maxRotation = 360f;
		float rotationStep = 15f; // Adjust this step value as needed
		int resultsCount = 0;
	
		for (float rotation = minRotation; rotation <= maxRotation; rotation += rotationStep)
		{
			Quaternion desiredRotation = originalMovingRotation;
			if (rotation > minRotation)
			{
				desiredRotation = Quaternion.Euler(0, 0, rotationStep);
				newObjectExpendedVertices =
						PolygonUtility.RotateVertex(newObjectExpendedVertices, desiredRotation);
				
			}
			
			// Compute the world space position of the reference vertex
			for (int j = 0; j < newObjectExpendedVertices.Count(); j++)
			{
				Vector2 referenceVertexWorldPosition = newObjectExpendedVertices[j] + originalMovingPosition;
				for (int i = 0; i < stationaryVertices.Count(); i++)
				{
					Vector2 stationaryVertex = stationaryVertices[i];
					Vector2 delta = stationaryVertex - referenceVertexWorldPosition;
					Vector2 newMovingPosition = originalMovingPosition + delta;
					// moving.UpdatePosition(newMovingPosition);
					IEnumerable<Vector2> newObjectExpendedVerticesTranslated =
							newObjectExpendedVertices.Select(vec => new Vector2( vec.x  + newMovingPosition.x, vec.y + newMovingPosition.y));

					// moving.UpdatePosition(newMovingPosition);
					
					// if (IsValidPlacement(newObjectExpendedVerticesTranslated, AllPlacedObjects))
					// {
					// 	// Store the NFPResult with position, rotation, and stationary object
					// 	Quaternion desiredRotation2 = (Quaternion.Euler(0, 0, rotation) * Quaternion.Inverse(originalMovingRotation));
					//
					// 	float totalDistance = CalculateTotalDistanceToBinVertices(newObjectExpendedVerticesTranslated, BinOutlineVertices.ToList());
					// 	ANFPResult result = new ANFPResult(newMovingPosition, desiredRotation2)
					// 	{
					// 			TotalDistanceToBinVertices = totalDistance
					// 	};
					// 	Results[resultsCount] = (result);
					// 	resultsCount++;
					// }
				}
			}
		}
	}
	
	public bool IsValidPlacement(List<Vector2> newObjectExpendedVertices, AllPlacedObjects placedObjects)
	{
		if (!IsInsideBin(newObjectExpendedVertices))
		{
			return false;
		}

		// Check if newObject intersects with any of the already placed objects.
		foreach (PlaceObjectData placedObject in placedObjects.ObjectsData)
		{
			// // Translate the rotated vertices of the placed object to their respective positions
			// List<Vector2> translatedPlacedObjectVertices = placedObject.OutlineVertices
			// 		.Select(v => v + (Vector2)placedObject.transform.position).ToList();
			if (PolygonUtility.CheckCollision(newObjectExpendedVertices, placedObject.CreateSubArray(placedObjects.allVertices).ToList()))
			{
				// Debug.Log("Objects are colliding");
				return false; // Found an intersection with one of the placed objects.
			}
		}

		return true;
	}
	
	private float CalculateTotalDistanceToBinVertices(List<Vector2> objectVertices, List<Vector2> binVertices)
	{
		float totalDistance = 0;
		foreach (Vector2 objectVertex in objectVertices)
		{
			float minDistance = float.MaxValue;
			foreach (Vector2 binVertex in binVertices)
			{
				float distance = Vector2.Distance(objectVertex, binVertex);
				if (distance < minDistance)
				{
					minDistance = distance;
				}
			}

			totalDistance += minDistance;
		}

		return totalDistance;
	}

	
	private bool IsInsideBin(List<Vector2> newObjectVertex)
	{
		if (BinHoleVertices == null)
		{
			// Check if newObject is inside the bin.
			if (PolygonUtility.IsObjectInsideBin(newObjectVertex, BinOutlineVertices.ToList()))
			{
				// Debug.Log("Object is not Inside the BIN");
				return true;
			}
		}
		else
		{
			if (PolygonUtility.IsObjectInsideBinWithHole(newObjectVertex, BinOutlineVertices.ToList(), BinHoleVertices.Value.ToList()))
			{
				// Debug.Log("Object is not Inside the BIN");
				return true;
			}
		}

		return false;
	}
}
