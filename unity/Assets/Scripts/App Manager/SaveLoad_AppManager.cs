
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class AppManager
{
	private const string FILE_NAME_SAVE = "SaveData.json";

	public void SaveAppState()
	{
		SaveDataApp saveDataApp = new SaveDataApp
		{
				binsData = mosaicNesting.GetMosaicData(),
				currentImage = (UiManager.Instance.displayImage.texture as Texture2D).GetPNGData()
		};

		// Convert the list to JSON format as shown in the previous example
		string jsonData = JsonUtility.ToJson(saveDataApp);
		
		
		// Create a file path to save the data
		string filePath = Path.Combine(Application.persistentDataPath, FILE_NAME_SAVE);
		
		// Write the JSON string to the file
		File.WriteAllText(filePath, jsonData);
		
		Debug.Log("Data saved to: " + filePath);
	}
	
	public bool SaveExists()
	{
		// Check if save data exists
		string filePath = Path.Combine(Application.persistentDataPath, FILE_NAME_SAVE);
		return File.Exists(filePath);
	}

	public void LoadApp()
	{
		StartCoroutine(LoadAppState());
	}

	protected IEnumerator LoadAppState()
	{
		if (SaveExists() == false)
			yield break;
		UiManager.Instance.DisableButton("Load");
		// Choose the file name for the save file
		string filePath = Path.Combine(Application.persistentDataPath, FILE_NAME_SAVE);
		SaveDataApp loadedDataApp = null;
		// Create the file path
		try
		{
			// Read the JSON string from the file
			string jsonData = File.ReadAllText(filePath);
	
			// Deserialize the JSON string to the SaveDataList wrapper class
			loadedDataApp = JsonUtility.FromJson<SaveDataApp>(jsonData);
			
			Debug.Log("Data loaded from: " + filePath);
		}
		catch (Exception ex)
		{
			Debug.LogError("Error loading data: " + ex.Message);
		}

		if (loadedDataApp != null)
		{
			UiManager.Instance.displayImage.SetImage(loadedDataApp.currentImage);
			//Send Bins Data To Mosaic Nesting
			yield return mosaicNesting.Initialize_From_SaveData(loadedDataApp.binsData);
			UiManager.Instance.EnableButton("Start");
			UiManager.Instance.EnableButton("Clear");
		}

		UiManager.Instance.EnableButton("Load");
	}
}
