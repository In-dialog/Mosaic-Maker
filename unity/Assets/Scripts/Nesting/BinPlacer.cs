using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;

public class BinPlacer : MonoBehaviour
{
	public List<ScannedObjectSvg> objectToPlace; //FOR DEBUG PURPOSE
	public BinSvg bin;
	public List<ScannedObjectSvg> PlacedObjects = new List<ScannedObjectSvg>();
	private NFPGenerator _generator;
	private NFPSelector _selector;
	
	private void Start()
	{
		_generator = new NFPGenerator(this);
		_generator.roteteOn = false;
		_selector = new NFPSelector(this);
	}

	/// <summary>
	/// Debug Perpose of this update in case there are problem with the bin placer
	/// </summary>
	/// <param name="objectSvgs"></param>
	/// <returns></returns>
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			StartCoroutine(PlaceAllObjectsAtOnce(objectToPlace));
		}
	}

	private IEnumerator PlaceAllObjectsAtOnce(List<ScannedObjectSvg> objectSvgs)
	{
		foreach (ScannedObjectSvg objectSvg in objectSvgs)
		{
			yield return (PlaceObjectInBinCoroutine(objectSvg, null));
		}
	}

	public IEnumerator PlaceObjectInBinCoroutine(ScannedObjectSvg newObject,
			Action<bool> ObjectPlaced)
	{
		if (PlacedObjects.Count == 0) // If there are no objects in the bin yet
		{
			yield return PlaceAtBottomLeft(newObject, ObjectPlaced);
		}
		else
		{
			List<ANFPResult> nfp = new List<ANFPResult>();

			foreach (ScannedObjectSvg placedObject in PlacedObjects)
			{
				yield return _generator.GenerateNfp(placedObject, newObject, nfp);
			}

			if (nfp.Count == 0)
			{
				Debug.Log($"No valid position found for the object {newObject.transform.name}");
				ObjectPlaced?.Invoke(false);
				yield return null;
				yield break;
			}
			
			yield return _selector.FindBestFitResultCoroutine(nfp, newObject, ObjectPlaced);
		}
	}

	private bool CheckIfAllJobsFinished(List<JobHandle> jobHandles)
	{
		int countJobsFinised = 0;
		for (int i = 0; i < jobHandles.Count; i++)
		{
			
			if (jobHandles[i].IsCompleted)
				countJobsFinised++;
		}
		
		return countJobsFinised == jobHandles.Count - 1;
	}

	private AllPlacedObjects GetPlacedObjectsDataForIJob()
	{
		List<Vector2> allVertices = new List<Vector2>();
		List<Vector2> allVerticesExpanded = new List<Vector2>();
		List<PlaceObjectData> placeObjectsData = new List<PlaceObjectData>();

		for (int i = 0; i < PlacedObjects.Count; i++)
		{
			allVertices.AddRange(PlacedObjects[i].OutlineVertices.Select(vec => new Vector2(vec.x + PlacedObjects[i].transform.position.x, vec.y + PlacedObjects[i].transform.position.y)).ToList().ToNativeArray(Allocator.Persistent));
			allVerticesExpanded.AddRange(PlacedObjects[i].ExpandedOutlineVertices.Select(vec => new Vector2(vec.x + PlacedObjects[i].transform.position.x, vec.y + PlacedObjects[i].transform.position.y)).ToList().ToNativeArray(Allocator.Persistent));
			placeObjectsData.Add(new PlaceObjectData
			{
					Length = PlacedObjects[i].OutlineVertices.Count,
					StartIndex = allVertices.Count - PlacedObjects[i].OutlineVertices.Count
			});
		}

		return new AllPlacedObjects()
		{
				allVertices = allVertices.ToNativeArray(Allocator.Persistent),
				allVerticesExpanded = allVerticesExpanded.ToNativeArray(Allocator.Persistent),
				ObjectsData = placeObjectsData.ToNativeArray(Allocator.Persistent)
		};
	}

	//--------------------------- BOTTOM LEFT PLACEMENT --------------------------------------------

	private IEnumerator PlaceAtBottomLeft(ScannedObjectSvg newObject, Action<bool> ObjectPlaced)
	{
		// Adjust these offset values as needed
		float xOffset = 0.001f; 
		float yOffset = 0.001f;
		Vector3 originalPosition = newObject.transform.position;

		Vector3 binMin = bin.bottomLeft + (Vector2)bin.transform.position; // Assuming the bin's transform is at the center of the bounding box
		Vector3 binMax = bin.transform.position + bin.MyCollider.bounds.size * 0.5f;

		for (float y = binMin.y; y <= binMax.y; y += yOffset)
		{
			for (float x = binMin.x; x <= binMax.x; x += xOffset)
			{
				Vector3 worldPosition = new Vector2(x, y);

				if (_generator.IsValidPlacement(newObject, worldPosition, PlacedObjects))
				{
					PlacedObjects.Add(newObject);
					bin.areaLeft -= newObject.AreaTotal;
					newObject.transform.position = new Vector3(worldPosition.x, worldPosition.y, 0);
					newObject.MyBin = this.bin;
					newObject.transform.SetParent(this.transform);
					var transformPosition = newObject.transform.position;
					transformPosition.z += newObject.transform.forward.z * newObject.DepthObject;
					newObject.transform.position = transformPosition;
					Setup3DPosition(newObject);
					ObjectPlaced?.Invoke(true);
					yield break; 
				}

				newObject.transform.position = worldPosition;
				// Adjust this wait time as needed for visual feedback or performance
				yield return new WaitForSeconds(.0001f); 
			}
		}
		newObject.transform.position = originalPosition;
		// If the code reaches here, it means the object could not be placed
		ObjectPlaced?.Invoke(false);
	}

	public void Setup3DPosition(ScannedObjectSvg newObject)
	{
		GameObject tmp3DObject = Instantiate(newObject.gameObject);
		tmp3DObject.name += "_DUMMY";
		tmp3DObject.transform.SetParent(this.bin.my3DClone);
		MosaicNesting mosaicNesting = FindObjectOfType<MosaicNesting>();
		tmp3DObject.transform.localPosition = new Vector3(newObject.transform.localPosition.x,
				newObject.transform.localPosition.y, newObject.transform.localPosition.z - ParametersManager.OffsetZAxisTiles);
		tmp3DObject.transform.localRotation = Quaternion.Euler(newObject.transform.eulerAngles.x, newObject.transform.eulerAngles.y, newObject.transform.eulerAngles.z);
		tmp3DObject.layer = 6;
		// Debug.Log($"Object its placed and it rotation is {newObject.transform.localRotation.eulerAngles} and the 3d rotation {tmp3DObject.transform.rotation.eulerAngles}");
		newObject.TargetPosition = tmp3DObject.transform.position;
		newObject.TargetRotation = tmp3DObject.transform.localRotation;
		// Destroy(tmp3DObject);
		// newObject.myClone = tmp3DObject;
	}
	
	private void OnDrawGizmosSelected()
	{
		if (_generator != null)
			_generator.GizmoDrawing();
		if (_selector != null)
			_selector.GizmoDrawing();
	}
}