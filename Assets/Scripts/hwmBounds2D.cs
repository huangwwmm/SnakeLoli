using UnityEngine;

[System.Serializable]
public struct hwmBounds2D
{
	public Vector2 center;

	public Vector2 extents;

	public Vector2 size
	{
		get
		{
			return extents * 2f;
		}
		set
		{
			extents = value * 0.5f;
		}
	}

	public Vector2 min
	{
		get
		{
			return center - extents;
		}
		set
		{
			SetMinMax(value, max);
		}
	}

	public Vector2 max
	{
		get
		{
			return center + extents;
		}
		set
		{
			SetMinMax(min, value);
		}
	}

	public static bool operator ==(hwmBounds2D lhs, hwmBounds2D rhs)
	{
		return lhs.center == rhs.center && lhs.extents == rhs.extents;
	}

	public static bool operator !=(hwmBounds2D lhs, hwmBounds2D rhs)
	{
		return !(lhs == rhs);
	}

	public static implicit operator hwmBounds2D(Bounds bound)
	{
		return new hwmBounds2D(bound.center, bound.size);
	}

	public static implicit operator Bounds(hwmBounds2D bound)
	{
		return new Bounds(bound.center, bound.size);
	}

	public hwmBounds2D(Vector2 center, Vector2 size)
	{
		this.center = center;
		extents = size * 0.5f;
	}

	public override int GetHashCode()
	{
		return center.GetHashCode() ^ extents.GetHashCode() << 2;
	}

	public override bool Equals(object other)
	{
		if (!(other is hwmBounds2D))
			return false;

		hwmBounds2D bounds = (hwmBounds2D)other;
		return center.Equals(bounds.center) && extents.Equals(bounds.extents);
	}

	public override string ToString()
	{
		return string.Format("Center: {0}, Extents: {1}", center, extents);
	}

	public void SetMinMax(Vector2 min, Vector2 max)
	{
		extents = (max - min) * 0.5f;
		center = min + extents;
	}

	public bool Contains(hwmBounds2D bounds)
	{
		Vector2 myMin = min;
		Vector2 myMax = max;
		Vector2 otherMin = bounds.min;
		Vector2 otherMax = bounds.max;

		return myMin.x <= otherMin.x
			&& myMin.y <= otherMin.y
			&& myMax.x >= otherMax.x
			&& myMax.y >= otherMax.y;
	}

	public bool Contains(Vector2 point)
	{
		Vector2 myMin = min;
		Vector2 myMax = max;

		return myMin.x <= point.x
			&& myMin.y <= point.y
			&& myMax.x >= point.x
			&& myMax.y >= point.y;
	}

	public bool Intersects(hwmBounds2D bounds)
	{
		Vector2 myMin = min;
		Vector2 myMax = max;
		Vector2 otherMin = bounds.min;
		Vector2 otherMax = bounds.max;

		return myMin.x <= otherMax.x 
			&& myMax.x >= otherMin.x 
			&& myMin.y <= otherMax.y 
			&& myMax.y >= otherMin.y;
	}
}