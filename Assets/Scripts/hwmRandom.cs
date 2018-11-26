using UnityEngine;

public static class hwmRandom
{
	private static System.Random ms_Random = new System.Random();

	public static float RandFloat()
	{
		return (float)ms_Random.NextDouble();
	}

	public static float RandFloat(float min, float max)
	{
		return (float)ms_Random.NextDouble() * (max - min) + min;
	}

	public static Color RandColorRGBA()
	{
		return new Color(RandFloat(), RandFloat(), RandFloat(), RandFloat());
	}

	public static Color RandColorRGB(float a = 1.0f)
	{
		return new Color(RandFloat(), RandFloat(), RandFloat(), a);
	}

	public static Vector2 RandVector2(Vector2 min, Vector2 max)
	{
		return new Vector2(RandFloat(min.x, max.x), RandFloat(min.y, max.y));
	}
}