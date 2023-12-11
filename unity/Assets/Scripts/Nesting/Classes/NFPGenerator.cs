using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public class NFPGenerator
{
	public BinPlacer BinPlacer;
	
	private List<Vector2> tmpObjectVertices;
	private List<Vector2> tmpPlacedObjectVertices;
	private List<Vector2> tmpBinVertices;

	private Vector2 curVertexMoving;
	private Vector2 curVerticesStationary;

	public List<Vector2> translatedBinOutline;
	public List<Vector2> translatedBinHole;

	public bool roteteOn;
	public NFPGenerator(BinPlacer binPlacer)
	{
		this.BinPlacer = binPlacer;
		translatedBinOutline =
				binPlacer.bin.OutlineVertices.Select(vec => new Vector2(vec.x + BinPlacer.bin.transform.position.x, vec.y + BinPlacer.bin.transform.position.y)).ToList();
		if (binPlacer.bin.holeOutline != null)
		{
			translatedBinHole = binPlacer.bin.holeOutline.Select(vec =>
					new Vector2(vec.x + BinPlacer.bin.transform.position.x,
							vec.y + BinPlacer.bin.transform.position.y)).ToList();
		}
		else
			translatedBinHole = null;
	}
	
	public IEnumerator GenerateNfp(ScannedObjectSvg stationary, ScannedObjectSvg moving,
			List<ANFPResult> nfpResults)
	{
		Vector2 originalMovingPosition = moving.transform.position;
		Quaternion originalMovingRotation = moving.transform.rotation;
	
		List<Vector2> newObjectExpendedVertices =
				moving.ExpandedOutlineVertices.Select(vec => new Vector2(vec.x, vec.y)).ToList();
		
		// Define a range of rotation angles (e.g., from 0 to 360 degrees)
		float minRotation = 0f;
		float maxRotation = 360f;
		float rotationStep = roteteOn ? 15f : 361f; // Adjust this step value as needed
		
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
			for (int j = 0; j < newObjectExpendedVertices.Count; j++)
			{
				Vector2 referenceVertexWorldPosition = newObjectExpendedVertices[j] + originalMovingPosition;
				for (int i = 0; i < stationary.ExpandedOutlineVertices.Count; i++)
				{
					Vector2 stationaryVertex = stationary.ExpandedOutlineVertices[i] + (Vector2)stationary.transform.position;
					Vector2 delta = stationaryVertex - referenceVertexWorldPosition;
					Vector2 newMovingPosition = originalMovingPosition + delta;
					
					//Debug value for gizmo
					curVerticesStationary = stationaryVertex;
					curVertexMoving = delta + referenceVertexWorldPosition;
					
					// moving.UpdatePosition(newMovingPosition);
					List<Vector2> newObjectExpendedVerticesTranslated =
							newObjectExpendedVertices.Select(vec => new Vector2( vec.x  + newMovingPosition.x, vec.y + newMovingPosition.y)).ToList();

					// moving.UpdatePosition(newMovingPosition);
					tmpObjectVertices = newObjectExpendedVerticesTranslated;

					if (IsValidPlacement(newObjectExpendedVerticesTranslated, moving,newMovingPosition, BinPlacer.PlacedObjects))
					{
						// Store the NFPResult with position, rotation, and stationary object
						Quaternion desiredRotation2 = (Quaternion.Euler(0, 0, rotation) * Quaternion.Inverse(originalMovingRotation));
						// moving.UpdateTransformVertex(desiredRotation2, newMovingPosition);
						yield return new WaitForSeconds(.01f); // Pause for visualization
						
						float totalDistance = CalculateTotalDistanceToBinVertices(
								newObjectExpendedVerticesTranslated, Vector2.zero,
								BinPlacer.bin.OutlineVertices);
						ANFPResult result = new ANFPResult(newMovingPosition, desiredRotation2)
						{
								TotalDistanceToBinVertices = totalDistance
						};
						nfpResults.Add(result);
						
					}
					// yield return new WaitForSeconds(.5f); // Pause for visualization
				}
				
			}
		}
		

		tmpObjectVertices = null;
		// // Reset the rotation of the moving object
		// moving.UpdateTransformVertex(originalMovingRotation,
		// 		originalMovingPosition);
	}
	
	// 	public IEnumerator GenerateNfp(ScannedObjectSvg stationary, ScannedObjectSvg moving,
	// 		List<ANFPResult> nfpResults)
	// {
	// 	Vector2 originalMovingPosition = moving.transform.position;
	// 	// Debug.Log($" {moving.transform.name}   {moving.transform.rotation}");
	// 	Quaternion originalMovingRotation = moving.transform.rotation;
	//
	// 	// Define a range of rotation angles (e.g., from 0 to 360 degrees)
	// 	float minRotation = 0f;
	// 	float maxRotation = 360f;
	// 	float rotationStep = 15f; // Adjust this step value as needed
	//
	// 	for (float rotation = minRotation; rotation <= maxRotation; rotation += rotationStep)
	// 	{
	// 		moving.UpdateRotation(Quaternion.Euler(originalMovingRotation.eulerAngles.x, originalMovingRotation.eulerAngles.y, rotation));
	// 		// Compute the world space position of the reference vertex
	// 		for (int j = 0; j < moving.ExpandedOutlineVertices.Count; j++)
	// 		{
	// 			Vector2 referenceVertexWorldPosition =
	// 					moving.ExpandedOutlineVertices[j] + originalMovingPosition;
	// 			for (int i = 0; i < stationary.ExpandedOutlineVertices.Count; i++)
	// 			{
	// 				Vector2 stationaryVertex = stationary.ExpandedOutlineVertices[i] +
	// 				                           (Vector2)stationary.transform.position;
	// 				Vector2 delta = stationaryVertex - referenceVertexWorldPosition;
	// 				Vector2 newMovingPosition = originalMovingPosition + delta;
	// 				
	// 				//Debug value for gizmo
	// 				curVerticesStationary = stationaryVertex;
	// 				curVertexMoving = delta + referenceVertexWorldPosition;
	// 				// Visualize the moving object at the new position
	// 				moving.UpdatePosition(newMovingPosition);
	// 				if (IsValidPlacement(moving, newMovingPosition, BinPlacer.PlacedObjects))
	// 				{
	// 					// Store the NFPResult with position, rotation, and stationary object
	// 					yield return new WaitForSeconds(.01f); // Pause for visualization
	// 					float totalDistance = CalculateTotalDistanceToBinVertices(
	// 							moving.ExpandedOutlineVertices, newMovingPosition,
	// 							BinPlacer.bin.OutlineVertices);
	// 					ANFPResult result = new ANFPResult(newMovingPosition, moving.transform.rotation)
	// 					{
	// 							TotalDistanceToBinVertices = totalDistance
	// 					};
	// 					nfpResults.Add(result);
	// 				}
	// 				// yield return new WaitForSeconds(.1f); // Pause for visualization
	// 			}
	// 		}
	// 	}
	//
	// 	// Reset the rotation of the moving object
	// 	moving.UpdateTransformVertex(originalMovingRotation,
	// 			originalMovingPosition);
	// 	yield return null;
	// }

	public bool IsValidPlacement(ScannedObjectSvg newObject, Vector2 position,
			List<ScannedObjectSvg> placedObjects)
	{
		// Translate the vertices to the specified position
		List<Vector2> translatedVertices =
				newObject.ExpandedOutlineVertices.Select(v => v + position).ToList();

		if (!IsInsideBin(newObject, position))
		{
			return false;
		}

		// Check if newObject intersects with any of the already placed objects.
		foreach (ScannedObjectSvg placedObject in placedObjects)
		{
			// Translate the rotated vertices of the placed object to their respective positions
			List<Vector2> translatedPlacedObjectVertices = placedObject.OutlineVertices
					.Select(v => v + (Vector2)placedObject.transform.position).ToList();
			if (PolygonUtility.CheckCollision(translatedVertices, translatedPlacedObjectVertices))
			{
				// Debug.Log("Objects are colliding");
				return false; // Found an intersection with one of the placed objects.
			}
		}

		return true;
	}
	
	public bool IsValidPlacement(List<Vector2> newObjectExpendedVertices, ScannedObjectSvg newObject ,Vector2 position,
			List<ScannedObjectSvg> placedObjects)
	{
		
		if (!IsInsideBin(newObject, position))
		{
			return false;
		}

		// Check if newObject intersects with any of the already placed objects.
		foreach (ScannedObjectSvg placedObject in placedObjects)
		{
			// Translate the rotated vertices of the placed object to their respective positions
			List<Vector2> translatedPlacedObjectVertices = placedObject.OutlineVertices
					.Select(v => v + (Vector2)placedObject.transform.position).ToList();
			if (PolygonUtility.CheckCollision(newObjectExpendedVertices, translatedPlacedObjectVertices))
			{
				// Debug.Log("Objects are colliding");
				return false; // Found an intersection with one of the placed objects.
			}
		}

		return true;
	}
	

	private bool IsInsideBin(ScannedObjectSvg newObject, Vector2 position)
	{
		List<Vector2> translatedPlacedObjectVertices = newObject.ExpandedOutlineVertices
				.Select(v => v + position).ToList();
		if (translatedBinHole == null)
		{
			// List<Vector2> translatedVerticesBin = BinPlacer.bin.OutlineVertices
			// 		.Select(v => v + (Vector2)BinPlacer.bin.transform.position).ToList();

			// Check if newObject is inside the bin.
			if (PolygonUtility.IsObjectInsideBin(translatedPlacedObjectVertices, translatedBinOutline))
			{
				// Debug.Log("Object is not Inside the BIN");
				return true;
			}
		}
		else
		{
			// List<Vector2> translatedOutlineVerticesBin = BinPlacer.bin.OutlineVertices
			// 		.Select(v => v + (Vector2)BinPlacer.bin.transform.position).ToList();
			
			// List<Vector2> translatedHoleVerticesBin = BinPlacer.bin.HoleOutline
			// 		.Select(v => v + (Vector2)BinPlacer.bin.transform.position).ToList();
			if (PolygonUtility.IsObjectInsideBinWithHole(translatedPlacedObjectVertices, translatedBinOutline, translatedBinHole))
			{
				// Debug.Log("Object is not Inside the BIN");
				return true;
			}
		}

		return false;
	}

	private float CalculateTotalDistanceToBinVertices(List<Vector2> objectVertices,
			Vector2 newPosition, List<Vector2> binVertices)
	{
		float totalDistance = 0;
		foreach (Vector2 objectVertex in objectVertices)
		{
			float minDistance = float.MaxValue;
			foreach (Vector2 binVertex in binVertices)
			{
				float distance = Vector2.Distance(objectVertex + newPosition, binVertex);
				if (distance < minDistance)
				{
					minDistance = distance;
				}
			}

			totalDistance += minDistance;
		}

		return totalDistance;
	}



	private IEnumerator IsValidPlacementCoroutine(ScannedObjectSvg newObject, Vector2 position,
			List<ScannedObjectSvg> placedObjects)
	{
		MeshRenderer meshRenderer = newObject.transform.GetComponent<MeshRenderer>();
		
		// Translate the vertices to the specified position
		List<Vector2> translatedVertices =
				newObject.OutlineVertices.Select(v => v + position).ToList();
		List<Vector2> translatedVerticesBin = BinPlacer.bin.OutlineVertices
				.Select(v => v + (Vector2)BinPlacer.bin.transform.position).ToList();
		tmpBinVertices = translatedVerticesBin;
		tmpObjectVertices = translatedVertices;
		// // Check if newObject is inside the bin.
		if (!PolygonUtility.IsObjectInsideBin(translatedVertices, translatedVerticesBin))
		{
			// IsValidPlacementResult = false;
			ChangeMaterialColor(meshRenderer, Color.magenta);
			yield break;
		}

		// Check if newObject intersects with any of the already placed objects.
		foreach (ScannedObjectSvg placedObject in placedObjects)
		{
			// Translate the rotated vertices of the placed object to their respective positions
			List<Vector2> translatedPlacedObjectVertices = placedObject.OutlineVertices
					.Select(v => v + (Vector2)placedObject.transform.position).ToList();
			tmpPlacedObjectVertices = translatedPlacedObjectVertices;
			// Debug.Log($"New object touching {placedObject.MyCollider.IsTouching(newObject.MyCollider)}");
			if (PolygonUtility.CheckCollision(translatedVertices, translatedPlacedObjectVertices))
			{
				// IsValidPlacementResult = false; // Found an intersection with one of the placed objects.
				ChangeMaterialColor(meshRenderer, Color.red);
				yield break;
			}
			yield return new WaitForSeconds(0.2f); // You can adjust the time for visualization
		}
		ChangeMaterialColor(meshRenderer, Color.green);
		// IsValidPlacementResult = true;
	}


	public IEnumerator CheckIfObjectPlacementITsGood()
	{
		foreach (ScannedObjectSvg scannedObject in BinPlacer.PlacedObjects)
		{
			MeshRenderer meshRenderer = scannedObject.transform.GetComponent<MeshRenderer>();

			// Create a copy of the objectToPlace list and remove the current object
			List<ScannedObjectSvg> objectsToCheck = new List<ScannedObjectSvg>(BinPlacer.PlacedObjects);
			objectsToCheck.Remove(scannedObject);
			// yield return IsValidPlacementCoroutine(scannedObject, scannedObject.transform.position,
			// objectsToCheck);
			if (IsValidPlacement(scannedObject, scannedObject.transform.position, objectsToCheck))
			{
				ChangeMaterialColor(meshRenderer, Color.green);
			}
			else
			{
				ChangeMaterialColor(meshRenderer, Color.red);
			}

		}

		yield return null;
	}

	private void ChangeMaterialColor(MeshRenderer meshRenderer, Color color)
	{
		if (meshRenderer != null && meshRenderer.materials.Length > 0)
		{
			Material materialToChange = meshRenderer.materials[0];
			materialToChange.color = color;
		}
	}


	public void GizmoDrawing()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(curVertexMoving, 0.002f);
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(curVerticesStationary, 0.003f);
		
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(curVertexMoving, 0.002f);
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(curVerticesStationary, 0.003f);
		
		DrawVertices2D(tmpObjectVertices, Color.white, Color.green, 0.01f);
		DrawVertices2D(translatedBinOutline, Color.black, Color.blue, 0.001f);
		DrawVertices2D(translatedBinHole, Color.gray, Color.magenta, 0.001f);
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

