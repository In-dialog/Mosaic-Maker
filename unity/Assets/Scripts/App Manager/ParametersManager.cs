using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class ParamsSaveData
{
	public float GapTiles;
	public float OffsetZAxisTiles;
	public float DepthTiles;
	public SerializableVector2 SizeMosaic;
	public int NumberDominantColors;
}


public static class ParametersManager
{
	private const string FILE_NAME_SAVE = "Param.json";

	public static float GapTiles = 0.003f;
	public static float OffsetZAxisTiles = -0.0075f;
	public static float DepthTiles = 0.001f;

	public static Vector2 SizeMosaic = Vector2.one;
	public static int NumberDominantColors = 4;


	public static void UpdateParameters(Dictionary<string, TMP_InputField> paramsInput)
	{
		bool allUpdatesSuccessful = true;

		allUpdatesSuccessful &= UpdateFloatParameter(paramsInput, "Gap", value => GapTiles = value);
		allUpdatesSuccessful &= UpdateFloatParameter(paramsInput, "Offset_Z", value => OffsetZAxisTiles = value);
		allUpdatesSuccessful &= UpdateFloatParameter(paramsInput, "Depth", value => DepthTiles = value);
		allUpdatesSuccessful &= UpdateVector2Parameter(paramsInput, "X", "Y", value => SizeMosaic = value);
		allUpdatesSuccessful &= UpdateIntParameter(paramsInput, "Dominant_Colors", value => NumberDominantColors = value);

		if (allUpdatesSuccessful)
		{
			SaveParameters(); // Call the method to save the parameters
		}
		else
		{
			Debug.LogWarning("One or more parameter updates failed. Changes will not be saved.");
		}
	}

	
	private static bool UpdateVector2Parameter(Dictionary<string, TMP_InputField> paramsInput, string keyX, string keyY, Action<Vector2> updateAction)
	{
		if (paramsInput.TryGetValue(keyX, out TMP_InputField fieldX) && float.TryParse(fieldX.text, out float x) &&
		    paramsInput.TryGetValue(keyY, out TMP_InputField fieldY) && float.TryParse(fieldY.text, out float y))
		{
			updateAction(new Vector2(x, y));
			return true;
		}
		else
		{
			Debug.LogWarning($"Could not find or parse '{keyX}' or '{keyY}' in dictionary");
		}

		return false;
	}
	
	private static bool UpdateFloatParameter(Dictionary<string, TMP_InputField> paramsInput, string key, Action<float> updateAction)
	{
		if (paramsInput.TryGetValue(key, out TMP_InputField field) && float.TryParse(field.text, out float value))
		{
			updateAction(value);
			return true;
		}
		else
		{
			Debug.LogWarning($"Could not find or parse '{key}' in dictionary");
		}

		return false;
	}

	private static bool UpdateIntParameter(Dictionary<string, TMP_InputField> paramsInput, string key, Action<int> updateAction)
	{
		if (paramsInput.TryGetValue(key, out TMP_InputField field) && int.TryParse(field.text, out int value))
		{
			updateAction(value);
			return true;
		}
		else
		{
			Debug.LogWarning($"Could not find or parse '{key}' in dictionary");
		}

		return false;
	}
	
	public static void SaveParameters()
	{
		ParamsSaveData data = new ParamsSaveData
		{
				GapTiles = GapTiles,
				OffsetZAxisTiles = OffsetZAxisTiles,
				DepthTiles = DepthTiles,
				SizeMosaic = SerializableVector2.FromVector2(SizeMosaic),
				NumberDominantColors = NumberDominantColors
		};

		string json = JsonUtility.ToJson(data);
		System.IO.File.WriteAllText(Application.persistentDataPath + "/" + FILE_NAME_SAVE, json);
	}

	public static void LoadParameters()
	{
		string path = Application.persistentDataPath + "/" + FILE_NAME_SAVE;
		if (System.IO.File.Exists(path))
		{
			string json = System.IO.File.ReadAllText(path);
			ParamsSaveData data = JsonUtility.FromJson<ParamsSaveData>(json);

			GapTiles = data.GapTiles;
			OffsetZAxisTiles = data.OffsetZAxisTiles;
			DepthTiles = data.DepthTiles;
			SizeMosaic = data.SizeMosaic.ToVector2();
			NumberDominantColors = data.NumberDominantColors;
		}
	}
	
	public static string GetParametersAsString()
	{
		return $"GapTiles: {GapTiles}, " +
		       $"OffsetZAxisTiles: {OffsetZAxisTiles}, " +
		       $"DepthTiles: {DepthTiles}, " +
		       $"SizeMosaic: {SizeMosaic}, " +
		       $"NumberDominantColors: {NumberDominantColors}";
	}
}
