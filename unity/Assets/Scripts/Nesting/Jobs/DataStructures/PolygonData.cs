
using Unity.Collections;
using UnityEngine;

public struct PolygonData
{
	public Vector2 pos;
	public Quaternion rot;

	public NativeArray<Vector2> vertices;
}
