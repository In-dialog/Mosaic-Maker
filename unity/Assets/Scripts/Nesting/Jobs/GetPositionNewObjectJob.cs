using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct GetPositionNewObjectJob : IJobParallelFor
{
	[ReadOnly] public BinData bin;

	[ReadOnly] public PolygonData StaticObject;
	public PolygonData NewObject;
	public PositionResult Result;

	public void Execute(int index)
	{
		
	}
}
