
using System;
using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class ParallelJob : MonoBehaviour
{
	// Job adding two floating point values together
	private struct MyParallelJob : IJobParallelFor
	{
		[ReadOnly]
		public NativeList<float> a;
		[ReadOnly]
		public NativeList<float> b;

		
		public NativeArray<float> result;

		public void Execute(int i)
		{
			result[i]= (a[i] + b[i]);
		}
	}

	private NativeList<float> a;
	private NativeList<float> b;
	private NativeArray<float> result;

	private void Start()
	{
		a = new NativeList<float>(Allocator.Persistent);
		b = new NativeList<float>(Allocator.Persistent);
		result = new NativeArray<float>(2, Allocator.Persistent);
		a.Add(1f);
		b.Add(2f);
		a.Add(3f);
		b.Add(6f);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			StartCoroutine(CorJob());
		}
	}
	


	IEnumerator CorJob()
	{
		MyParallelJob job = new MyParallelJob()
		{
				a = this.a,
				b = b,
				result = result
		};
		JobHandle handle = job.Schedule(2, 1);
		handle.Complete();

		yield return new WaitUntil(() => handle.IsCompleted);
		for (int i = 0; i < result.Length; i++)
		{
			Debug.Log(result[i]);
		}
	}

	private void OnDestroy()
	{
		a.Dispose();
		b.Dispose();
	}
}
