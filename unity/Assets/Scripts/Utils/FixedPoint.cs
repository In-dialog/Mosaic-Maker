using System;

public struct FixedPoint
{
	private int value; // Store the value directly

	public FixedPoint(int fixedValue)
	{
		value = fixedValue;
	}

	public static FixedPoint FromMeters(float meters)
	{
		// Convert meters to the appropriate fixed-point value
		return new FixedPoint((int)(meters * 1000)); // 1 meter = 1000 fixed-point units
	}
	
	public static FixedPoint Zero => new FixedPoint(0);

	public float ToMeters()
	{
		// Convert the fixed-point value to meters
		return (float)value / 1000;
	}
	
	public static FixedPoint Abs(FixedPoint value)
	{
		return new FixedPoint(Math.Abs(value.value));
	}

	public static FixedPoint operator +(FixedPoint a, FixedPoint b)
	{
		return new FixedPoint(a.value + b.value);
	}

	public static FixedPoint operator -(FixedPoint a, FixedPoint b)
	{
		return new FixedPoint(a.value - b.value);
	}

	public static FixedPoint operator *(FixedPoint a, FixedPoint b)
	{

		return new FixedPoint((a.value * b.value) / 1000);
	}

	public static FixedPoint operator /(FixedPoint a, FixedPoint b)
	{
		if (b.value == 0)
			return new FixedPoint(0);
		return new FixedPoint((a.value * 1000) / b.value);
	}
	
	public static bool operator ==(FixedPoint a, FixedPoint b)
	{
		return a.value == b.value;
	}

	public static bool operator !=(FixedPoint a, FixedPoint b)
	{
		return a.value != b.value;
	}

	public static bool operator <(FixedPoint a, FixedPoint b)
	{
		return a.value < b.value;
	}

	public static bool operator >(FixedPoint a, FixedPoint b)
	{
		return a.value > b.value;
	}

	public static bool operator <=(FixedPoint a, FixedPoint b)
	{
		return a.value <= b.value;
	}

	public static bool operator >=(FixedPoint a, FixedPoint b)
	{
		return a.value >= b.value;
	}
	public override bool Equals(object obj)
	{
		if (obj is FixedPoint)
		{
			FixedPoint other = (FixedPoint)obj;
			return this.value == other.value;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return value.GetHashCode();
	}
}