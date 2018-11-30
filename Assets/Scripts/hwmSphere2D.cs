using UnityEngine;

[System.Serializable]
public struct hwmSphere2D
{
	/// <summary>
	/// The sphere's center point.
	/// </summary>
	public Vector2 Center;

	/// <summary>
	/// The sphere's radius.
	/// </summary>
	public float Radius;

	public hwmSphere2D(Vector2 center, float radius)
	{
		Center = center;
		Radius = radius;
	}

	/// <summary>
	/// Checks whether a given box is fully encapsulated by this sphere.
	/// </summary>
	/// <param name="box"></param>
	/// <param name="tolerance">Tolerance Error Tolerance.</param>
	public bool IsInside(hwmBox2D box, float tolerance = hwmConstants.KINDA_SMALL_NUMBER)
	{
		float radiusSquared = Radius * Radius + tolerance * tolerance;
		return (box.Min - Center).sqrMagnitude <= radiusSquared
			&& (box.Max - Center).sqrMagnitude <= radiusSquared;
	}

	/// <summary>
	/// Check whether sphere is inside of another.
	/// </summary>
	/// <param name="other">Other The other sphere.</param>
	/// <param name="tolerance">Tolerance Error Tolerance.</param>
	/// <returns>true if sphere is inside another, otherwise false.</returns>
	public bool IsInside(hwmSphere2D other, float tolerance = hwmConstants.KINDA_SMALL_NUMBER)
	{
		if (Radius > other.Radius + tolerance)
		{
			return false;
		}

		return (Center - other.Center).sqrMagnitude <= Mathf.Sqrt(other.Radius + tolerance - Radius);
	}

	/// <summary>
	/// Checks whether the given location is inside this sphere.
	/// </summary>
	/// <param name="point">In The location to test for inside the bounding volume.</param>
	/// <param name="tolerance">Tolerance Error Tolerance</param>
	/// <returns>true if location is inside this volume.</returns>
	public bool IsInside(Vector2 point, float tolerance = hwmConstants.KINDA_SMALL_NUMBER)
	{
		return (Center - point).sqrMagnitude <= Mathf.Sqrt(Radius + tolerance);
	}
}