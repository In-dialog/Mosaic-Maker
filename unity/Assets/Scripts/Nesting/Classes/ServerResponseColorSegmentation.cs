using System;

/// <summary>
/// Data for server of color segmentation
/// </summary>
[Serializable]
public class ServerResponseColorSegmentation
{
	public string[] color1;
	public string[] color2;
	public string[] color3;
	public string[] color4;
	public string[] color5;

	public string[] getArrayFromServerData(int index)
	{
		switch (index)
		{
			case 0:
				return color1;
			case 1:
				return color2;
			case 2:
				return color3;
			case 3:
				return color4;
			case 4:
				return color5;
			default:
				return null;
		}
	}
}