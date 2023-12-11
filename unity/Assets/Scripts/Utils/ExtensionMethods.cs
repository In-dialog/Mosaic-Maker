
using UnityEngine;
using UnityEngine.UI;

public static class ExtensionMethods
{
	public static Vector3 ToVector(this Color color)
	{
		Color32 converted = color;
		Vector3 tmp = new Vector3
		{
				x = converted.r,
				y = converted.g,
				z = converted.b
		};
		return tmp;
	}
	
	// Extension method for Texture2D to get PNG data
	public static byte[] GetPNGData(this Texture2D texture2D)
	{
		return texture2D.EncodeToPNG();
	}

	// Extension method for RawImage to set image from byte array
	public static void SetImage(this RawImage rawImage, byte[] rawData)
	{
		Texture2D texture = new Texture2D(2, 2);
		texture.LoadImage(rawData);
		texture.Apply();
		rawImage.texture = texture;
	}

	// Extension method for RawImage to set image from Color array
	public static void SetImage(this RawImage rawImage, Color[] colorsData)
	{
		if (rawImage.texture is Texture2D texture2D)
		{
			texture2D.SetPixels(colorsData);
			texture2D.Apply();
		}
		else
		{
			Debug.LogError("RawImage does not have a Texture2D as its texture.");
		}
	}
	
	// Helper method to check if Vector3 is approximately equal to Vector2 (ignoring Z-axis)
	public static bool Approximately(this Vector3 v3, Vector2 v2)
	{
		return Mathf.Approximately(v3.x, v2.x) && Mathf.Approximately(v3.y, v2.y);
	}
}
