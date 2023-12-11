using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ColourBinsData
{
	public string name;
	public List<BinDataSave> myBinsData;

	public ColourBinsData(ColourBinsManager binsManager)
	{
		myBinsData = new List<BinDataSave>();
		foreach (BinSvg bin in binsManager.myBins)
		{
			myBinsData.Add(bin.GetBinData());
		}
		name = binsManager.name;
	}

	public ColourBinsManager InstantiateColourBinsManager()
	{
		GameObject binsManager = GameObject.Find(name);
		if (binsManager == null)
		{
			binsManager = new GameObject(name);
			binsManager.transform.position = Vector3.zero;
		}
		ColourBinsManager refBinsManager = binsManager.GetComponent<ColourBinsManager>();

		if (refBinsManager == null)
		{
			refBinsManager = binsManager.AddComponent<ColourBinsManager>();
		}
		
		refBinsManager.InstantiateBins(this);
		return refBinsManager;
	}
}


[Serializable]
public class ColourBinsManager : MonoBehaviour
{
	public List<BinSvg> myBins;
	public Color MyColour;
	
	public void InstantiateBins(string pathToFile)
	{
		TextAsset[] svgTexts = Resources.LoadAll<TextAsset>(pathToFile);
		myBins = new List<BinSvg>();
		for (int i = 0; i < svgTexts.Length; i++)
		{
			GameObject colorPartObj = new GameObject(svgTexts[i].name);
			colorPartObj.transform.SetParent(this.transform);
			BinSvg myRefSvgMesh = colorPartObj.AddComponent<BinSvg>();
			myRefSvgMesh.GenerateObject(svgTexts[i].text);
			MyColour = myRefSvgMesh.MColor;
			myBins.Add(myRefSvgMesh);
		}
	}
	
	public void InstantiateBins(ColourBinsData savedData)
	{
		myBins = new List<BinSvg>();
		for (int i = 0; i < savedData.myBinsData.Count; i++)
		{
			myBins.Add(savedData.myBinsData[i].InstantiateBinWithObject());
			myBins[^1].transform.SetParent( this.transform);
			MyColour = myBins[^1].MColor;
		}
	}

	public void InstantiateBins(string[] colorData, int indexColor)
	{
		myBins = new List<BinSvg>();

		for (int i = 0; i < colorData.Length; i++)
		{
			GameObject colorPartObj = new GameObject($"color_{indexColor}_{i}");
			colorPartObj.transform.SetParent(this.transform);
			BinSvg myRefSvgMesh = colorPartObj.AddComponent<BinSvg>();
			myRefSvgMesh.GenerateObject(colorData[i]);
			if (myRefSvgMesh.OutlineVertices != null)
			{			
				MyColour = myRefSvgMesh.MColor;
				myBins.Add(myRefSvgMesh);
			}
			else
			{
				colorPartObj.transform.SetParent(null);
				Destroy(colorPartObj);
				Debug.LogWarning("Could not construct bin");
			}
		}
	}

	public void Set3DClone(Transform my3DClone)
	{
		my3DClone.gameObject.layer = 6;
		// Debug.Log($"The count of 3d child {my3DClone.childCount} and the bin child count {myBins.Count}");
		for (int i = 0; i < my3DClone.childCount; i++)
		{
			Transform clone3DBin = my3DClone.GetChild(i);
			myBins[i].SetUp3dClone(clone3DBin);
		}

		ColourBinsManager tmpClone = my3DClone.gameObject.GetComponent<ColourBinsManager>();
		if (tmpClone != null)
		{
			Destroy(tmpClone);
		}
		else
		{
			Debug.Log("Cant find Colour Bin Manager in clone");
		}
	}

	public void ClearPlacedObjects()
	{
		foreach (BinSvg bin in myBins)
		{
			bin.ClearPlacedObjects();
		}
	}


	public List<BinSvg> FindBinForPlacing(ScannedObjectSvg newObject)
	{
		List<BinSvg> binsForNesting = new List<BinSvg>(); 
		foreach (BinSvg bin in myBins)
		{
			if (bin.CheckIfCanBePlaced(newObject))
				binsForNesting.Add(bin);
		}

		return binsForNesting.Count == 0 ? null : binsForNesting;
	}

	public ColourBinsData GetDataForSaving()
	{
		return new ColourBinsData(this);
	}
}
