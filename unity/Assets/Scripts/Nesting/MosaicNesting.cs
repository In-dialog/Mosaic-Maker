using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Data that will be saved for continuing from where it was left off the nesting
/// </summary>
[Serializable]
public class SaveDataApp
{
	public byte[] currentImage;
	public List<ColourBinsData> binsData;
}

public class MosaicNesting : MonoBehaviour
{
	public string pathToMosaic;
	public GameObject placeHolderMosaic;
	public GameObject placeHolderMosaic3D;
	public event Action<bool> ObjectPlacedState;
	
	
	private List<ColourBinsManager> _binsManagers = new List<ColourBinsManager>();
	private bool _objectIsPlaced;
	private List<Color> _colors = new List<Color>();
	private GameObject _clone3D;
	
	public IEnumerator PlaceNewObject(ScannedObjectSvg newObject)
	{
		// Debug.Log($"Placing obj = {newObject.name}");

		List<BinSvg> BinsForNesting = FindBinsForObject(newObject);
		if (BinsForNesting == null)
		{
			Debug.Log($"Could not find any bin New Object {newObject.name} Color {newObject.MColor}");
			yield break;
		}

		foreach (BinSvg binSvg in BinsForNesting)
		{
			_objectIsPlaced = false;
			yield return binSvg.placer.PlaceObjectInBinCoroutine(newObject,
					ObjectCheckStatus);
			if (_objectIsPlaced)
			{
				Debug.Log("OBJ PLACED");
				yield break;
			}
			else
			{
				Debug.Log($"Object {newObject.name} cant be placed in bin {binSvg.name}");
			}
		}

		Debug.Log($"NO POSITION FOUND FOR {newObject.name}");
	}

	public void ObjectCheckStatus(bool status)
	{
		_objectIsPlaced = status;
		ObjectPlacedState?.Invoke(status);
	}

	private List<BinSvg> FindBinsForObject(ScannedObjectSvg newObject)
	{
		int BinWithSimilarColours = UtilsColor.FindMostSimilarColorIndex(newObject.MColor, _colors);
		return _binsManagers[BinWithSimilarColours].FindBinForPlacing(newObject);
	}

	public List<ColourBinsData> GetMosaicData()
	{
		List<ColourBinsData> dataToSave = new List<ColourBinsData>();
		foreach (ColourBinsManager binsSelector in _binsManagers)
		{
			dataToSave.Add(binsSelector.GetDataForSaving());
		}

		return dataToSave;
	}

	public void ClearAllPlacedObjects()
	{
		foreach (ColourBinsManager binsManager in _binsManagers)
		{
			binsManager.ClearPlacedObjects();
		}
	}


	private void Initialize_From_Default()
	{
		Vector3 curPosition = placeHolderMosaic.transform.position;
		placeHolderMosaic.transform.position = new Vector3(0.51f, 0.51f, 0);
		Initialize_Default();
		placeHolderMosaic.transform.position = curPosition;
	}

	public IEnumerator Initialize_From_Server(ServerResponseColorSegmentation response)
	{
		yield return CleanCurrentMosaic();

		for (int i = 0; i < ParametersManager.NumberDominantColors; i++)
		{
			InitializeBinsManager(response: response, index: i);
		}
		this.transform.position = placeHolderMosaic.transform.position;
		this.transform.position -= new Vector3(ParametersManager.SizeMosaic.x/2, ParametersManager.SizeMosaic.y/2, 0);
		
		Setup3DMosaic();
	}

	public IEnumerator Initialize_From_SaveData(List<ColourBinsData> binsData)
	{
		yield return CleanCurrentMosaic();

		try
		{
			foreach (ColourBinsData binDataSave in binsData)
			{
				InitializeBinsManager(binData: binDataSave);
			}
			this.transform.position = placeHolderMosaic.transform.position;
			this.transform.position -= new Vector3(ParametersManager.SizeMosaic.x/2, ParametersManager.SizeMosaic.y/2, 0);
			Setup3DMosaic();
		}
		catch (Exception ex)
		{
			Debug.LogError("Error loading data: " + ex.Message);
		}
	}

	private void Initialize_Default()
	{
		CleanCurrentMosaic();
		
		string[] directories =
				Directory.GetDirectories(Application.dataPath + "/Resources/" + pathToMosaic);

		foreach (string directory in directories)
		{
			string nameOfManager = directory.Split("/")[^1];
			InitializeBinsManager(binManagerName: nameOfManager);
		}
	}

	private void InitializeBinsManager(ColourBinsData binData = null, string binManagerName = null,
			ServerResponseColorSegmentation response = null, int index = 0)
	{
		GameObject binsManager;
		if (!string.IsNullOrEmpty(binManagerName))
		{
			binsManager = new GameObject(binManagerName);
		}
		else if (binData != null)
		{
			binsManager = new GameObject(binData.name);
		}
		else
		{
			binsManager = new GameObject("color_manager_" + index);
		}
		// binsManager.transform.position = Vector3.zero;

		ColourBinsManager refBinsManager = binsManager.GetComponent<ColourBinsManager>();

		if (refBinsManager == null)
		{

			refBinsManager = binsManager.AddComponent<ColourBinsManager>();
		}

		if (binData != null)
		{

			refBinsManager.InstantiateBins(binData);
		}
		else if (response != null)
		{

			refBinsManager.InstantiateBins(response.getArrayFromServerData(index), index);
		}
		else if (!string.IsNullOrEmpty(binManagerName))
		{

			refBinsManager.InstantiateBins(pathToMosaic + $"/{binManagerName}");
		}

		_binsManagers.Add(refBinsManager);
		_colors.Add(refBinsManager.MyColour);
		_binsManagers[^1].transform.SetParent(this.transform);
	}

	private IEnumerator CleanCurrentMosaic()
	{
		if (_binsManagers == null || _binsManagers.Count <= 0)
			yield break;
		Debug.Log("Clean up mosaic");
		foreach (var colourBins in _binsManagers)
		{
			Destroy(colourBins.gameObject, 0);
		}


		_binsManagers.Clear();
		_colors.Clear();
		if (_clone3D != null)
			Destroy(_clone3D, 0);

		
		//Wait for the next frame in order for the destroy to be executed
		this.transform.position = Vector3.zero;
		yield return null;
	}

	private void Setup3DMosaic()
	{
		if (AppManager.Instance != null && !AppManager.Scene3dIsLoaded)
			return;
		
		placeHolderMosaic3D = GameObject.Find("PlaceHolder_3D");

		if (placeHolderMosaic3D == null)
			return;

		GameObject clone3D = Instantiate(this.gameObject, (null), true);

		clone3D.layer = 6;
		clone3D.transform.position = placeHolderMosaic3D.transform.position;
		clone3D.transform.rotation = placeHolderMosaic3D.transform.rotation;
		// clone3D.transform.position.x -= ParametersManager.SizeMosaic.x / 2;
		clone3D.transform.position += new Vector3( -placeHolderMosaic3D.transform.localScale.x/2, 0,
				ParametersManager.SizeMosaic.y - placeHolderMosaic3D.transform.localScale.z/2);
		// clone3D.transform.position += new Vector3(ParametersManager.SizeMosaic.x/2, 0, ParametersManager.SizeMosaic.y/2);

		ColourBinsManager[] copy3dBinsManager =
				clone3D.gameObject.GetComponentsInChildren<ColourBinsManager>();
		for (int i = 0; i < copy3dBinsManager.Length; i++)
		{
			_binsManagers[i].Set3DClone(copy3dBinsManager[i].transform);
		}

		MosaicNesting tmpClone = clone3D.GetComponent<MosaicNesting>();
		if (tmpClone != null)
		{
			Destroy(tmpClone);
		}
		else
		{
			Debug.Log("Cannot find Mosaic Nesting in clone");
		}
		_clone3D = clone3D;
	}
}
