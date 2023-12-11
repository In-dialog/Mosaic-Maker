using UnityEngine;

[System.Serializable]
public class SerializableVector2
{
	public float x;
	public float y;

	public SerializableVector2(float x, float y)
	{
		this.x = x;
		this.y = y;
	}

	public Vector2 ToVector2()
	{
		return new Vector2(x, y);
	}

	public static SerializableVector2 FromVector2(Vector2 vector)
	{
		return new SerializableVector2(vector.x, vector.y);
	}
}