
using UnityEngine;

public struct ANFPResult
{
	public Vector2 Position { get; set; }
	public Quaternion Rotation { get; set; }
	public float TotalDistanceToBinVertices { get; set; }

	public ANFPResult(Vector2 position, Quaternion rotation)
	{
		Position = position;
		Rotation = rotation;
		TotalDistanceToBinVertices = 0;
	}
}
