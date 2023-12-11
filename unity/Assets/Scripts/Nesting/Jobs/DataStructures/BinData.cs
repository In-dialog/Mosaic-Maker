
using Unity.Collections;
using UnityEngine;

public struct BinData
{
	public Vector2 Pos;
	public Quaternion Rot;

	public NativeArray<Vector2> VerticesOutline;
	public NativeArray<Vector2> VerticesHole;

	public NativeArray<PolygonData> PlacedObjects;
}